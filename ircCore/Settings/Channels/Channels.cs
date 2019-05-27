/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ircCore.Settings.Channels
{
    public class ChannelList
    {
        public string Name { get; set; }

        public int Users { get; set; }

        public string Topic { get; set; }
    }

    [Serializable]
    public class ChannelFavorites
    {
        [Serializable]
        public class ChannelFavoriteData : IComparable
        {
            [XmlAttribute("name")]
            public string Name { get; set; }

            [XmlAttribute("description")]
            public string Description { get; set; }

            [XmlAttribute("password")]
            public string Password { get; set; }

            public int CompareTo(object obj)
            {
                return string.Compare(Name, ((ChannelFavoriteData)obj).Name, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        [XmlElement("favorite")]
        public List<ChannelFavoriteData> Favorite = new List<ChannelFavoriteData>();
    }

    [Serializable]
    public class ChannelRecent
    {
        [Serializable]
        public class ChannelData : IComparable<ChannelData>
        {
            [XmlAttribute("name")]
            public string Name { get; set; }

            public int CompareTo(ChannelData other)
            {
                return Name.CompareTo(other.Name);
            }
        }

        [Serializable]
        public class ChannelNetworkData : IComparable<ChannelNetworkData>
        {
            [XmlAttribute("network")]
            public string Network { get; set; }

            [XmlElement("channel")]
            public List<ChannelData> Channel = new List<ChannelData>();

            public int CompareTo(ChannelNetworkData other)
            {
                return Network.CompareTo(other.Network);
            }
        }

        /* I would probably change this class later to organize channels by network... */
        [XmlElement("channels")]
        public List<ChannelNetworkData> Channels = new List<ChannelNetworkData>(); 
    }

    [Serializable, XmlRoot("channels")]
    public class Channels
    {
        [XmlElement("favorites")]
        public ChannelFavorites Favorites = new ChannelFavorites();

        [XmlElement("recent")]
        public ChannelRecent Recent = new ChannelRecent();
    }
}
