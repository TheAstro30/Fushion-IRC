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
using libolv.Rendering.Adornments;

namespace libolv.Rendering.Overlays
{
    [TypeConverter("libolv.OverlayConverter")]
    public class TextOverlay : TextAdornment, ITransparentOverlay
    {
        private int _insetX = 20;
        private int _insetY = 20;

        public TextOverlay()
        {
            Alignment = ContentAlignment.BottomRight;
        }

        /* Public properties */
        [Category("ObjectListView"), Description("The horizontal inset by which the position of the overlay will be adjusted"), DefaultValue(20), NotifyParentProperty(true)]
        public int InsetX
        {
            get { return _insetX; }
            set { _insetX = Math.Max(0, value); }
        }

        [Category("ObjectListView"), Description("Gets or sets the vertical inset by which the position of the overlay will be adjusted"), DefaultValue(20), NotifyParentProperty(true)]
        public int InsetY
        {
            get { return _insetY; }
            set { _insetY = Math.Max(0, value); }
        }

        [Browsable(false), Obsolete("Use CornerRounding instead", false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RoundCorneredBorder
        {
            get { return CornerRounding > 0; }
            set { CornerRounding = value ? 16.0f : 0.0f; }
        }

        /* Commands */
        public virtual void Draw(ObjectListView olv, Graphics g, Rectangle r)
        {
            if (string.IsNullOrEmpty(Text))
            {
                return;
            }
            var insetRect = r;
            insetRect.Inflate(-InsetX, -InsetY);
            /* We hard code a transparency of 255 here since transparency is handled by the glass panel */
            DrawText(g, insetRect, Text, 255);
        }
    }
}
