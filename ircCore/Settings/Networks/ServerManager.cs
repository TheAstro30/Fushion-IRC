/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Linq;
using ircCore.Utils;
using ircCore.Utils.Serialization;

namespace ircCore.Settings.Networks
{    
    public static class ServerManager
    {
        /* Management class for our server data */
        public static Servers Servers = new Servers();

        public static void Load()
        {
            if (!XmlSerialize<Servers>.Load(Functions.MainDir(@"\data\servers.xml"), ref Servers))
            {
                Servers = new Servers();
            }
        }

        public static void Save()
        {
            XmlSerialize<Servers>.Save(Functions.MainDir(@"\data\servers.xml"), Servers);
        }

        public static object[] GetAllNetworks()
        {
            /* Get all network names */
            return Servers.Networks.Network.ToArray();
        }

        public static NetworkData GetNetworkByName(string name)
        {
            return Servers.GetNetworkByName(name);
        }

        public static NetworkData GetNetworkByServer(ServerData server)
        {
            return Servers.GetNetworkByServer(server);
        }

        public static string GetNetworkNameByServerAddress(string address)
        {
            return (from n in Servers.Networks.Network from s in n.Server where string.Compare(s.Address, address, StringComparison.InvariantCultureIgnoreCase) == 0 select n.NetworkName).FirstOrDefault();
        }

        public static Server GetNextServer(string network)
        {
            var n = GetNetwork(network);
            if (n == null || n.Server.Count == 0)
            {
                return null;
            }
            n.LastServerIndex++;
            if (n.LastServerIndex > n.Server.Count - 1)
            {
                n.LastServerIndex = 0;
            }
            return n.Server[n.LastServerIndex].GetServer();
        }

        /* Private methods */
        private static NetworkData GetNetwork(string network)
        {
            return Servers.Networks.Network.FirstOrDefault(o => o.NetworkName.Equals(network, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
