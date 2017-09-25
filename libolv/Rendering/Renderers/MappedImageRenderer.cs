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
using System.Collections;
using System.Drawing;

namespace libolv.Rendering.Renderers
{
    public class MappedImageRenderer : BaseRenderer
    {
        private readonly Hashtable _map; /* Track the association between values and images */
        private Object _nullImage; /* image to be drawn for null values (since null can't be a key) */

        public static MappedImageRenderer Boolean(Object trueImage, Object falseImage)
        {
            return new MappedImageRenderer(true, trueImage, false, falseImage);
        }

        public static MappedImageRenderer TriState(Object trueImage, Object falseImage, Object nullImage)
        {
            return new MappedImageRenderer(new[] { true, trueImage, false, falseImage, null, nullImage });
        }

        public MappedImageRenderer()
        {
            _map = new Hashtable();
        }

        public MappedImageRenderer(Object key, Object image) : this()
        {
            Add(key, image);
        }

        public MappedImageRenderer(Object key1, Object image1, Object key2, Object image2) : this()
        {
            Add(key1, image1);
            Add(key2, image2);
        }

        public MappedImageRenderer(Object[] keysAndImages) : this()
        {
            if ((keysAndImages.GetLength(0) % 2) != 0)
            {
                throw new ArgumentException("Array must have key/image pairs");
            }
            for (var i = 0; i < keysAndImages.GetLength(0); i += 2)
            {
                Add(keysAndImages[i], keysAndImages[i + 1]);
            }
        }
        
        public void Add(Object value, Object image)
        {
            if (value == null)
            {
                _nullImage = image;
            }
            else
            {
                _map[value] = image;
            }
        }

        public override void Render(Graphics g, Rectangle r)
        {
            DrawBackground(g, r);
            r = ApplyCellPadding(r);
            var aspectAsCollection = Aspect as ICollection;
            if (aspectAsCollection == null)
            {
                RenderOne(g, r, Aspect);
            }
            else
            {
                RenderCollection(g, r, aspectAsCollection);
            }
        }

        protected void RenderCollection(Graphics g, Rectangle r, ICollection imageSelectors)
        {
            var images = new ArrayList();
            foreach (var selector in imageSelectors)
            {
                Image image;
                if (selector == null)
                {
                    image = GetImage(_nullImage);
                }
                else if (_map.ContainsKey(selector))
                {
                    image = GetImage(_map[selector]);
                }
                else
                {
                    image = null;
                }
                if (image != null)
                {
                    images.Add(image);
                }
            }
            DrawImages(g, r, images);
        }

        protected void RenderOne(Graphics g, Rectangle r, Object selector)
        {
            Image image = null;
            if (selector == null)
            {
                image = GetImage(_nullImage);
            }
            else if (_map.ContainsKey(selector))
            {
                image = GetImage(_map[selector]);
            }
            if (image != null)
            {
                DrawAlignedImage(g, r, image);
            }
        }
    }
}
