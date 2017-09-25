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
using System.Drawing;

namespace libolv.Rendering.Decoration
{
    public class EditingCellBorderDecoration : BorderDecoration
    {
        private bool _useLightbox;

        public EditingCellBorderDecoration()
        {
            FillBrush = null;
            BorderPen = new Pen(Color.DarkBlue, 2);
            CornerRounding = 8;
            BoundsPadding = new Size(10, 8);

        }

        public EditingCellBorderDecoration(bool useLightBox)
            : this()
        {
            UseLightbox = useLightBox;
        }

        /* Properties */
        public bool UseLightbox
        {
            get { return _useLightbox; }
            set
            {
                if (_useLightbox == value)
                {
                    return;
                }
                _useLightbox = value;
                if (!_useLightbox) { return; }
                if (FillBrush == null)
                {
                    FillBrush = new SolidBrush(Color.FromArgb(64, Color.Black));
                }
            }
        }

        /* Implementation */
        public override void Draw(ObjectListView olv, Graphics g, Rectangle r)
        {
            if (!olv.IsCellEditing)
            {
                return;
            }
            var bounds = olv.CellEditor.Bounds;
            if (bounds.IsEmpty)
            {
                return;
            }
            bounds.Inflate(BoundsPadding);
            var path = GetRoundedRect(bounds, CornerRounding);
            if (FillBrush != null)
            {
                if (UseLightbox)
                {
                    using (var newClip = new Region(r))
                    {
                        newClip.Exclude(path);
                        var originalClip = g.Clip;
                        g.Clip = newClip;
                        g.FillRectangle(FillBrush, r);
                        g.Clip = originalClip;
                    }
                }
                else
                {
                    g.FillPath(FillBrush, path);
                }
            }
            if (BorderPen != null)
            {
                g.DrawPath(BorderPen, path);
            }
        }
    }
}
