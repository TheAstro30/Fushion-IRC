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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using libolv.Filtering.TextMatch;
using libolv.Implementation;

namespace libolv.Rendering.Renderers
{
    public class HighlightTextRenderer : BaseRenderer
    {
        private float _cornerRoundness = 3.0f;
        private bool _useRoundedRectangle = true;

        public HighlightTextRenderer()
        {
            FramePen = Pens.DarkGreen;
            FillBrush = Brushes.Yellow;
        }

        public HighlightTextRenderer(TextMatchFilter filter) : this()
        {
            Filter = filter;
        }

        /* Configuration properties */
        [Category("Appearance"), DefaultValue(3.0f), Description("How rounded will be the corners of the text match frame?")]
        public float CornerRoundness
        {
            get { return _cornerRoundness; }
            set { _cornerRoundness = value; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Brush FillBrush { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TextMatchFilter Filter { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Pen FramePen { get; set; }

        [Category("Appearance"), DefaultValue(true), Description("Will the frame around a text match will have rounded corners?")]
        public bool UseRoundedRectangle
        {
            get { return _useRoundedRectangle; }
            set { _useRoundedRectangle = value; }
        }

        /* IRenderer interface overrides */
        protected override Rectangle HandleGetEditRectangle(Graphics g, Rectangle cellBounds, OlvListItem item, int subItemIndex, Size preferredSize)
        {
            return StandardGetEditRectangle(g, cellBounds, preferredSize);
        }

        /* Rendering */
        protected override void DrawTextGdi(Graphics g, Rectangle r, string txt)
        {
            if (ShouldDrawHighlighting)
            {
                DrawGdiTextHighlighting(g, r, txt);
            }
            base.DrawTextGdi(g, r, txt);
        }

        protected virtual void DrawGdiTextHighlighting(Graphics g, Rectangle r, string txt)
        {
            const TextFormatFlags flags = TextFormatFlags.NoPrefix |
                                          TextFormatFlags.VerticalCenter |
                                          TextFormatFlags.PreserveGraphicsTranslateTransform;
            /* TextRenderer puts horizontal padding around the strings, so we need to take
             * that into account when measuring strings */
            const int paddingAdjustment = 6;
            /* Cache the font */
            var f = Font;
            foreach (var range in Filter.FindAllMatchedRanges(txt))
            {
                /* Measure the text that comes before our substring */
                var precedingTextSize = Size.Empty;
                if (range.First > 0)
                {
                    var precedingText = txt.Substring(0, range.First);
                    precedingTextSize = TextRenderer.MeasureText(g, precedingText, f, r.Size, flags);
                    precedingTextSize.Width -= paddingAdjustment;
                }
                /* Measure the length of our substring (may be different each time due to case differences) */
                var highlightText = txt.Substring(range.First, range.Length);
                var textToHighlightSize = TextRenderer.MeasureText(g, highlightText, f, r.Size, flags);
                textToHighlightSize.Width -= paddingAdjustment;
                float textToHighlightLeft = r.X + precedingTextSize.Width + 1;
                float textToHighlightTop = AlignVertically(r, textToHighlightSize.Height);
                /* Draw a filled frame around our substring */
                DrawSubstringFrame(g, textToHighlightLeft, textToHighlightTop, textToHighlightSize.Width,
                                   textToHighlightSize.Height);
            }
        }

        protected virtual void DrawSubstringFrame(Graphics g, float x, float y, float width, float height)
        {
            if (UseRoundedRectangle)
            {
                using (var path = GetRoundedRect(x, y, width, height, 3.0f))
                {
                    if (FillBrush != null)
                    {
                        g.FillPath(FillBrush, path);
                    }
                    if (FramePen != null)
                    {
                        g.DrawPath(FramePen, path);
                    }
                }
            }
            else
            {
                if (FillBrush != null)
                {
                    g.FillRectangle(FillBrush, x, y, width, height);
                }
                if (FramePen != null)
                {
                    g.DrawRectangle(FramePen, x, y, width, height);
                }
            }
        }

        protected override void DrawTextGdiPlus(Graphics g, Rectangle r, string txt)
        {
            if (ShouldDrawHighlighting)
            {
                DrawGdiPlusTextHighlighting(g, r, txt);
            }
            base.DrawTextGdiPlus(g, r, txt);
        }

        protected virtual void DrawGdiPlusTextHighlighting(Graphics g, Rectangle r, string txt)
        {
            /* Find the substrings we want to highlight */
            var ranges = new List<CharacterRange>(Filter.FindAllMatchedRanges(txt));
            if (ranges.Count == 0)
            {
                return;
            }
            using (var fmt = StringFormatForGdiPlus)
            {
                RectangleF rf = r;
                fmt.SetMeasurableCharacterRanges(ranges.ToArray());
                var stringRegions = g.MeasureCharacterRanges(txt, Font, rf, fmt);
                foreach (var bounds in stringRegions.Select(region => region.GetBounds(g)))
                {
                    DrawSubstringFrame(g, bounds.X - 1, bounds.Y - 1, bounds.Width + 2, bounds.Height);
                }
            }
        }

        /* Utilities */
        protected bool ShouldDrawHighlighting
        {
            get
            {
                return Column == null || (Column.Searchable && Filter != null && Filter.HasComponents);
            }
        }

        protected GraphicsPath GetRoundedRect(float x, float y, float width, float height, float diameter)
        {
            return GetRoundedRect(new RectangleF(x, y, width, height), diameter);
        }

        protected GraphicsPath GetRoundedRect(RectangleF rect, float diameter)
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
