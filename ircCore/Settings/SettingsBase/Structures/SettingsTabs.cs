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
    public class SettingsTabs
    {
        [XmlAttribute("settings")]
        public string Settings { get; set; }

        [XmlAttribute("userList")]
        public int UserList { get; set; }

        [XmlAttribute("autoList")]
        public int AutoList { get; set; }

        [XmlAttribute("theme")]
        public int Theme { get; set; }
    }
}
