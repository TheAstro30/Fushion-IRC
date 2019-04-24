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
using ircScript.Controls.SyntaxHighlight.Helpers;

namespace ircScript.Controls.SyntaxHighlight.Styles
{
    public class ShortcutStyle : Style
    {
        public Pen BorderPen { get; set; }

        public ShortcutStyle(Pen borderPen)
        {
            BorderPen = borderPen;
        }

        public override void Draw(Graphics gr, Point position, Range range)
        {
            /* Get last char coordinates */
            var p = range.tb.PlaceToPoint(range.End);
            /* Draw small square under char */
            var rect = new Rectangle(p.X - 5, p.Y + range.tb.CharHeight - 2, 4, 3);
            gr.FillPath(Brushes.White, GetRoundedRectangle(rect, 1));
            gr.DrawPath(BorderPen, GetRoundedRectangle(rect, 1));
            /* Add visual marker for handle mouse events */
            AddVisualMarker(range.tb, new StyleVisualMarker(new Rectangle(p.X - range.tb.CharWidth, p.Y, range.tb.CharWidth, range.tb.CharHeight), this));
        }
    }
}
