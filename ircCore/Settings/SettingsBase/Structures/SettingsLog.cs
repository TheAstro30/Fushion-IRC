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
    public class SettingsLog
    {
        [XmlAttribute("path")]
        public string LogPath { get; set; }

        [XmlAttribute("logChannels")]
        public bool LogChannels { get; set; }

        [XmlAttribute("logChats")]
        public bool LogChats { get; set; }

        [XmlAttribute("reloadChannels")]
        public bool ReloadChannelLogs { get; set; }

        [XmlAttribute("reloadChats")]
        public bool ReloadChatLogs { get; set; }

        [XmlAttribute("truncate")]
        public bool Truncate { get; set; }

        [XmlAttribute("truncateBytes")]
        public int TruncateBytes { get; set; } /* NOTE: This is KB, not bytes */

        [XmlAttribute("createFolder")]
        public bool CreateFolder { get; set; }

        [XmlAttribute("dateByDay")]
        public bool DateByDay { get; set; }
    }
}
