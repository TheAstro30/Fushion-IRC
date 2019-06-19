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
    public class SettingsFlash
    {
        [XmlAttribute("channel")]
        public bool Channel { get; set; }

        [XmlAttribute("private")]
        public bool Private { get; set; }

        [XmlAttribute("chat")]
        public bool Chat { get; set; }
    }
}
