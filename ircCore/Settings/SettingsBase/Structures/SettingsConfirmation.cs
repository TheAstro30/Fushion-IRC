/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Xml.Serialization;

namespace ircCore.Settings.SettingsBase.Structures
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
