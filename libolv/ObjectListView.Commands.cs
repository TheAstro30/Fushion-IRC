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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using libolv.DragDrop;
using libolv.Implementation;
using libolv.Implementation.Comparers;
using libolv.Implementation.Events;
using libolv.Rendering.Renderers;
using libolv.Utilities;

namespace libolv
{
    public partial class ObjectListView
    {
        private bool _isOwnerOfObjects; /* does this ObjectListView own the Objects collection? */
        private Dictionary<string, bool> _visitedUrlMap = new Dictionary<string, bool>(); /* Which urls have been visited? */
        private Hashtable _subscribedModels = new Hashtable();
        private bool _useNotifyPropertyChanged;

        public virtual void AddObject(object modelObject)
        {            
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)(() => AddObject(modelObject)));
            }
            else
            {
                AddObjects(new[] {modelObject});
            }
        }

        public virtual void AddObjects(ICollection modelObjects)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)(() => AddObjects(modelObjects)));
                return;
            }
            InsertObjects(GetItemCount(), modelObjects);
            Sort(PrimarySortColumn, PrimarySortOrder);
        }
		
        public virtual void AutoResizeColumns()
        {
            foreach (OlvColumn c in Columns)
            {
                c.Width = -2;
            }
        }

        public virtual void AutoSizeColumns()
        {
            /* If we are supposed to resize to content, but there is no content, resize to
             * the header size instead. */
            var resizeToContentStyle = ColumnHeaderAutoResizeStyle.ColumnContent;
            if (GetItemCount() == 0)
            {
                resizeToContentStyle = ColumnHeaderAutoResizeStyle.HeaderSize;
            }
            foreach (ColumnHeader column in Columns)
            {
                switch (column.Width)
                {
                    case 0:
                        AutoResizeColumn(column.Index, resizeToContentStyle);
                        break;

                    case -1:
                        AutoResizeColumn(column.Index, ColumnHeaderAutoResizeStyle.HeaderSize);
                        break;
                }
            }
        }

        public virtual void BuildGroups()
        {
            BuildGroups(PrimarySortColumn, PrimarySortOrder == SortOrder.None ? SortOrder.Ascending : PrimarySortOrder);
        }

        public virtual void BuildGroups(OlvColumn column, SortOrder order)
        {
            var args = BuildBeforeSortingEventArgs(column, order);
            OnBeforeSorting(args);
            if (args.Canceled)
            {
                return;
            }
            BuildGroups(args.ColumnToGroupBy, args.GroupByOrder, args.ColumnToSort, args.SortOrder, args.SecondaryColumnToSort, args.SecondarySortOrder);
            OnAfterSorting(new AfterSortingEventArgs(args));
        }

        private BeforeSortingEventArgs BuildBeforeSortingEventArgs(OlvColumn column, SortOrder order)
        {
            var groupBy = AlwaysGroupByColumn ?? column ?? GetColumn(0);
            var groupByOrder = AlwaysGroupBySortOrder;
            if (order == SortOrder.None)
            {
                order = Sorting;
                if (order == SortOrder.None)
                {
                    order = SortOrder.Ascending;
                }
            }
            if (groupByOrder == SortOrder.None)
            {
                groupByOrder = order;
            }
            var args = new BeforeSortingEventArgs(
                groupBy, groupByOrder,
                column, order,
                SecondarySortColumn ?? GetColumn(0),
                SecondarySortOrder == SortOrder.None ? order : SecondarySortOrder);
            if (column != null)
            {
                args.Canceled = !column.Sortable;
            }
            return args;
        }

        public virtual void BuildGroups(OlvColumn groupByColumn, SortOrder groupByOrder, OlvColumn column, SortOrder order, OlvColumn secondaryColumn, SortOrder secondaryOrder)
        {
            /* Sanity checks */
            if (groupByColumn == null)
            {
                return;
            }
            /* Getting the Count forces any internal cache of the ListView to be flushed. Without
             * this, iterating over the Items will not work correctly if the ListView handle
             * has not yet been created. */
#pragma warning disable 168
            var dummy = Items.Count;
#pragma warning restore 168
            /* Collect all the information that governs the creation of groups */
            var parms = CollectGroupingParameters(groupByColumn, groupByOrder, column, order, secondaryColumn, secondaryOrder);
            /* Trigger an event to let the world create groups if they want */
            var args = new CreateGroupsEventArgs(parms);
            if (parms.GroupByColumn != null)
            {
                args.Canceled = !parms.GroupByColumn.Groupable;
            }
            OnBeforeCreatingGroups(args);
            if (args.Canceled)
            {
                return;
            }
            /* If the event didn't create them for us, use our default strategy */
            if (args.Groups == null)
            {
                args.Groups = MakeGroups(parms);
            }
            /* Give the world a chance to munge the groups before they are created */
            OnAboutToCreateGroups(args);
            if (args.Canceled)
            {
                return;
            }
            /* Create the groups now */
            OlvGroups = args.Groups;
            CreateGroups(args.Groups);
            /* Tell the world that new groups have been created */
            OnAfterCreatingGroups(args);
        }

        protected virtual GroupingParameters CollectGroupingParameters(OlvColumn groupByColumn, SortOrder groupByOrder,
                                                                       OlvColumn column, SortOrder order,
                                                                       OlvColumn secondaryColumn,
                                                                       SortOrder secondaryOrder)
        {
            var titleFormat = ShowItemCountOnGroups ? groupByColumn.GroupWithItemCountFormatOrDefault : null;
            var titleSingularFormat = ShowItemCountOnGroups
                                          ? groupByColumn.GroupWithItemCountSingularFormatOrDefault
                                          : null;
            var parms = new GroupingParameters(this, groupByColumn, groupByOrder,
                                               column, order, secondaryColumn, secondaryOrder,
                                               titleFormat, titleSingularFormat,
                                               SortGroupItemsByPrimaryColumn);
            return parms;
        }

        protected virtual IList<OlvGroup> MakeGroups(GroupingParameters parms)
        {

            /* There is a lot of overlap between this method and FastListGroupingStrategy.MakeGroups()
             * Any changes made here may need to be reflected there
             * Separate the list view items into groups, using the group key as the descrimanent */
            var map = new NullableDictionary<object, List<OlvListItem>>();
            foreach (OlvListItem olvi in parms.ListView.Items)
            {
                var key = parms.GroupByColumn.GetGroupKey(olvi.RowObject);
                if (!map.ContainsKey(key))
                {
                    map[key] = new List<OlvListItem>();
                }
                map[key].Add(olvi);
            }
            /* Sort the items within each group (unless specifically turned off) */
            var sortColumn = parms.SortItemsByPrimaryColumn ? parms.ListView.GetColumn(0) : parms.PrimarySort;
            if (sortColumn != null && parms.PrimarySortOrder != SortOrder.None)
            {
                var itemSorter = parms.ItemComparer ??
                                 new ColumnComparer(sortColumn, parms.PrimarySortOrder,
                                                    parms.SecondarySort, parms.SecondarySortOrder);
                foreach (var key in map.Keys)
                {
                    map[key].Sort(itemSorter);
                }
            }
            /* Make a list of the required groups */
            var groups = new List<OlvGroup>();
            foreach (var key in map.Keys)
            {
                var title = parms.GroupByColumn.ConvertGroupKeyToTitle(key);
                if (!string.IsNullOrEmpty(parms.TitleFormat))
                {
                    var count = map[key].Count;
                    var format = (count == 1 ? parms.TitleSingularFormat : parms.TitleFormat);
                    try
                    {
                        title = string.Format(format, title, count);
                    }
                    catch (FormatException)
                    {
                        title = "Invalid group format: " + format;
                    }
                }
                var lvg = new OlvGroup(title)
                              {
                                  Collapsible = HasCollapsibleGroups,
                                  Key = key,
                                  SortValue = key as IComparable,
                                  Items = map[key]
                              };
                if (parms.GroupByColumn.GroupFormatter != null)
                {
                    parms.GroupByColumn.GroupFormatter(lvg, parms);
                }
                groups.Add(lvg);
            }
            /* Sort the groups */
            if (parms.GroupByOrder != SortOrder.None)
            {
                groups.Sort(parms.GroupComparer ?? new OlvGroupComparer(parms.GroupByOrder));
            }
            return groups;
        }

        public virtual void BuildList()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(BuildList));
            }
            else
            {
                BuildList(true);
            }
        }

        public virtual void BuildList(bool shouldPreserveState)
        {
            if (Frozen)
            {
                return;
            }
            ApplyExtendedStyles();
            ClearHotItem();
            var previousTopIndex = TopItemIndex;
            var currentScrollPosition = LowLevelScrollPosition;
            IList previousSelection = new ArrayList();
            Object previousFocus = null;
            if (shouldPreserveState && _objects != null)
            {
                previousSelection = SelectedObjects;
                var focusedItem = FocusedItem as OlvListItem;
                if (focusedItem != null)
                {
                    previousFocus = focusedItem.RowObject;
                }
            }
            var objectsToDisplay = FilteredObjects;
            BeginUpdate();
            try
            {
                Items.Clear();
                ListViewItemSorter = null;
                if (objectsToDisplay != null)
                {
                    /* Build a list of all our items and then display them. (Building
                     * a list and then doing one AddRange is about 10-15% faster than individual adds) */
                    var itemList = new List<ListViewItem>();
                    // use ListViewItem to avoid co-variant conversion
                    foreach (var rowObject in objectsToDisplay)
                    {
                        var lvi = new OlvListItem(rowObject);
                        FillInValues(lvi, rowObject);
                        itemList.Add(lvi);
                    }
                    Items.AddRange(itemList.ToArray());
                    Sort();
                    if (shouldPreserveState)
                    {
                        SelectedObjects = previousSelection;
                        FocusedItem = ModelToItem(previousFocus);
                    }
                    RefreshHotItem();
                }
            }
            finally
            {
                EndUpdate();
            }
            /* We can only restore the scroll position after the EndUpdate() because
             * of caching that the ListView does internally during a BeginUpdate/EndUpdate pair. */
            if (shouldPreserveState)
            {
                RefreshHotItem();
                /* Restore the scroll position. TopItemIndex is best, but doesn't work
                 * when the control is grouped. */
                if (ShowGroups)
                {
                    LowLevelScroll(currentScrollPosition.X, currentScrollPosition.Y);
                }
                else
                {
                    TopItemIndex = previousTopIndex;                
                }
            }
        }

        protected virtual void ApplyExtendedStyles()
        {
            const int lvsExSubitemimages = 0x00000002;
            const int lvsExHeaderinallviews = 0x02000000;
            const int styleMask = lvsExSubitemimages | lvsExHeaderinallviews;
            var style = 0;
            if (ShowImagesOnSubItems && !VirtualMode)
            {
                style ^= lvsExSubitemimages;
            }
            if (ShowHeaderInAllViews)
            {
                style ^= lvsExHeaderinallviews;
            }
            NativeMethods.SetExtendedStyle(this, style, styleMask);
        }

        public virtual void CalculateReasonableTileSize()
        {
            if (Columns.Count <= 0)
            {
                return;
            }
            var columns = AllColumns.FindAll(x => (x.Index == 0) || x.IsTileViewColumn);
            var imageHeight = (LargeImageList == null ? 16 : LargeImageList.ImageSize.Height);
            var dataHeight = (Font.Height + 1) * columns.Count;
            var tileWidth = (TileSize.Width == 0 ? 200 : TileSize.Width);
            var tileHeight = Math.Max(TileSize.Height, Math.Max(imageHeight, dataHeight));
            TileSize = new Size(tileWidth, tileHeight);
        }

        public virtual void ChangeToFilteredColumns(View view)
        {
            /* Store the state */
            SuspendSelectionEvents();
            var previousSelection = SelectedObjects;
            var previousTopIndex = TopItemIndex;
            Freeze();
            Clear();
            var columns = GetFilteredColumns(view);
            if (view == View.Details || ShowHeaderInAllViews)
            {
                /* Make sure all columns have a reasonable LastDisplayIndex */
                for (var index = 0; index < columns.Count; index++)
                {
                    if (columns[index].LastDisplayIndex == -1)
                    {
                        columns[index].LastDisplayIndex = index;
                    }
                }
                /* ListView will ignore DisplayIndex FOR ALL COLUMNS if there are any errors, 
                 * e.g. duplicates (two columns with the same DisplayIndex) or gaps. 
                 * LastDisplayIndex isn't guaranteed to be unique, so we just sort the columns by
                 * the last position they were displayed and use that to generate a sequence 
                 * we can use for the DisplayIndex values. */
                var columnsInDisplayOrder = new List<OlvColumn>(columns);
                columnsInDisplayOrder.Sort((x, y) => (x.LastDisplayIndex - y.LastDisplayIndex));
                var i = 0;
                foreach (var col in columnsInDisplayOrder)
                {
                    col.DisplayIndex = i++;
                }
            }
            foreach (var col in columns)
            {
                Columns.Add(col);
            }
            if (view == View.Details || ShowHeaderInAllViews)
            {
                ShowSortIndicator();
            }
            BuildList();
            Unfreeze();
            /* Restore the state */
            SelectedObjects = previousSelection;
            TopItemIndex = previousTopIndex;
            ResumeSelectionEvents();
        }

        public virtual void ClearObjects()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(ClearObjects));
            }
            else
            {
                SetObjects(null);
            }
        }

        public virtual void ClearUrlVisited()
        {
            _visitedUrlMap = new Dictionary<string, bool>();
        }

        public virtual void CopySelectionToClipboard()
        {
            var selection = SelectedObjects;
            if (selection.Count == 0)
            {
                return;
            }
            /* Use the DragSource object to create the data object, if so configured.
             * This relies on the assumption that DragSource will handle the selected objects only.
             * It is legal for StartDrag to return null. */
            object data = null;
            if (CopySelectionOnControlCUsesDragSource && DragSource != null)
            {
                data = DragSource.StartDrag(this, MouseButtons.Left, ModelToItem(selection[0]));
            }
            Clipboard.SetDataObject(data ?? new OlvDataObject(this, selection));
        }

        public virtual void CopyObjectsToClipboard(IList objectsToCopy)
        {
            if (objectsToCopy.Count == 0)
            {
                return;
            }
            /* We don't know where these objects came from, so we can't use the DragSource to create
             * the data object, like we do with CopySelectionToClipboard() above. */
            var dataObject = new OlvDataObject(this, objectsToCopy);
            dataObject.CreateTextFormats();
            Clipboard.SetDataObject(dataObject);
        }

        public virtual string ObjectsToHtml(IList objectsToConvert)
        {
            if (objectsToConvert.Count == 0)
            {
                return string.Empty;
            }
            var exporter = new OlvExporter(this, objectsToConvert);
            return exporter.ExportTo(OlvExporter.ExportFormat.Html);
        }

        public virtual void DeselectAll()
        {
            NativeMethods.DeselectAllItems(this);
        }

        public virtual void EnableCustomSelectionColors()
        {
            UseCustomSelectionColors = true;
        }

        public virtual OlvListItem GetNextItem(OlvListItem itemToFind)
        {
            if (ShowGroups)
            {
                var isFound = (itemToFind == null);
                foreach (var olvi in from ListViewGroup @group in Groups from OlvListItem olvi in @group.Items select olvi)
                {
                    if (isFound)
                    {
                        return olvi;
                    }
                    isFound = (itemToFind == olvi);
                }
                return null;
            }
            if (GetItemCount() == 0)
            {
                return null;
            }
            if (itemToFind == null)
            {
                return GetItem(0);
            }
            return itemToFind.Index == GetItemCount() - 1 ? null : GetItem(itemToFind.Index + 1);
        }

        public virtual OlvListItem GetLastItemInDisplayOrder()
        {
            if (!ShowGroups)
            {
                return GetItem(GetItemCount() - 1);
            }
            if (Groups.Count > 0)
            {
                var lastGroup = Groups[Groups.Count - 1];
                if (lastGroup.Items.Count > 0)
                {
                    return (OlvListItem)lastGroup.Items[lastGroup.Items.Count - 1];
                }
            }
            return null;
        }

        public virtual OlvListItem GetNthItemInDisplayOrder(int n)
        {
            if (!ShowGroups)
            {
                return GetItem(n);
            }
            foreach (ListViewGroup group in Groups)
            {
                if (n < group.Items.Count)
                {
                    return (OlvListItem)group.Items[n];
                }
                n -= group.Items.Count;
            }
            return null;
        }

        public virtual int GetDisplayOrderOfItemIndex(int itemIndex)
        {
            if (!ShowGroups)
            {
                return itemIndex;
            }
            /* TODO: This could be optimized */
            var i = 0;
            foreach (var lvi in from ListViewGroup lvg in Groups from ListViewItem lvi in lvg.Items select lvi)
            {
                if (lvi.Index == itemIndex)
                {
                    return i;
                }
                i++;
            }
            return -1;
        }

        public virtual OlvListItem GetPreviousItem(OlvListItem itemToFind)
        {
            if (ShowGroups)
            {
                OlvListItem previousItem = null;
                foreach (var lvi in from ListViewGroup @group in Groups from OlvListItem lvi in @group.Items select lvi)
                {
                    if (lvi == itemToFind)
                    {
                        return previousItem;
                    }
                    previousItem = lvi;
                }
                return itemToFind == null ? previousItem : null;
            }
            if (GetItemCount() == 0)
            {
                return null;
            }
            if (itemToFind == null)
            {
                return GetItem(GetItemCount() - 1);
            }
            return itemToFind.Index == 0 ? null : GetItem(itemToFind.Index - 1);
        }

        public virtual void InsertObjects(int index, ICollection modelObjects)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)(() => InsertObjects(index, modelObjects)));
                return;
            }
            if (modelObjects == null)
            {
                return;
            }
            BeginUpdate();
            try
            {
                /* Give the world a chance to cancel or change the added objects */
                var args = new ItemsAddingEventArgs(modelObjects);
                OnItemsAdding(args);
                if (args.Canceled)
                {
                    return;
                }
                modelObjects = args.ObjectsToAdd;
                TakeOwnershipOfObjects();
                var ourObjects = EnumerableToArray(Objects, false);
                /* If we are filtering the list, there is no way to efficiently
                 * insert the objects, so just put them into our collection and rebuild. */
                if (IsFiltering)
                {
                    ourObjects.InsertRange(index, modelObjects);
                    BuildList(true);
                }
                else
                {
                    ListViewItemSorter = null;
                    index = Math.Max(0, Math.Min(index, GetItemCount()));
                    var i = index;
                    foreach (var modelObject in modelObjects.Cast<object>().Where(modelObject => modelObject != null))
                    {
                        ourObjects.Insert(i, modelObject);
                        var lvi = new OlvListItem(modelObject);
                        FillInValues(lvi, modelObject);
                        Items.Insert(i, lvi);
                        i++;
                    }
                    for (i = index; i < GetItemCount(); i++)
                    {
                        var lvi = GetItem(i);
                        SetSubItemImages(lvi.Index, lvi);
                    }
                    PostProcessRows();
                }
                /* Tell the world that the list has changed */
                SubscribeNotifications(modelObjects);
                OnItemsChanged(new ItemsChangedEventArgs());
            }
            finally
            {
                EndUpdate();
            }
        }

        public bool IsSelected(object model)
        {
            var item = ModelToItem(model);
            return item != null && item.Selected;
        }

        public virtual bool IsUrlVisited(string url)
        {
            return _visitedUrlMap.ContainsKey(url);
        }

        internal void LowLevelScroll(int dx, int dy)
        {
            NativeMethods.Scroll(this, dx, dy);
        }

        internal Point LowLevelScrollPosition
        {
            get
            {
                return new Point(NativeMethods.GetScrollPosition(this, true),
                                 NativeMethods.GetScrollPosition(this, false));
            }
        }

        public virtual void MarkUrlVisited(string url)
        {
            _visitedUrlMap[url] = true;
        }

        public virtual void MoveObjects(int index, ICollection modelObjects)
        {
            /* We are going to remove all the given objects from our list
             * and then insert them at the given location */
            TakeOwnershipOfObjects();
            var ourObjects = EnumerableToArray(Objects, false);
            var indicesToRemove = new List<int>();
            foreach (var modelObject in modelObjects)
            {
                if (modelObject == null) { continue; }
                var i = IndexOf(modelObject);
                if (i < 0) { continue; }
                indicesToRemove.Add(i);
                ourObjects.Remove(modelObject);
                if (i <= index)
                {
                    index--;
                }
            }
            /* Remove the objects in reverse order so earlier
             * deletes don't change the index of later ones */
            indicesToRemove.Sort();
            indicesToRemove.Reverse();
            try
            {
                BeginUpdate();
                foreach (var i in indicesToRemove)
                {
                    Items.RemoveAt(i);
                }
                InsertObjects(index, modelObjects);
            }
            finally
            {
                EndUpdate();
            }
        }

        public new ListViewHitTestInfo HitTest(int x, int y)
        {
            /* Everything costs something. Playing with the layout of the header can cause problems
             * with the hit testing. If the header shrinks, the underlying control can throw a tantrum. */
            try
            {
                return base.HitTest(x, y);
            }
            catch (ArgumentOutOfRangeException)
            {
                return new ListViewHitTestInfo(null, null, ListViewHitTestLocations.None);
            }
        }

        protected OlvListViewHitTestInfo LowLevelHitTest(int x, int y)
        {
            /* If it's not even in the control, don't bother with anything else */
            if (!ClientRectangle.Contains(x, y))
            {
                return new OlvListViewHitTestInfo(null, null, 0, null);
            }            
            /* Call the native hit test method, which is a little confusing. */
            var lParam = new NativeMethods.Lvhittestinfo
                             {
                                 pt_x = x, 
                                 pt_y = y
                             };
            var index = NativeMethods.HitTest(this, ref lParam);
            /* Setup the various values we need to make our hit test structure */
            var isGroupHit = (lParam.flags & (int)HitTestLocationEx.LvhtExGroup) != 0;
            var hitItem = isGroupHit || index == -1 ? null : GetItem(index);
            var subItem = (View == View.Details && hitItem != null)
                              ? hitItem.GetSubItem(lParam.iSubItem)
                              : null;
            /* Figure out which group is involved in the hit test. This is a little complicated:
             * If the list is virtual:
             * - the returned value is list view item index
             * - iGroup is the *index* of the hit group.
             * If the list is not virtual:
             * - iGroup is always -1.
             * - if the point is over a group, the returned value is the *id* of the hit group.
             * - if the point is not over a group, the returned value is list view item index. */        
            OlvGroup group = null;
            if (ShowGroups && OlvGroups != null)
            {
                if (VirtualMode)
                {
                    group = lParam.iGroup >= 0 && lParam.iGroup < OlvGroups.Count
                                ? OlvGroups[lParam.iGroup]
                                : null;
                }
                else
                {
                    if (isGroupHit)
                    {
                        foreach (var olvGroup in OlvGroups.Where(olvGroup => olvGroup.GroupId == index))
                        {
                            @group = olvGroup;
                            break;
                        }
                    }
                }
            }
            var olvListViewHitTest = new OlvListViewHitTestInfo(hitItem, subItem, lParam.flags, group);            
            return olvListViewHitTest;
        }

        public virtual OlvListViewHitTestInfo OlvHitTest(int x, int y)
        {
            var hti = LowLevelHitTest(x, y);
            if (hti.Item == null && !FullRowSelect && View == View.Details)
            {
                /* Is the point within the column 0? If it is, maybe it should have been a hit.
                 * Let's test slightly to the right and then to left of column 0. Hopefully one
                 * of those will hit a subitem */
                var sides = NativeMethods.GetScrolledColumnSides(this, 0);
                if (x >= sides.X && x <= sides.Y)
                {
                    /* We look for:
                     * - any subitem to the right of cell 0?
                     * - any subitem to the left of cell 0?
                     * - cell 0 at the left edge of the screen */
                    hti = LowLevelHitTest(sides.Y + 4, y);
                    if (hti.Item == null)
                    {
                        hti = LowLevelHitTest(sides.X - 4, y);
                    }
                    if (hti.Item == null)
                    {
                        hti = LowLevelHitTest(4, y);
                    }
                    if (hti.Item != null)
                    {
                        /* We hit something! So, the original point must have been in cell 0 */
                        hti.SubItem = hti.Item.GetSubItem(0);
                        hti.Location = ListViewHitTestLocations.None;
                        hti.HitTestLocation = HitTestLocation.InCell;
                    }
                }
            }
            if (OwnerDraw)
            {
                CalculateOwnerDrawnHitTest(hti, x, y);
            }
            else
            {
                CalculateStandardHitTest(hti, x, y);
            }
            return hti;
        }

        protected virtual void CalculateStandardHitTest(OlvListViewHitTestInfo hti, int x, int y)
        {
            /* Standard hit test works fine for the primary column */
            if (View != View.Details || hti.ColumnIndex == 0 || hti.SubItem == null || hti.Column == null)
            {
                return;
            }
            var cellBounds = hti.SubItem.Bounds;
            var hasImage = (GetActualImageIndex(hti.SubItem.ImageSelector) != -1);
            /* Unless we say otherwise, it was an general incell hit */
            hti.HitTestLocation = HitTestLocation.InCell;
            /* Check if the point is over where an image should be.
             * If there is a checkbox or image there, tag it and exit. */
            var r = cellBounds;
            r.Width = SmallImageSize.Width;
            if (r.Contains(x, y))
            {
                if (hti.Column.CheckBoxes)
                {
                    hti.HitTestLocation = HitTestLocation.CheckBox;
                    return;
                }
                if (hasImage)
                {
                    hti.HitTestLocation = HitTestLocation.Image;
                    return;
                }
            }
            /* Figure out where the text actually is and if the point is in it
             * The standard HitTest assumes that any point inside a subitem is
             * a hit on Text -- which is clearly not true. */
            var textBounds = cellBounds;
            textBounds.X += 4;
            if (hasImage)
            {
                textBounds.X += SmallImageSize.Width;
            }
            var proposedSize = new Size(textBounds.Width, textBounds.Height);
            var textSize = TextRenderer.MeasureText(hti.SubItem.Text, Font, proposedSize,
                                                    TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine |
                                                    TextFormatFlags.NoPrefix);
            textBounds.Width = textSize.Width;
            switch (hti.Column.TextAlign)
            {
                case HorizontalAlignment.Center:
                    textBounds.X += (cellBounds.Right - cellBounds.Left - textSize.Width) / 2;
                    break;

                case HorizontalAlignment.Right:
                    textBounds.X = cellBounds.Right - textSize.Width;
                    break;
            }
            if (textBounds.Contains(x, y))
            {
                hti.HitTestLocation = HitTestLocation.Text;
            }
        }

        protected virtual void CalculateOwnerDrawnHitTest(OlvListViewHitTestInfo hti, int x, int y)
        {
            /* If the click wasn't on an item, give up */
            if (hti.Item == null)
            {
                return;
            }
            /* If the list is showing column, but they clicked outside the columns, also give up */
            if (View == View.Details && hti.Column == null)
            {
                return;
            }
            /* Which renderer was responsible for drawing that point */
            var renderer = View == View.Details
                               ? (hti.Column.Renderer ?? DefaultRenderer)
                               : ItemRenderer;
            /* We can't decide who was responsible. Give up */
            if (renderer == null)
            {
                return;
            }
            /* Ask the responsible renderer politely what is at that point */
            renderer.HitTest(hti, x, y);
        }

        public virtual void PauseAnimations(bool isPause)
        {
            for (var i = 0; i < Columns.Count; i++)
            {
                var col = GetColumn(i);
                var renderer = col.Renderer as ImageRenderer;
                if (renderer != null)
                {
                    renderer.Paused = isPause;
                }
            }
        }

        public virtual void RebuildColumns()
        {
            ChangeToFilteredColumns(View);
        }

        public virtual void RemoveObject(object modelObject)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)(() => RemoveObject(modelObject)));
            }
            else
            {
                RemoveObjects(new[] {modelObject});
            }
        }

        public virtual void RemoveObjects(ICollection modelObjects)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)(() => RemoveObjects(modelObjects)));
                return;
            }
            if (modelObjects == null)
            {
                return;
            }
            BeginUpdate();
            try
            {
                /* Give the world a chance to cancel or change the added objects */
                var args = new ItemsRemovingEventArgs(modelObjects);
                OnItemsRemoving(args);
                if (args.Canceled)
                {
                    return;
                }
                modelObjects = args.ObjectsToRemove;
                TakeOwnershipOfObjects();
                var ourObjects = EnumerableToArray(Objects, false);
                foreach (var modelObject in modelObjects.Cast<object>().Where(modelObject => modelObject != null))
                {
                    // ReSharper disable PossibleMultipleEnumeration
                    ourObjects.Remove(modelObject);
                    // ReSharper restore PossibleMultipleEnumeration
                    var i = IndexOf(modelObject);
                    if (i >= 0)
                    {
                        Items.RemoveAt(i);
                    }
                }
                PostProcessRows();
                /* Tell the world that the list has changed */
                UnsubscribeNotifications(modelObjects);
                OnItemsChanged(new ItemsChangedEventArgs());
            }
            finally
            {
                EndUpdate();
            }
        }

        public virtual void SelectAll()
        {
            NativeMethods.SelectAllItems(this);
        }

        public void SetNativeBackgroundWatermark(Image image)
        {
            NativeMethods.SetBackgroundImage(this, image, true, false, 0, 0);
        }

        public void SetNativeBackgroundImage(Image image, int xOffset, int yOffset)
        {
            NativeMethods.SetBackgroundImage(this, image, false, false, xOffset, yOffset);
        }

        public void SetNativeBackgroundTiledImage(Image image)
        {
            NativeMethods.SetBackgroundImage(this, image, false, true, 0, 0);
        }

        public virtual void SetObjects(IEnumerable collection)
        {
            SetObjects(collection, false);
        }

        public virtual void SetObjects(IEnumerable collection, bool preserveState)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)(() => SetObjects(collection, preserveState)));
                return;
            }
            /* Give the world a chance (or a cookie) to cancel or change the assigned collection */
            var args = new ItemsChangingEventArgs(_objects, collection);
            OnItemsChanging(args);
            if (args.Canceled)
            {
                return;
            }
            collection = args.NewObjects;
            /* If we own the current list and they change to another list, we don't own it anymore */
            if (_isOwnerOfObjects && !_objects.Equals(collection))
            {
                _isOwnerOfObjects = false;
            }
            _objects = collection;
            BuildList(preserveState);
            /* Tell the world that the list has changed */
            UpdateNotificationSubscriptions(_objects);
            OnItemsChanged(new ItemsChangedEventArgs());
        }
        protected virtual void UpdateNotificationSubscriptions(IEnumerable collection)
        {
            if (!UseNotifyPropertyChanged)
            {
                return;
            }
            /* We could calculate a symmetric difference between the old models and the new models
             * except that we don't have the previous models at this point. */
            UnsubscribeNotifications(null);
            SubscribeNotifications(collection ?? Objects);
        }

        [Category("ObjectListView"),
         Description("Should ObjectListView listen for property changed events on the model objects?"),
         DefaultValue(false)]
        public bool UseNotifyPropertyChanged
        {
            get { return _useNotifyPropertyChanged; }
            set
            {
                if (_useNotifyPropertyChanged == value)
                {
                    return;
                }
                _useNotifyPropertyChanged = value;
                if (value)
                {
                    SubscribeNotifications(Objects);
                }
                else
                {
                    UnsubscribeNotifications(null);
                }
            }
        }
        
        private void SubscribeNotifications(IEnumerable models)
        {
            if (!UseNotifyPropertyChanged || models == null)
            {
                return;
            }
            foreach (var notifier in models.Cast<object>().OfType<INotifyPropertyChanged>().Where(notifier => !_subscribedModels.ContainsKey(notifier)))
            {
                notifier.PropertyChanged += HandleModelOnPropertyChanged;
                _subscribedModels[notifier] = notifier;
            }
        }

        private void UnsubscribeNotifications(IEnumerable models)
        {
            if (models == null)
            {
                foreach (INotifyPropertyChanged notifier in _subscribedModels.Keys)
                {
                    notifier.PropertyChanged -= HandleModelOnPropertyChanged;
                }
                _subscribedModels = new Hashtable();
            }
            else
            {
                foreach (var notifier in models.OfType<INotifyPropertyChanged>())
                {
                    notifier.PropertyChanged -= HandleModelOnPropertyChanged;
                    _subscribedModels.Remove(notifier);
                }
            }
        }

        private void HandleModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {            
            RefreshObject(sender);
        }        
    }
}
