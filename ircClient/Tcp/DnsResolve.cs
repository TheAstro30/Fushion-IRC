/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using ircCore.Utils;

namespace ircClient.Tcp
{
    public class DnsResolve
    {
        private readonly UiSynchronize _sync;

        public event Action<DnsResolve, DnsResult> DnsResolved;
        public event Action<DnsResolve, DnsResult> DnsFailed;

        public DnsResolve(ISynchronizeInvoke syncObject)
        {
            _sync = new UiSynchronize(syncObject);
        }

        public void Resolve(string address)
        {
            IPAddress ip;
            if (IPAddress.TryParse(address, out ip))
            {
                /* An IP address was passed, so we get the host name */
                Dns.BeginGetHostEntry(ip, GetHostFromAddress, address);
            }
            else
            {
                /* A host name was passed, we need to resolve the IP */
                Dns.BeginGetHostEntry(address, GetHostEntry, address);
            }
        }

        private void GetHostEntry(IAsyncResult ar)
        {
            var result = new DnsResult {Lookup = (string) ar.AsyncState};
            try
            {
                var host = Dns.EndGetHostEntry(ar);
                if (host.AddressList.Length == 0)
                {
                    if (DnsFailed != null)
                    {
                        _sync.Execute(() => DnsFailed(this, result));
                    }
                    return;
                }
                var addresses = Array.FindAll(host.AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);
                if (addresses.Length > 0)
                {
                    result.Address = addresses[0].ToString();
                    result.HostName = result.Lookup;
                    if (DnsResolved != null)
                    {
                        _sync.Execute(() => DnsResolved(this, result));
                    }
                    return;
                }
                if (DnsFailed != null)
                {
                    _sync.Execute(() => DnsFailed(this, result));
                }
            }
            catch
            {
                if (DnsFailed != null)
                {
                    _sync.Execute(() => DnsFailed(this, result));
                }
            }
        }

        private void GetHostFromAddress(IAsyncResult ar)
        {
            var result = new DnsResult {Lookup = (string) ar.AsyncState};
            try
            {
                var host = Dns.EndGetHostEntry(ar);
                if (string.IsNullOrEmpty(host.HostName))
                {
                    if (DnsFailed != null)
                    {
                        _sync.Execute(() => DnsFailed(this, result));
                    }
                    return;
                }
                result.Address = result.Lookup;
                result.HostName = host.HostName;
                if (DnsResolved != null)
                {
                    _sync.Execute(() => DnsResolved(this, result));
                }
            }
            catch (Exception)
            {
                if (DnsFailed != null)
                {
                    _sync.Execute(() => DnsFailed(this, result));
                }
            }
        }
    }
}
