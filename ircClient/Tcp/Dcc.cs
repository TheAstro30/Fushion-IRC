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
        Failed = 5,
        Timeout = 6
    }

    public enum DccWriteMode
    {
        Overwrite = 0,
        SaveAs = 1,
        Resume = 2
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

        private readonly Timer _timerSpeed;
        private int _speed;

        private readonly Timer _timerTimeOut;
        private int _timeOut;

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

        /* Yes, child windows already have ClientConnection attached to them, however ... this class as a file
         * transfer is created independantly from a child window, so - in order for "Resend" to work, this class
         * needs to know which ClientConnection it was created on :) */
        public ClientConnection Client { get; set; }

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
        public uint FilePos { get; set; }
        public uint BytesTotal { get; set; }
        public int Remaining { get; set; }

        public bool IsResume { get; set; }
        
        public string UserNameToString
        {
            get { return DccFileType == DccFileType.Download ? string.Format("{0} ({1})", UserName, Address) : UserName; }
        }

        public string SpeedToString
        {
            get { return IsConnected ? string.Format("{0}/s", Functions.FormatBytes(Speed.ToString())) : "--"; }
        }

        public string RemainingToString
        {
            get { return IsConnected ? FormatTime(Remaining) : "--"; }
        }

        /* Constructor */
        public Dcc(ISynchronizeInvoke syncObject)
        {
            _sync = syncObject;
            _sock = new ClientSock(_sync);  
            /* Timer */
            _timerSpeed = new Timer {Interval = 1000};
            _timerSpeed.Tick += TimerSpeed;
            _timerTimeOut = new Timer {Interval = 1000};
            _timerTimeOut.Tick += TimerTimeOut;
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
            _timeOut = 0;
            _timerTimeOut.Enabled = true;
            _sock.Close();              
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
                            /* Prepare file output */
                            _fileOutput = new FileStream(string.Format(@"{0}\{1}", DccFolder, FileName),
                                                         !IsResume ? FileMode.Create : FileMode.Append,
                                                         FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
                            _fileWrite = new BinaryWriter(_fileOutput);
                            if (!IsResume)
                            {                                
                                /* Connect to host */
                                _sock.Connect(Address, Port);
                            }
                            break;

                        case DccFileType.Upload:
                            /* Prepare file input */
                            _fileInput = new FileStream(string.Format(@"{0}\{1}", DccFolder, FileName), FileMode.Open,
                                                        FileAccess.Read,
                                                        FileShare.ReadWrite);
                            _fileReader = new BinaryReader(_fileInput);
                            /* Create socket as server */
                            FilePos = 0;
                            BytesTotal = 0;
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
                    Status = DccFileStatus.Waiting;
                    if (OnDccTransferProgress != null)
                    {
                        OnDccTransferProgress(this);
                    }
                    break;
            }
        }

        public void BeginGetResume()
        {
            Progress = Percentage();
            BytesTotal = FilePos;
            _sock.Connect(Address, Port);
        }

        public void BeginSendResume()
        {
            Progress = Percentage();
            _fileReader.BaseStream.Position = FilePos;
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
                            _timeOut = 0;
                            _timerTimeOut.Enabled = false;
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
            IsConnected = false;
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
                    Progress = Percentage();
                    if (Status != DccFileStatus.Timeout)
                    {
                        Status = BytesTotal == FileSize ? DccFileStatus.Completed : DccFileStatus.Failed;
                        if (OnDccTransferProgress != null)
                        {
                            OnDccTransferProgress(this);
                        }
                    }
                    /* Timeout timer */
                    _timeOut = 0;
                    _timerTimeOut.Enabled = false;
                    break;

                case DccType.DccChat:
                    if (OnDccDisconnected != null)
                    {
                        OnDccDisconnected(this);
                    }
                    break;
            }            
        }

        private void OnSocketError(ClientSock sock, string description)
        {
            IsConnected = false;
            switch (DccType)
            {
                case DccType.DccFileTransfer:
                    _timeOut = 0;
                    _timerTimeOut.Enabled = false;
                    _timerSpeed.Enabled = false;
                    if (Status != DccFileStatus.Timeout)
                    {
                        Status = DccFileStatus.Failed;
                        if (OnDccTransferProgress != null)
                        {
                            OnDccTransferProgress(this);
                        }
                    }
                    break;

                case DccType.DccChat:
                    if (OnDccError != null)
                    {
                        OnDccError(this, description);
                    }
                    break;
            }
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
                            _timeOut = 0;
                            _timerTimeOut.Enabled = false;
                            Status = DccFileStatus.Downloading;
                            if (OnDccTransferProgress != null)
                            {
                                OnDccTransferProgress(this);
                            }
                            break;
                    }                    
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
                    if (OnDccTransferProgress != null)
                    {
                        OnDccTransferProgress(this);
                    }
                    _timerSpeed.Enabled = true;
                    /* Timeout timer disable */
                    _timeOut = 0;
                    _timerTimeOut.Enabled = false;
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
                    byte[] bytes = {};
                    switch (DccFileType)
                    {
                        case DccFileType.Download:
                            /* Receiving file */ 
                            if (_fileWrite == null)
                            {
                                return;
                            }
                            _sock.GetData(ref bytes);
                            BytesTotal += (uint)bytes.Length;
                            /* Dump to file */
                            _fileWrite.Write(bytes);
                            /* Confirm bytes sent (total bytes) */
                            _sock.SendData(PutBytes(BytesTotal));
                            _speed += bytesTotal;
                            break;

                        case DccFileType.Upload:
                            /* Sending file, we get the confirm bytes */
                            _sock.GetData(ref bytes);
                            BytesTotal = GetBytes(bytes);
                            _speed += SettingsManager.Settings.Dcc.Options.General.PacketSize;
                            if (BytesTotal == FilePos)
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
            if (FileSize - FilePos < buffSize)
            {
                buffSize = (int)(FileSize - FilePos);
            }
            /* No more to send, wait for remote client */
            if (buffSize == 0)
            {
                return;
            }
            var bytes = _fileReader.ReadBytes(buffSize);
            FilePos += (uint)buffSize;
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
            var percent = ((float)BytesTotal / FileSize) * 100;
            return (int)percent >= 0 ? Math.Round(percent, 1) : 0;
        }

        private static string FormatTime(int seconds)
        {
            var t = TimeSpan.FromSeconds(seconds);
            return seconds >= 3600
                       ? string.Format("{0}h {1}m {2}s", t.Hours, t.Minutes, t.Seconds)
                       : seconds >= 60
                             ? string.Format("{0}m {1}s", t.Minutes, t.Seconds)
                             : string.Format("{0}s", t.Seconds);
        }

        /* Timers */
        private void TimerSpeed(object sender, EventArgs e)
        {
            Progress = Percentage();
            Speed = _speed;
            if (Speed > 0)
            {
                Remaining = (int)((FileSize - BytesTotal) / Speed);
            } 
            if (OnDccTransferProgress != null)
            {
                OnDccTransferProgress(this);
            }
            _speed = 0;
        }

        private void TimerTimeOut(object sender, EventArgs e)
        {
            if (_timeOut >= SettingsManager.Settings.Dcc.Options.Timeouts.GetSendTransfer)
            {
                _timeOut = 0;
                _timerTimeOut.Enabled = false;
                _sock.Close();
                Status = DccFileStatus.Timeout;
                if (OnDccTransferProgress != null)
                {
                    OnDccTransferProgress(this);
                }
                return;
            }
            _timeOut++;
        }
    }
}
