using System;
using System.Xml.Serialization;

namespace ircCore.Settings
{
    public enum CloseConfirmation
    {
        None = 0,
        Connected = 1,
        Always = 2
    }

    [Serializable]
    public class SettingsConfirmation
    {
        [XmlAttribute("clientClose")]
        public CloseConfirmation ClientClose { get; set; }

        [XmlAttribute("consoleClose")]
        public CloseConfirmation ConsoleClose { get; set; }

        [XmlAttribute("url")]
        public bool Url { get; set; }
    }
}
