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
using System.Windows.Forms;
using libolv.Implementation;

namespace libolv.Rendering.Decoration
{
    public class TintedColumnDecoration : AbstractDecoration
    {
        private Color _tint;
        private SolidBrush _tintBrush;

        public TintedColumnDecoration()
        {
            Tint = Color.FromArgb(15, Color.Blue);
        }

        public TintedColumnDecoration(OlvColumn column)
            : this()
        {
            ColumnToTint = column;
        }

        /*  Properties */
        public OlvColumn ColumnToTint { get; set; }

        public Color Tint
        {
            get { return _tint; }
            set
            {
                if (_tint == value)
                {
                    return;
                }
                if (_tintBrush != null)
                {
                    _tintBrush.Dispose();
                    _tintBrush = null;
                }
                _tint = value;
                _tintBrush = new SolidBrush(_tint);
            }
        }

        /* IOverLay members */
        public override void Draw(ObjectListView olv, Graphics g, Rectangle r)
        {
            if (olv.View != View.Details || olv.GetItemCount() == 0)
            {
                return;
            }
            var column = ColumnToTint ?? olv.SelectedColumn;
            if (column == null)
            {
                return;
            }
            var sides = NativeMethods.GetScrolledColumnSides(olv, column.Index);
            if (sides.X == -1)
            {
                return;
            }
            var columnBounds = new Rectangle(sides.X, r.Top, sides.Y - sides.X, r.Bottom);
            /* Find the bottom of the last item. The tinting should extend only to there. */
            var lastItem = olv.GetLastItemInDisplayOrder();
            if (lastItem != null)
            {
                var lastItemBounds = lastItem.Bounds;
                if (!lastItemBounds.IsEmpty && lastItemBounds.Bottom < columnBounds.Bottom)
                {
                    columnBounds.Height = lastItemBounds.Bottom - columnBounds.Top;
                }
            }
            g.FillRectangle(_tintBrush, columnBounds);
        }
    }
}
