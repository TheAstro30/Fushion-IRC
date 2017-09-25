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
using System.Runtime.InteropServices;

namespace libolv.Implementation.VirtualGroups
{
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("44C09D56-8D3B-419D-A462-7B956B105B47")]
    internal interface IOwnerDataCallback
    {
        void GetItemPosition(int i, out NativeMethods.InteropPoint pt);
        void SetItemPosition(int t, NativeMethods.InteropPoint pt);
        void GetItemInGroup(int groupIndex, int n, out int itemIndex);
        void GetItemGroup(int itemIndex, int occurrenceCount, out int groupIndex);
        void GetItemGroupCount(int itemIndex, out int occurrenceCount);
        void OnCacheHint(NativeMethods.Lvitemindex i, NativeMethods.Lvitemindex j);
    }

    [Guid("6FC61F50-80E8-49b4-B200-3F38D3865ABD")]
    internal class OwnerDataCallbackImpl : IOwnerDataCallback
    {
        private readonly VirtualObjectListView _olv;

        public OwnerDataCallbackImpl(VirtualObjectListView olv)
        {
            _olv = olv;
        }

        /* IOwnerDataCallback Members */
        public void GetItemPosition(int i, out NativeMethods.InteropPoint pt)
        {
            throw new NotSupportedException();
        }

        public void SetItemPosition(int t, NativeMethods.InteropPoint pt)
        {
            throw new NotSupportedException();
        }

        public void GetItemInGroup(int groupIndex, int n, out int itemIndex)
        {
            itemIndex = _olv.GroupingStrategy.GetGroupMember(_olv.OlvGroups[groupIndex], n);
        }

        public void GetItemGroup(int itemIndex, int occurrenceCount, out int groupIndex)
        {
            groupIndex = _olv.GroupingStrategy.GetGroup(itemIndex);
        }

        public void GetItemGroupCount(int itemIndex, out int occurrenceCount)
        {
            occurrenceCount = 1;
        }

        public void OnCacheHint(NativeMethods.Lvitemindex from, NativeMethods.Lvitemindex to)
        {
            _olv.GroupingStrategy.CacheHint(from.iGroup, from.iItem, to.iGroup, to.iItem);
        }
    }
}
