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
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using ircScript.Controls.SyntaxHighlight.Styles;

namespace ircScript.Controls.SyntaxHighlight.Export
{
    public class ExportToHtml
    {
        public string LineNumbersCss = "<style type=\"text/css\"> .lineNumber{font-family : monospace; font-size : small; font-style : normal; font-weight : normal; color : Teal; background-color : ThreedFace;} </style>";

        public bool UseNbsp { get; set; }

        public bool UseForwardNbsp { get; set; }

        public bool UseOriginalFont { get; set; }

        public bool UseStyleTag { get; set; }

        public bool UseBr { get; set; }

        public bool IncludeLineNumbers { get; set; }

        public FastColoredTextBox TextBox { get; set; }

        public ExportToHtml()
        {
            UseNbsp = true;
            UseOriginalFont = true;
            UseStyleTag = true;
            UseBr = true;
        }

        public string GetHtml(FastColoredTextBox tb)
        {
            TextBox = tb;
            var sel = new Range(tb);
            sel.SelectAll();
            return GetHtml(sel);
        }
        
        public string GetHtml(Range r)
        {
            TextBox = r.tb;
            var styles = new Dictionary<StyleIndex, object>();
            var sb = new StringBuilder();
            var tempSb = new StringBuilder();
            var currentStyleId = StyleIndex.None;
            r.Normalize();
            var currentLine = r.Start.Line;
            styles[currentStyleId] = null;
            if (UseOriginalFont)
            {
                sb.AppendFormat("<font style=\"font-family: {0}, monospace; font-size: {1}pt; line-height: {2}px;\">",
                                                r.tb.Font.Name, r.tb.Font.SizeInPoints, r.tb.CharHeight);
            }
            if (IncludeLineNumbers)
            {
                tempSb.AppendFormat("<span class=lineNumber>{0}</span>  ", currentLine + 1);
            }
            var hasNonSpace = false;
            foreach (var p in r)
            {
                var c = r.tb[p.Line][p.Char];
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
                        tempSb.Append(UseBr ? "<br>" : "\r\n");
                        if (IncludeLineNumbers)
                        {
                            tempSb.AppendFormat("<span class=lineNumber>{0}</span>  ", i + 2);
                        }
                    }
                    currentLine = p.Line;
                    hasNonSpace = false;
                }
                switch (c.C)
                {
                    case ' ':
                        if ((hasNonSpace || !UseForwardNbsp) && !UseNbsp)
                        {
                            goto default;
                        }
                        tempSb.Append("&nbsp;");
                        break;

                    case '<':
                        tempSb.Append("&lt;");
                        break;

                    case '>':
                        tempSb.Append("&gt;");
                        break;

                    case '&':
                        tempSb.Append("&amp;");
                        break;

                    default:
                        hasNonSpace = true;
                        tempSb.Append(c.C);
                        break;
                }
            }
            Flush(sb, tempSb, currentStyleId);
            if (UseOriginalFont)
            {
                sb.Append("</font>");
            }
            /* Build styles */
            if (UseStyleTag)
            {
                tempSb.Length = 0;
                tempSb.Append("<style type=\"text/css\">");
                foreach (var styleId in styles.Keys)
                {
                    tempSb.AppendFormat(".fctb{0}{{ {1} }}\r\n", GetStyleName(styleId), GetCss(styleId));
                }
                tempSb.Append("</style>");
                sb.Insert(0, tempSb.ToString());
            }
            if (IncludeLineNumbers)
            {
                sb.Insert(0, LineNumbersCss);
            }
            return sb.ToString();
        }

        private string GetCss(StyleIndex styleIndex)
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
            var result = !hasTextStyle ? TextBox.DefaultStyle.GetCss() : textStyle.GetCss();
            /* Add no TextStyle css */
            return styles.Where(style => !(style is TextStyle)).Aggregate(result, (current, style) => current + style.GetCss());
        }

        public static string GetColorAsString(Color color)
        {
            return color==Color.Transparent ? "" : string.Format("#{0:x2}{1:x2}{2:x2}", color.R, color.G, color.B);
        }

        public string GetStyleName(StyleIndex styleIndex)
        {
            return styleIndex.ToString().Replace(" ", "").Replace(",", "");
        }

        private void Flush(StringBuilder sb, StringBuilder tempSb, StyleIndex currentStyle)
        {
            /* Find textRenderer */
            if (tempSb.Length == 0)
            {
                return;
            }
            if (UseStyleTag)
            {
                sb.AppendFormat("<font class=fctb{0}>{1}</font>", GetStyleName(currentStyle), tempSb);
            }
            else
            {
                var css = GetCss(currentStyle);
                if(css!="")
                {
                    sb.AppendFormat("<font style=\"{0}\">", css);
                }
                sb.Append(tempSb.ToString());
                if (css != "")
                {
                    sb.Append("</font>");
                }
            }
            tempSb.Length = 0;
        }
    }
}
