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
using System.Windows.Forms;

namespace libolv.Implementation.Events
{
    public class CellEditEventArgs : EventArgs
    {
        public bool Cancel;
        public Control Control;

        public CellEditEventArgs(OlvColumn column, Control control, Rectangle r, OlvListItem item, int subItemIndex)
        {
            Control = control;
            Column = column;
            CellBounds = r;
            ListViewItem = item;
            RowObject = item.RowObject;
            SubItemIndex = subItemIndex;
            Value = column.GetValue(item.RowObject);
        }

        public OlvColumn Column { get; private set; }
        public object RowObject { get; private set; }
        public OlvListItem ListViewItem { get; private set; }
        public object NewValue { get; set; }
        public int SubItemIndex { get; private set; }
        public object Value { get; private set; }
        public Rectangle CellBounds { get; private set; }
    }
}
