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
        public class SettingsEditorColors
        {
            [XmlIgnore]
            public Color KeyWordColor
            {
                get { return ColorTranslator.FromHtml(KeyWordColorString); }
                set { KeyWordColorString = ColorTranslator.ToHtml(value); }
            }

            [XmlAttribute("keyWord")]
            public string KeyWordColorString { get; set; }

            [XmlIgnore]
            public Color IdentifierColor
            {
                get { return ColorTranslator.FromHtml(IdentifierColorString); }
                set { IdentifierColorString = ColorTranslator.ToHtml(value); }
            }

            [XmlAttribute("identifier")]
            public string IdentifierColorString { get; set; }

            [XmlIgnore]
            public Color CustomIdentifierColor
            {
                get { return ColorTranslator.FromHtml(CustomIdentifierColorString); }
                set { CustomIdentifierColorString = ColorTranslator.ToHtml(value); }
            }

            [XmlAttribute("customerIdentifier")]
            public string CustomIdentifierColorString { get; set; }

            [XmlIgnore]
            public Color VariableColor
            {
                get { return ColorTranslator.FromHtml(VariableColorString); }
                set { VariableColorString = ColorTranslator.ToHtml(value); }
            }

            [XmlAttribute("variable")]
            public string VariableColorString { get; set; }

            [XmlIgnore]
            public Color CommandColor
            {
                get { return ColorTranslator.FromHtml(CommandColorString); }
                set { CommandColorString = ColorTranslator.ToHtml(value); }
            }

            [XmlAttribute("command")]
            public string CommandColorString { get; set; }

            [XmlIgnore]
            public Color CommentColor
            {
                get { return ColorTranslator.FromHtml(CommentColorString); }
                set { CommentColorString = ColorTranslator.ToHtml(value); }
            }

            [XmlAttribute("comment")]
            public string CommentColorString { get; set; }

            [XmlIgnore]
            public Color MiscColor //rename this later
            {
                get { return ColorTranslator.FromHtml(MiscColorString); }
                set { MiscColorString = ColorTranslator.ToHtml(value); }
            }

            [XmlAttribute("misc")]
            public string MiscColorString { get; set; }
        }

        [XmlAttribute("splitSize")]
        public int SplitSize { get; set; }

        [XmlAttribute("syntaxHighlight")]
        public bool SyntaxHighlight { get; set; }

        [XmlAttribute("lineNumbering")]
        public bool LineNumbering { get; set; }

        [XmlAttribute("zoom")]
        public int Zoom { get; set; }

        [XmlAttribute("last")]
        public string Last { get; set; }

        [XmlElement("colors")]
        public SettingsEditorColors Colors = new SettingsEditorColors();
    }
}
