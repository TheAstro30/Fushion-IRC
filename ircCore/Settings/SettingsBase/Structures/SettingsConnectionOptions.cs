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
    public class SettingsConnectionOptions
    {
        [XmlAttribute("defaultPort")]
        public int DefaultPort { get; set; }

        [XmlAttribute("reconnect")]
        public bool Reconnect { get; set; }

        [XmlAttribute("retryConnection")]
        public bool RetryConnection { get; set; }

        [XmlAttribute("retryTimes")]
        public int RetryTimes { get; set; }

        [XmlAttribute("retryDelay")]
        public int RetryDelay { get; set; }

        [XmlAttribute("nextServer")]
        public bool NextServer { get; set; }
    }
}
