/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
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
using System.ComponentModel.Design;
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
    public class ClientSock : Component
    {
        /* Winsock component class
           Original author: Unknown
           Modified by: Jason James Newland (2008-2009/2011-2012)
         */
        private const int BufferSize = 32769;

        private string _remoteIp;
        private int _localPort;
        private int _remotePort;
        private List<byte[]> _byteData;
        private byte[] _buffer = new byte[BufferSize];
        private TcpListener _listenSocket;
        private Socket _clientSocket;
        private ISynchronizeInvoke _syncObject;

        private NetworkStream _networkStream;
        private SslStream _sslStream;

        private bool _sslWaitingAccept;
        private bool _sslCertificateAccepted;

        /* Need to be virtual so they can be "overriden" in another class */
        public virtual event Action<ClientSock> OnConnected;
        public virtual event Action<ClientSock> OnDisconnected;
        public virtual event Action<ClientSock, int> OnDataArrival;
        public virtual event Action<ClientSock, Socket> OnConnectionRequest;
        public virtual event Action<ClientSock, string> OnError;
        public virtual event Action<ClientSock, WinsockStates> OnStateChanged;
        public virtual event Action<ClientSock, string> OnDebugOut;
        public virtual event Action<ClientSock, X509Certificate> OnSslInvalidCertificate;

        public ClientSock()
            : this(80)
        {
            /* Empty */
        }

        public ClientSock(int port)
            : this("127.0.0.1", port)
        {
            /* Empty */
        }

        public ClientSock(string ip)
            : this(ip, 80)
        {
            /* Empty */
        }

        public ClientSock(string ip, int port)
        {
            Bind = "0";
            RemoteIp = ip;
            RemotePort = port;
            LocalPort = port;
            _byteData = new List<byte[]>();
        }

        [Browsable(false)]
        public ISynchronizeInvoke SynchronizingObject
        {
            get
            {
                if (_syncObject == null & DesignMode)
                {
                    var designer = (IDesignerHost) GetService(typeof (IDesignerHost));
                    if (designer != null)
                    {
                        _syncObject = (ISynchronizeInvoke) designer.RootComponent;
                    }
                }
                return _syncObject;
            }
            set
            {
                if (DesignMode)
                {
                    return;
                }
                if (_syncObject != null && !ReferenceEquals(_syncObject, value))
                {
                    throw new Exception("Property can not be set at run-time");
                }
                _syncObject = value;
            }
        }

        public bool IsSsl { get; set; }

        [DefaultValue(80)]
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

        [DefaultValue(80)]
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

        [DefaultValue("127.0.0.1")]
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

        [DefaultValue(WinsockStates.Closed)]
        public WinsockStates GetState { get; private set; }

        [DefaultValue("0")]
        public string Bind { private get; set; }

        [DefaultValue(false)]
        public bool EnableSslAuthentication { get; set; }

        [DefaultValue(false)]
        public bool SslAutoAccept { get; set; }

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
                    _syncObject.Invoke(OnConnected, new object[] {this});
                }
                ChangeState(WinsockStates.Connected);
                _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnClientRead, null);
            }
            catch (SocketException ex)
            {
                SocketErrorHandler(ex);
            }
            catch
            {
                System.Diagnostics.Debug.Assert(true);
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
                System.Diagnostics.Debug.Assert(true);
            }
        }

        public void SendData(string data)
        {
            try
            {
                SendData(Utf8.StringToByteArray(data));
                if (OnDebugOut != null)
                {
                    _syncObject.Invoke(OnDebugOut, new object[] {this, data});
                }
            }
            catch
            {
                System.Diagnostics.Debug.Assert(true);
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
                        System.Diagnostics.Debug.Assert(true);
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
                        _syncObject.Invoke(OnConnected, new object[] {this});
                    }
                }
            }
            catch (SocketException ex)
            {
                SocketErrorHandler(ex);
            }
            catch
            {
                System.Diagnostics.Debug.Assert(true);
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
                        _syncObject.Invoke(OnSslInvalidCertificate, new object[] {this, certificate});
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
                        _syncObject.Invoke(OnConnectionRequest, new object[] {this, tmpSock});
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
                        _syncObject.Invoke(OnError, new object[] {this, "Unknown error"});
                    }
                }
            }
            catch
            {
                System.Diagnostics.Debug.Assert(true);
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
                            _syncObject.Invoke(OnDisconnected, new object[] {this});
                        }
                        return;
                    }
                    Array.Resize(ref _buffer, intCount);
                    _byteData.Add(_buffer);
                    if (OnDataArrival != null)
                    {
                        _syncObject.Invoke(OnDataArrival, new object[] {this, _buffer.Length});
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
                        _syncObject.Invoke(OnDisconnected, new object[] {this});
                    }
                }
                catch
                {
                    System.Diagnostics.Debug.Assert(true);
                }
            }
        }

        private void SocketErrorHandler(ExternalException ex)
        {
            try
            {
                if (OnError != null)
                {
                    _syncObject.Invoke(
                        OnError,
                        ex != null
                            ? new object[] {this, ErrorHandling.GetErrorDescription(ex.ErrorCode)}
                            : new object[] {this, ErrorHandling.GetErrorDescription(-1)});
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
                System.Diagnostics.Debug.Assert(true);
            }
        }

        private void ChangeState(WinsockStates newState)
        {
            try
            {
                GetState = newState;
                if (OnStateChanged != null)
                {
                    _syncObject.Invoke(OnStateChanged, new object[] {this, GetState});
                }
            }
            catch
            {
                System.Diagnostics.Debug.Assert(true);
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
                System.Diagnostics.Debug.Assert(true);
            }
        }
    }
}
