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
using System.Drawing;

namespace libolv.Rendering.Overlays
{
    public class BillboardOverlay : TextOverlay
    {
        public BillboardOverlay()
        {
            Transparency = 255;
            BackColor = Color.PeachPuff;
            TextColor = Color.Black;
            BorderColor = Color.Empty;
            Font = new Font("Tahoma", 10);
        }

        public Point Location { get; set; }

        public override void Draw(ObjectListView olv, Graphics g, Rectangle r)
        {
            if (string.IsNullOrEmpty(Text))
            {
                return;
            }
            /* Calculate the bounds of the text, and then move it to where it should be */
            var textRect = CalculateTextBounds(g, r, Text);
            textRect.Location = Location;
            /* Make sure the billboard is within the bounds of the List, as far as is possible */
            if (textRect.Right > r.Width)
            {
                textRect.X = Math.Max(r.Left, r.Width - textRect.Width);
            }
            if (textRect.Bottom > r.Height)
            {
                textRect.Y = Math.Max(r.Top, r.Height - textRect.Height);
            }
            DrawBorderedText(g, textRect, Text, 255);
        }
    }
}
