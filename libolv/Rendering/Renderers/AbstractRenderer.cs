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
using System.Windows.Forms;
using libolv.Implementation;

namespace libolv.Rendering.Renderers
{
    public interface IRenderer
    {
        bool RenderItem(DrawListViewItemEventArgs e, Graphics g, Rectangle itemBounds, Object rowObject);
        bool RenderSubItem(DrawListViewSubItemEventArgs e, Graphics g, Rectangle cellBounds, Object rowObject);
        void HitTest(OlvListViewHitTestInfo hti, int x, int y);
        Rectangle GetEditRectangle(Graphics g, Rectangle cellBounds, OlvListItem item, int subItemIndex,
                                   Size preferredSize);
    }

    [Browsable(true), ToolboxItem(false)]
    public class AbstractRenderer : Component, IRenderer
    {
        /* IRenderer Members */
        public virtual bool RenderItem(DrawListViewItemEventArgs e, Graphics g, Rectangle itemBounds, object rowObject)
        {
            return true;
        }

        public virtual bool RenderSubItem(DrawListViewSubItemEventArgs e, Graphics g, Rectangle cellBounds, object rowObject)
        {
            return false;
        }

        public virtual void HitTest(OlvListViewHitTestInfo hti, int x, int y)
        {
            /* Not impemented */
        }

        public virtual Rectangle GetEditRectangle(Graphics g, Rectangle cellBounds, OlvListItem item, int subItemIndex, Size preferredSize)
        {
            return cellBounds;
        }
    }
}
