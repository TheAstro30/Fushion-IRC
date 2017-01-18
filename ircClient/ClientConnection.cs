/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using ircClient.Classes;
using ircClient.Tcp;
using ircCore.Settings;
using ircCore.Utils;

namespace ircClient
{
    public class ClientConnection
    {
        /* This class does most of the work of parsing IRC messages - all events raised must return this class */
        public readonly SettingsUserInfo UserInfo;
        public readonly Parser Parser;

        private readonly ClientSock _sock;
        
        private StringBuilder _buffer = new StringBuilder();
        private readonly List<string> _sockData = new List<string>();

        private Timer _tmrParse;
        private readonly Timer _tmrWaitToReconnect;

        public bool IsConnecting { get; internal set; }
        public bool IsConnected { get; internal set; }

        public bool IsWaitingToReconnect { get; set; }

        public string Server { get; set; }
        public int Port { get; set; }

        public event Action<ClientConnection, string> OnDebugOut;
        public event Action<ClientConnection> OnClientBeginConnect;
        public event Action<ClientConnection> OnClientConnected;
        public event Action<ClientConnection> OnClientDisconnected;
        public event Action<ClientConnection> OnClientCancelConnection;
        public event Action<ClientConnection, string> OnClientConnectionError;

        public event Action<ClientConnection> OnClientConnectionClosed;

        /* Constructor */
        public ClientConnection(ISynchronizeInvoke syncObjcet, SettingsUserInfo userInfo)
        {
            _sock = new ClientSock
                        {
                            SynchronizingObject = syncObjcet
                        };
            _sock.OnConnected += OnConnected;
            _sock.OnDisconnected += OnDisconnected;
            _sock.OnError += OnError;
            _sock.OnDataArrival += OnDataArrival;
            _sock.OnStateChanged += OnStateChanged;
            Parser = new Parser(this);

            UserInfo = new SettingsUserInfo(userInfo);

            _tmrWaitToReconnect = new Timer
                                      {
                                          Interval = 2000
                                      };
            _tmrWaitToReconnect.Tick += TimerWaitToReconnect;
        }

        /* Connect overloads */
        public void Connect()
        {
            if (string.IsNullOrEmpty(Server))
            {
                return;
            }
            Connect(Server, Port > 0 ? Port : 6667);
        }

        public void Connect(string address, int port)
        {
            Server = address;
            Port = port;
            IsConnecting = true;
            _sock.Connect(address, port);
            if (OnClientBeginConnect != null)
            {
                OnClientBeginConnect(this);
            }
        }

        /* Disconnection */
        public void Disconnect()
        {
            if (IsConnected)
            {
                Send("QUIT :Leaving");
            }
            else
            {
                _sock.Close();
            }
            IsConnecting = false;
            IsConnected = false;
        }

        public void CancelConnection()
        {
            if (IsConnected)
            {
                Send("QUIT :Leaving.");
            }
            else
            {
                _sock.Close();
            }
            IsConnecting = false;
            IsConnected = false;
            if (OnClientCancelConnection != null)
            {
                OnClientCancelConnection(this);
            }
        }

        /* Sockwrite */
        public void Send(string data)
        {
            _sock.SendData(string.Format("{0}\r\n", data));
        }

        /* Socket callbacks */
        private void OnConnected(ClientSock sock)
        {
            if (OnClientConnected != null)
            {
                OnClientConnected(this);
            }
            Send(string.Format("NICK {0}", UserInfo.Nick));
            Send(string.Format("USER {0} {1} {2} :{3}", UserInfo.Ident, _sock.LocalIp, _sock.RemoteHostIp, UserInfo.RealName));
        }

        private void OnDisconnected(ClientSock sock)
        {
            if (OnClientDisconnected != null)
            {
                OnClientDisconnected(this);
            }
            IsConnecting = false;
            IsConnected = false;
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
                    System.Diagnostics.Debug.Print("Closed");
                    IsConnecting = false;
                    IsConnected = false;
                    _tmrWaitToReconnect.Enabled = true;
                    break;
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
            _sockData.RemoveAt(0);
            if (string.IsNullOrEmpty(s))
            {
                return;
            }
            /* Output to debug event */
            if (OnDebugOut != null)
            {
                OnDebugOut(this, s);
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
    }
}
