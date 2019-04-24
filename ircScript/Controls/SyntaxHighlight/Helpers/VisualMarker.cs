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
using System.Windows.Forms;
using ircScript.Controls.SyntaxHighlight.Styles;

namespace ircScript.Controls.SyntaxHighlight.Helpers
{
    public class VisualMarker
    {
        public readonly Rectangle Rectangle;

        public VisualMarker(Rectangle rectangle)
        {
            Rectangle = rectangle;
        }

        public virtual Cursor Cursor
        {
            get { return Cursors.Hand; }
        }

        public virtual void Draw(Graphics gr, Pen pen)
        {
            /* Not implemented on base class */
        }
    }

    public class CollapseFoldingMarker : VisualMarker
    {
        public readonly int Line;

        public CollapseFoldingMarker(int line, Rectangle rectangle) : base(rectangle)
        {
            Line = line;
        }

        public void Draw(Graphics gr, Pen pen, Brush backgroundBrush, Pen forePen)
        {
            /* Draw minus */
            gr.FillRectangle(backgroundBrush, Rectangle);
            gr.DrawRectangle(pen, Rectangle);
            gr.DrawLine(forePen, Rectangle.Left + 2, Rectangle.Top + Rectangle.Height/2, Rectangle.Right - 2,
                        Rectangle.Top + Rectangle.Height/2);
        }
    }

    public class ExpandFoldingMarker : VisualMarker
    {
        public readonly int Line;

        public ExpandFoldingMarker(int line, Rectangle rectangle) : base(rectangle)
        {
            Line = line;
        }

        public void Draw(Graphics gr, Pen pen, Brush backgroundBrush, Pen forePen)
        {
            /* Draw plus */
            gr.FillRectangle(backgroundBrush, Rectangle);
            gr.DrawRectangle(pen, Rectangle);
            gr.DrawLine(forePen, Rectangle.Left + 2, Rectangle.Top + Rectangle.Height/2, Rectangle.Right - 2,
                        Rectangle.Top + Rectangle.Height/2);
            gr.DrawLine(forePen, Rectangle.Left + Rectangle.Width/2, Rectangle.Top + 2,
                        Rectangle.Left + Rectangle.Width/2, Rectangle.Bottom - 2);
        }
    }

    public class FoldedAreaMarker : VisualMarker
    {
        public readonly int Line;

        public FoldedAreaMarker(int line, Rectangle rectangle) : base(rectangle)
        {
            Line = line;
        }

        public override void Draw(Graphics gr, Pen pen)
        {
            gr.DrawRectangle(pen, Rectangle);
        }
    }

    public class StyleVisualMarker : VisualMarker
    {
        public StyleVisualMarker(Rectangle rectangle, Style style) : base(rectangle)
        {
            Style = style;
        }

        public Style Style { get; private set; }
    }

    public class VisualMarkerEventArgs : MouseEventArgs
    {
        public VisualMarkerEventArgs(Style style, StyleVisualMarker marker, MouseEventArgs args)
            : base(args.Button, args.Clicks, args.X, args.Y, args.Delta)
        {
            Style = style;
            Marker = marker;
        }

        public Style Style { get; private set; }
        public StyleVisualMarker Marker { get; private set; }
    }
}