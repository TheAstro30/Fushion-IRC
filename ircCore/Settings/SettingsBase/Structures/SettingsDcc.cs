/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ircCore.Settings.SettingsBase.Structures
{
    public enum DccRequestAction
    {
        Ask = 0,
        AutoAccept = 1,
        Ignore = 2
    }

    public enum DccFileExistsAction
    {
        [Description("Ask")]
        Ask = 0,

        [Description("Overwrite")]
        Overwrite = 1,

        [Description("Cancel")]
        Cancel
    }

    public enum DccFilterMethod
    {
        [Description("Disabled")]
        Disabled = 0,

        [Description("Accept only")]
        AcceptOnly = 1,

        [Description("Ignore only")]
        IgnoreOnly = 2
    }

    [Serializable]
    public class SettingsDcc
    {
        public class SettingsDccRequests
        {
            [XmlAttribute("getRequest")]
            public DccRequestAction GetRequest { get; set; }

            [XmlAttribute("getFileExists")]
            public DccFileExistsAction GetFileExists { get; set; }

            [XmlAttribute("chatRequest")]
            public DccRequestAction ChatRequest { get; set; }
        }

        public class SettingsDccOptions
        {
            public class SettingsDccGeneral
            {
                [XmlAttribute("minPort")]
                public int MinimumPort { get; set; }

                [XmlAttribute("maxPort")]
                public int MaximumPort { get; set; }

                [XmlAttribute("randomize")]
                public bool Randomize { get; set; }

                [XmlAttribute("bind")]
                public bool BindToIp { get; set; }

                [XmlAttribute("ip")]
                public string BindIpAddress { get; set; }

                [XmlAttribute("packetSize")]
                public int PacketSize { get; set; }
            }

            public class SettingsDccTimeouts
            {
                [XmlAttribute("getSendRequest")]
                public int GetSendRequest { get; set; }

                [XmlAttribute("getSendTransfer")]
                public int GetSendTransfer { get; set; }

                [XmlAttribute("chatConnection")]
                public int ChatConnection { get; set; }
            }

            public class SettingsDccFilter
            {
                public class SettingsDccExtension : IComparable<SettingsDccExtension>
                {
                    [XmlAttribute("name")]
                    public string Name { get; set; }

                    public int CompareTo(SettingsDccExtension other)
                    {
                        return Name.CompareTo(other.Name);
                    }
                }

                [XmlAttribute("filterMethod")]
                public DccFilterMethod FilterMethod { get; set; }

                [XmlAttribute("showRejectionDialog")]
                public bool ShowRejectionDialog { get; set; }

                [XmlElement("extension")]
                public List<SettingsDccExtension> Extension = new List<SettingsDccExtension>();
            }

            [XmlElement("general")]
            public SettingsDccGeneral General = new SettingsDccGeneral();

            [XmlElement("timeouts")]
            public SettingsDccTimeouts Timeouts = new SettingsDccTimeouts();

            [XmlElement("filter")]
            public SettingsDccFilter Filter = new SettingsDccFilter();            
        }

        public class SettingsDccHistory
        {
            public class SettingsDccHistoryData
            {
                [XmlAttribute("nick")]
                public string Nick { get; set; }

                public override string ToString()
                {
                    return Nick;
                }
            }

            [XmlElement("data")]
            public List<SettingsDccHistoryData> Data = new List<SettingsDccHistoryData>();
        }

        [XmlElement("requests")]
        public SettingsDccRequests Requests = new SettingsDccRequests();

        [XmlElement("options")]
        public SettingsDccOptions Options = new SettingsDccOptions();

        [XmlElement("history")]
        public SettingsDccHistory History = new SettingsDccHistory();
    }
}
