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
using System.Drawing;
using System.Drawing.Drawing2D;
using ircScript.Controls.SyntaxHighlight.Helpers;

namespace ircScript.Controls.SyntaxHighlight.Bookmarks
{
    public class Bookmark
    {
        public Bookmark(FastColoredTextBox tb, string name, int lineIndex)
        {
            TextBox = tb;
            Name = name;
            LineIndex = lineIndex;
            Color = tb.BookmarkColor;
        }

        public FastColoredTextBox TextBox { get; private set; }

        public string Name { get; set; }

        public int LineIndex { get; set; }

        public Color Color { get; set; }

        public virtual void DoVisible()
        {
            TextBox.Selection.Start = new Place(0, LineIndex);
            TextBox.DoRangeVisible(TextBox.Selection, true);
            TextBox.Invalidate();
        }

        public virtual void Paint(Graphics gr, Rectangle lineRect)
        {
            var size = TextBox.CharHeight - 1;
            using (
                var brush = new LinearGradientBrush(new Rectangle(0, lineRect.Top, size, size), Color.White, Color, 45))
            {
                gr.FillEllipse(brush, 0, lineRect.Top, size, size);
            }
            using (var pen = new Pen(Color))
            {
                gr.DrawEllipse(pen, 0, lineRect.Top, size, size);
            }
        }
    }
}
