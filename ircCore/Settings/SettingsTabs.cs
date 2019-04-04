/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Xml.Serialization;

namespace ircCore.Settings
{
    [Serializable]
    public class SettingsTabs
    {
        [XmlAttribute("settings")]
        public string Settings { get; set; }

        [XmlAttribute("userList")]
        public int UserList { get; set; }
    }
}
