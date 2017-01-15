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
    public class SettingsUserInfo
    {
        [XmlAttribute("nick")]
        public string Nick { get; set; }

        [XmlAttribute("alternative")]
        public string Alternative { get; set; }

        [XmlAttribute("ident")]
        public string Ident { get; set; }

        [XmlAttribute("realname")]
        public string RealName { get; set; }
    }
}
