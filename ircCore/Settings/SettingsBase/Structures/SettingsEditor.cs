/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Xml.Serialization;
using ircCore.Utils;

namespace ircCore.Settings.SettingsBase.Structures
{
    [Serializable]
    public class SettingsEditor
    {
        [XmlAttribute("syntaxHighlight")]
        public bool SyntaxHighlight { get; set; }

        [XmlAttribute("font")]
        public string FontString
        {
            get { return XmlFormatting.WriteFontFormat(Font); }
            set { Font = XmlFormatting.ParseFontFormat(value); }
        }

        [XmlIgnore]
        public Font Font { get; set; }

        [XmlAttribute("last")]
        public string Last { get; set; }
    }
}
