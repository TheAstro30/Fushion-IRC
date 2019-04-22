/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.ComponentModel;
using System.Xml.Serialization;
using ircCore.Utils;

namespace ircCore.Settings.SettingsBase.Structures
{
    public enum LocalInfoLookupMethod
    {
        [Description("None (info will not be updated)")]
        None = 0,

        [Description("Normal (local socket-side look-up)")]
        Socket = 1,

        [Description("Server (remote socket-side look-up)")]
        Server = 2
    }

    [Serializable]
    public class SettingsLocalInfo
    {
        [XmlAttribute("lookupMethod")]
        public LocalInfoLookupMethod LookupMethod { get; set; }

        [XmlElement("hostInfo")]
        public DnsResult HostInfo = new DnsResult();
    }
}
