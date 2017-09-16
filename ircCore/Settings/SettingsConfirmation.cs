using System;
using System.Xml.Serialization;

namespace ircCore.Settings
{
    public enum ClientCloseConfirmation
    {
        None = 0,
        Connected = 1,
        Always = 2
    }

    [Serializable]
    public class SettingsConfirmation
    {
        [XmlAttribute("clientClose")]
        public ClientCloseConfirmation ClientClose { get; set; }

        [XmlAttribute("url")]
        public bool Url { get; set; }
    }
}
