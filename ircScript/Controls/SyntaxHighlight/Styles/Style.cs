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
using System;
using System.Drawing.Drawing2D;
using ircScript.Controls.SyntaxHighlight.Export;
using ircScript.Controls.SyntaxHighlight.Helpers;

namespace ircScript.Controls.SyntaxHighlight.Styles
{
    public abstract class Style : IDisposable
    {
        public virtual bool IsExportable { get; set; }

        public event EventHandler<VisualMarkerEventArgs> VisualMarkerClick;

        protected Style()
        {
            Init();
        }

        public abstract void Draw(Graphics gr, Point position, Range range);

        public virtual void OnVisualMarkerClick(FastColoredTextBox tb, VisualMarkerEventArgs args)
        {
            if (VisualMarkerClick != null)
            {
                VisualMarkerClick(tb, args);
            }
        }

        protected virtual void AddVisualMarker(FastColoredTextBox tb, StyleVisualMarker marker)
        {
            tb.AddVisualMarker(marker);
        }

        public static Size GetSizeOfRange(Range range)
        {
            return new Size((range.End.Char - range.Start.Char) * range.tb.CharWidth, range.tb.CharHeight);
        }

        public static GraphicsPath GetRoundedRectangle(Rectangle rect, int d)
        {
            var gp = new GraphicsPath();
            gp.AddArc(rect.X, rect.Y, d, d, 180, 90);
            gp.AddArc(rect.X + rect.Width - d, rect.Y, d, d, 270, 90);
            gp.AddArc(rect.X + rect.Width - d, rect.Y + rect.Height - d, d, d, 0, 90);
            gp.AddArc(rect.X, rect.Y + rect.Height - d, d, d, 90, 90);
            gp.AddLine(rect.X, rect.Y + rect.Height - d, rect.X, rect.Y + d / 2);
            return gp;
        }

        public virtual void Dispose()
        {
            /* IDisposable */
        }

        public virtual string GetCss()
        {
            return "";
        }

        public virtual RtfStyleDescriptor GetRtf()
        {
            return new RtfStyleDescriptor();
        }

        private void Init()
        {
            IsExportable = new bool();
        }
    }
}
