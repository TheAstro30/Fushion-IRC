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
using libolv.Implementation;

namespace libolv.DragDrop.DropSink
{
    public class OlvDropEventArgs : EventArgs
    {
        private int _dropTargetIndex = -1;
        private int _dropTargetSubItemIndex = -1;

        /* Data Properties */
        public DragEventArgs DragEventArgs { get; internal set; }
        public object DataObject { get; internal set; }
        public SimpleDropSink DropSink { get; internal set; }

        public int DropTargetIndex
        {
            get { return _dropTargetIndex; }
            set { _dropTargetIndex = value; }
        }

        public DropTargetLocation DropTargetLocation { get; set; }

        public int DropTargetSubItemIndex
        {
            get { return _dropTargetSubItemIndex; }
            set { _dropTargetSubItemIndex = value; }
        }

        public OlvListItem DropTargetItem
        {
            get { return ListView.GetItem(DropTargetIndex); }
            set { DropTargetIndex = value == null ? -1 : value.Index; }
        }

        public DragDropEffects Effect { get; set; }
        public bool Handled { get; set; }
        public string InfoMessage { get; set; }
        public ObjectListView ListView { get; internal set; }
        public Point MouseLocation { get; internal set; }

        public DragDropEffects StandardDropActionFromKeys
        {
            get { return DropSink.CalculateStandardDropActionFromKeys(); }
        }
    }
}
