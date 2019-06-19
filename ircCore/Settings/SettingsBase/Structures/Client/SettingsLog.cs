/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ircCore.Settings.SettingsBase.Structures.Client
{
    public enum LoggingType
    {
        [Description("None")]
        None= 0,

        [Description("Channels")]
        Channels = 1,

        [Description("Private chats")]
        Chats = 2,

        [Description("Both")]
        Both = 3
    }

    [Serializable]
    public class SettingsLog
    {
        [XmlAttribute("path")]
        public string LogPath { get; set; }

        [XmlAttribute("loggingType")]
        public LoggingType KeepLogsType { get; set; }

        [XmlAttribute("reloadType")]
        public LoggingType ReloadLogsType { get; set; }

        [XmlAttribute("createFolder")]
        public bool CreateFolder { get; set; }

        [XmlAttribute("dateByDay")]
        public bool DateByDay { get; set; }

        [XmlAttribute("stripCodes")]
        public bool StripCodes { get; set; }
    }
}
