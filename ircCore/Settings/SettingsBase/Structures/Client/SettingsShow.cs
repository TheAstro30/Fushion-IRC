/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Xml.Serialization;

namespace ircCore.Settings.SettingsBase.Structures.Client
{
    [Serializable]
    public class SettingsShow
    {
        [XmlAttribute("pingPong")]
        public bool PingPong { get; set; }

        [XmlAttribute("motd")]
        public bool Motd { get; set; }

        [XmlAttribute("notices")]
        public bool Notices { get; set; }

        [XmlAttribute("ctcps")]
        public bool Ctcps { get; set; }
    }
}
