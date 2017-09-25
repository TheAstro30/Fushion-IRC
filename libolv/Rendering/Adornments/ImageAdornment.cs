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
using System.Drawing.Imaging;

namespace libolv.Rendering.Adornments
{
    public class ImageAdornment : GraphicAdornment
    {
        [Category("ObjectListView"), Description("The image that will be drawn"), DefaultValue(null), NotifyParentProperty(true)]
        public Image Image { get; set; }

        [Category("ObjectListView"), Description("Will the image be shrunk to fit within its width?"), DefaultValue(false)]
        public bool ShrinkToWidth { get; set; }

        /* Commands */
        public virtual void DrawImage(Graphics g, Rectangle r)
        {
            if (ShrinkToWidth)
            {
                DrawScaledImage(g, r, Image, Transparency);
            }
            else
            {
                DrawImage(g, r, Image, Transparency);
            }
        }
        
        public virtual void DrawImage(Graphics g, Rectangle r, Image image, int transparency)
        {
            if (image == null) { return; }
            DrawImage(g, r, image, image.Size, transparency);
        }
        
        public virtual void DrawImage(Graphics g, Rectangle r, Image image, Size sz, int transparency)
        {
            if (image == null)
            {
                return;
            }
            var adornmentBounds = CreateAlignedRectangle(r, sz);
            try
            {
                ApplyRotation(g, adornmentBounds);
                DrawTransparentBitmap(g, adornmentBounds, image, transparency);
            }
            finally
            {
                UnapplyRotation(g);
            }
        }
        
        public virtual void DrawScaledImage(Graphics g, Rectangle r, Image image, int transparency)
        {
            if (image == null)
            {
                return;
            }
            /* If the image is too wide to be drawn in the space provided, proportionally scale it down. Too tall images are not scaled. */
            var size = image.Size;
            if (image.Width > r.Width)
            {
                var scaleRatio = (float)r.Width/image.Width;
                size.Height = (int)(image.Height*scaleRatio);
                size.Width = r.Width - 1;
            }
            DrawImage(g, r, image, size, transparency);
        }

        protected virtual void DrawTransparentBitmap(Graphics g, Rectangle r, Image image, int transparency)
        {
            ImageAttributes imageAttributes = null;
            if (transparency != 255)
            {
                imageAttributes = new ImageAttributes();
                var a = transparency/255.0f;
                float[][] colorMatrixElements =
                    {
                        new float[] {1, 0, 0, 0, 0},
                        new float[] {0, 1, 0, 0, 0},
                        new float[] {0, 0, 1, 0, 0},
                        new[] {0, 0, 0, a, 0},
                        new float[] {0, 0, 0, 0, 1}
                    };
                imageAttributes.SetColorMatrix(new ColorMatrix(colorMatrixElements));
            }
            g.DrawImage(image,
                        r, /* destination rectangle */
                        0, 0, image.Size.Width, image.Size.Height, /* source rectangle */
                        GraphicsUnit.Pixel,
                        imageAttributes);
        }
    }
}
