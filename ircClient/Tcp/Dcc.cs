/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.ComponentModel;
using System.Net.Sockets;
using ircCore.Dcc;
using ircCore.Settings;
using ircCore.Utils;

namespace ircClient.Tcp
{
    public class Dcc
    {
        /* This class handles both direct connection communication plus file transfers */
        private readonly ClientSock _sock;
        private readonly ISynchronizeInvoke _sync;

        public event Action<Dcc> OnDccConnecting;
        public event Action<Dcc> OnDccConnected;
        public event Action<Dcc> OnDccDisconnected;
        public event Action<Dcc, string> OnDccError;        
        public event Action<Dcc, string> OnDccChatText;
        public event Action<Dcc, string> OnDccChatAction;

        public string Address { get; set; }
        public int Port { get; set; }

        public bool IsConnected { get; set; }

        public DccType DccType { get; set; }
        public DccChatType DccChatType { get; set; }

        public DccFile DccFile { get; set; }
        public DccFileType DccFileType { get; set; }

        public Dcc(ISynchronizeInvoke syncObject)
        {
            _sync = syncObject;
            _sock = new ClientSock(_sync);            
            /* Socket handlers */
            _sock.OnDisconnected += OnSocketDisconnected;
            _sock.OnError += OnSocketError;
            _sock.OnConnected += OnSocketConnected;
            _sock.OnConnectionRequest += OnSocketConnectionRequest;
            _sock.OnDataArrival += OnSocketDataArrival;
        }

        public void BeginConnect()
        {
            /* Called when connecting to the remote host */
            switch (DccType)
            {
                case DccType.DccChat:
                    switch (DccChatType)
                    {
                        case DccChatType.Send:
                            /* Create socket as server */
                            _sock.Close();
                            _sock.LocalPort = Port;
                            /* Binding */
                            if (SettingsManager.Settings.Dcc.Options.General.BindToIp)
                            {
                                _sock.Bind = SettingsManager.Settings.Dcc.Options.General.BindIpAddress;
                            }
                            /* Begin listening */
                            _sock.Listen();
                            break;

                        case DccChatType.Receive:
                            if (OnDccConnecting != null)
                            {
                                OnDccConnecting(this);
                            }
                            _sock.Connect(Address, Port);
                            break;
                    }                                        
                    break;

                case DccType.DccFileTransfer:
                    switch (DccFileType)
                    {
                        case DccFileType.Download:
                            break;

                        case DccFileType.Upload:
                            break;
                    }
                    break;
            }
        }

        public void Disconnect()
        {
            _sock.Close();
        }

        public void Send(string text)
        {
            if (!IsConnected)
            {
                return;
            }
            _sock.SendData(string.Format("{0}\r\n", text));
        }

        /* Socket callbacks */
        private void OnSocketDisconnected(ClientSock sock)
        {
            if (OnDccDisconnected != null)
            {
                OnDccDisconnected(this);
            }
            IsConnected = false;
        }

        private void OnSocketError(ClientSock sock, string description)
        {
            if (OnDccError != null)
            {
                OnDccError(this, description);
            }
            IsConnected = false;
        }

        private void OnSocketConnected(ClientSock sock)
        {
            IsConnected = true;
            if (OnDccConnected != null)
            {
                OnDccConnected(this);
            }
        }

        private void OnSocketConnectionRequest(ClientSock sock, Socket requestId)
        {
            _sock.Accept(requestId);
            IsConnected = true;
        }

        private void OnSocketDataArrival(ClientSock sock, int bytesTotal)
        {
            switch (DccType)
            {
                case DccType.DccChat:
                    string text = null;
                    _sock.GetData(ref text);
                    text = Functions.StripControlCodes(Utf8.ConvertToUtf8(text, true),
                                                       SettingsManager.Settings.Client.Messages.StripCodes);
                    switch (text[0])
                    {
                        case '\x01':
                            var i = text.IndexOf(' ');
                            if (i != -1)
                            {
                                if (OnDccChatAction != null)
                                {
                                    OnDccChatAction(this, text.Substring(i + 1).Replace('\x01', (char)0));
                                }
                            }
                            break;

                        default:
                            if (OnDccChatText != null)
                            {
                                OnDccChatText(this, text);
                            }
                            break;
                    }
                    break;

                case DccType.DccFileTransfer:
                    /* Implement data transfer here */
                    break;
            }
        }

        /* Private helper methods */
    }
}
