/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Windows.Forms;
using ircCore.Settings;
using ircCore.Utils;

namespace ircClient.Tcp
{
    public enum DccType
    {
        DccChat = 0,
        DccFileTransfer = 1
    }

    public enum DccChatType
    {
        Send = 0,
        Receive = 1
    }

    public enum DccFileType
    {
        Download = 0,
        Upload = 1
    }

    public enum DccFileStatus
    {
        Waiting = 0,
        Downloading = 1,
        Uploading = 2,
        Completed = 3,
        Cancelled = 4,
        Failed = 5
    }

    public class Dcc
    {
        /* This class handles both direct connection communication plus file transfers */
        private readonly ClientSock _sock;
        private readonly ISynchronizeInvoke _sync;

        private FileStream _fileOutput;
        private BinaryWriter _fileWrite;

        private FileStream _fileInput;
        private BinaryReader _fileReader;

        private uint _filePos;
        private uint _bytesTotal;

        private readonly Timer _timerSpeed;
        private int _speed;

        /* Events raised by this class */
        public event Action<Dcc> OnDccConnecting;
        public event Action<Dcc> OnDccConnected;
        public event Action<Dcc> OnDccDisconnected;
        public event Action<Dcc, string> OnDccError;        
        public event Action<Dcc, string> OnDccChatText;
        public event Action<Dcc, string> OnDccChatAction;

        public event Action<Dcc> OnDccTransferProgress;

        /* Public properties - IP address and port */
        public string Address { get; set; }
        public int Port { get; set; }

        public bool IsConnected { get; set; }

        public DccType DccType { get; set; }
        public DccChatType DccChatType { get; set; }

        /* File data */
        public DccFileType DccFileType { get; set; }
        public DccFileStatus Status { get; set; }

        public string FileName { get; set; }
        public string DccFolder { get; set; }
        public string UserName { get; set; }        
        public uint FileSize { get; set; }
        public double Progress { get; set; }
        public int Speed { get; set; }

        public bool IsResume { get; set; }
        
        public string UserNameToString
        {
            get { return DccFileType == DccFileType.Download ? string.Format("{0} ({1})", UserName, Address) : UserName; }
        }

        public string SpeedToString
        {
            get { return string.Format("{0}/s", Functions.FormatBytes(Speed.ToString())); }
        }

        /* Constructor */
        public Dcc(ISynchronizeInvoke syncObject)
        {
            _sync = syncObject;
            _sock = new ClientSock(_sync);  
            /* Timer */
            _timerSpeed = new Timer {Interval = 1000};
            _timerSpeed.Tick += TimerSpeed;
            /* Socket handlers */
            _sock.OnDisconnected += OnSocketDisconnected;
            _sock.OnError += OnSocketError;
            _sock.OnConnected += OnSocketConnected;
            _sock.OnConnectionRequest += OnSocketConnectionRequest;
            _sock.OnDataArrival += OnSocketDataArrival;
        }

        /* Public methods */
        public void BeginConnect()
        {
            /* Called when connecting to the remote host */
            if (IsConnected)
            {
                _sock.Close();                
            }
            switch (DccType)
            {
                case DccType.DccChat:
                    switch (DccChatType)
                    {
                        case DccChatType.Send:
                            /* Create socket as server */
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
                            _sock.Connect(Address, Port);
                            break;
                    }
                    if (OnDccConnecting != null)
                    {
                        OnDccConnecting(this);
                    }              
                    break;

                case DccType.DccFileTransfer:
                    switch (DccFileType)
                    {
                        case DccFileType.Download:
                            if (!IsResume)
                            {
                                /* Prepare file output */
                                _fileOutput = new FileStream(string.Format(@"{0}\{1}", DccFolder, FileName),
                                                             FileMode.Create,
                                                             FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
                                _fileWrite = new BinaryWriter(_fileOutput);
                                /* Connect to host */
                                _sock.Connect(Address, Port);
                            }
                            else
                            {
                                /* This will be implemented later */
                            }
                            break;

                        case DccFileType.Upload:
                            /* Prepare file input */
                            _fileInput = new FileStream(string.Format(@"{0}\{1}", DccFolder, FileName), FileMode.Open,
                                                        FileAccess.Read,
                                                        FileShare.ReadWrite);
                            _fileReader = new BinaryReader(_fileInput);
                            /* Create socket as server */
                            _filePos = 0;
                            _bytesTotal = 0;
                            _sock.LocalPort = Port;
                            /* Binding */
                            if (SettingsManager.Settings.Dcc.Options.General.BindToIp)
                            {
                                _sock.Bind = SettingsManager.Settings.Dcc.Options.General.BindIpAddress;
                            }
                            /* Begin listening */
                            _sock.Listen();
                            break;
                    }
                    break;
            }
        }

        public void Disconnect()
        {
            _sock.Close();
            IsConnected = false;
            switch (DccType)
            {
                case DccType.DccFileTransfer:
                    switch (Status)
                    {
                        case DccFileStatus.Waiting:
                        case DccFileStatus.Downloading:
                        case DccFileStatus.Uploading:
                            if (_fileOutput != null)
                            {
                                _fileOutput.Close();
                                _fileWrite.Close();
                            }
                            if (_fileInput != null)
                            {
                                _fileInput.Close();
                                _fileReader.Close();
                            }
                            _timerSpeed.Enabled = false;
                            Status = DccFileStatus.Cancelled;
                            if (OnDccTransferProgress != null)
                            {
                                OnDccTransferProgress(this);
                            }
                            break;
                    }
                    break;
            }
        }

        public void ChatSendText(string text)
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
            switch (DccType)
            {
                case DccType.DccFileTransfer:
                    if (_fileOutput != null)
                    {
                        _fileOutput.Close();
                        _fileWrite.Close();
                    }
                    if (_fileInput != null)
                    {
                        _fileInput.Close();
                        _fileReader.Close();
                    }
                    _timerSpeed.Enabled = false;
                    switch (DccFileType)
                    {
                        case DccFileType.Download:
                            if (_bytesTotal == FileSize)
                            {
                                /* Success */
                                System.Diagnostics.Debug.Print("DCC get success");
                                Status = DccFileStatus.Completed;
                            }
                            else
                            {
                                /* Failed */
                                System.Diagnostics.Debug.Print("DCC get failed");
                                Status = DccFileStatus.Failed;
                            }
                            break;

                        case DccFileType.Upload:
                            if (_bytesTotal == FileSize)
                            {
                                /* Success */
                                System.Diagnostics.Debug.Print("DCC send success");
                                Status = DccFileStatus.Completed;
                            }
                            else
                            {
                                /* Failed */
                                System.Diagnostics.Debug.Print("DCC send failed");
                                Status = DccFileStatus.Failed;
                            }
                            break;
                    }
                    Progress = Percentage();
                    if (OnDccTransferProgress != null)
                    {
                        OnDccTransferProgress(this);
                    }
                    /* Timeout timer */
                    break;

                case DccType.DccChat:
                    if (OnDccDisconnected != null)
                    {
                        OnDccDisconnected(this);
                    }
                    break;
            }
            IsConnected = false;
        }

        private void OnSocketError(ClientSock sock, string description)
        {            
            switch (DccType)
            {
                case DccType.DccFileTransfer:
                    _timerSpeed.Enabled = false;
                    Status = DccFileStatus.Failed;
                    break;

                case DccType.DccChat:
                    if (OnDccError != null)
                    {
                        OnDccError(this, description);
                    }
                    break;
            }
            IsConnected = false;
        }

        private void OnSocketConnected(ClientSock sock)
        {
            IsConnected = true;
            switch (DccType)
            {
                case DccType.DccFileTransfer:
                    switch (DccFileType)
                    {
                        case DccFileType.Download:
                            _timerSpeed.Enabled = true;
                            /* Timeout timer disable */
                            break;
                    }
                    Status = DccFileStatus.Downloading;
                    break;

                case DccType.DccChat:
                    if (OnDccConnected != null)
                    {
                        OnDccConnected(this);
                    }
                    break;
            }
        }

        private void OnSocketConnectionRequest(ClientSock sock, Socket requestId)
        {
            _sock.Accept(requestId);
            IsConnected = true;
            switch (DccType)
            {
                case DccType.DccFileTransfer:
                    Status = DccFileStatus.Uploading;
                    _timerSpeed.Enabled = true;
                    /* Send first packet of data to remote client */
                    SendPacket();
                    break;
            }
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
                    byte[] bytes = { };
                    switch (DccFileType)
                    {
                        case DccFileType.Download:
                            /* Receiving file */
                            _sock.GetData(ref bytes);
                            _bytesTotal += (uint)bytes.Length;
                            /* Dump to file */
                            _fileWrite.Write(bytes);
                            /* Are we at the end of file? */
                            if (_bytesTotal >= FileSize)
                            {
                                /* Transfer complete (begin socket close) */
                                
                            }
                            /* Confirm bytes sent */
                            _sock.SendData(PutBytes(_bytesTotal));
                            _speed += bytesTotal;
                            break;

                        case DccFileType.Upload:
                            /* Sending file, we get the confirm bytes */
                            _sock.GetData(ref bytes);
                            _bytesTotal = GetBytes(bytes);
                            _speed += SettingsManager.Settings.Dcc.Options.General.PacketSize;
                            if (_bytesTotal == _filePos)
                            {
                                /* Send more data */
                                SendPacket();
                            }  
                            break;
                    }
                    break;
            }
        }

        /* Private helper methods */
        private void SendPacket()
        {
            if (_sock.GetState != WinsockStates.Connected)
            {
                return;
            }
            var buffSize = SettingsManager.Settings.Dcc.Options.General.PacketSize;
            /* Get a data packet within the reading stream by our send buffer size
               (was orignally FilePos + 1) */
            if (FileSize - _filePos < buffSize)
            {
                buffSize = (int)(FileSize - _filePos);
            }
            /* No more to send, wait for remote client */
            if (buffSize == 0)
            {
                return;
            }
            var bytes = _fileReader.ReadBytes(buffSize);
            _filePos += (uint)buffSize;
            if (_sock.GetState != WinsockStates.Connected)
            {
                _timerSpeed.Enabled = false;
            }
            else
            {
                _sock.SendData(bytes);
            }
        }

        private static byte[] PutBytes(uint i)
        {
            /* This function was written by Ryan Alexander (sexist), this is basically the same as winsock's htonl */
            var b = new byte[4];
            b[3] = (byte)(i & 0xffL);
            b[2] = (byte)((i & 0xff00L) >> 8);
            b[1] = (byte)((i & 0xff0000L) >> 0x10);
            b[0] = (byte)((i & -16777216L) >> 0x18);
            return b;
        }

        private static uint GetBytes(IList<byte> b)
        {
            /* This function was written by Ryan Alexander (sexist), this is basically the same as winsock's ntohl */
            return (uint)((((b[0] << 0x18) + (b[1] << 0x10)) + (b[2] << 8)) + b[3]);
        }

        private double Percentage()
        {
            /* This shouldn't fail but can sometimes return Infinaty */
            var percent = ((float)_bytesTotal / FileSize) * 100;
            return (int)percent >= 0 ? Math.Round(percent, 1) : 0;
        }

        /* Timers */
        private void TimerSpeed(object sender, EventArgs e)
        {
            Progress = Percentage();
            Speed = _speed;
            if (OnDccTransferProgress != null)
            {
                OnDccTransferProgress(this);
            }
            _speed = 0;
        }
    }
}
