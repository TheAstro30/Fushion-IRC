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
using ircScript.Controls.SyntaxHighlight.Helpers.TextRange;

namespace ircScript.Controls.SyntaxHighlight.Styles
{
    public class FoldedBlockStyle : TextStyle
    {
        public FoldedBlockStyle(Brush foreBrush, Brush backgroundBrush, FontStyle fontStyle) :
            base(foreBrush, backgroundBrush, fontStyle)
        {
            /* Empty */
        }

        public override void Draw(Graphics gr, Point position, Range range)
        {
            if (range.End.Char > range.Start.Char)
            {
                base.Draw(gr, position, range);
                var firstNonSpaceSymbolX = position.X;
                /* Find first non space symbol */
                for (var i = range.Start.Char; i < range.End.Char; i++)
                {
                    if (range.TextBox[range.Start.Line][i].C != ' ')
                    {
                        break;
                    }
                    firstNonSpaceSymbolX += range.TextBox.CharWidth;
                }
                /* Create marker */
                range.TextBox.AddVisualMarker(new FoldedAreaMarker(range.Start.Line, new Rectangle(firstNonSpaceSymbolX, position.Y, position.X + (range.End.Char - range.Start.Char) * range.TextBox.CharWidth - firstNonSpaceSymbolX, range.TextBox.CharHeight)));
            }
            else
            {
                /* Draw '...' */
                using (var f = new Font(range.TextBox.Font, FontStyle))
                {
                    gr.DrawString("...", f, ForeBrush, range.TextBox.LeftIndent, position.Y - 2);
                }
                /* Create marker */
                range.TextBox.AddVisualMarker(new FoldedAreaMarker(range.Start.Line, new Rectangle(range.TextBox.LeftIndent + 2, position.Y, 2 * range.TextBox.CharHeight, range.TextBox.CharHeight)));
            }
        }
    }
}
