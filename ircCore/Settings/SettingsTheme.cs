using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        }

        [XmlAttribute("currentTheme")]
        public int CurrentTheme { get; set; }

        [XmlElement("theme")]
        public List<ThemeListData> Theme = new List<ThemeListData>();
    }
}
