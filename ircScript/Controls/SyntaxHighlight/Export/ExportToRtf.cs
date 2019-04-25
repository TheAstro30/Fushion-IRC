//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE.
//
//  License: GNU Lesser General Public License (LGPLv3)
//
//  Email: pavel_torgashov@ukr.net
//
//  Copyright (C) Pavel Torgashov, 2011-2016.
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using ircScript.Controls.SyntaxHighlight.Helpers;
using ircScript.Controls.SyntaxHighlight.Helpers.TextRange;
using ircScript.Controls.SyntaxHighlight.Styles;

namespace ircScript.Controls.SyntaxHighlight.Export
{
    public class RtfStyleDescriptor
    {
        public Color ForeColor { get; set; }
        public Color BackColor { get; set; }
        public string AdditionalTags { get; set; }
    }

    public class ExportToRtf
    {
        private readonly Dictionary<Color, int> _colorTable = new Dictionary<Color, int>();

        public bool IncludeLineNumbers { get; set; }

        public bool UseOriginalFont { get; set; }

        public FastColoredTextBox TextBox { get; set; }
        
        public ExportToRtf()
        {
            UseOriginalFont = true;
        }

        public string GetRtf(FastColoredTextBox tb)
        {
            TextBox = tb;
            var sel = new Range(tb);
            sel.SelectAll();
            return GetRtf(sel);
        }

        public string GetRtf(Range r)
        {
            TextBox = r.TextBox;
            var styles = new Dictionary<StyleIndex, object>();
            var sb = new StringBuilder();
            var tempSb = new StringBuilder();
            var currentStyleId = StyleIndex.None;
            r.Normalize();
            var currentLine = r.Start.Line;
            styles[currentStyleId] = null;
            _colorTable.Clear();
            var lineNumberColor = GetColorTableNumber(r.TextBox.LineNumberColor);
            if (IncludeLineNumbers)
            {
                tempSb.AppendFormat(@"{{\cf{1} {0}}}\tab", currentLine + 1, lineNumberColor);
            }
            foreach (var p in r)
            {
                var c = r.TextBox[p.Line][p.Char];
                if (c.Style != currentStyleId)
                {
                    Flush(sb, tempSb, currentStyleId);
                    currentStyleId = c.Style;
                    styles[currentStyleId] = null;
                }
                if (p.Line != currentLine)
                {
                    for (var i = currentLine; i < p.Line; i++)
                    {
                        tempSb.AppendLine(@"\line");
                        if (IncludeLineNumbers)
                        {
                            tempSb.AppendFormat(@"{{\cf{1} {0}}}\tab", i + 2, lineNumberColor);
                        }
                    }
                    currentLine = p.Line;
                }
                switch (c.C)
                {
                    case '\\':
                        tempSb.Append(@"\\");
                        break;

                    case '{':
                        tempSb.Append(@"\{");
                        break;

                    case '}':
                        tempSb.Append(@"\}");
                        break;

                    default:
                        var ch = c.C;
                        var code = (int)ch;
                        if(code < 128)
                        {
                            tempSb.Append(c.C);
                        }
                        else
                        {
                            tempSb.AppendFormat(@"{{\u{0}}}", code);
                        }
                        break;
                }
            }
            Flush(sb, tempSb, currentStyleId);           
            /* Build color table */
            var list = new SortedList<int, Color>();
            foreach (var pair in _colorTable)
            {
                list.Add(pair.Value, pair.Key);
            }
            tempSb.Length = 0;
            tempSb.AppendFormat(@"{{\colortbl;");
            foreach (var pair in list)
            {
                tempSb.Append(GetColorAsString(pair.Value)+";");
            }
            tempSb.AppendLine("}");
            if (UseOriginalFont)
            {
                sb.Insert(0, string.Format(@"{{\fonttbl{{\f0\fmodern {0};}}}}{{\fs{1} ",
                                TextBox.Font.Name, (int)(2 * TextBox.Font.SizeInPoints)));
                sb.AppendLine(@"}");
            }
            sb.Insert(0, tempSb.ToString());
            sb.Insert(0, @"{\rtf1\ud\deff0");
            sb.AppendLine(@"}");
            return sb.ToString();
        }

        private RtfStyleDescriptor GetRtfDescriptor(StyleIndex styleIndex)
        {
            var styles = new List<Style>();
            /* Find text renderer */
            TextStyle textStyle = null;
            var mask = 1;
            var hasTextStyle = false;
            foreach (var t in TextBox.Styles)
            {
                if (t != null && ((int)styleIndex & mask) != 0)
                {
                    if (t.IsExportable)
                    {
                        var style = t;
                        styles.Add(style);
                        var isTextStyle = style is TextStyle;
                        if (isTextStyle)
                        {
                            if (!hasTextStyle || TextBox.AllowSeveralTextStyleDrawing)
                            {
                                hasTextStyle = true;
                                textStyle = style as TextStyle;
                            }
                        }
                    }
                }
                mask = mask << 1;
            }
            /* Add TextStyle css */
            var result = !hasTextStyle ? TextBox.DefaultStyle.GetRtf() : textStyle.GetRtf();
            return result;
        }

        public static string GetColorAsString(Color color)
        {
            return color == Color.Transparent ? "" : string.Format(@"\red{0}\green{1}\blue{2}", color.R, color.G, color.B);
        }

        private void Flush(StringBuilder sb, StringBuilder tempSb, StyleIndex currentStyle)
        {
            /* Find textRenderer */
            if (tempSb.Length == 0)
            {
                return;
            }
            var desc = GetRtfDescriptor(currentStyle);
            var cf = GetColorTableNumber(desc.ForeColor);
            var cb = GetColorTableNumber(desc.BackColor);
            var tags = new StringBuilder();
            if (cf >= 0)
            {
                tags.AppendFormat(@"\cf{0}", cf);
            }
            if (cb >= 0)
            {
                tags.AppendFormat(@"\highlight{0}", cb);
            }
            if(!string.IsNullOrEmpty(desc.AdditionalTags))
            {
                tags.Append(desc.AdditionalTags.Trim());
            }
            if(tags.Length > 0)
            {
                sb.AppendFormat(@"{{{0} {1}}}", tags, tempSb);
            }
            else
            {
                sb.Append(tempSb.ToString());
            }
            tempSb.Length = 0;
        }

        private int GetColorTableNumber(Color color)
        {
            if (color.A == 0)
            {
                return -1;
            }
            if (!_colorTable.ContainsKey(color))
            {
                _colorTable[color] = _colorTable.Count + 1;
            }
            return _colorTable[color];
        }
    }
}
