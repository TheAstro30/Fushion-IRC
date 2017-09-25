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
using System.Globalization;
using libolv.Implementation;

namespace libolv.Rendering.Renderers
{
    public class MultiImageRenderer : BaseRenderer
    {
        private Object _imageSelector;
        private int _maxNumberImages = 10;
        private int _maximumValue = 100;

        public MultiImageRenderer()
        {
            MinimumValue = 0;
            /* Empty */
        }

        public MultiImageRenderer(Object imageSelector, int maxImages, int minValue, int maxValue) : this()
        {
            ImageSelector = imageSelector;
            MaxNumberImages = maxImages;
            MinimumValue = minValue;
            MaximumValue = maxValue;
        }

        /* Configuration Properties */
        [Category("Behavior"), Description("The index of the image that should be drawn"), DefaultValue(-1)]
        public int ImageIndex
        {
            get { return _imageSelector is Int32 ? (Int32)_imageSelector : -1; }
            set { _imageSelector = value; }
        }

        [Category("Behavior"), Description("The index of the image that should be drawn"), DefaultValue(null)]
        public string ImageName
        {
            get { return _imageSelector as String; }
            set { _imageSelector = value; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Object ImageSelector
        {
            get { return _imageSelector; }
            set { _imageSelector = value; }
        }

        [Category("Behavior"), Description("The maximum number of images that this renderer should draw"), DefaultValue(10)]
        public int MaxNumberImages
        {
            get { return _maxNumberImages; }
            set { _maxNumberImages = value; }
        }

        [Category("Behavior"), Description("Values less than or equal to this will have 0 images drawn"), DefaultValue(0)]
        public int MinimumValue { get; set; }

        [Category("Behavior"), Description("Values greater than or equal to this will have MaxNumberImages images drawn"), DefaultValue(100)]
        public int MaximumValue
        {
            get { return _maximumValue; }
            set { _maximumValue = value; }
        }

        public override void Render(Graphics g, Rectangle r)
        {
            DrawBackground(g, r);
            r = ApplyCellPadding(r);
            var image = GetImage(ImageSelector);
            if (image == null)
            {
                return;
            }
            /* Convert our aspect to a numeric value */
            var convertable = Aspect as IConvertible;
            if (convertable == null)
            {
                return;
            }
            var aspectValue = convertable.ToDouble(NumberFormatInfo.InvariantInfo);
            /* Calculate how many images we need to draw to represent our aspect value */
            int numberOfImages;
            if (aspectValue <= MinimumValue)
            {
                numberOfImages = 0;
            }
            else if (aspectValue < MaximumValue)
            {
                numberOfImages = 1 + (int)(MaxNumberImages * (aspectValue - MinimumValue) / MaximumValue);
            }
            else
            {
                numberOfImages = MaxNumberImages;
            }
            /* If we need to shrink the image, what will its on-screen dimensions be? */
            var imageScaledWidth = image.Width;
            var imageScaledHeight = image.Height;
            if (r.Height < image.Height)
            {
                imageScaledWidth = (int)((float)image.Width * r.Height / image.Height);
                imageScaledHeight = r.Height;
            }
            /* Calculate where the images should be drawn */
            var imageBounds = r;
            imageBounds.Width = (MaxNumberImages * (imageScaledWidth + Spacing)) - Spacing;
            imageBounds.Height = imageScaledHeight;
            imageBounds = AlignRectangle(r, imageBounds);
            /* Finally, draw the images */
            for (var i = 0; i < numberOfImages; i++)
            {
                g.DrawImage(image, imageBounds.X, imageBounds.Y, imageScaledWidth, imageScaledHeight);
                imageBounds.X += (imageScaledWidth + Spacing);
            }
        }

        protected override Rectangle HandleGetEditRectangle(Graphics g, Rectangle cellBounds, OlvListItem item, int subItemIndex, Size preferredSize)
        {
            return CalculatePaddedAlignedBounds(g, cellBounds, preferredSize);
        }
    }
}
