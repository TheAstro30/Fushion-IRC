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
using libolv.Implementation;
using libolv.Implementation.Munger;

namespace libolv.Rendering.Renderers
{
    public class DescribedTaskRenderer : BaseRenderer
    {
        private Color _descriptionColor = Color.DimGray;
        private int _imageTextSpace = 4;
        private Munger _descriptionGetter;

        /* Configuration properties */
        [Category("ObjectListView"), Description("The font that will be used to draw the title of the task"), DefaultValue(null)]
        public Font TitleFont { get; set; }

        [Browsable(false)]
        public Font TitleFontOrDefault
        {
            get { return TitleFont ?? ListView.Font; }
        }

        [Category("ObjectListView"), Description("The color of the title"), DefaultValue(typeof(Color), "")]
        public Color TitleColor { get; set; }


        [Browsable(false)]
        public Color TitleColorOrDefault
        {
            get { return IsItemSelected || TitleColor.IsEmpty ? GetForegroundColor() : TitleColor; }
        }

        [Category("ObjectListView"), Description("The font that will be used to draw the description of the task"), DefaultValue(null)]
        public Font DescriptionFont { get; set; }

        [Browsable(false)]
        public Font DescriptionFontOrDefault
        {
            get { return DescriptionFont ?? ListView.Font; }
        }

        [Category("ObjectListView"), Description("The color of the description"), DefaultValue(typeof(Color), "DimGray")]
        public Color DescriptionColor
        {
            get { return _descriptionColor; }
            set { _descriptionColor = value; }
        }

        public Color DescriptionColorOrDefault
        {
            get {
                return DescriptionColor.IsEmpty || (IsItemSelected && !ListView.UseTranslucentSelection)
                           ? GetForegroundColor()
                           : DescriptionColor;
            }
        }

        [Category("ObjectListView"), Description("The number of pixels that that will be left between the image and the text"), DefaultValue(4)]
        public int ImageTextSpace
        {
            get { return _imageTextSpace; }
            set { _imageTextSpace = value; }
        }

        [Category("ObjectListView"), Description("The name of the aspect of the model object that contains the task description"), DefaultValue(null)]
        public string DescriptionAspectName { get; set; }

        /* Calculating */
        protected virtual string GetDescription()
        {
            if (string.IsNullOrEmpty(DescriptionAspectName))
            {
                return string.Empty;
            }
            if (_descriptionGetter == null)
            {
                _descriptionGetter = new Munger(DescriptionAspectName);
            }
            return _descriptionGetter.GetValue(RowObject) as String;
        }

        /* Rendering */
        public override void Render(Graphics g, Rectangle r)
        {
            DrawBackground(g, r);
            r = ApplyCellPadding(r);
            DrawDescribedTask(g, r, Aspect as String, GetDescription(), GetImage());
        }

        protected virtual void DrawDescribedTask(Graphics g, Rectangle r, string title, string description, Image image)
        {
            var cellBounds = ApplyCellPadding(r);
            var textBounds = cellBounds;
            if (image != null)
            {
                g.DrawImage(image, cellBounds.Location);
                var gapToText = image.Width + ImageTextSpace;
                textBounds.X += gapToText;
                textBounds.Width -= gapToText;
            }
            /* Color the background if the row is selected and we're not using a translucent selection */
            if (IsItemSelected && !ListView.UseTranslucentSelection)
            {
                using (var b = new SolidBrush(GetTextBackgroundColor()))
                {
                    g.FillRectangle(b, textBounds);
                }
            }
            /* Draw the title */
            if (!string.IsNullOrEmpty(title))
            {
                using (var fmt = new StringFormat(StringFormatFlags.NoWrap))
                {
                    fmt.Trimming = StringTrimming.EllipsisCharacter;
                    fmt.Alignment = StringAlignment.Near;
                    fmt.LineAlignment = StringAlignment.Near;
                    var f = TitleFontOrDefault;
                    using (var b = new SolidBrush(TitleColorOrDefault))
                    {
                        g.DrawString(title, f, b, textBounds, fmt);
                    }
                    /* How tall was the title? */
                    var size = g.MeasureString(title, f, textBounds.Width, fmt);
                    textBounds.Y += (int)size.Height;
                    textBounds.Height -= (int)size.Height;
                }
            }
            /* Draw the description */
            if (string.IsNullOrEmpty(description)) { return; }
            using (var fmt2 = new StringFormat())
            {
                fmt2.Trimming = StringTrimming.EllipsisCharacter;
                using (var b = new SolidBrush(DescriptionColorOrDefault))
                {
                    g.DrawString(description, DescriptionFontOrDefault, b, textBounds, fmt2);
                }
            }
        }

        /* Hit Testing */
        protected override void HandleHitTest(Graphics g, OlvListViewHitTestInfo hti, int x, int y)
        {
            if (Bounds.Contains(x, y))
            {
                hti.HitTestLocation = HitTestLocation.Text;
            }
        }
    }
}
