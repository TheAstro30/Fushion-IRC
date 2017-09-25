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
    public class CellEventArgs : EventArgs
    {
        private int _rowIndex = -1;
        private int _columnIndex = -1;

        public ObjectListView ListView { get; internal set; }
        public object Model { get; internal set; }

        public int RowIndex
        {
            get { return _rowIndex; }
            internal set { _rowIndex = value; }
        }
        
        public int ColumnIndex
        {
            get { return _columnIndex; }
            internal set { _columnIndex = value; }
        }

        public OlvColumn Column { get; internal set; }
        public Point Location { get; internal set; }
        public Keys ModifierKeys { get; internal set; }
        public OlvListItem Item { get; internal set; }
        public OlvListSubItem SubItem { get; internal set; }
        public OlvListViewHitTestInfo HitTest { get; internal set; }

        public bool Handled;
    }
}
