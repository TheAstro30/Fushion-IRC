/* Object List View
 * Copyright (C) 2006-2012 Phillip Piper
 * Refactored by Jason James Newland - 2014
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * If you wish to use this code in a closed source application, please contact phillip_piper@bigfoot.com.
 */
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using libolv.Implementation;

namespace libolv.Rendering.Renderers
{
    public class BarRenderer : BaseRenderer
    {
        private bool _useStandardBar = true;
        private int _padding = 2;
        private Color _backgroundColor = Color.AliceBlue;
        private Color _frameColor = Color.Black;
        private float _frameWidth = 1.0f;
        private Color _fillColor = Color.BlueViolet;
        private Color _startColor = Color.CornflowerBlue;
        private Color _endColor = Color.DarkBlue;
        private Color _progressPercentageColor = Color.Black;
        private int _maximumWidth = 100;
        private int _maximumHeight = 16;
        private double _maximumValue = 100.0;

        private Pen _pen;
        private Brush _brush;
        private Brush _backgroundBrush;

        private readonly StringFormat _sf;

        public BarRenderer()
        {
            _sf = new StringFormat {Alignment = StringAlignment.Center};            
        }

        public BarRenderer(int minimum, int maximum) : this()
        {
            MinimumValue = minimum;
            MaximumValue = maximum;
            _sf = new StringFormat { Alignment = StringAlignment.Center };
        }

        public BarRenderer(Pen pen, Brush brush) : this()
        {
            Pen = pen;
            Brush = brush;
            UseStandardBar = false;
            _sf = new StringFormat { Alignment = StringAlignment.Center };
        }

        public BarRenderer(int minimum, int maximum, Pen pen, Brush brush) : this(minimum, maximum)
        {
            Pen = pen;
            Brush = brush;
            UseStandardBar = false;
            _sf = new StringFormat { Alignment = StringAlignment.Center };
        }

        public BarRenderer(Pen pen, Color start, Color end) : this()
        {
            Pen = pen;
            SetGradient(start, end);
            _sf = new StringFormat { Alignment = StringAlignment.Center };
        }

        public BarRenderer(int minimum, int maximum, Pen pen, Color start, Color end) : this(minimum, maximum)
        {
            Pen = pen;
            SetGradient(start, end);
            _sf = new StringFormat { Alignment = StringAlignment.Center };
        }

        [Category("ObjectListView"), Description("Sets the font for the progress value text"), DefaultValue(true)]
        public new Font Font { get; set; }

        /* Configuration Properties */
        [Category("ObjectListView"), Description("Should this bar be drawn in the system style?"), DefaultValue(true)]
        public bool UseStandardBar
        {
            get { return _useStandardBar; }
            set { _useStandardBar = value; }
        }

        [Category("ObjectListView"), Description("How many pixels in from our cell border will this bar be drawn"),
         DefaultValue(2)]
        public int Padding
        {
            get { return _padding; }
            set { _padding = value; }
        }

        [Category("ObjectListView"), Description("The color of the interior of the bar"),
         DefaultValue(typeof (Color), "AliceBlue")]
        public Color BackgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; }
        }

        [Category("ObjectListView"), Description("What color should the frame of the progress bar be"),
         DefaultValue(typeof (Color), "Black")]
        public Color FrameColor
        {
            get { return _frameColor; }
            set { _frameColor = value; }
        }

        public float FrameWidth
        {
            get { return _frameWidth; }
            set { _frameWidth = value; }
        }

        [Category("ObjectListView"), Description("What color should the 'filled in' part of the progress bar be"),
         DefaultValue(typeof (Color), "BlueViolet")]
        public Color FillColor
        {
            get { return _fillColor; }
            set { _fillColor = value; }
        }

        [Category("ObjectListView"), Description("Use a gradient to fill the progress bar starting with this color"),
         DefaultValue(typeof (Color), "CornflowerBlue")]
        public Color GradientStartColor
        {
            get { return _startColor; }
            set { _startColor = value; }
        }

        [Category("ObjectListView"), Description("Use a gradient to fill the progress bar ending with this color"),
         DefaultValue(typeof (Color), "DarkBlue")]
        public Color GradientEndColor
        {
            get { return _endColor; }
            set { _endColor = value; }
        }

        [Category("ObjectListView"), Description("Shows the percentage text in the center of the bar")]
        public bool ShowProgressPercentage { get; set; }

        [Category("ObjectListView"), Description("Gets or sets the percentage text color"), DefaultValue(typeof(Color), "Black")]
        public Color ProgressPercentageColor
        {
            get { return _progressPercentageColor; }
            set { _progressPercentageColor = value; }
        }

        [Category("Behavior"), Description("The progress bar will never be wider than this"), DefaultValue(100)]
        public int MaximumWidth
        {
            get { return _maximumWidth; }
            set { _maximumWidth = value; }
        }

        [Category("Behavior"), Description("The progress bar will never be taller than this"), DefaultValue(16)]
        public int MaximumHeight
        {
            get { return _maximumHeight; }
            set { _maximumHeight = value; }
        }

        [Category("Behavior"), Description("The minimum data value expected. Values less than this will given an empty bar"), DefaultValue(0.0)]
        public double MinimumValue { get; set; }

        [Category("Behavior"), Description("The maximum value for the range. Values greater than this will give a full bar"), DefaultValue(100.0)]
        public double MaximumValue
        {
            get { return _maximumValue; }
            set { _maximumValue = value; }
        }

        /* Public Properties (non-IDE) */
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Pen Pen
        {
            get { return _pen == null && !FrameColor.IsEmpty ? new Pen(FrameColor, FrameWidth) : _pen; }
            set { _pen = value; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Brush Brush
        {
            get { return _brush == null && !FillColor.IsEmpty ? new SolidBrush(FillColor) : _brush; }
            set { _brush = value; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Brush BackgroundBrush
        {
            get {
                return _backgroundBrush == null && !BackgroundColor.IsEmpty
                           ? new SolidBrush(BackgroundColor)
                           : _backgroundBrush;
            }
            set { _backgroundBrush = value; }
        }

        /* Methods */
        public void SetGradient(Color start, Color end)
        {
            GradientStartColor = start;
            GradientEndColor = end;
        }

        public override void Render(Graphics g, Rectangle r)
        {
            DrawBackground(g, r);
            r = ApplyCellPadding(r);
            var frameRect = Rectangle.Inflate(r, 0 - Padding, 0 - Padding);
            frameRect.Width = Math.Min(frameRect.Width, MaximumWidth);
            frameRect.Height = Math.Min(frameRect.Height, MaximumHeight);
            frameRect = AlignRectangle(r, frameRect);
            /* Convert our aspect to a numeric value */
            var convertable = Aspect as IConvertible;
            if (convertable == null)
            {                
                return;
            }
            var aspectValue = convertable.ToDouble(NumberFormatInfo.InvariantInfo);
            var fillRect = Rectangle.Inflate(frameRect, -1, -1);
            if (aspectValue <= MinimumValue)
            {
                fillRect.Width = 0;
            }
            else if (aspectValue < MaximumValue)
            {
                fillRect.Width = (int)(fillRect.Width*(aspectValue - MinimumValue)/MaximumValue);
            }
            /* MS-themed progress bars don't work when printing */
            if (UseStandardBar && ProgressBarRenderer.IsSupported && !IsPrinting)
            {
                ProgressBarRenderer.DrawHorizontalBar(g, frameRect);
                ProgressBarRenderer.DrawHorizontalChunks(g, fillRect);
            }
            else
            {
                g.FillRectangle(BackgroundBrush, frameRect);
                if (fillRect.Width > 0)
                {
                    /* FillRectangle fills inside the given rectangle, so expand it a little */
                    fillRect.Width++;
                    fillRect.Height++;
                    if (GradientStartColor == Color.Empty)
                    {
                        g.FillRectangle(Brush, fillRect);
                    }
                    else
                    {
                        using (
                            var gradient = new LinearGradientBrush(frameRect, GradientStartColor,
                                                                                   GradientEndColor,
                                                                                   LinearGradientMode.Horizontal))
                        {
                            g.FillRectangle(gradient, fillRect);
                        }
                    }
                }
                if (ShowProgressPercentage)
                {
                    var percent = (aspectValue/MaximumValue)*100;
                    using (var b = new SolidBrush(_progressPercentageColor))
                    {
                        g.DrawString(string.Format("{0}%", percent), Font, b, frameRect, _sf);
                    }
                }
                /* Draw frame rect */
                g.DrawRectangle(Pen, frameRect);
            }
        }

        protected override Rectangle HandleGetEditRectangle(Graphics g, Rectangle cellBounds, OlvListItem item, int subItemIndex, Size preferredSize)
        {
            return CalculatePaddedAlignedBounds(g, cellBounds, preferredSize);
        }
    }
}
