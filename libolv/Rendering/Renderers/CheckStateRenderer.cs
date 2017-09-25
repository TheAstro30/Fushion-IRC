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

namespace libolv.Rendering.Renderers
{
    public class CheckStateRenderer : BaseRenderer
    {
        public override void Render(Graphics g, Rectangle r)
        {
            DrawBackground(g, r);
            if (Column == null)
            {
                return;
            }
            r = ApplyCellPadding(r);
            var state = Column.GetCheckState(RowObject);
            if (IsPrinting)
            {
                /* Renderers don't work onto printer DCs, so we have to draw the image ourselves */
                var key = ObjectListView.CheckedKey;
                if (state == CheckState.Unchecked)
                {
                    key = ObjectListView.UncheckedKey;
                }
                if (state == CheckState.Indeterminate)
                {
                    key = ObjectListView.IndeterminateKey;
                }
                DrawAlignedImage(g, r, ListView.SmallImageList.Images[key]);
            }
            else
            {
                r = CalculateCheckBoxBounds(g, r);
                CheckBoxRenderer.DrawCheckBox(g, r.Location, GetCheckBoxState(state));
            }
        }

        protected override Rectangle HandleGetEditRectangle(Graphics g, Rectangle cellBounds, OlvListItem item, int subItemIndex, Size preferredSize)
        {
            return CalculatePaddedAlignedBounds(g, cellBounds, preferredSize);
        }

        protected override void HandleHitTest(Graphics g, OlvListViewHitTestInfo hti, int x, int y)
        {
            var r = CalculateCheckBoxBounds(g, Bounds);
            if (r.Contains(x, y))
            {
                hti.HitTestLocation = HitTestLocation.CheckBox;
            }
        }
    }
}
