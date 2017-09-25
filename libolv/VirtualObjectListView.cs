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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using libolv.Implementation;
using libolv.Implementation.Adapters;
using libolv.Implementation.Events;
using libolv.Implementation.VirtualGroups;

namespace libolv
{
    public class VirtualObjectListView : ObjectListView
    {
        private bool _showGroups;
        private IVirtualListDataSource _virtualListDataSource;
        private static FieldInfo _virtualListSizeFieldInfo;
        private OwnerDataCallbackImpl _ownerDataCallbackImpl;

        private OlvListItem _lastRetrieveVirtualItem;
        private int _lastRetrieveVirtualItemIndex = -1;

        public VirtualObjectListView()
        {
            Init();
        }

        private void Init()
        {
            VirtualMode = true; /* Virtual lists have to be virtual -- no prizes for guessing that :) */
            CacheVirtualItems += HandleCacheVirtualItems;
            RetrieveVirtualItem += HandleRetrieveVirtualItem;
            SearchForVirtualItem += HandleSearchForVirtualItem;
            VirtualListDataSource = new VirtualListVersion1DataSource(this);
            /* Virtual lists have to manage their own check state, since the normal ListView control 
             * doesn't even allow checkboxes on virtual lists */
            PersistentCheckBoxes = true;
        }

        /* Public Properties */
        [Browsable(false)]
        public override bool CanShowGroups
        {
            get
            {
                /* Virtual lists need Vista and a grouping strategy to show groups */
                return (IsVistaOrLater && GroupingStrategy != null);
            }
        }

        [Category("Appearance"), Description("Should the list view show checkboxes?"), DefaultValue(false)]
        public new bool CheckBoxes
        {
            get { return base.CheckBoxes; }
            set
            {
                try
                {
                    base.CheckBoxes = value;
                }
                catch (InvalidOperationException)
                {
                    StateImageList = null;
                    VirtualMode = false;
                    base.CheckBoxes = value;
                    VirtualMode = true;
                    ShowGroups = ShowGroups;
                    BuildList(true);
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override IList CheckedObjects
        {
            get
            {
                /* If we aren't should checkboxes, then no objects can be checked */
                if (!CheckBoxes)
                {
                    return new ArrayList();
                }
                /* If the data source has somehow vanished, we can't do anything */
                if (VirtualListDataSource == null)
                {
                    return new ArrayList();
                }
                /* If a custom check state getter is install, we can't use our check state management
                 * We have to use the (slower) base version. */
                if (CheckStateGetter != null)
                {
                    return base.CheckedObjects;
                }
                /* Collect items that are checked AND that still exist in the list. */
                var objects = new ArrayList();
                foreach (var kvp in CheckStateMap.Where(kvp => kvp.Value == CheckState.Checked && VirtualListDataSource.GetObjectIndex(kvp.Key) >= 0))
                {
                    objects.Add(kvp.Key);
                }
                return objects;
            }
            set
            {
                if (!CheckBoxes)
                {
                    return;
                }
                /* Set up an efficient way of testing for the presence of a particular model */
                var table = new Hashtable(GetItemCount());
                if (value != null)
                {
                    foreach (var x in value)
                    {
                        table[x] = true;
                    }
                }
                /* Uncheck anything that is no longer checked */
                var keys = new Object[CheckStateMap.Count];
                CheckStateMap.Keys.CopyTo(keys, 0);
                foreach (var key in keys.Where(key => !table.Contains(key)))
                {
                    SetObjectCheckedness(key, CheckState.Unchecked);
                }
                /* Check all the new checked objects */
                foreach (var x in table.Keys)
                {
                    SetObjectCheckedness(x, CheckState.Checked);
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override IEnumerable FilteredObjects
        {
            get
            {
                for (var i = 0; i < GetItemCount(); i++)
                {
                    yield return GetModelObject(i);
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IVirtualGroups GroupingStrategy { get; set; }

        public override bool IsFiltering
        {
            get { return base.IsFiltering && (VirtualListDataSource is IFilterableDataSource); }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override IEnumerable Objects
        {
            get
            {
                try
                {
                    /* If we are filtering, we have to temporarily disable filtering so we get
                     * the whole collection */
                    if (IsFiltering)
                    {
                        ((IFilterableDataSource)VirtualListDataSource).ApplyFilters(null, null);
                    }
                    return FilteredObjects;
                }
                finally
                {
                    if (IsFiltering)
                    {
                        ((IFilterableDataSource)VirtualListDataSource).ApplyFilters(ModelFilter, ListFilter);
                    }
                }
            }
            set { base.Objects = value; }
        }
        
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual RowGetterDelegate RowGetter
        {
            get { return ((VirtualListVersion1DataSource)_virtualListDataSource).RowGetter; }
            set { ((VirtualListVersion1DataSource)_virtualListDataSource).RowGetter = value; }
        }

        [Category("Appearance"), Description("Should the list view show items in groups?"), DefaultValue(true)]
        public override bool ShowGroups
        {
            get
            {
                /* Pre-Vista, virtual lists cannot show groups */
                return IsVistaOrLater && _showGroups;
            }
            set
            {
                _showGroups = value;
                if (Created && !value)
                {
                    DisableVirtualGroups();
                }
            }
        }
        
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual IVirtualListDataSource VirtualListDataSource
        {
            get { return _virtualListDataSource; }
            set
            {
                _virtualListDataSource = value;
                CustomSorter = delegate(OlvColumn column, SortOrder sortOrder)
                                   {
                                       ClearCachedInfo();
                                       _virtualListDataSource.Sort(column, sortOrder);
                                   };
                BuildList(false);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected new virtual int VirtualListSize
        {
            get { return base.VirtualListSize; }
            set
            {
                if (value == VirtualListSize || value < 0)
                {
                    return;
                }
                /* Get around the 'private' marker on 'virtualListSize' field using reflection */
                if (_virtualListSizeFieldInfo == null)
                {
                    _virtualListSizeFieldInfo = typeof (ListView).GetField("virtualListSize",
                                                                          BindingFlags.NonPublic | BindingFlags.Instance);
                    System.Diagnostics.Debug.Assert(_virtualListSizeFieldInfo != null);
                }
                /* Set the base class private field so that it keeps on working */
                _virtualListSizeFieldInfo.SetValue(this, value);
                /* Send a raw message to change the virtual list size *without* changing the scroll position */
                if (IsHandleCreated && !DesignMode)
                {
                    NativeMethods.SetItemCount(this, value);
                }
            }
        }

        /* OLV accessing */
        public override int GetItemCount()
        {
            return VirtualListSize;
        }

        public override object GetModelObject(int index)
        {
            return VirtualListDataSource != null && index >= 0 && index < GetItemCount()
                       ? VirtualListDataSource.GetNthObject(index)
                       : null;
        }

        public override int IndexOf(Object modelObject)
        {
            return VirtualListDataSource == null || modelObject == null
                       ? -1
                       : VirtualListDataSource.GetObjectIndex(modelObject);
        }

        public override OlvListItem ModelToItem(object modelObject)
        {
            if (VirtualListDataSource == null || modelObject == null)
            {
                return null;
            }
            var index = VirtualListDataSource.GetObjectIndex(modelObject);
            return index >= 0 ? GetItem(index) : null;
        }

        /* Object manipulation */
        public override void AddObjects(ICollection modelObjects)
        {
            if (VirtualListDataSource == null)
            {
                return;
            }
            /* Give the world a chance to cancel or change the added objects */
            var args = new ItemsAddingEventArgs(modelObjects);
            OnItemsAdding(args);
            if (args.Canceled)
            {
                return;
            }
            try
            {
                BeginUpdate();
                VirtualListDataSource.AddObjects(args.ObjectsToAdd);
                BuildList();
            }
            finally
            {
                EndUpdate();
            }
        }

        public override void ClearObjects()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(ClearObjects));
            }
            else
            {
                CheckStateMap.Clear();
                SetObjects(new ArrayList());
            }
        }

        public virtual void EnsureNthGroupVisible(int groupIndex)
        {
            if (!ShowGroups)
            {
                return;
            }
            if (groupIndex <= 0 || groupIndex >= OlvGroups.Count)
            {
                /* There is no easy way to scroll back to the beginning of the list */
                var delta = 0 - NativeMethods.GetScrollPosition(this, false);
                NativeMethods.Scroll(this, 0, delta);
            }
            else
            {
                /* Find the display rectangle of the last item in the previous group */
                var previousGroup = OlvGroups[groupIndex - 1];
                var lastItemInGroup = GroupingStrategy.GetGroupMember(previousGroup, previousGroup.VirtualItemCount - 1);
                var r = GetItemRect(lastItemInGroup);
                /* Scroll so that the last item of the previous group is just out of sight,
                 * which will make the desired group header visible. */
                var delta = r.Y + r.Height/2;
                NativeMethods.Scroll(this, 0, delta);
            }
        }

        public override void RefreshObjects(IList modelObjects)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)(() => RefreshObjects(modelObjects)));
                return;
            }
            /* Without a data source, we can't do */
            if (VirtualListDataSource == null)
            {
                return;
            }
            try
            {
                BeginUpdate();
                ClearCachedInfo();
                foreach (var index in modelObjects.Cast<object>().Select(modelObject => VirtualListDataSource.GetObjectIndex(modelObject)).Where(index => index >= 0))
                {
                    RedrawItems(index, index, true);
                }
            }
            finally
            {
                EndUpdate();
            }
        }

        public override void RefreshSelectedObjects()
        {
            foreach (int index in SelectedIndices)
            {
                RedrawItems(index, index, true);
            }
        }

        public override void RemoveObjects(ICollection modelObjects)
        {
            if (VirtualListDataSource == null)
            {
                return;
            }
            /* Give the world a chance to cancel or change the removed objects */
            var args = new ItemsRemovingEventArgs(modelObjects);
            OnItemsRemoving(args);
            if (args.Canceled)
            {
                return;
            }
            try
            {
                BeginUpdate();
                VirtualListDataSource.RemoveObjects(args.ObjectsToRemove);
                BuildList();
            }
            finally
            {
                EndUpdate();
            }
        }

        public override void SelectObject(object modelObject, bool setFocus)
        {
            /* Without a data source, we can't do */
            if (VirtualListDataSource == null)
            {
                return;
            }
            /* Check that the object is in the list (plus not all data sources can locate objects) */
            var index = VirtualListDataSource.GetObjectIndex(modelObject);
            if (index < 0 || index >= VirtualListSize)
            {
                return;
            }
            /* If the given model is already selected, don't do anything else (prevents an flicker) */
            if (SelectedIndices.Count == 1 && SelectedIndices[0] == index)
            {
                return;
            }
            /* Finally, select the row */
            SelectedIndices.Clear();
            SelectedIndices.Add(index);
            if (setFocus)
            {
                SelectedItem.Focused = true;
            }
        }

        public override void SelectObjects(IList modelObjects)
        {
            /* Without a data source, we can't do */
            if (VirtualListDataSource == null)
            {
                return;
            }
            SelectedIndices.Clear();
            if (modelObjects == null)
            {
                return;
            }
            foreach (var index in modelObjects.Cast<object>().Select(modelObject => VirtualListDataSource.GetObjectIndex(modelObject)).Where(index => index >= 0 && index < VirtualListSize))
            {
                SelectedIndices.Add(index);
            }
        }

        public override void SetObjects(IEnumerable collection, bool preserveState)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)(() => SetObjects(collection, preserveState)));
                return;
            }
            if (VirtualListDataSource == null)
            {
                return;
            }
            /* Give the world a chance to cancel or change the assigned collection */
            var args = new ItemsChangingEventArgs(null, collection);
            OnItemsChanging(args);
            if (args.Canceled)
            {
                return;
            }
            BeginUpdate();
            try
            {
                VirtualListDataSource.SetObjects(args.NewObjects);
                BuildList();
                UpdateNotificationSubscriptions(args.NewObjects);
            }
            finally
            {
                EndUpdate();
            }
        }

        /* Implementation */
        public override void BuildList(bool shouldPreserveSelection)
        {
            UpdateVirtualListSize();
            ClearCachedInfo();
            if (ShowGroups)
            {
                BuildGroups();
            }
            else
            {
                Sort();
            }
            Invalidate();
        }

        public virtual void ClearCachedInfo()
        {
            _lastRetrieveVirtualItemIndex = -1;
        }

        protected override void CreateGroups(IEnumerable<OlvGroup> groups)
        {
            /* In a virtual list, we cannot touch the Groups property.
             * It was obviously not written for virtual list and often throws exceptions. */
            NativeMethods.ClearGroups(this);
            EnableVirtualGroups();
            foreach (var group in groups)
            {
                System.Diagnostics.Debug.Assert(group.Items.Count == 0,
                                                "Groups in virtual lists cannot set Items. Use VirtualItemCount instead.");
                System.Diagnostics.Debug.Assert(group.VirtualItemCount > 0, "VirtualItemCount must be greater than 0.");

                group.InsertGroupNewStyle(this);
            }
        }

        protected void DisableVirtualGroups()
        {
            NativeMethods.ClearGroups(this);
            const int lvmEnablegroupview = 0x1000 + 157;
            NativeMethods.SendMessage(Handle, lvmEnablegroupview, 0, 0);
            const int lvmSetownerdatacallback = 0x10BB;
            NativeMethods.SendMessage(Handle, lvmSetownerdatacallback, 0, 0);
        }

        protected void EnableVirtualGroups()
        {
            /* We need to implement the IOwnerDataCallback interface */
            if (_ownerDataCallbackImpl == null)
            {
                _ownerDataCallbackImpl = new OwnerDataCallbackImpl(this);
            }
            const int lvmSetownerdatacallback = 0x10BB;
            var ptr = Marshal.GetComInterfaceForObject(_ownerDataCallbackImpl, typeof (IOwnerDataCallback));
            NativeMethods.SendMessage(Handle, lvmSetownerdatacallback, ptr, 0);            
            Marshal.Release(ptr);
            const int lvmEnablegroupview = 0x1000 + 157;
            NativeMethods.SendMessage(Handle, lvmEnablegroupview, 1, 0);            
        }

        protected override CheckState? GetCheckState(object modelObject)
        {
            if (CheckStateGetter != null)
            {
                return base.GetCheckState(modelObject);
            }
            var state = CheckState.Unchecked;
            if (modelObject != null)
            {
                CheckStateMap.TryGetValue(modelObject, out state);
            }
            return state;
        }

        public override int GetDisplayOrderOfItemIndex(int itemIndex)
        {
            if (!ShowGroups)
            {
                return itemIndex;
            }
            var groupIndex = GroupingStrategy.GetGroup(itemIndex);
            var displayIndex = 0;
            for (var i = 0; i < groupIndex - 1; i++)
            {
                displayIndex += OlvGroups[i].VirtualItemCount;
            }
            displayIndex += GroupingStrategy.GetIndexWithinGroup(OlvGroups[groupIndex], itemIndex);
            return displayIndex;
        }

        public override OlvListItem GetLastItemInDisplayOrder()
        {
            if (!ShowGroups)
            {
                return base.GetLastItemInDisplayOrder();
            }
            if (OlvGroups.Count > 0)
            {
                var lastGroup = OlvGroups[OlvGroups.Count - 1];
                if (lastGroup.VirtualItemCount > 0)
                {
                    return GetItem(GroupingStrategy.GetGroupMember(lastGroup, lastGroup.VirtualItemCount - 1));
                }
            }
            return null;
        }

        public override OlvListItem GetNthItemInDisplayOrder(int n)
        {
            if (!ShowGroups)
            {
                return GetItem(n);
            }
            foreach (var group in OlvGroups)
            {
                if (n < group.VirtualItemCount)
                {
                    return GetItem(GroupingStrategy.GetGroupMember(group, n));
                }
                n -= group.VirtualItemCount;
            }
            return null;
        }

        public override OlvListItem GetNextItem(OlvListItem itemToFind)
        {
            if (!ShowGroups)
            {
                return base.GetNextItem(itemToFind);
            }
            /* Sanity */
            if (OlvGroups == null || OlvGroups.Count == 0)
            {
                return null;
            }
            /* If the given item is null, return the first member of the first group */
            if (itemToFind == null)
            {
                return GetItem(GroupingStrategy.GetGroupMember(OlvGroups[0], 0));
            }
            /* Find where this item occurs (which group and where in that group) */
            var groupIndex = GroupingStrategy.GetGroup(itemToFind.Index);
            var indexWithinGroup = GroupingStrategy.GetIndexWithinGroup(OlvGroups[groupIndex], itemToFind.Index);
            /* If it's not the last member, just return the next member */
            if (indexWithinGroup < OlvGroups[groupIndex].VirtualItemCount - 1)
            {
                return GetItem(GroupingStrategy.GetGroupMember(OlvGroups[groupIndex], indexWithinGroup + 1));
            }
            /* The item is the last member of its group. Return the first member of the next group
             * (unless there isn't a next group) */
            return groupIndex < OlvGroups.Count - 1 ? GetItem(GroupingStrategy.GetGroupMember(OlvGroups[groupIndex + 1], 0)) : null;
        }

        public override OlvListItem GetPreviousItem(OlvListItem itemToFind)
        {
            if (!ShowGroups)
            {
                return base.GetPreviousItem(itemToFind);
            }
            /* Sanity */
            if (OlvGroups == null || OlvGroups.Count == 0)
            {
                return null;
            }
            /* If the given items is null, return the last member of the last group */
            if (itemToFind == null)
            {
                var lastGroup = OlvGroups[OlvGroups.Count - 1];
                return GetItem(GroupingStrategy.GetGroupMember(lastGroup, lastGroup.VirtualItemCount - 1));
            }
            /* Find where this item occurs (which group and where in that group) */
            var groupIndex = GroupingStrategy.GetGroup(itemToFind.Index);
            var indexWithinGroup = GroupingStrategy.GetIndexWithinGroup(OlvGroups[groupIndex], itemToFind.Index);
            /* If it's not the first member of the group, just return the previous member */
            if (indexWithinGroup > 0)
            {
                return GetItem(GroupingStrategy.GetGroupMember(OlvGroups[groupIndex], indexWithinGroup - 1));
            }
            /* The item is the first member of its group. Return the last member of the previous group
             * (if there is one) */
            if (groupIndex > 0)
            {
                var previousGroup = OlvGroups[groupIndex - 1];
                return GetItem(GroupingStrategy.GetGroupMember(previousGroup, previousGroup.VirtualItemCount - 1));
            }
            return null;
        }

        protected override IList<OlvGroup> MakeGroups(GroupingParameters parms)
        {
            return GroupingStrategy == null ? new List<OlvGroup>() : GroupingStrategy.GetGroups(parms);
        }

        public virtual OlvListItem MakeListViewItem(int itemIndex)
        {
            var olvi = new OlvListItem(GetModelObject(itemIndex));
            FillInValues(olvi, olvi.RowObject);
            PostProcessOneRow(itemIndex, GetDisplayOrderOfItemIndex(itemIndex), olvi);
            if (HotRowIndex == itemIndex)
            {
                UpdateHotRow(olvi);
            }
            return olvi;
        }

        protected override void PostProcessRows()
        {
            /* Empty */
        }

        protected override CheckState PutCheckState(object modelObject, CheckState state)
        {
            state = base.PutCheckState(modelObject, state);
            CheckStateMap[modelObject] = state;
            return state;
        }

        public override void RefreshItem(OlvListItem olvi)
        {
            ClearCachedInfo();
            RedrawItems(olvi.Index, olvi.Index, false);
        }

        protected virtual void SetVirtualListSize(int newSize)
        {
            if (newSize < 0 || VirtualListSize == newSize)
            {
                return;
            }
            var oldSize = VirtualListSize;
            ClearCachedInfo();
            /* There is a bug in .NET when a virtual ListView is cleared
             * (i.e. VirtuaListSize set to 0) AND it is scrolled vertically: the scroll position 
             * is wrong when the list is next populated. To avoid this, before 
             * clearing a virtual list, we make sure the list is scrolled to the top.
             * [6 weeks later] Damn this is a pain! There are cases where this can also throw exceptions! */
            try
            {
                if (newSize == 0 && TopItemIndex > 0)
                {
                    TopItemIndex = 0;
                }
            }
            catch (Exception ex)
            {
                /* Ignore any failures */
                System.Diagnostics.Debug.Print(ex.ToString());
            }
            /* In strange cases, this can throw the exceptions too. The best we can do is ignore them :( */
            try
            {
                VirtualListSize = newSize;
            }
            catch (ArgumentOutOfRangeException)
            {
                /* pass */
            }
            catch (NullReferenceException)
            {
                /* pass */
            }
            /* Tell the world that the size of the list has changed */
            OnItemsChanged(new ItemsChangedEventArgs(oldSize, VirtualListSize));
        }

        protected override void TakeOwnershipOfObjects()
        {
            /* Empty */
        }

        protected override void UpdateFiltering()
        {
            var filterable = VirtualListDataSource as IFilterableDataSource;
            if (filterable == null)
            {
                return;
            }
            BeginUpdate();
            try
            {
                filterable.ApplyFilters(ModelFilter, ListFilter);
                BuildList();
            }
            finally
            {
                EndUpdate();
            }
        }

        public virtual void UpdateVirtualListSize()
        {
            if (VirtualListDataSource != null)
            {
                SetVirtualListSize(VirtualListDataSource.GetObjectCount());
            }
        }

        /* Event handlers */
        protected virtual void HandleCacheVirtualItems(object sender, CacheVirtualItemsEventArgs e)
        {
            if (VirtualListDataSource != null)
            {
                VirtualListDataSource.PrepareCache(e.StartIndex, e.EndIndex);
            }
        }

        protected virtual void HandleRetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            /* .NET 2.0 seems to generate a lot of these events. Before drawing *each* sub-item,
             * this event is triggered 4-8 times for the same index. So we save lots of CPU time
             * by caching the last result. */            
            if (_lastRetrieveVirtualItemIndex != e.ItemIndex)
            {
                _lastRetrieveVirtualItemIndex = e.ItemIndex;
                _lastRetrieveVirtualItem = MakeListViewItem(e.ItemIndex);
            }
            e.Item = _lastRetrieveVirtualItem;
        }

        protected virtual void HandleSearchForVirtualItem(object sender, SearchForVirtualItemEventArgs e)
        {
            /* The event has e.IsPrefixSearch, but as far as I can tell, this is always false (maybe that's different under
             * Vista) So we ignore IsPrefixSearch and IsTextSearch and always to a case insensitve prefix match.
             * We can't do anything if we don't have a data source */
            if (VirtualListDataSource == null)
            {
                return;
            }
            /* Where should we start searching? If the last row is focused, the SearchForVirtualItemEvent starts searching
             * from the next row, which is actually an invalidate index -- so we make sure we never go past the last object. */
            var start = Math.Min(e.StartIndex, VirtualListDataSource.GetObjectCount() - 1);
            /* Give the world a chance to fiddle with or completely avoid the searching process */
            var args = new BeforeSearchingEventArgs(e.Text, start);
            OnBeforeSearching(args);
            if (args.Canceled)
            {
                return;
            }
            /* Do the search */
            var i = FindMatchingRow(args.StringToFind, args.StartSearchFrom, e.Direction);
            /* Tell the world that a search has occurred */
            var args2 = new AfterSearchingEventArgs(args.StringToFind, i);
            OnAfterSearching(args2);
            /* If we found a match, tell the event */
            if (i != -1)
            {
                e.Index = i;
            }
        }

        protected override int FindMatchInRange(string text, int first, int last, OlvColumn column)
        {
            return VirtualListDataSource.SearchText(text, first, last, column);
        }
    }
}
