/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.ComponentModel;
using ircClient.Tcp.Helpers;
using ircCore.Utils;

namespace ircClient.Tcp
{
    public enum WinsockStates
    {
        Closed = 0,
        Open = 1,
        Listening = 2,
        ConnectionPending = 3,
        ResolvingHost = 4,
        HostResolved = 5,
        Connecting = 6,
        Connected = 7,
        Closing = 8,
        Error = 9
    }

    [DefaultEvent("OnError")]
    public class ClientSock
    {
        /* Winsock component class
           Original author: Unknown
           Modified by: Jason James Newland (2008-2009/2011-2012)
         */
        private readonly UiSynchronize _sync;

        private const int BufferSize = 32769;

        private string _remoteIp;
        private int _localPort;
        private int _remotePort;
        private List<byte[]> _byteData = new List<byte[]>();
        private byte[] _buffer = new byte[BufferSize];
        private TcpListener _listenSocket;
        private Socket _clientSocket;

        private NetworkStream _networkStream;
        private SslStream _sslStream;

        private bool _sslWaitingAccept;
        private bool _sslCertificateAccepted;

        /* Need to be virtual so they can be "overriden" in another class */
        public event Action<ClientSock> OnConnected;
        public event Action<ClientSock> OnDisconnected;
        public event Action<ClientSock, int> OnDataArrival;
        public event Action<ClientSock, Socket> OnConnectionRequest;
        public event Action<ClientSock, string> OnError;
        public event Action<ClientSock, WinsockStates> OnStateChanged;
        public event Action<ClientSock, string> OnDebugOut;
        public event Action<ClientSock, X509Certificate> OnSslInvalidCertificate;

        /* Public properties */
        public bool IsSsl { get; set; }

        public int LocalPort
        {
            get { return _localPort; }
            set
            {
                if (GetState == WinsockStates.Closed)
                {
                    _localPort = value;
                }
            }
        }

        public int RemotePort
        {
            get { return _remotePort; }
            set
            {
                if (GetState != WinsockStates.Connected)
                {
                    _remotePort = value;
                }
            }
        }

        public string RemoteIp
        {
            get { return _remoteIp; }
            set
            {
                if (GetState == WinsockStates.Closed)
                {
                    _remoteIp = value;
                }
            }
        }

        public string RemoteHostIp
        {
            get
            {
                try
                {
                    var iEp = (IPEndPoint) _clientSocket.RemoteEndPoint;
                    return (iEp != null) ? iEp.Address.ToString() : null;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        public string LocalIp
        {
            get
            {
                UnicastIPAddressInformation ip = null;
                try
                {
                    var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                    foreach (var address in from network in networkInterfaces
                                            where network.OperationalStatus == OperationalStatus.Up
                                            select network.GetIPProperties()
                                            into properties
                                            where properties.GatewayAddresses.Count != 0
                                            from address in properties.UnicastAddresses
                                            where address.Address.AddressFamily == AddressFamily.InterNetwork
                                            where !IPAddress.IsLoopback(address.Address)
                                            select address)
                    {
                        if (!address.IsDnsEligible)
                        {
                            if (ip == null)
                            {
                                ip = address;
                            }
                            continue;
                        }
                        /* The best IP is the IP got from DHCP server */
                        if (address.PrefixOrigin != PrefixOrigin.Dhcp)
                        {
                            if (ip == null || !ip.IsDnsEligible)
                            {
                                ip = address;
                            }
                            continue;
                        }
                        return address.Address.ToString();
                    }
                    return ip != null ? ip.Address.ToString() : string.Empty;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        public WinsockStates GetState { get; private set; }

        public string Bind { private get; set; }

        public bool EnableSslAuthentication { get; set; }

        public bool SslAutoAccept { get; set; }

        /* Constructor */
        public ClientSock(ISynchronizeInvoke syncObject)
        {
            _sync = new UiSynchronize(syncObject);
            Bind = "0";
            RemoteIp = "127.0.0.1";
            RemotePort = 80;
            LocalPort = 80;
        }

        /* Methods */
        public void Listen()
        {
            var x = new System.Threading.Thread(BeginListen);
            x.Start();
        }

        public void Close()
        {
            switch (GetState)
            {
                case WinsockStates.Listening:
                    ChangeState(WinsockStates.Closing);
                    try
                    {
                        _listenSocket.Stop();
                    }
                    catch
                    {
                        return;
                    }
                    break;

                case WinsockStates.Connected:
                case WinsockStates.Connecting:
                case WinsockStates.ConnectionPending:
                case WinsockStates.HostResolved:
                case WinsockStates.Open:
                case WinsockStates.ResolvingHost:
                    ChangeState(WinsockStates.Closing);
                    try
                    {
                        _clientSocket.Shutdown(SocketShutdown.Both);
                        _clientSocket.Close();
                        if (_networkStream != null)
                        {
                            _networkStream.Close();
                        }
                        if (_sslStream != null)
                        {
                            _sslStream.Close();
                        }
                    }
                    catch
                    {
                        return;
                    }
                    break;

                case WinsockStates.Error:
                    ChangeState(WinsockStates.Closed);
                    break;

                case WinsockStates.Closed:
                    /* Do nothing */
                    break;
            }
            ChangeState(WinsockStates.Closed);
        }

        public void Accept(Socket requestId)
        {
            try
            {
                if (GetState != WinsockStates.Listening)
                {
                    return;
                }
                /* Only accept if listening */
                ChangeState(WinsockStates.ConnectionPending);
                _clientSocket = requestId;
                if (OnConnected != null)
                {
                    _sync.Execute(() => OnConnected(this));
                }
                ChangeState(WinsockStates.Connected);
                _buffer = new byte[BufferSize];
                _byteData = new List<byte[]>();
                _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnClientRead, null);
            }
            catch (SocketException ex)
            {
                SocketErrorHandler(ex);
            }
            catch
            {
                Debug.Assert(true);
            }
        }

        public void Connect(string address, int port)
        {
            try
            {
                _sslWaitingAccept = false;
                _sslCertificateAccepted = false;
                /* Clear buffers! */
                _buffer = new byte[BufferSize];
                _byteData = new List<byte[]>();
                RemoteIp = address;
                RemotePort = port;
                /* Attempt to connect */
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ChangeState(WinsockStates.Connecting);
                _clientSocket.BeginConnect(address, port, OnClientConnected, null);
            }
            catch (SocketException ex)
            {
                SocketErrorHandler(ex);
            }
            catch
            {
                Debug.Assert(true);
            }
        }

        public void SendData(string data)
        {
            try
            {
                SendData(Utf8.StringToByteArray(data));
                if (OnDebugOut != null)
                {
                    _sync.Execute(() => OnDebugOut(this, data));
                }
            }
            catch
            {
                Debug.Assert(true);
            }
        }

        public void SendData(byte[] data)
        {
            switch (GetState)
            {
                case WinsockStates.Closed:
                    /* Can't send - not connected */
                    break;

                case WinsockStates.Listening:
                    /* Listening */
                    break;

                case WinsockStates.Connected:
                    try
                    {
                        /* Send the bytes that are passed */
                        if (IsSsl)
                        {
                            if (_sslStream != null && _sslStream.CanWrite)
                            {
                                try
                                {
                                    _sslStream.Write(data);
                                }
                                catch
                                {
                                    Close();
                                    SocketErrorHandler(null);
                                }
                            }
                            else
                            {
                                Close();
                                SocketErrorHandler(null);
                            }
                        }
                        else
                        {
                            _clientSocket.Send(data, data.Length, SocketFlags.None);
                        }
                    }
                    catch (SocketException ex)
                    {
                        Close();
                        SocketErrorHandler(ex);
                    }
                    catch
                    {
                        Debug.Assert(true);
                    }
                    break;
            }
        }

        public void GetData(ref string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                return;
            }
            byte[] byt = {};
            var s = new StringBuilder();
            GetData(ref byt);
            for (var i = 0; i <= byt.Length - 1; i++)
            {
                if (byt[i] == 10)
                {
                    s.Append((char) 10);
                }
                else
                {
                    s.Append(Utf8.AsciiChr(byt[i]));
                }
            }
            data = s.ToString();
        }

        public void GetData(ref byte[] bytes)
        {
            if (_byteData.Count == 0)
            {
                return;
            }
            var byt = _byteData[0];
            _byteData.RemoveAt(0);
            bytes = new byte[byt.Length];
            byt.CopyTo(bytes, 0);
        }

        public void SslAcceptCertificate(bool accept)
        {
            _sslCertificateAccepted = accept;
            _sslWaitingAccept = false;
        }

        /* Socket callback events */
        private void OnClientConnected(IAsyncResult ar)
        {
            try
            {
                if (GetState == WinsockStates.Closed)
                {
                    return;
                }
                _clientSocket.EndConnect(ar);
                if (IsSsl)
                {
                    _networkStream = new NetworkStream(_clientSocket);
                    _sslStream = new SslStream(_networkStream, true, OnRemoteCertificateValidation);
                    try
                    {
                        _sslStream.AuthenticateAsClient(RemoteIp);
                    }
                    catch
                    {
                        Close();
                        SocketErrorHandler(null);
                        return;
                    }
                }
                if (_clientSocket.Connected)
                {
                    ChangeState(WinsockStates.Connected);
                    if (IsSsl)
                    {
                        if (_sslStream != null && _sslStream.CanRead)
                        {
                            try
                            {
                                _sslStream.BeginRead(_buffer, 0, _buffer.Length, OnClientRead, _sslStream);
                            }
                            catch
                            {
                                Close();
                                SocketErrorHandler(null);
                                return;
                            }
                        }
                        else
                        {
                            Close();
                            SocketErrorHandler(null);
                            return;
                        }
                    }
                    else
                    {
                        _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnClientRead,
                                                   _clientSocket);
                    }
                    if (OnConnected != null)
                    {                        
                        _sync.Execute(() => OnConnected(this));
                    }
                }
            }
            catch (SocketException ex)
            {
                SocketErrorHandler(ex);
            }
            catch
            {
                Debug.Assert(true);
            }
        }

        private bool OnRemoteCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (!EnableSslAuthentication)
            {
                return false;
            }
            switch (sslPolicyErrors)
            {
                case SslPolicyErrors.None:                    
                    return true;

                case SslPolicyErrors.RemoteCertificateNameMismatch:
                case SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateChainErrors:
                    /* Certificate error */
                    if (SslAutoAccept)
                    {                        
                        return true;
                    }
                    if (OnSslInvalidCertificate != null)
                    {
                        _sslWaitingAccept = true;
                        _sync.Execute(() => OnSslInvalidCertificate(this, certificate));                        
                    }
                    else
                    {
                        return false;
                    }
                    while (_sslWaitingAccept)
                    {
                        /* Basic block operation */
                        System.Threading.Thread.Sleep(10);
                    }
                    return _sslCertificateAccepted;
            }
            return false;
        }

        private void OnClientAccept(IAsyncResult ar)
        {
            try
            {
                if (GetState == WinsockStates.Listening)
                {
                    var tmpSock = _listenSocket.EndAcceptSocket(ar);
                    if (OnConnectionRequest != null)
                    {
                        _sync.Execute(() => OnConnectionRequest(this, tmpSock));
                    }
                    /* Stop listening as we no longer need it */
                    _listenSocket.Stop();
                }
                else
                {
                    Close();
                    ChangeState(WinsockStates.Error);
                    if (OnError != null)
                    {                        
                        _sync.Execute(() => OnError(this, "Unknown error"));
                    }
                }
            }
            catch
            {
                Debug.Assert(true);
            }
        }

        private void OnClientRead(IAsyncResult ar)
        {
            try
            {
                if (GetState != WinsockStates.Closing && GetState != WinsockStates.Closed)
                {
                    var intCount = 0;
                    try
                    {
                        if (IsSsl)
                        {
                            try
                            {
                                if (_sslStream != null)
                                {
                                    intCount = _sslStream.EndRead(ar);
                                }
                            }
                            catch
                            {
                                Close();
                                SocketErrorHandler(null);
                                return;
                            }
                        }
                        else
                        {
                            try
                            {
                                intCount = _clientSocket.EndReceive(ar);
                            }
                            catch (SocketException ex)
                            {
                                Close();
                                SocketErrorHandler(ex);
                                return;
                            }
                        }
                    }
                    catch
                    {
                        return;
                    }
                    if (intCount < 1)
                    {
                        Close();
                        _buffer = new byte[BufferSize];
                        if (OnDisconnected != null)
                        {
                            _sync.Execute(() => OnDisconnected(this));
                        }
                        return;
                    }                  
                    var buffer = _buffer; /* Marshal-by-reference may cause runtime exception when passed by ref/out as in the next line... */
                    Array.Resize(ref buffer, intCount);
                    _buffer = buffer; /* Shouldn't have to re-assign it back, but doesn't work without doing so... */
                    _byteData.Add(_buffer);
                    if (OnDataArrival != null)
                    {
                        _sync.Execute(() => OnDataArrival(this, _buffer.Length));                        
                    }
                    _buffer = new byte[BufferSize];
                    if (IsSsl)
                    {
                        try
                        {
                            if (_sslStream != null && _sslStream.CanRead)
                            {
                                _sslStream.BeginRead(_buffer, 0, _buffer.Length, OnClientRead, _sslStream);
                            }
                            else
                            {
                                Close();
                                SocketErrorHandler(null);
                            }
                        }
                        catch
                        {
                            Close();
                            SocketErrorHandler(null);
                        }
                    }
                    else
                    {
                        _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnClientRead, null);
                    }
                }
            }
            catch
            {
                Close();
                _buffer = new byte[BufferSize];
                try
                {
                    if (OnDisconnected != null)
                    {
                        _sync.Execute(() => OnDisconnected(this));
                    }
                }
                catch
                {
                    Debug.Assert(true);
                }
            }
        }

        private void SocketErrorHandler(ExternalException ex)
        {
            try
            {
                if (OnError != null)
                {
                    _sync.Execute(() => OnError(this, ex != null
                                                          ? ErrorHandling.GetErrorDescription(ex.ErrorCode)
                                                          : ErrorHandling.GetErrorDescription(-1)));
                }
                ChangeState(WinsockStates.Error);
                if (_sslStream != null)
                {
                    _sslStream.Close();
                }
                if (_networkStream != null)
                {
                    _networkStream.Close();
                }
            }
            catch
            {
                Debug.Assert(true);
            }
        }

        private void ChangeState(WinsockStates newState)
        {
            try
            {
                GetState = newState;
                if (OnStateChanged != null)
                {
                    _sync.Execute(() => OnStateChanged(this, GetState));
                }
            }
            catch
            {
                Debug.Assert(true);
            }
        }

        private void BeginListen()
        {
            try
            {
                IPEndPoint ipLocal;
                if (Bind != "0")
                {
                    var ipAddr = IPAddress.Parse(Bind);
                    ipLocal = new IPEndPoint(ipAddr, LocalPort);
                    Bind = "0";
                }
                else
                {
                    ipLocal = new IPEndPoint(IPAddress.Any, LocalPort);
                }
                _listenSocket = new TcpListener(ipLocal);
                _listenSocket.Start();
                ChangeState(WinsockStates.Listening);
                _listenSocket.BeginAcceptSocket(OnClientAccept, _clientSocket);
            }
            catch (SocketException ex)
            {
                Close();
                SocketErrorHandler(ex);
            }
            catch
            {
                Debug.Assert(true);
            }
        }
    }
}
