/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ircCore.Settings.SettingsBase.Structures
{
    public enum SearchDirection
    {
        Up = 0,
        Down = 1
    }

    [Serializable]
    public class SettingsFind
    {
        [XmlAttribute("direction")]
        public SearchDirection Direction { get; set; }

        [XmlAttribute("matchCase")]
        public bool MatchCase { get; set; }

        [XmlElement("history")]
        public List<string> History = new List<string>();        
    }
}
