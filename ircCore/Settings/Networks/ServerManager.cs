/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Linq;
using ircCore.Utils.Serialization;

namespace ircCore.Settings.Networks
{    
    public static class ServerManager
    {
        /* Management class for our server data */
        public static Servers Servers = new Servers();

        public static void Load(string fileName)
        {
            if (!XmlSerialize<Servers>.Load(fileName, ref Servers))
            {
                Servers = new Servers();
            }
        }

        public static void Save(string fileName)
        {
            XmlSerialize<Servers>.Save(fileName, Servers);
        }   
 
        /* Adding a server methods */
        public static void AddServer(string address)
        {
            AddServer("Unknown", address);
        }

        public static void AddServer(string network, string address)
        {
            AddServer(network, address, "6667");
        }

        public static void AddServer(string network, string address, string portRange)
        {
            AddServer(network, address, portRange, "", "");
        }

        public static void AddServer(string network, string address, string portRange, string description)
        {
            AddServer(network, address, portRange, "", description);
        }

        public static void AddServer(string network, string address, string portRange, string password, string description)
        {            
            /* Find or create a network object */
            var newNetwork = false;
            var n = Servers.Networks.Network.FirstOrDefault(o => o.NetworkName.ToLower() == network.ToLower());
            if (n == null)
            {
                newNetwork = true;
                n = new NetworkData();
            }
            if (string.IsNullOrEmpty(n.NetworkName))
            {
                n.NetworkName = network;
            }
            /* Create a new server object */
            var s = new ServerData
                        {
                            Address = address,
                            PortRange = portRange                            
                        };
            if (!string.IsNullOrEmpty(description))
            {
                s.Description = description;
            }
            if (!string.IsNullOrEmpty(password))
            {
                s.Password = password;
            }
            n.Server.Add(s);
            n.Server.Sort();
            /* Make sure to add the network - if it was not found */
            if (!newNetwork)
            {
                return;
            }
            Servers.Networks.Network.Add(n);
            Servers.Networks.Network.Sort();
        }

        /* Removal of a server */
        public static void RemoveServer()
        {
            
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
            return Servers.Networks.Network.FirstOrDefault(o => o.NetworkName.ToLower() == network.ToLower());
        }
    }
}
