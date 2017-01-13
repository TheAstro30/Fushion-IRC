/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ircCore.Settings.Networks
{
    public class Server
    {
        /* This class is returned via ServerManager's GetNextServer method */
        public string Address { get; set; }

        public int Port { get; set; }

        public bool IsSsl { get; set; }

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
        [XmlAttribute("name")]
        public string NetworkName { get; set; }

        [XmlAttribute("last")]
        public int LastServerIndex { get; set; }

        [XmlElement("server")]
        public List<ServerData> Server = new List<ServerData>();

        /* Comparator */
        public int CompareTo(object obj)
        {
            return string.Compare(NetworkName, ((NetworkData) obj).NetworkName, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Serializable]
    public class ServerData : IComparable
    {
        /* Class that holds a server address for a particular network */
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
                port = 6667;
            }
            s.Port = port;
            /* Now we should have a server, but would be a good idea to test for null at the other end... */
            return s;
        }

        /* Comparator */
        public int CompareTo(object obj)
        {
            return string.Compare(Address, ((ServerData)obj).Address, StringComparison.OrdinalIgnoreCase);
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
    }
}
