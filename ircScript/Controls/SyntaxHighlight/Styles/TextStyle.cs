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
using System;
using System.Drawing;
using ircScript.Controls.SyntaxHighlight.Export;

namespace ircScript.Controls.SyntaxHighlight.Styles
{
    public class TextStyle : Style
    {
        public Brush ForeBrush { get; set; }
        public Brush BackgroundBrush { get; set; }
        public FontStyle FontStyle { get; set; }
        public StringFormat StringFormat { get; set; }

        public TextStyle(Brush foreBrush, Brush backgroundBrush, FontStyle fontStyle)
        {
            ForeBrush = foreBrush;
            BackgroundBrush = backgroundBrush;
            FontStyle = fontStyle;
            StringFormat = new StringFormat(StringFormatFlags.MeasureTrailingSpaces);
        }

        public override void Draw(Graphics gr, Point position, Range range)
        {
            /* Draw background */
            if (BackgroundBrush != null)
            {
                gr.FillRectangle(BackgroundBrush, position.X, position.Y,
                                 (range.End.Char - range.Start.Char)*range.tb.CharWidth, range.tb.CharHeight);
            }
            /* Draw chars */
            using (var f = new Font(range.tb.Font, FontStyle))
            {
                var line = range.tb[range.Start.Line];
                float dx = range.tb.CharWidth;
                float y = position.Y + range.tb.LineInterval/2;
                float x = position.X - range.tb.CharWidth/3;
                if (ForeBrush == null)
                {
                    ForeBrush = new SolidBrush(range.tb.ForeColor);
                }
                if (range.tb.ImeAllowed)
                {
                    /* IME mode */
                    for (var i = range.Start.Char; i < range.End.Char; i++)
                    {
                        var size = FastColoredTextBox.GetCharSize(f, line[i].C);
                        var gs = gr.Save();
                        var k = size.Width > range.tb.CharWidth + 1 ? range.tb.CharWidth/size.Width : 1;
                        gr.TranslateTransform(x, y + (1 - k)*range.tb.CharHeight/2);
                        gr.ScaleTransform(k, (float) Math.Sqrt(k));
                        gr.DrawString(line[i].C.ToString(), f, ForeBrush, 0, 0, StringFormat);
                        gr.Restore(gs);
                        x += dx;
                    }
                }
                else
                {
                    /* Classic mode */
                    for (var i = range.Start.Char; i < range.End.Char; i++)
                    {
                        /* Draw char */
                        gr.DrawString(line[i].C.ToString(), f, ForeBrush, x, y, StringFormat);
                        x += dx;
                    }
                }
            }
        }

        public override string GetCss()
        {
            var result = "";
            if (BackgroundBrush is SolidBrush)
            {
                var s = ExportToHtml.GetColorAsString((BackgroundBrush as SolidBrush).Color);
                if (s != "")
                {
                    result += "background-color:" + s + ";";
                }
            }
            if (ForeBrush is SolidBrush)
            {
                var s = ExportToHtml.GetColorAsString((ForeBrush as SolidBrush).Color);
                if (s != "")
                {
                    result += "color:" + s + ";";
                }
            }
            if ((FontStyle & FontStyle.Bold) != 0)
            {
                result += "font-weight:bold;";
            }
            if ((FontStyle & FontStyle.Italic) != 0)
            {
                result += "font-style:oblique;";
            }
            if ((FontStyle & FontStyle.Strikeout) != 0)
            {
                result += "text-decoration:line-through;";
            }
            if ((FontStyle & FontStyle.Underline) != 0)
            {
                result += "text-decoration:underline;";
            }
            return result;
        }

        public override RtfStyleDescriptor GetRtf()
        {
            var result = new RtfStyleDescriptor();
            if (BackgroundBrush is SolidBrush)
            {
                result.BackColor = (BackgroundBrush as SolidBrush).Color;
            }
            if (ForeBrush is SolidBrush)
            {
                result.ForeColor = (ForeBrush as SolidBrush).Color;
            }
            if ((FontStyle & FontStyle.Bold) != 0)
            {
                result.AdditionalTags += @"\b";
            }
            if ((FontStyle & FontStyle.Italic) != 0)
            {
                result.AdditionalTags += @"\i";
            }
            if ((FontStyle & FontStyle.Strikeout) != 0)
            {
                result.AdditionalTags += @"\strike";
            }
            if ((FontStyle & FontStyle.Underline) != 0)
            {
                result.AdditionalTags += @"\ul";
            }
            return result;
        }
    }
}