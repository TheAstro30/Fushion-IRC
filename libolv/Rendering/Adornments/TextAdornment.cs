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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace libolv.Rendering.Adornments
{
    public class TextAdornment : GraphicAdornment
    {
        private Color _backColor = Color.Empty;
        private Color _borderColor = Color.Empty;
        private float _cornerRounding = 16.0f;
        private StringFormat _stringFormat;
        private Color _textColor = Color.DarkBlue;
        private bool _wrap = true;
        private int _workingTransparency;

        [Category("ObjectListView"), Description("The background color of the text"), DefaultValue(typeof(Color), "")]
        public Color BackColor
        {
            get { return _backColor; }
            set { _backColor = value; }
        }

        [Browsable(false)]
        public Brush BackgroundBrush
        {
            get { return new SolidBrush(Color.FromArgb(_workingTransparency, BackColor)); }
        }

        [Category("ObjectListView"), Description("The color of the border around the text"), DefaultValue(typeof(Color), "")]
        public Color BorderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; }
        }

        [Browsable(false)]
        public Pen BorderPen
        {
            get { return new Pen(Color.FromArgb(_workingTransparency, BorderColor), BorderWidth); }
        }

        [Category("ObjectListView"), Description("The width of the border around the text"), DefaultValue(0.0f)]
        public float BorderWidth { get; set; }

        [Category("ObjectListView"), Description("How rounded should the corners of the border be? 0 means no rounding."), DefaultValue(16.0f), NotifyParentProperty(true)]
        public float CornerRounding
        {
            get { return _cornerRounding; }
            set { _cornerRounding = value; }
        }

        [Category("ObjectListView"), Description("The font that will be used to draw the text"), DefaultValue(null), NotifyParentProperty(true)]
        public Font Font { get; set; }

        [Browsable(false)]
        public Font FontOrDefault
        {
            get { return Font ?? new Font("Tahoma", 16); }
        }

        [Browsable(false)]
        public bool HasBackground
        {
            get { return BackColor != Color.Empty; }
        }

        [Browsable(false)]
        public bool HasBorder
        {
            get { return BorderColor != Color.Empty && BorderWidth > 0; }
        }

        [Category("ObjectListView"), Description("The maximum width the text (0 means no maximum). Text longer than this will wrap"), DefaultValue(0)]
        public int MaximumTextWidth { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual StringFormat StringFormat
        {
            get
            {
                if (_stringFormat == null)
                {
                    _stringFormat = new StringFormat
                                       {
                                           Alignment = StringAlignment.Center,
                                           LineAlignment = StringAlignment.Center,
                                           Trimming = StringTrimming.EllipsisCharacter
                                       };
                    if (!Wrap)
                    {
                        _stringFormat.FormatFlags = StringFormatFlags.NoWrap;
                    }
                }
                return _stringFormat;
            }
            set { _stringFormat = value; }
        }

        [Category("ObjectListView"), Description("The text that will be drawn over the top of the ListView"), DefaultValue(null), NotifyParentProperty(true), Localizable(true)]
        public string Text { get; set; }

        [Browsable(false)]
        public Brush TextBrush
        {
            get { return new SolidBrush(Color.FromArgb(_workingTransparency, TextColor)); }
        }

        [Category("ObjectListView"), Description("The color of the text"), DefaultValue(typeof(Color), "DarkBlue"), NotifyParentProperty(true)]
        public Color TextColor
        {
            get { return _textColor; }
            set { _textColor = value; }
        }

        [Category("ObjectListView"), Description("Will the text wrap?"), DefaultValue(true)]
        public bool Wrap
        {
            get { return _wrap; }
            set { _wrap = value; }
        }
        
        /* Implementation */
        public virtual void DrawText(Graphics g, Rectangle r)
        {
            DrawText(g, r, Text, Transparency);
        }

        public virtual void DrawText(Graphics g, Rectangle r, string s, int transparency)
        {
            if (string.IsNullOrEmpty(s))
            {
                return;
            }
            var textRect = CalculateTextBounds(g, r, s);
            DrawBorderedText(g, textRect, s, transparency);
        }

        protected virtual void DrawBorderedText(Graphics g, Rectangle textRect, string text, int transparency)
        {
            var borderRect = textRect;
            borderRect.Inflate((int)BorderWidth / 2, (int)BorderWidth / 2);
            borderRect.Y -= 1; /* Look better a little higher */
            try
            {
                ApplyRotation(g, textRect);
                using (var path = GetRoundedRect(borderRect, CornerRounding))
                {
                    _workingTransparency = transparency;
                    if (HasBackground)
                    {
                        using (var b = BackgroundBrush)
                        {
                            g.FillPath(b, path);
                        }
                    }
                    using (var b = TextBrush)
                    {
                        g.DrawString(text, FontOrDefault, b, textRect, StringFormat);
                    }
                    if (!HasBorder)
                    {
                        UnapplyRotation(g);
                        return;
                    }
                    using (var p = BorderPen)
                    {
                        g.DrawPath(p, path);
                    }
                }
            }
            finally
            {
                UnapplyRotation(g);
            }
        }

        protected virtual Rectangle CalculateTextBounds(Graphics g, Rectangle r, string s)
        {
            var maxWidth = MaximumTextWidth <= 0 ? r.Width : MaximumTextWidth;
            var sizeF = g.MeasureString(s, FontOrDefault, maxWidth, StringFormat);
            var size = new Size(1 + (int)sizeF.Width, 1 + (int)sizeF.Height);
            return CreateAlignedRectangle(r, size);
        }

        protected virtual GraphicsPath GetRoundedRect(Rectangle rect, float diameter)
        {
            var path = new GraphicsPath();
            if (diameter > 0)
            {
                var arc = new RectangleF(rect.X, rect.Y, diameter, diameter);
                path.AddArc(arc, 180, 90);
                arc.X = rect.Right - diameter;
                path.AddArc(arc, 270, 90);
                arc.Y = rect.Bottom - diameter;
                path.AddArc(arc, 0, 90);
                arc.X = rect.Left;
                path.AddArc(arc, 90, 90);
                path.CloseFigure();
            }
            else
            {
                path.AddRectangle(rect);
            }
            return path;
        }        
    }
}
