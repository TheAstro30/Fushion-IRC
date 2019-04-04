/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ircCore.Settings
{
    [Serializable]
    public class SettingsTheme
    {
        [Serializable]
        public class ThemeListData
        {
            [XmlAttribute("name")]
            public string Name { get; set; }

            [XmlAttribute("path")]
            public string Path { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }

        [XmlAttribute("currentTheme")]
        public int CurrentTheme { get; set; }

        [XmlElement("theme")]
        public List<ThemeListData> Theme = new List<ThemeListData>();
    }
}
