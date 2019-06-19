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
    public class SettingsTrayIcon
    {
        [XmlAttribute("alwaysShow")]
        public bool AlwaysShow { get; set; }

        [XmlAttribute("hideMinimized")]
        public bool HideMinimized { get; set; }

        [XmlAttribute("icon")]
        public string Icon { get; set; }

        [XmlAttribute("showNotifications")]
        public bool ShowNotifications { get; set; }
    }
}
