/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Serialization;
using ircCore.Utils;

namespace ircCore.Settings.SettingsBase.Structures
{
    [Serializable]
    public class MdiBackground
    {
        [XmlAttribute("path")]
        public string Path { get; set; }

        [XmlAttribute("layout")]
        public ImageLayout Layout { get; set; }
    }

    [Serializable]
    public class SettingsWindow
    {
        [XmlAttribute("childrenMaximized")]
        public bool ChildrenMaximized { get; set; }

        [XmlAttribute("switchTreeWidth")]
        public int SwitchTreeWidth { get; set; }

        [XmlAttribute("nicklistWidth")]
        public int NicklistWidth { get; set; }

        [XmlElement("search")]
        public SettingsFind Search = new SettingsFind();

        [XmlElement("caching")]
        public SettingsCaching Caching = new SettingsCaching();

        [XmlElement("window")]
        public List<WindowData> Window = new List<WindowData>();  

        [XmlElement("mdiBackground")]
        public MdiBackground MdiBackground = new MdiBackground();
    }

    [Serializable]
    public class WindowData
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("size")]
        public string SizeString
        {
            get { return XmlFormatting.WriteSizeFormat(Size); }
            set { Size = XmlFormatting.ParseSizeFormat(value); }
        }

        [XmlAttribute("position")]
        public string PositionString
        {
            get { return XmlFormatting.WritePointFormat(Position); }
            set { Position = XmlFormatting.ParsePointFormat(value); }
        }

        [XmlIgnore]
        public Size Size { get; set; }

        [XmlIgnore]
        public Point Position { get; set; }

        [XmlAttribute("maximized")]
        public bool Maximized { get; set; }
    }
}
