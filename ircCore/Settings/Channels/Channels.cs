/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
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
        public class ChannelFavoriteData
        {
            [XmlAttribute("name")]
            public string Name { get; set; }

            [XmlAttribute("description")]
            public string Description { get; set; }

            [XmlAttribute("password")]
            public string Password { get; set; }
        }

        [XmlElement("favorite")]
        public List<ChannelFavoriteData> Favorite = new List<ChannelFavoriteData>();
    }

    [Serializable]
    public class ChannelRecent
    {
        [Serializable]
        public class ChannelData
        {
            [XmlAttribute("name")]
            public string Name { get; set; }
        }

        /* I would probably change this class later to organize channels by network... */
        [XmlElement("channel")]
        public List<ChannelData> Channel = new List<ChannelData>(); 
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
