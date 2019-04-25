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
using ircScript.Controls.SyntaxHighlight.Helpers;
using ircScript.Controls.SyntaxHighlight.Helpers.TextRange;

namespace ircScript.Controls.SyntaxHighlight.Controls
{
    public sealed class DocumentMap : Control
    {
        public EventHandler TargetChanged;
        private bool _needRepaint = true;
        private float _scale = 0.3f;
        private bool _scrollbarVisible = true;
        private Place _startPlace = Place.Empty;
        private FastColoredTextBox _target;

        public DocumentMap()
        {
            ForeColor = Color.Maroon;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw, true);
            Application.Idle += ApplicationIdle;
        }

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
                if (value != null)
                {
                    Subscribe(_target);
                }
                OnTargetChanged();
            }
        }

        [Description("Scale")]
        [DefaultValue(0.3f)]
        public new float Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                NeedRepaint();
            }
        }

        [Description("Scrollbar visibility")]
        [DefaultValue(true)]
        public bool ScrollbarVisible
        {
            get { return _scrollbarVisible; }
            set
            {
                _scrollbarVisible = value;
                NeedRepaint();
            }
        }

        private void ApplicationIdle(object sender, EventArgs e)
        {
            if (_needRepaint)
            {
                Invalidate();
            }
        }

        private void OnTargetChanged()
        {
            NeedRepaint();
            if (TargetChanged != null)
            {
                TargetChanged(this, EventArgs.Empty);
            }
        }

        private void UnSubscribe(FastColoredTextBox target)
        {
            target.Scroll -= Target_Scroll;
            target.SelectionChangedDelayed -= Target_SelectionChanged;
            target.VisibleRangeChanged -= Target_VisibleRangeChanged;
        }

        private void Subscribe(FastColoredTextBox target)
        {
            target.Scroll += Target_Scroll;
            target.SelectionChangedDelayed += Target_SelectionChanged;
            target.VisibleRangeChanged += Target_VisibleRangeChanged;
        }

        private void Target_VisibleRangeChanged(object sender, EventArgs e)
        {
            NeedRepaint();
        }

        private void Target_SelectionChanged(object sender, EventArgs e)
        {
            NeedRepaint();
        }

        private void Target_Scroll(object sender, ScrollEventArgs e)
        {
            NeedRepaint();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            NeedRepaint();
        }

        public void NeedRepaint()
        {
            _needRepaint = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_target == null)
            {
                return;
            }
            float zoom = Scale*100/_target.Zoom;
            if (zoom <= float.Epsilon)
            {
                return;
            }
            /* Calc startPlace */
            var r = _target.VisibleRange;
            if (_startPlace.Line > r.Start.Line)
            {
                _startPlace.Line = r.Start.Line;
            }
            else
            {
                var endP = _target.PlaceToPoint(r.End);
                endP.Offset(0, -(int) (ClientSize.Height/zoom) + _target.CharHeight);
                var pp = _target.PointToPlace(endP);
                if (pp.Line > _startPlace.Line)
                {
                    _startPlace.Line = pp.Line;
                }
            }
            _startPlace.Char = 0;
            /* Calc scroll pos */
            var linesCount = _target.Lines.Count;
            var sp1 = (float) r.Start.Line/linesCount;
            var sp2 = (float) r.End.Line/linesCount;
            /* Scale graphics */
            e.Graphics.ScaleTransform(zoom, zoom);
            /* Draw text */
            var size = new SizeF(ClientSize.Width/zoom, ClientSize.Height/zoom);
            _target.DrawText(e.Graphics, _startPlace, size.ToSize());
            /* Draw visible rect */
            var p0 = _target.PlaceToPoint(_startPlace);
            var p1 = _target.PlaceToPoint(r.Start);
            var p2 = _target.PlaceToPoint(r.End);
            var y1 = p1.Y - p0.Y;
            var y2 = p2.Y + _target.CharHeight - p0.Y;
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            using (var brush = new SolidBrush(Color.FromArgb(50, ForeColor)))
            using (var pen = new Pen(brush, 1/zoom))
            {
                var rect = new Rectangle(0, y1, (int) ((ClientSize.Width - 1)/zoom), y2 - y1);
                e.Graphics.FillRectangle(brush, rect);
                e.Graphics.DrawRectangle(pen, rect);
            }
            /* Draw scrollbar */
            if (_scrollbarVisible)
            {
                e.Graphics.ResetTransform();
                e.Graphics.SmoothingMode = SmoothingMode.None;

                using (var brush = new SolidBrush(Color.FromArgb(200, ForeColor)))
                {
                    var rect = new RectangleF(ClientSize.Width - 3, ClientSize.Height*sp1, 2,
                                              ClientSize.Height*(sp2 - sp1));
                    e.Graphics.FillRectangle(brush, rect);
                }
            }
            _needRepaint = false;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Scroll(e.Location);
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Scroll(e.Location);
            }
            base.OnMouseMove(e);
        }

        private void Scroll(Point point)
        {
            if (_target == null)
            {
                return;
            }
            var zoom = Scale*100/_target.Zoom;
            if (zoom <= float.Epsilon)
            {
                return;
            }
            var p0 = _target.PlaceToPoint(_startPlace);
            p0 = new Point(0, p0.Y + (int) (point.Y/zoom));
            var pp = _target.PointToPlace(p0);
            _target.DoRangeVisible(new Range(_target, pp, pp), true);
            BeginInvoke((MethodInvoker) OnScroll);
        }

        private void OnScroll()
        {
            Refresh();
            _target.Refresh();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Application.Idle -= ApplicationIdle;
                if (_target != null)
                {
                    UnSubscribe(_target);
                }
            }
            base.Dispose(disposing);
        }
    }
}