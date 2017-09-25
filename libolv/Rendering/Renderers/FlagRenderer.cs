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
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using libolv.Implementation;

namespace libolv.Rendering.Renderers
{
    public class FlagRenderer : BaseRenderer
    {
        private readonly List<Int32> _keysInOrder = new List<Int32>();
        private readonly Dictionary<Int32, Object> _imageMap = new Dictionary<Int32, object>();

        public void Add(Object key, Object imageSelector)
        {
            var k2 = ((IConvertible)key).ToInt32(NumberFormatInfo.InvariantInfo);
            _imageMap[k2] = imageSelector;
            _keysInOrder.Remove(k2);
            _keysInOrder.Add(k2);
        }

        public override void Render(Graphics g, Rectangle r)
        {
            DrawBackground(g, r);
            var convertable = Aspect as IConvertible;
            if (convertable == null)
            {
                return;
            }
            r = ApplyCellPadding(r);
            var v2 = convertable.ToInt32(NumberFormatInfo.InvariantInfo);
            var images = new ArrayList();
            foreach (var image in _keysInOrder.Where(key => (v2 & key) == key).Select(key => GetImage(_imageMap[key])).Where(image => image != null))
            {
                images.Add(image);
            }
            if (images.Count > 0)
            {
                DrawImages(g, r, images);
            }
        }

        protected override void HandleHitTest(Graphics g, OlvListViewHitTestInfo hti, int x, int y)
        {
            var convertable = Aspect as IConvertible;
            if (convertable == null)
            {
                return;
            }
            var v2 = convertable.ToInt32(NumberFormatInfo.InvariantInfo);
            var pt = Bounds.Location;
            foreach (var key in _keysInOrder)
            {
                if ((v2 & key) != key) { continue; }
                var image = GetImage(_imageMap[key]);
                if (image == null) { continue; }
                var imageRect = new Rectangle(pt, image.Size);
                if (imageRect.Contains(x, y))
                {
                    hti.UserData = key;
                    return;
                }
                pt.X += (image.Width + Spacing);
            }
        }
    }
}
