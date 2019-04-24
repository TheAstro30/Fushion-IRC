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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ircScript.Controls.SyntaxHighlight.Controls
{
    public sealed class Ruler : UserControl
    {
        public EventHandler TargetChanged;
        private FastColoredTextBox _target;

        public Ruler()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            MinimumSize = new Size(0, 24);
            MaximumSize = new Size(int.MaxValue/2, 24);

            BackColor2 = SystemColors.ControlLight;
            TickColor = Color.DarkGray;
            CaretTickColor = Color.Black;
        }

        [DefaultValue(typeof (Color), "ControlLight")]
        public Color BackColor2 { get; set; }

        [DefaultValue(typeof (Color), "DarkGray")]
        public Color TickColor { get; set; }

        [DefaultValue(typeof (Color), "Black")]
        public Color CaretTickColor { get; set; }

        [Description("Target FastColoredTextBox")]
        public FastColoredTextBox Target
        {
            get { return _target; }
            set
            {
                if (_target != null)
                {
                    UnSubscribe(_target);
                }
                _target = value;
                Subscribe(_target);
                OnTargetChanged();
            }
        }

        private void OnTargetChanged()
        {
            if (TargetChanged != null)
            {
                TargetChanged(this, EventArgs.Empty);
            }
        }

        private void UnSubscribe(FastColoredTextBox target)
        {
            target.Scroll -= target_Scroll;
            target.SelectionChanged -= target_SelectionChanged;
            target.VisibleRangeChanged -= target_VisibleRangeChanged;
        }

        private void Subscribe(FastColoredTextBox target)
        {
            target.Scroll += target_Scroll;
            target.SelectionChanged += target_SelectionChanged;
            target.VisibleRangeChanged += target_VisibleRangeChanged;
        }

        private void target_VisibleRangeChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void target_SelectionChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void target_Scroll(object sender, ScrollEventArgs e)
        {
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_target == null)
            {
                return;
            }
            var car = PointToClient(_target.PointToScreen(_target.PlaceToPoint(_target.Selection.Start)));
            var fontSize = TextRenderer.MeasureText("W", Font);
            var column = 0;
            e.Graphics.FillRectangle(
                new LinearGradientBrush(new Rectangle(0, 0, Width, Height), BackColor, BackColor2, 270),
                new Rectangle(0, 0, Width, Height));
            float columnWidth = _target.CharWidth;
            var sf = new StringFormat
                         {
                             Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near
                         };

            var zeroPoint = _target.PositionToPoint(0);
            zeroPoint = PointToClient(_target.PointToScreen(zeroPoint));

            using (var pen = new Pen(TickColor))
            {
                using (var textBrush = new SolidBrush(ForeColor))
                {
                    for (float x = zeroPoint.X; x < Right; x += columnWidth, ++column)
                    {
                        if (column%10 == 0)
                        {
                            e.Graphics.DrawString(column.ToString(), Font, textBrush, x, 0f, sf);
                        }
                        e.Graphics.DrawLine(pen, (int) x, fontSize.Height + (column%5 == 0 ? 1 : 3), (int) x, Height - 4);
                    }
                }
            }

            using (var pen = new Pen(TickColor))
            {
                e.Graphics.DrawLine(pen, new Point(car.X - 3, Height - 3), new Point(car.X + 3, Height - 3));
            }

            using (var pen = new Pen(CaretTickColor))
            {
                e.Graphics.DrawLine(pen, new Point(car.X - 2, fontSize.Height + 3), new Point(car.X - 2, Height - 4));
                e.Graphics.DrawLine(pen, new Point(car.X, fontSize.Height + 1), new Point(car.X, Height - 4));
                e.Graphics.DrawLine(pen, new Point(car.X + 2, fontSize.Height + 3), new Point(car.X + 2, Height - 4));
            }
        }
    }
}