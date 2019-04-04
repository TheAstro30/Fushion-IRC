/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Windows.Forms;
using ircCore.Settings;

namespace ircClient.Tcp
{
    public class Identd
    {
        /* Ident daemon class - some servers require identd to be running before allowing connection to the network */
        private readonly SettingsIdentd _identd;
        private readonly ClientSock _sock;
        private readonly Timer _tmrTimeout;
        private int _timeOut;

        public event Action<string, string> OnIdentDaemonData;

        public Identd(ISynchronizeInvoke syncObject, SettingsIdentd identd)
        {
            _sock = new ClientSock(syncObject);
            _sock.OnConnectionRequest += OnConnectionRequest;
            _sock.OnDataArrival += OnDataArrival;
            _identd = identd;
            _tmrTimeout = new Timer
                              {
                                  Interval = 1000
                              };
            _tmrTimeout.Tick += TimerTimeout;
        }

        public void BeginIdentDaemon()
        {
            if (_sock.GetState == WinsockStates.Listening || !_identd.Enable)
            {
                return;
            }
            _timeOut = 0;
            _tmrTimeout.Enabled = true;
            _sock.LocalPort = _identd.Port;
            _sock.Listen();
        }

        public void StopIdentDaemon()
        {
            if (_sock.GetState == WinsockStates.Closed)
            {
                return;
            }
            _timeOut = 0;
            _tmrTimeout.Enabled = false;
            _sock.Close();
        }

        /* Socket callbacks */
        private void OnConnectionRequest(ClientSock sock, Socket accept)
        {
            _sock.Accept(accept);
        }

        private void OnDataArrival(ClientSock sock, int byteLength)
        {
            if (byteLength <= 1)
            {
                _sock.Close();
                return;
            }
            /* Get incoming socket data */
            var buffer = string.Empty;
            _sock.GetData(ref buffer);
            var b = buffer.Split(new[] {'\r', '\n'});
            var data = string.Format("{0} : USERID : {1} : {2}", b.Length < 1 ? buffer : b[0], _identd.System, _identd.UserId);
            /* Raise event */
            if (OnIdentDaemonData != null)
            {
                OnIdentDaemonData(_sock.RemoteHostIp, data);
            }
            /* Respond to identd request */
            _sock.SendData(string.Format("{0}\r\n", data));
        }

        /* Timer event */
        private void TimerTimeout(object sender, EventArgs e)
        {
            /* Close identd after 120 seconds (2 minutes - which should be more than long enough */
            _timeOut++;
            if (_timeOut >= 120)
            {
                StopIdentDaemon();
            }
        }
    }
}
