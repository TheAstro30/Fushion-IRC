/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Xml.Serialization;

namespace ircCore.Settings.SettingsBase.Structures
{
    [Serializable]
    public class SettingsCaching
    {
        [XmlAttribute("output")]
        public int Output { get; set; }

        [XmlAttribute("input")]
        public int Input { get; set; }

        [XmlAttribute("chatSearch")]
        public int ChatSearch { get; set; }
    }
}
