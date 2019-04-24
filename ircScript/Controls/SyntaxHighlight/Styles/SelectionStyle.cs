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
using System.Drawing.Drawing2D;

namespace ircScript.Controls.SyntaxHighlight.Styles
{
    public class SelectionStyle : Style
    {
        public SelectionStyle(Brush backgroundBrush, Brush foregroundBrush = null)
        {
            BackgroundBrush = backgroundBrush;
            ForegroundBrush = foregroundBrush;
        }

        public Brush BackgroundBrush { get; set; }
        public Brush ForegroundBrush { get; private set; }

        public override bool IsExportable
        {
            get { return false; }
            set
            {
                /* Empty */
            }
        }

        public override void Draw(Graphics gr, Point position, Range range)
        {
            /* Draw background */
            if (BackgroundBrush == null)
            {
                return;
            }
            gr.SmoothingMode = SmoothingMode.None;
            var rect = new Rectangle(position.X, position.Y,
                                     (range.End.Char - range.Start.Char)*range.tb.CharWidth, range.tb.CharHeight);
            if (rect.Width == 0)
            {
                return;
            }
            gr.FillRectangle(BackgroundBrush, rect);
            if (ForegroundBrush == null)
            {
                return;
            }
            /* Draw text */
            gr.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Range(range.tb, range.Start.Char, range.Start.Line,
                              Math.Min(range.tb[range.End.Line].Count, range.End.Char), range.End.Line);
            using (var style = new TextStyle(ForegroundBrush, null, FontStyle.Regular))
            {
                style.Draw(gr, new Point(position.X, position.Y - 1), r);
            }
        }
    }
}