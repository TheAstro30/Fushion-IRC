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
    public class SettingsIdentd
    {
        [XmlAttribute("enable")]
        public bool Enable { get; set; }

        [XmlAttribute("userID")]
        public string UserId { get; set; }

        [XmlAttribute("system")]
        public string System { get; set; }

        [XmlAttribute("port")]
        public int Port { get; set; }
    }
}
