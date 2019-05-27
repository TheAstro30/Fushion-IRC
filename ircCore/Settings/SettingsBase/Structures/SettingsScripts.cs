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
    public enum PopupType
    {
        None = 0,
        Commands = 1,
        Console = 2,
        Channel = 3,
        Nicklist = 4,
        Private = 5,
        DccChat = 6
    }

    [Serializable]
    public class SettingsScripts
    {
        public class SettingsScriptPath
        {
            [XmlAttribute("path")]
            public string Path { get; set; }

            [XmlAttribute("type")]
            public PopupType Type { get; set; }
        }

        [XmlElement("alias")]
        public List<SettingsScriptPath> Aliases = new List<SettingsScriptPath>();

        [XmlElement("event")]
        public List<SettingsScriptPath> Events = new List<SettingsScriptPath>();

        [XmlElement("popup")]
        public List<SettingsScriptPath> Popups = new List<SettingsScriptPath>();
    }
}
