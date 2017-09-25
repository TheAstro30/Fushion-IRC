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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace libolv.Implementation
{
    [Flags]
    public enum GroupState
    {
        LvgsNormal = 0x0,
        LvgsCollapsed = 0x1,
        LvgsHidden = 0x2,
        LvgsNoheader = 0x4,
        LvgsCollapsible = 0x8,
        LvgsFocused = 0x10,
        LvgsSelected = 0x20,
        LvgsSubseted = 0x40,
        LvgsSubsetlinkfocused = 0x80,
        LvgsAll = 0xFFFF
    }

    [Flags]
    public enum GroupMask
    {
        LvgfNone = 0,
        LvgfHeader = 1,
        LvgfFooter = 2,
        LvgfState = 4,
        LvgfAlign = 8,
        LvgfGroupid = 0x10,
        LvgfSubtitle = 0x00100,
        LvgfTask = 0x00200,
        LvgfDescriptiontop = 0x00400,
        LvgfDescriptionbottom = 0x00800,
        LvgfTitleimage = 0x01000,
        LvgfExtendedimage = 0x02000,
        LvgfItems = 0x04000,
        LvgfSubset = 0x08000,
        LvgfSubsetitems = 0x10000
    }

    [Flags]
    public enum GroupMetricsMask
    {
        LvgmfNone = 0,
        LvgmfBordersize = 1,
        LvgmfBordercolor = 2,
        LvgmfTextcolor = 4
    }

    public class OlvGroup
    {
        private static int _nextId;
        private static PropertyInfo _groupIdPropInfo;
        private IList<OlvListItem> _items = new List<OlvListItem>();

        public OlvGroup() : this("Default group header")
        {
            /* Empty */
        }

        public OlvGroup(string header)
        {
            Header = header;
            Id = _nextId++;
            TitleImage = -1;
            ExtendedImage = -1;
        }

        /* Public properties */
        public string BottomDescription { get; set; }

        public bool Collapsed
        {
            get { return GetOneState(GroupState.LvgsCollapsed); }
            set { SetOneState(value, GroupState.LvgsCollapsed); }
        }

        public bool Collapsible
        {
            get { return GetOneState(GroupState.LvgsCollapsible); }
            set { SetOneState(value, GroupState.LvgsCollapsible); }
        }

        public IList Contents { get; set; }

        public bool Created
        {
            get { return ListView != null; }
        }

        public object ExtendedImage { get; set; }
        public string Footer { get; set; }

        public int GroupId
        {
            get
            {
                if (ListViewGroup == null)
                {
                    return Id;
                }
                /* Use reflection to get around the access control on the ID property */
                if (_groupIdPropInfo == null)
                {
                    _groupIdPropInfo = typeof (ListViewGroup).GetProperty("ID",
                                                                         BindingFlags.NonPublic |
                                                                         BindingFlags.Instance);
                    System.Diagnostics.Debug.Assert(_groupIdPropInfo != null);
                }
                var groupId = _groupIdPropInfo.GetValue(ListViewGroup, null) as int?;
                return groupId.HasValue ? groupId.Value : -1;
            }
        }

        public string Header { get; set; }
        public HorizontalAlignment HeaderAlignment { get; set; }
        public int Id { get; set; }

        public IList<OlvListItem> Items
        {
            get { return _items; }
            set { _items = value; }
        }

        public object Key { get; set; }
        public ObjectListView ListView { get; protected set; }
        public string Name { get; set; }

        public bool Focused
        {
            get { return GetOneState(GroupState.LvgsFocused); }
            set { SetOneState(value, GroupState.LvgsFocused); }
        }

        public bool Selected
        {
            get { return GetOneState(GroupState.LvgsSelected); }
            set { SetOneState(value, GroupState.LvgsSelected); }
        }

        public string SubsetTitle { get; set; }
        public string Subtitle { get; set; }
        public IComparable SortValue { get; set; }
        public GroupState State { get; set; }
        public GroupState StateMask { get; set; }

        public bool Subseted
        {
            get { return GetOneState(GroupState.LvgsSubseted); }
            set { SetOneState(value, GroupState.LvgsSubseted); }
        }

        public object Tag { get; set; }
        public string Task { get; set; }
        public object TitleImage { get; set; }
        public string TopDescription { get; set; }
        public int VirtualItemCount { get; set; }

        /* Protected properties */
        protected ListViewGroup ListViewGroup { get; set; }

        /*  Calculations/Conversions */
        public int GetImageIndex(object imageSelector)
        {
            if (imageSelector == null || ListView == null || ListView.GroupImageList == null)
            {
                return -1;
            }
            if (imageSelector is Int32)
            {
                return (int)imageSelector;
            }
            var imageSelectorAsString = imageSelector as String;
            if (imageSelectorAsString != null)
            {
                return ListView.GroupImageList.Images.IndexOfKey(imageSelectorAsString);
            }
            return -1;
        }

        public override string ToString()
        {
            return Header;
        }

        /* Commands */
        public void InsertGroupNewStyle(ObjectListView olv)
        {
            ListView = olv;
            NativeMethods.InsertGroup(olv, AsNativeGroup(true));
        }

        public void InsertGroupOldStyle(ObjectListView olv)
        {
            ListView = olv;
            /* Create/update the associated ListViewGroup */
            if (ListViewGroup == null)
            {
                ListViewGroup = new ListViewGroup();
            }
            ListViewGroup.Header = Header;
            ListViewGroup.HeaderAlignment = HeaderAlignment;
            ListViewGroup.Name = Name;
            /* Remember which OLVGroup created the ListViewGroup */
            ListViewGroup.Tag = this;
            /* Add the group to the control */
            olv.Groups.Add(ListViewGroup);
            /* Add any extra information */
            NativeMethods.SetGroupInfo(olv, GroupId, AsNativeGroup(false));
        }

        public void SetItemsOldStyle()
        {
            var list = Items as List<OlvListItem>;
            if (list == null)
            {
                foreach (var item in Items)
                {
                    ListViewGroup.Items.Add(item);
                }
            }
            else
            {
                foreach (var item in list)
                {
                    ListViewGroup.Items.Add(item);
                }
            }
        }

        /* Implementation */
        internal NativeMethods.Lvgroup2 AsNativeGroup(bool withId)
        {
            var group = new NativeMethods.Lvgroup2();
            group.cbSize = (uint)Marshal.SizeOf(typeof (NativeMethods.Lvgroup2));
            group.mask = (uint)(GroupMask.LvgfHeader ^ GroupMask.LvgfAlign ^ GroupMask.LvgfState);
            group.pszHeader = Header;
            group.uAlign = (uint)HeaderAlignment;
            group.stateMask = (uint)StateMask;
            group.state = (uint)State;
            if (withId)
            {
                group.iGroupId = GroupId;
                group.mask ^= (uint)GroupMask.LvgfGroupid;
            }
            if (!string.IsNullOrEmpty(Footer))
            {
                group.pszFooter = Footer;
                group.mask ^= (uint)GroupMask.LvgfFooter;
            }
            if (!string.IsNullOrEmpty(Subtitle))
            {
                group.pszSubtitle = Subtitle;
                group.mask ^= (uint)GroupMask.LvgfSubtitle;
            }
            if (!string.IsNullOrEmpty(Task))
            {
                group.pszTask = Task;
                group.mask ^= (uint)GroupMask.LvgfTask;
            }
            if (!string.IsNullOrEmpty(TopDescription))
            {
                group.pszDescriptionTop = TopDescription;
                group.mask ^= (uint)GroupMask.LvgfDescriptiontop;
            }
            if (!string.IsNullOrEmpty(BottomDescription))
            {
                group.pszDescriptionBottom = BottomDescription;
                group.mask ^= (uint)GroupMask.LvgfDescriptionbottom;
            }
            var imageIndex = GetImageIndex(TitleImage);
            if (imageIndex >= 0)
            {
                group.iTitleImage = imageIndex;
                group.mask ^= (uint)GroupMask.LvgfTitleimage;
            }
            imageIndex = GetImageIndex(ExtendedImage);
            if (imageIndex >= 0)
            {
                group.iExtendedImage = imageIndex;
                group.mask ^= (uint)GroupMask.LvgfExtendedimage;
            }
            if (!string.IsNullOrEmpty(SubsetTitle))
            {
                group.pszSubsetTitle = SubsetTitle;
                group.mask ^= (uint)GroupMask.LvgfSubset;
            }
            if (VirtualItemCount > 0)
            {
                group.cItems = VirtualItemCount;
                group.mask ^= (uint)GroupMask.LvgfItems;
            }
            return group;
        }

        private bool GetOneState(GroupState mask)
        {
            if (Created)
            {
                State = GetState();
            }
            return (State & mask) == mask;
        }

        protected GroupState GetState()
        {
            return NativeMethods.GetGroupState(ListView, GroupId, GroupState.LvgsAll);
        }

        protected int SetState(GroupState newState, GroupState mask)
        {
            var group = new NativeMethods.Lvgroup2();
            group.cbSize = ((uint)Marshal.SizeOf(typeof (NativeMethods.Lvgroup2)));
            group.mask = (uint)GroupMask.LvgfState;
            group.state = (uint)newState;
            group.stateMask = (uint)mask;
            return NativeMethods.SetGroupInfo(ListView, GroupId, group);
        }

        private void SetOneState(bool value, GroupState mask)
        {
            StateMask ^= mask;
            if (value)
            {
                State ^= mask;
            }
            else
            {
                State &= ~mask;
            }
            if (Created)
            {
                SetState(State, mask);
            }
        }
    }
}
