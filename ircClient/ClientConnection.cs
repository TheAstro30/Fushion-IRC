/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;
using ircClient.Parsing;
using ircClient.Parsing.Helpers;
using ircClient.Tcp;
using ircCore.Settings;
using ircCore.Settings.Networks;
using ircCore.Settings.SettingsBase.Structures;
using ircCore.Utils;

namespace ircClient
{
    public class ClientConnection
    {
        /* This class does most of the work of parsing IRC messages - all events raised must return this class */
        public readonly SettingsUserInfo UserInfo;
        public readonly Parser Parser;

        private readonly ClientSock _sock;
        private readonly DnsResolve _localInfo;
        private readonly DnsResolve _dns;
        private readonly Identd _identd;
        
        private StringBuilder _buffer = new StringBuilder();
        private readonly List<string> _sockData = new List<string>();

        private Timer _tmrParse;
        private readonly Timer _tmrWaitToReconnect;

        private readonly Timer _tmrPingTimeout;
        private int _pingCheck;

        public bool IsConnecting { get; internal set; }
        public bool IsConnected { get; internal set; }

        public bool MotdShown { get; set; }

        public bool IsWaitingToReconnect { get; set; }
        public bool IsManualDisconnect { get; set; }

        public string SocketLocalIp { get { return _sock.LocalIp; } }
        public string SocketRemoteIp { get { return _sock.RemoteHostIp; } }

        public Server Server = new Server();
        public string Network { get; set; }

        public int ConnectionId { get; set; }

        public event Action<ClientConnection> OnClientBeginConnect;
        public event Action<ClientConnection> OnClientConnected;
        public event Action<ClientConnection> OnClientDisconnected;
        public event Action<ClientConnection> OnClientCancelConnection;
        public event Action<ClientConnection, string> OnClientConnectionError;
        public event Action<ClientConnection> OnClientConnectionClosed;
        public event Action<ClientConnection, X509Certificate> OnClientSslInvalidCertificate;

        public event Action<ClientConnection, DnsResult> OnClientLocalInfoResolved;
        public event Action<ClientConnection, DnsResult> OnClientLocalInfoFailed;

        public event Action<ClientConnection, DnsResult> OnClientDnsResolved;
        public event Action<ClientConnection, DnsResult> OnClientDnsFailed;

        public event Action<ClientConnection, string, string> OnClientIdentDaemonRequest;

        public event Action<ClientConnection> OnClientBeginQuit;

        /* Constructor */
        public ClientConnection(ISynchronizeInvoke syncObject, SettingsUserInfo userInfo)
        {
            _sock = new ClientSock(syncObject)
                        {
                            EnableSslAuthentication = true
                        };
            _sock.OnConnected += OnConnected;
            _sock.OnDisconnected += OnDisconnected;
            _sock.OnError += OnError;
            _sock.OnDataArrival += OnDataArrival;
            _sock.OnStateChanged += OnStateChanged;
            _sock.OnSslInvalidCertificate += OnSslInvalidCertificate;
            _sock.OnDebugOut += OnDataSent;
            
            Parser = new Parser(this);

            UserInfo = new SettingsUserInfo(userInfo);

            _localInfo = new DnsResolve(syncObject);
            _localInfo.DnsResolved += OnLocalInfoResolved;
            _localInfo.DnsFailed += OnLocalInfoFailed;

            _dns = new DnsResolve(syncObject);
            _dns.DnsResolved += OnDnsResolved;
            _dns.DnsFailed += OnDnsFailed;

            _identd = new Identd(syncObject, SettingsManager.Settings.Connection.Identd);
            _identd.OnIdentDaemonData += OnIdentDaemonData;

            _tmrWaitToReconnect = new Timer
                                      {
                                          Interval = 2000
                                      };
            _tmrWaitToReconnect.Tick += TimerWaitToReconnect;

            _tmrPingTimeout = new Timer {Interval = 1000};
            _tmrPingTimeout.Tick += TimerPingTimeoutCheck;
        }

        /* Connect overloads */
        public void Connect()
        {
            if (string.IsNullOrEmpty(Server.Address))
            {
                return;
            }
            Connect(Server.Address,
                    Server.Port > 0 ? Server.Port : SettingsManager.Settings.Connection.Options.DefaultPort,
                    Server.IsSsl);
        }

        public void Connect(string address, int port, bool ssl)
        {
            /* Attempt to get Network the server belongs to from network list */
            Network = ServerManager.GetNetworkNameByServerAddress(address);
            /* Update current server details */
            Server.Address = address;
            Server.Port = port;
            Server.IsSsl = ssl;
            /* Clear other parser variables */
            Parser.IsAdministrator = false;
            Parser.UserModeCharacters = string.Empty;
            Parser.UserModes = string.Empty;
            Parser.ChannelPrefixTypes = new ChannelTypes();
            Parser.ChannelModes = new List<char>();
            /* Begin connection */
            IsConnecting = true;
            _sock.IsSsl = ssl;
            _sock.Connect(address, port);
            /* Turn on identd */
            _identd.BeginIdentDaemon();          
            if (OnClientBeginConnect != null)
            {
                OnClientBeginConnect(this);
            }
            MotdShown = false;
        }

        /* Disconnection */
        public void Disconnect()
        {
            if (IsConnected)
            {
                if (OnClientBeginQuit != null)
                {
                    OnClientBeginQuit(this);
                }
            }
            else
            {
                _sock.Close();
            }
        }

        public void CancelConnection()
        {
            if (IsConnected)
            {
                if (OnClientBeginQuit != null)
                {
                    OnClientBeginQuit(this);
                }
            }
            else
            {
                _sock.Close();
            }
            if (OnClientCancelConnection != null)
            {
                OnClientCancelConnection(this);
            }
        }

        public void SslAcceptCertificate(bool accept)
        {
            _sock.SslAcceptCertificate(accept);
        }

        /* Sockwrite */
        public void Send(string data)
        {
            _sock.SendData(string.Format("{0}\r\n", Utf8.ConvertFromUtf8(data, true)));
        }

        /* DNS Methods */
        public void ResolveLocalInfo(string ip)
        {
            _localInfo.Resolve(ip);
        }

        public void ResolveDns(string dns)
        {
            _dns.Resolve(dns);
        }        

        /* Socket callbacks */
        private void OnConnected(ClientSock sock)
        {
            if (OnClientConnected != null)
            {
                OnClientConnected(this);
            }
            Send(string.Format("NICK {0}\r\nUSER {1} {2} {3} :{4}", UserInfo.Nick, UserInfo.Ident, _sock.LocalIp, _sock.RemoteHostIp, UserInfo.RealName));
            /* Start the ping-time out check */
            _pingCheck = 0;
            _tmrPingTimeout.Enabled = true;
        }

        private void OnDisconnected(ClientSock sock)
        {
            if (OnClientDisconnected != null)
            {
                OnClientDisconnected(this);
            }
            IsConnecting = false;
            IsConnected = false;
            /* Disable the ping-time out check */
            _pingCheck = 0;
            _tmrPingTimeout.Enabled = false;
        }

        private void OnError(ClientSock sock, string error)
        {
            if (OnClientConnectionError != null)
            {
                OnClientConnectionError(this, error);
            }
            IsConnecting = false;
            IsConnected = false;
        }

        private void OnDataArrival(ClientSock sock, int bytes)
        {
            /* We begin the parsing of the incomming IRC data */
            var data = string.Empty;
            _sock.GetData(ref data);            
            _buffer.Append(data);
            if (data[data.Length - 1] != (char)10 && data[data.Length - 1] != (char)13)
            {
                return;
            }
            var s = _buffer.ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            _sockData.AddRange(s);
            _buffer = new StringBuilder();
            /* Reset ping-timeout check */
            _pingCheck = 0;
            if (_tmrParse != null) { return; }
            _tmrParse = new Timer
                            {
                                Interval = 1, 
                                Enabled = true
                            };
            _tmrParse.Tick += TmrParseTick;
        }

        private void OnStateChanged(ClientSock sock, WinsockStates state)
        {
            switch (state)
            {
                case WinsockStates.Closed:     
                    IsConnecting = false;
                    IsConnected = false;
                    _tmrWaitToReconnect.Enabled = true;
                    break;
            }
        }

        private void OnSslInvalidCertificate(ClientSock sock, X509Certificate certificate)
        {
            if (OnClientSslInvalidCertificate != null)
            {
                OnClientSslInvalidCertificate(this, certificate);
            }
        }

        private void OnDataSent(ClientSock sock, string data)
        {
            if (!IsConnected)
            {
                return;
            }           
            _pingCheck = 0;
        }

        /* DNS callbacks */
        private void OnLocalInfoResolved(DnsResolve dns, DnsResult result)
        {
            if (OnClientLocalInfoResolved != null)
            {
                OnClientLocalInfoResolved(this, result);
            }
        }

        private void OnLocalInfoFailed(DnsResolve dns, DnsResult result)
        {
            if (OnClientLocalInfoFailed != null)
            {
                OnClientLocalInfoFailed(this, result);
            }
        }

        private void OnDnsResolved(DnsResolve dns, DnsResult result)
        {
            if (OnClientDnsResolved != null)
            {
                OnClientDnsResolved(this, result);
            }
        }

        private void OnDnsFailed(DnsResolve dns, DnsResult result)
        {
            if (OnClientDnsFailed != null)
            {
                OnClientDnsFailed(this, result);
            }
        }

        /* Identd callback */
        private void OnIdentDaemonData(string remoteHost, string data)
        {
            _identd.StopIdentDaemon();
            if (OnClientIdentDaemonRequest != null)
            {
                OnClientIdentDaemonRequest(this, remoteHost, data);
            }
        }

        /* Timer callbacks */
        private void TmrParseTick(object sender, EventArgs e)
        {
            if (_sockData.Count == 0)
            {
                _tmrParse.Enabled = false;
                _tmrParse.Tick -= TmrParseTick;
                _tmrParse = null;
                return;
            }
            var first = string.Empty;
            var second = string.Empty;
            var third = string.Empty;
            var fourth = string.Empty;
            var s = Utf8.ConvertToUtf8(_sockData[0], true);
            if (SettingsManager.Settings.Client.Messages.StripCodes)
            {
                s = Functions.StripControlCodes(s);
            }
            _sockData.RemoveAt(0);
            if (string.IsNullOrEmpty(s))
            {
                return;
            }
            /* Now we can parse the elements we need */
            var i = s.IndexOf(' ');
            if (i > -1)
            {
                /* Which it should be...or else we're fucked */
                first = s.Substring(0, i);
                var i2 = s.IndexOf(' ', i + 1);
                if (i2 > -1)
                {
                    /* Second and third space */
                    second = s.Substring(i + 1, i2 - i - 1);
                    var i3 = s.IndexOf(' ', i2 + 1);
                    if (i3 > -1)
                    {
                        /* Third and fourth space */
                        third = s.Substring(i2 + 1, i3 - i2 - 1);
                        fourth = s.Substring(i3 + 1);
                    }
                    else
                    {
                        /* No third space */
                        third = s.Substring(i2 + 1);
                    }
                }
                else
                {
                    /* No second space (PING) */
                    i2 = s.IndexOf(':');
                    if (i2 > 0)
                    {
                        first = s.Substring(i2);
                        second = s.Substring(0, i2 - 1);
                    }
                    else
                    {
                        i2 = s.IndexOf(' ');
                        if (i2 > -1)
                        {
                            first = s.Substring(0, i2);
                            second = s.Substring(i2 + 1);
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(first) && first.ToUpper() == "ERROR")
            {
                second = "ERROR";
                first = null;
            }            
            /* Now we send the data to the main parser */            
            Parser.Parse(first, second, third, fourth);
        }

        private void TimerWaitToReconnect(object sender, EventArgs e)
        {
            /* Gives a small delay before raising the "closed" event */
            _tmrWaitToReconnect.Enabled = false;
            if (!IsWaitingToReconnect)
            {
                return;
            }
            IsWaitingToReconnect = false;
            if (OnClientConnectionClosed != null)
            {
                OnClientConnectionClosed(this);
            }
        }

        private void TimerPingTimeoutCheck(object sender, EventArgs e)
        {
            if (!IsConnected)
            {
                return;
            }
            _pingCheck++;     
            if (_pingCheck <= 450)
            {
                return;
            }
            /* No messages have been sent or received by the socket, close the connection */
            _sock.Close();
            _tmrPingTimeout.Enabled = false;
            if (OnClientDisconnected != null)
            {
                OnClientDisconnected(this);
            }
        }
    }
}
