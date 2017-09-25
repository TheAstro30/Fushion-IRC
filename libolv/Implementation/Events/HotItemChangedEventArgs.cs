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

namespace libolv.Implementation.Events
{
    public class HotItemChangedEventArgs : EventArgs
    {
        public bool Handled { get; set; }
        public HitTestLocation HotCellHitLocation { get; internal set; }
        public virtual HitTestLocationEx HotCellHitLocationEx { get; internal set; }
        public int HotColumnIndex { get; internal set; }
        public int HotRowIndex { get; internal set; }
        public OlvGroup HotGroup { get; internal set; }
        public HitTestLocation OldHotCellHitLocation { get; internal set; }
        public virtual HitTestLocationEx OldHotCellHitLocationEx { get; internal set; }
        public int OldHotColumnIndex { get; internal set; }
        public int OldHotRowIndex { get; internal set; }
        public OlvGroup OldHotGroup { get; internal set; }

        public override string ToString()
        {
            return string.Format("NewHotCellHitLocation: {0}, HotCellHitLocationEx: {1}, NewHotColumnIndex: {2}, NewHotRowIndex: {3}, HotGroup: {4}", HotCellHitLocation, HotCellHitLocationEx, HotColumnIndex, HotRowIndex, HotGroup);
        }
    }
}
