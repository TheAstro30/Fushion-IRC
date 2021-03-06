﻿/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace ircCore.Settings.Networks
{
    [Serializable]
    public class Server
    {
        /* This class is returned via ServerManager's GetNextServer method */
        [XmlAttribute("address")]
        public string Address { get; set; }

        [XmlAttribute("port")]
        public int Port { get; set; }

        [XmlAttribute("ssl")]
        public bool IsSsl { get; set; }

        [XmlAttribute("password")]
        public string Password { get; set; }

        public Server()
        {
            /* Empty */
        }

        public Server(Server server)
        {
            Address = server.Address;
            Port = server.Port;
            IsSsl = server.IsSsl;
        }

        /* ToString() method "server.address:port" */
        public override string ToString()
        {
            return string.Format("{0}:{1}", Address, IsSsl ? string.Format("+{0}", Port) : Port.ToString());
        }
    }

    [Serializable]
    public class NetworkData : IComparable
    {
        /* Class that holds all servers for one network */
        [XmlIgnore]
        public string DisplayName
        {
            get { return NetworkName; }
        }

        [XmlAttribute("name")]
        public string NetworkName { get; set; }

        [XmlAttribute("last")]
        public int LastServerIndex { get; set; }

        [XmlElement("server")]
        public List<ServerData> Server = new List<ServerData>();

        /* Comparator */
        public int CompareTo(object obj)
        {
            return string.Compare(NetworkName, ((NetworkData) obj).NetworkName, StringComparison.InvariantCultureIgnoreCase);
        }

        public override string ToString()
        {
            return NetworkName;
        }
    }

    [Serializable]
    public class ServerData : IComparable
    {
        /* Class that holds a server address for a particular network */
        [XmlIgnore]
        public string DisplayName
        {
            get
            {
                return !string.IsNullOrEmpty(Description) ? string.Format("{0} ({1})", Description, Address) : Address;
            }
        }

        [XmlAttribute("address")]
        public string Address { get; set; }

        [XmlAttribute("portRange")]
        public string PortRange { get; set; }

        [XmlAttribute("password")]
        public string Password { get; set; } /* Mainly used for BNCs */

        [XmlAttribute("description")]
        public string Description { get; set; }

        /* Get this ServerData's address field and the port range randomized */
        public Server GetServer()
        {            
            var s = new Server
                        {
                            Address = Address
                        };
            /* First, let's pick a random port : 6667-6668,7000 etc */
            var portString = string.Empty;
            var ports = PortRange.Split(',');
            if (ports.Length > 1)
            {
                /* There's more than one set of ports delimited by "," */
                var r = new Random();
                var p = ports[r.Next(ports.Length)];
                var secondPort = p.Split('-');
                if (secondPort.Length > 1)
                {
                    /* We now need to get a range */
                    int range1;
                    if (int.TryParse(secondPort[0],out range1))
                    {
                        int range2;
                        portString = !int.TryParse(secondPort[1], out range2) ? secondPort[0] : r.Next(range1, range2 + 1).ToString();
                    }
                }
                else
                {
                    /* There was only one port delimited by "-", most likely an error in data entry; return first part found */
                    portString = secondPort[0];
                }
            }
            else
            {
                /* The port range has only one port assigned to it */
                portString = ports[0];
            }
            if (portString.StartsWith("+"))
            {
                /* Set SSL true and remove + */
                s.IsSsl = true;
                portString = portString.Substring(1);
            }
            /* Convert either the only port found or the randomly selected port to an integer and set it to our Server object's Port */
            int port;
            if (!int.TryParse(portString, out port))
            {
                port = SettingsManager.Settings.Connection.Options.DefaultPort;
            }
            s.Port = port;
            /* Now we should have a server, but would be a good idea to test for null at the other end... */
            return s;
        }

        /* Comparator */
        public int CompareTo(object obj)
        {
            return string.Compare(Address, ((ServerData)obj).Address, StringComparison.InvariantCultureIgnoreCase);
        }

        public override string ToString()
        {
            return Address;
        }
    }

    [Serializable, XmlRoot("servers")]
    public class Servers
    {
        /* Main IRC servers data class */
        public class NetworkList
        {
            [XmlElement("network")]
            public List<NetworkData> Network = new List<NetworkData>();     
       
            public NetworkList()
            {
                /* Empty constructor */
            }

            public NetworkList(NetworkList networkList)
            {
                /* Copy constructor */
                Network = new List<NetworkData>(networkList.Network.GetRange(0, networkList.Network.Count));
            }
        }

        public class RecentList
        {
            [XmlElement("server")]
            public List<Server> Server = new List<Server>(); 

            public RecentList()
            {
                /* Empty constructor */
            }

            public RecentList(RecentList recentList)
            {
                /* Copy constructor */
                Server = new List<Server>(recentList.Server.GetRange(0, recentList.Server.Count));
            }
        }

        /* The reason I put this as a class is because I wanted the formatted XML to look like the following:
         * <networks>
         *    <network name="Name">
         *       <server address="irc.blah.com" .. />
         *    </network>
         * <networks> 
         * much nicer looking formatted like this ;) */
        [XmlElement("networks")]
        public NetworkList Networks = new NetworkList();

        [XmlElement("recent")]
        public RecentList Recent = new RecentList();

        public Servers()
        {
            /* Empty constructor */
        }

        public Servers(Servers servers)
        {
            /* Copy constructor */
            Networks = new NetworkList(servers.Networks);
            Recent = new RecentList(servers.Recent);
        }

        public NetworkData GetNetworkByName(string name)
        {
            return Networks.Network.FirstOrDefault(o => string.Compare(o.NetworkName, name, StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        public NetworkData GetNetworkByServer(ServerData server)
        {
            return (from network in Networks.Network from s in network.Server where s == server select network).FirstOrDefault();
        }
    }
}
