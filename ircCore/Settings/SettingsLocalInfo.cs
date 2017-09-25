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
    public class SettingsLocalInfo
    {
        [XmlAttribute("address")]
        public string Address { get; set; }

        [XmlAttribute("host")]
        public string Host { get; set; }
    }
}
