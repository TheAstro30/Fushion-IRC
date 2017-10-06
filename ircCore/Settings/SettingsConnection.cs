/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Xml.Serialization;

namespace ircCore.Settings
{
    [Serializable]
    public class SettingsConnection
    {
        [XmlAttribute("address")]
        public string Address { get; set; }

        [XmlAttribute("port")]
        public int Port { get; set; }

        [XmlAttribute("channels")]
        public string Channels { get; set; }

        [XmlAttribute("newWindow")]
        public bool NewWindow { get; set; }

        [XmlAttribute("isSsl")]
        public bool IsSsl { get; set; }

        [XmlElement("localInfo")]
        public SettingsLocalInfo LocalInfo = new SettingsLocalInfo();
    }
}
