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
    public class SettingsConnection
    {
        [XmlAttribute("address")]
        public string Address { get; set; }

        [XmlAttribute("port")]
        public int Port { get; set; }

        [XmlAttribute("channels")]
        public string Channels { get; set; }

        [XmlAttribute("newWindow")]
        public bool NewWindow { get; set; }

        [XmlAttribute("isSsl")]
        public bool IsSsl { get; set; }

        [XmlAttribute("showConnectDialog")]
        public bool ShowConnectDialog { get; set; }

        [XmlAttribute("sslAcceptRequests")]
        public bool SslAcceptRequests { get; set; }

        [XmlAttribute("sslAutoAcceptInvalidCertificates")]
        public bool SslAutoAcceptInvalidCertificates { get; set; }

        [XmlElement("options")]
        public SettingsConnectionOptions Options = new SettingsConnectionOptions();

        [XmlElement("identd")]
        public SettingsIdentd Identd = new SettingsIdentd();

        [XmlElement("localInfo")]
        public SettingsLocalInfo LocalInfo = new SettingsLocalInfo();
    }
}
