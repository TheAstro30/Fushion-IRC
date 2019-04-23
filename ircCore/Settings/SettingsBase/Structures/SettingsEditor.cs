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
    public class SettingsEditor
    {
        [XmlAttribute("syntaxHighlight")]
        public bool SyntaxHighlight { get; set; }

        [XmlAttribute("lineNumbering")]
        public bool LineNumbering { get; set; }

        [XmlAttribute("zoom")]
        public int Zoom { get; set; }

        [XmlAttribute("last")]
        public string Last { get; set; }
    }
}
