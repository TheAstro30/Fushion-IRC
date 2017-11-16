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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Runtime.Serialization.Formatters;
using libolv.Filtering.Filters;
using libolv.Implementation;
using libolv.Implementation.Comparers;
using libolv.Implementation.Events;
using libolv.Implementation.Munger;
using libolv.Rendering.Styles;
using libolv.SubControls;

namespace libolv
{
    [Designer(typeof (ObjectListViewDesigner))]
    public partial class ObjectListView : ListView, ISupportInitialize
    {
        [Serializable]
        internal class ObjectListViewState
        {
            public int VersionNumber = 1;
            public int NumberOfColumns = 1;
            public View CurrentView;
            public int SortColumn = -1;
            public bool IsShowingGroups;
            public SortOrder LastSortOrder = SortOrder.None;
            public ArrayList ColumnIsVisible = new ArrayList();
            public ArrayList ColumnDisplayIndicies = new ArrayList();
            public ArrayList ColumnWidths = new ArrayList();
        }

        private class SuspendSelectionDisposable : IDisposable
        {
            private readonly ObjectListView _objectListView;

            public SuspendSelectionDisposable(ObjectListView objectListView)
            {
                _objectListView = objectListView;
                _objectListView.SuspendSelectionEvents();
            }

            public void Dispose()
            {
                _objectListView.ResumeSelectionEvents();
            }            
        }

        private static bool? _isVistaOrLater;
        private static bool? _isWin7OrLater;
        private static SmoothingMode _smoothingMode = SmoothingMode.HighQuality;
        private static TextRenderingHint _textRendereringHint = TextRenderingHint.SystemDefault;
        private int _freezeCount;

        private int _suspendSelectionEventCount; /* How many unmatched SuspendSelectionEvents() calls have been made? */
        private readonly List<GlassPanelForm> _glassPanels = new List<GlassPanelForm>(); /* The transparent panel that draws overlays */

        public const string SortIndicatorUpKey = "sort-indicator-up";
        public const string SortIndicatorDownKey = "sort-indicator-down";
        public const string CheckedKey = "checkbox-checked";
        public const string UncheckedKey = "checkbox-unchecked";
        public const string IndeterminateKey = "checkbox-indeterminate";

        /* Constructor */
        public ObjectListView()
        {
            Init();
        }

        private void Init()
        {
            ColumnClick += HandleColumnClick;
            Layout += HandleLayout;
            ColumnWidthChanging += HandleColumnWidthChanging;
            ColumnWidthChanged += HandleColumnWidthChanged;
            base.View = View.Details;
            DoubleBuffered = true; /* kill nasty flickers. hiss... me hates 'em */
            ShowSortIndicators = true;
            HasCollapsibleGroups = true;
            /* Setup the overlays that will be controlled by the IDE settings */
            InitializeStandardOverlays();
            InitializeEmptyListMsgOverlay();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
            {
                return;
            }
            foreach (var glassPanel in _glassPanels)
            {
                glassPanel.Unbind();
                glassPanel.Dispose();
            }
            _glassPanels.Clear();
            UnsubscribeNotifications(null);
        }

        /* Static properties */
        public static bool IsVistaOrLater
        {
            get
            {
                if (!_isVistaOrLater.HasValue)
                {
                    _isVistaOrLater = Environment.OSVersion.Version.Major >= 6;
                }
                return _isVistaOrLater.Value;
            }
        }

        public static bool IsWin7OrLater
        {
            get
            {
                if (!_isWin7OrLater.HasValue)
                {
                    /* For some reason, Win7 is v6.1, not v7.0 */
                    var version = Environment.OSVersion.Version;
                    _isWin7OrLater = version.Major > 6 || (version.Major == 6 && version.Minor > 0);
                }
                return _isWin7OrLater.Value;
            }
        }

        public static SmoothingMode SmoothingMode
        {
            get { return _smoothingMode; }
            set { _smoothingMode = value; }
        }

        public static TextRenderingHint TextRenderingHint
        {
            get { return _textRendereringHint; }
            set { _textRendereringHint = value; }
        }

        public static ArrayList EnumerableToArray(IEnumerable collection, bool alwaysCreate)
        {
            if (collection == null)
            {
                return new ArrayList();
            }
            if (!alwaysCreate)
            {
                var array = collection as ArrayList;
                if (array != null)
                {
                    return array;
                }
                var iList = collection as IList;
                if (iList != null)
                {
                    return ArrayList.Adapter(iList);
                }
            }
            var iCollection = collection as ICollection;
            if (iCollection != null)
            {
                return new ArrayList(iCollection);
            }
            var newObjects = new ArrayList();
            foreach (var x in collection)
            {
                newObjects.Add(x);
            }
            return newObjects;
        }

        public static bool IsEnumerableEmpty(IEnumerable collection)
        {
            return collection == null || (collection is string) || !collection.GetEnumerator().MoveNext();
        }

        public static bool IgnoreMissingAspects
        {
            get { return Munger.IgnoreMissingAspects; }
            set { Munger.IgnoreMissingAspects = value; }
        }

        public static bool ShowCellPaddingBounds { get; set; }

        /* Save/Restore State */
        public virtual byte[] SaveState()
        {
            var olvState = new ObjectListViewState
                               {
                                   VersionNumber = 1,
                                   NumberOfColumns = AllColumns.Count,
                                   CurrentView = View
                               };
            /* If we have a sort column, it is possible that it is not currently being shown, in which
             * case, it's Index will be -1. So we calculate its index directly. Technically, the sort
             * column does not even have to a member of AllColumns, in which case IndexOf will return -1,
             * which is works fine since we have no way of restoring such a column anyway. */
            if (PrimarySortColumn != null)
            {
                olvState.SortColumn = AllColumns.IndexOf(PrimarySortColumn);
            }
            olvState.LastSortOrder = PrimarySortOrder;
            olvState.IsShowingGroups = ShowGroups;
            if (AllColumns.Count > 0 && AllColumns[0].LastDisplayIndex == -1)
            {
                RememberDisplayIndicies();
            }
            foreach (var column in AllColumns)
            {
                olvState.ColumnIsVisible.Add(column.IsVisible);
                olvState.ColumnDisplayIndicies.Add(column.LastDisplayIndex);
                olvState.ColumnWidths.Add(column.Width);
            }
            /* Now that we have stored our state, convert it to a byte array */
            using (var ms = new MemoryStream())
            {
                var serializer = new BinaryFormatter
                                     {
                                         AssemblyFormat = FormatterAssemblyStyle.Simple
                                     };
                serializer.Serialize(ms, olvState);
                return ms.ToArray();
            }
        }

        public virtual bool RestoreState(byte[] state)
        {
            using (var ms = new MemoryStream(state))
            {
                var deserializer = new BinaryFormatter();
                ObjectListViewState olvState;
                try
                {
                    olvState = deserializer.Deserialize(ms) as ObjectListViewState;
                }
                catch (System.Runtime.Serialization.SerializationException)
                {
                    return false;
                }
                /* The number of columns has changed. We have no way to match old
                 * columns to the new ones, so we just give up. */
                if (olvState == null || olvState.NumberOfColumns != AllColumns.Count)
                {
                    return false;
                }
                if (olvState.SortColumn == -1)
                {
                    PrimarySortColumn = null;
                    PrimarySortOrder = SortOrder.None;
                }
                else
                {
                    PrimarySortColumn = AllColumns[olvState.SortColumn];
                    PrimarySortOrder = olvState.LastSortOrder;
                }
                for (var i = 0; i < olvState.NumberOfColumns; i++)
                {
                    var column = AllColumns[i];
                    column.Width = (int)olvState.ColumnWidths[i];
                    column.IsVisible = (bool)olvState.ColumnIsVisible[i];
                    column.LastDisplayIndex = (int)olvState.ColumnDisplayIndicies[i];
                }                
                // ReSharper disable RedundantCheckBeforeAssignment
                if (olvState.IsShowingGroups != ShowGroups)
                {
                    ShowGroups = olvState.IsShowingGroups;
                }
                // ReSharper restore RedundantCheckBeforeAssignment
                if (View == olvState.CurrentView)
                {
                    RebuildColumns();
                }
                else
                {
                    View = olvState.CurrentView;
                }
            }

            return true;
        }

        /* OLV accessing */
        public virtual OlvColumn GetColumn(int index)
        {
            return (OlvColumn)Columns[index];
        }

        public virtual OlvColumn GetColumn(string name)
        {
            return Columns.Cast<ColumnHeader>().Where(column => column.Text == name).Cast<OlvColumn>().FirstOrDefault();
        }

        public virtual List<OlvColumn> GetFilteredColumns(View view)
        {
            /* For both detail and tile view, the first column must be included. Normally, we would
             * use the ColumnHeader.Index property, but if the header is not currently part of a ListView
             * that property returns -1. So, we track the index of
             * the column header, and always include the first header. */
            var index = 0;
            return AllColumns.FindAll(x => (index++ == 0) || x.IsVisible);
        }

        public virtual int GetItemCount()
        {
            return Items.Count;
        }

        public virtual OlvListItem GetItem(int index)
        {
            if (index < 0 || index >= GetItemCount())
            {
                return null;
            }
            return (OlvListItem)Items[index];
        }

        public virtual object GetModelObject(int index)
        {
            var item = GetItem(index);
            return item == null ? null : item.RowObject;
        }

        public virtual OlvListItem GetItemAt(int x, int y, out OlvColumn hitColumn)
        {
            hitColumn = null;
            var info = HitTest(x, y);
            if (info.Item == null)
            {
                return null;
            }
            if (info.SubItem != null)
            {
                var subItemIndex = info.Item.SubItems.IndexOf(info.SubItem);
                hitColumn = GetColumn(subItemIndex);
            }
            return (OlvListItem)info.Item;
        }

        public virtual OlvListSubItem GetSubItem(int index, int columnIndex)
        {
            var olvi = GetItem(index);
            return olvi == null ? null : olvi.GetSubItem(columnIndex);
        }

        /* Object manipulation */
        public virtual void EnsureGroupVisible(ListViewGroup lvg)
        {
            if (!ShowGroups || lvg == null)
            {
                return;
            }
            var groupIndex = Groups.IndexOf(lvg);
            if (groupIndex <= 0)
            {
                /* There is no easy way to scroll back to the beginning of the list */
                var delta = 0 - NativeMethods.GetScrollPosition(this, false);
                NativeMethods.Scroll(this, 0, delta);
            }
            else
            {
                /* Find the display rectangle of the last item in the previous group */
                var previousGroup = Groups[groupIndex - 1];
                var lastItemInGroup = previousGroup.Items[previousGroup.Items.Count - 1];
                var r = GetItemRect(lastItemInGroup.Index);
                /* Scroll so that the last item of the previous group is just out of sight,
                 * which will make the desired group header visible. */
                var delta = r.Y + r.Height/2;
                NativeMethods.Scroll(this, 0, delta);
            }
        }

        public virtual void EnsureModelVisible(Object modelObject)
        {
            var index = IndexOf(modelObject);
            if (index >= 0)
            {
                EnsureVisible(index);
            }
        }

        [Obsolete("Use SelectedObject property instead of this method")]
        public virtual object GetSelectedObject()
        {
            return SelectedObject;
        }

        [Obsolete("Use SelectedObjects property instead of this method")]
        public virtual ArrayList GetSelectedObjects()
        {
            return EnumerableToArray(SelectedObjects, false);
        }

        [Obsolete("Use CheckedObject property instead of this method")]
        public virtual object GetCheckedObject()
        {
            return CheckedObject;
        }

        [Obsolete("Use CheckedObjects property instead of this method")]
        public virtual ArrayList GetCheckedObjects()
        {
            return EnumerableToArray(CheckedObjects, false);
        }

        public virtual int IndexOf(Object modelObject)
        {
            for (var i = 0; i < GetItemCount(); i++)
            {
                if (GetModelObject(i).Equals(modelObject))
                {
                    return i;
                }
            }
            return -1;
        }

        public virtual void RefreshItem(OlvListItem olvi)
        {
            olvi.UseItemStyleForSubItems = true;
            olvi.SubItems.Clear();
            FillInValues(olvi, olvi.RowObject);
            PostProcessOneRow(olvi.Index, GetDisplayOrderOfItemIndex(olvi.Index), olvi);
        }

        public virtual void RefreshObject(object modelObject)
        {
            RefreshObjects(new[] {modelObject});
        }

        public virtual void RefreshObjects(IList modelObjects)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)(() => RefreshObjects(modelObjects)));
                return;
            }
            foreach (var olvi in modelObjects.Cast<object>().Select(ModelToItem).Where(olvi => olvi != null))
            {
                RefreshItem(olvi);
            }
        }

        public virtual void RefreshSelectedObjects()
        {
            foreach (ListViewItem lvi in SelectedItems)
            {
                RefreshItem((OlvListItem)lvi);
            }
        }

        public virtual void SelectObject(object modelObject)
        {
            SelectObject(modelObject, false);
        }

        public virtual void SelectObject(object modelObject, bool setFocus)
        {
            var olvi = ModelToItem(modelObject);
            if (olvi == null) { return; }
            olvi.Selected = true;
            if (setFocus)
            {
                olvi.Focused = true;
            }
        }

        public virtual void SelectObjects(IList modelObjects)
        {
            SelectedIndices.Clear();
            if (modelObjects == null)
            {
                return;
            }
            foreach (var olvi in modelObjects.Cast<object>().Select(ModelToItem).Where(olvi => olvi != null))
            {
                olvi.Selected = true;
            }
        }

        /* Freezing/Suspending */
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool Frozen
        {
            get { return _freezeCount > 0; }
            set
            {
                if (value)
                {
                    Freeze();
                }
                else if (_freezeCount > 0)
                {
                    _freezeCount = 1;
                    Unfreeze();
                }
            }
        }

        public virtual void Freeze()
        {
            _freezeCount++;
            OnFreezing(new FreezeEventArgs(_freezeCount));
        }

        public virtual void Unfreeze()
        {
            if (_freezeCount <= 0)
            {
                return;
            }
            _freezeCount--;
            if (_freezeCount == 0)
            {
                DoUnfreeze();
            }
            OnFreezing(new FreezeEventArgs(_freezeCount));
        }

        protected virtual void DoUnfreeze()
        {
            ResizeFreeSpaceFillingColumns();
            BuildList();
        }

        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected bool SelectionEventsSuspended
        {
            get { return _suspendSelectionEventCount > 0; }
        }

        protected void SuspendSelectionEvents()
        {
            _suspendSelectionEventCount++;
        }

        protected void ResumeSelectionEvents()
        {
            Debug.Assert(SelectionEventsSuspended, "Mismatched called to ResumeSelectionEvents()");
            _suspendSelectionEventCount--;
        }

        protected IDisposable SuspendSelectionEventsDuring()
        {
            return new SuspendSelectionDisposable(this);
        }

        /* Column sorting */
        public new void Sort()
        {
            Sort(PrimarySortColumn, PrimarySortOrder);
        }

        public virtual void Sort(string columnToSortName)
        {
            Sort(GetColumn(columnToSortName), PrimarySortOrder);
        }

        public virtual void Sort(int columnToSortIndex)
        {
            if (columnToSortIndex >= 0 && columnToSortIndex < Columns.Count)
            {
                Sort(GetColumn(columnToSortIndex), PrimarySortOrder);
            }
        }

        public virtual void Sort(OlvColumn columnToSort)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)(() => Sort(columnToSort)));
            }
            else
            {
                Sort(columnToSort, PrimarySortOrder);
            }
        }

        public virtual void Sort(OlvColumn columnToSort, SortOrder order)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)(() => Sort(columnToSort, order)));
            }
            else
            {
                DoSort(columnToSort, order);
                PostProcessRows();
            }
        }

        private void DoSort(OlvColumn columnToSort, SortOrder order)
        {
            /* Sanity checks */
            if (GetItemCount() == 0 || Columns.Count == 0)
            {
                return;
            }
            /* Fill in default values, if the parameters don't make sense */
            if (ShowGroups)
            {
                columnToSort = columnToSort ?? GetColumn(0);
                if (order == SortOrder.None)
                {
                    order = Sorting;
                    if (order == SortOrder.None)
                    {
                        order = SortOrder.Ascending;
                    }
                }
            }
            /* Give the world a chance to fiddle with or completely avoid the sorting process */
            var args = BuildBeforeSortingEventArgs(columnToSort, order);
            OnBeforeSorting(args);
            if (args.Canceled)
            {
                return;
            }
            /* Virtual lists don't preserve selection, so we have to do it specifically
             * THINK: Do we need to preserve focus too? */
            var selection = VirtualMode ? SelectedObjects : null;
            SuspendSelectionEvents();
            ClearHotItem();
            /* Finally, do the work of sorting, unless an event handler has already done the sorting for us */
            if (!args.Handled)
            {
                /* Sanity checks */
                if (args.ColumnToSort != null && args.SortOrder != SortOrder.None)
                {
                    if (ShowGroups)
                    {
                        BuildGroups(args.ColumnToGroupBy, args.GroupByOrder, args.ColumnToSort, args.SortOrder,
                                    args.SecondaryColumnToSort, args.SecondarySortOrder);
                    }
                    else if (CustomSorter != null)
                    {
                        CustomSorter(args.ColumnToSort, args.SortOrder);
                    }
                    else
                    {
                        ListViewItemSorter = new ColumnComparer(args.ColumnToSort, args.SortOrder,
                                                                args.SecondaryColumnToSort, args.SecondarySortOrder);
                    }
                }
            }
            if (ShowSortIndicators)
            {
                ShowSortIndicator(args.ColumnToSort, args.SortOrder);
            }
            PrimarySortColumn = args.ColumnToSort;
            PrimarySortOrder = args.SortOrder;
            if (selection != null && selection.Count > 0)
            {
                SelectedObjects = selection;
            }
            ResumeSelectionEvents();
            RefreshHotItem();
            OnAfterSorting(new AfterSortingEventArgs(args));
        }

        public virtual void ShowSortIndicator()
        {
            if (ShowSortIndicators && PrimarySortOrder != SortOrder.None)
            {
                ShowSortIndicator(PrimarySortColumn, PrimarySortOrder);
            }
        }

        protected virtual void ShowSortIndicator(OlvColumn columnToSort, SortOrder sortOrder)
        {
            var imageIndex = -1;
            if (!NativeMethods.HasBuiltinSortIndicators())
            {
                /* If we can't use builtin image, we have to make and then locate the index of the
                 * sort indicator we want to use. SortOrder.None doesn't show an image. */
                if (SmallImageList == null || !SmallImageList.Images.ContainsKey(SortIndicatorUpKey))
                {
                    MakeSortIndicatorImages();
                }
                if (SmallImageList != null)
                {
                    imageIndex = SmallImageList.Images.IndexOfKey(sortOrder == SortOrder.Ascending
                                                                      ? SortIndicatorUpKey
                                                                      : SortIndicatorDownKey);
                }
            }
            /* Set the image for each column */
            for (var i = 0; i < Columns.Count; i++)
            {
                if (columnToSort != null && i == columnToSort.Index)
                {
                    NativeMethods.SetColumnImage(this, i, sortOrder, imageIndex);
                }
                else
                {
                    NativeMethods.SetColumnImage(this, i, SortOrder.None, -1);
                }
            }
        }

        protected virtual void MakeSortIndicatorImages()
        {
            /* Don't mess with the image list in design mode */
            if (DesignMode)
            {
                return;
            }
            var il = SmallImageList ?? new ImageList
                                           {
                                               ImageSize = new Size(16, 16),
                                               ColorDepth = ColorDepth.Depth32Bit
                                           };
            /* This arrangement of points works well with (16,16) images, and OK with others */
            var midX = il.ImageSize.Width/2;
            var midY = (il.ImageSize.Height/2) - 1;
            var deltaX = midX - 2;
            var deltaY = deltaX/2;
            if (il.Images.IndexOfKey(SortIndicatorUpKey) == -1)
            {
                var pt1 = new Point(midX - deltaX, midY + deltaY);
                var pt2 = new Point(midX, midY - deltaY - 1);
                var pt3 = new Point(midX + deltaX, midY + deltaY);
                il.Images.Add(SortIndicatorUpKey, MakeTriangleBitmap(il.ImageSize, new[] {pt1, pt2, pt3}));
            }
            if (il.Images.IndexOfKey(SortIndicatorDownKey) == -1)
            {
                var pt1 = new Point(midX - deltaX, midY - deltaY);
                var pt2 = new Point(midX, midY + deltaY);
                var pt3 = new Point(midX + deltaX, midY - deltaY);
                il.Images.Add(SortIndicatorDownKey, MakeTriangleBitmap(il.ImageSize, new[] {pt1, pt2, pt3}));
            }
            SmallImageList = il;
        }

        private static Bitmap MakeTriangleBitmap(Size sz, Point[] pts)
        {
            var bm = new Bitmap(sz.Width, sz.Height);
            var g = Graphics.FromImage(bm);
            g.FillPolygon(new SolidBrush(Color.Gray), pts);
            return bm;
        }

        public virtual void Unsort()
        {
            ShowGroups = false;
            PrimarySortColumn = null;
            PrimarySortOrder = SortOrder.None;
            BuildList();
        }

        /* Utilities */
        protected virtual void CreateGroups(IEnumerable<OlvGroup> groups)
        {
            Groups.Clear();
            /* The group must be added before it is given items, otherwise an exception is thrown (is this documented?) */
            foreach (var group in groups)
            {
                group.InsertGroupOldStyle(this);
                group.SetItemsOldStyle();
            }
        }

        protected virtual void CorrectSubItemColors(ListViewItem olvi)
        {
            /* Empty */
        }

        protected virtual void FillInValues(OlvListItem lvi, object rowObject)
        {
            if (Columns.Count == 0)
            {
                return;
            }
            var subItem = MakeSubItem(rowObject, GetColumn(0));
            lvi.SubItems[0] = subItem;
            lvi.ImageSelector = subItem.ImageSelector;
            /* Only Details and Tile views have subitems */
            switch (View)
            {
                case View.Details:
                    for (var i = 1; i < Columns.Count; i++)
                    {
                        lvi.SubItems.Add(MakeSubItem(rowObject, GetColumn(i)));
                    }
                    break;

                case View.Tile:
                    for (var i = 1; i < Columns.Count; i++)
                    {
                        var column = GetColumn(i);
                        if (column.IsTileViewColumn)
                        {
                            lvi.SubItems.Add(MakeSubItem(rowObject, column));
                        }
                    }
                    break;
            }
            /* Give the item the same font/colors as the control */
            lvi.Font = Font;
            lvi.BackColor = BackColor;
            lvi.ForeColor = ForeColor;
            /* Set the check state of the row, if we are showing check boxes */
            if (CheckBoxes)
            {
                var state = GetCheckState(lvi.RowObject);
                if (state.HasValue)
                {
                    lvi.CheckState = state.Value;
                }
            }
            /* Give the RowFormatter a chance to mess with the item */
            if (RowFormatter != null)
            {
                RowFormatter(lvi);
            }
        }

        private OlvListSubItem MakeSubItem(object rowObject, OlvColumn column)
        {
            var cellValue = column.GetValue(rowObject);
            var subItem = new OlvListSubItem(cellValue,
                                             column.ValueToString(cellValue),
                                             column.GetImage(rowObject));
            if (UseHyperlinks && column.Hyperlink)
            {
                var args = new IsHyperlinkEventArgs
                               {
                                   ListView = this,
                                   Model = rowObject,
                                   Column = column,
                                   Text = subItem.Text,
                                   Url = subItem.Text
                               };
                OnIsHyperlink(args);
                subItem.Url = args.Url;
            }
            return subItem;
        }

        private void ApplyHyperlinkStyle(OlvListItem olvi)
        {
            olvi.UseItemStyleForSubItems = false;
            /* If subitem 0 is given a back color, the item back color is changed too.
             * So we have to remember it here so we can used it even if subitem 0 is changed. */
            Color itemBackColor = olvi.BackColor;
            for (var i = 0; i < Columns.Count; i++)
            {
                var subItem = olvi.GetSubItem(i);
                if (subItem == null)
                {
                    continue;
                }
                var column = GetColumn(i);
                subItem.BackColor = itemBackColor;
                if (column.Hyperlink && !string.IsNullOrEmpty(subItem.Url))
                {
                    ApplyCellStyle(olvi, i,
                                   IsUrlVisited(subItem.Url)
                                       ? HyperlinkStyle.Visited
                                       : HyperlinkStyle.Normal);
                }
            }
        }

        protected virtual void ForceSubItemImagesExStyle()
        {
            /* Virtual lists can't show subitem images natively, so don't turn on this flag */
            if (!VirtualMode)
            {
                NativeMethods.ForceSubItemImagesExStyle(this);
            }
        }

        protected virtual int GetActualImageIndex(Object imageSelector)
        {
            if (imageSelector == null)
            {
                return -1;
            }
            if (imageSelector is Int32)
            {
                return (int)imageSelector;
            }
            var imageSelectorAsString = imageSelector as String;
            if (imageSelectorAsString != null && SmallImageList != null)
            {
                return SmallImageList.Images.IndexOfKey(imageSelectorAsString);
            }
            return -1;
        }

        public virtual String GetHeaderToolTip(int columnIndex)
        {
            var column = GetColumn(columnIndex);
            if (column == null)
            {
                return null;
            }
            var tooltip = column.ToolTipText;
            if (HeaderToolTipGetter != null)
            {
                tooltip = HeaderToolTipGetter(column);
            }
            return tooltip;
        }

        public virtual String GetCellToolTip(int columnIndex, int rowIndex)
        {
            if (CellToolTipGetter != null)
            {
                return CellToolTipGetter(GetColumn(columnIndex), GetModelObject(rowIndex));
            }
            /* Show the URL in the tooltip if it's different to the text */
            if (columnIndex >= 0)
            {
                var subItem = GetSubItem(rowIndex, columnIndex);
                if (subItem != null && !string.IsNullOrEmpty(subItem.Url) && subItem.Url != subItem.Text &&
                    HotCellHitLocation == HitTestLocation.Text)
                {
                    return subItem.Url;
                }
            }
            return null;
        }

        public virtual OlvListItem ModelToItem(object modelObject)
        {
            if (modelObject == null)
            {
                return null;
            }
            return Items.Cast<OlvListItem>().FirstOrDefault(olvi => olvi.RowObject != null && olvi.RowObject.Equals(modelObject));
        }

        protected virtual void PostProcessRows()
        {
            /* If this method is called during a BeginUpdate/EndUpdate pair, changes to the
             * Items collection are cached. Getting the Count flushes that cache. */
#pragma warning disable 168
            var count = Items.Count;
#pragma warning restore 168
            var i = 0;
            if (ShowGroups)
            {
                foreach (var olvi in from ListViewGroup @group in Groups from OlvListItem olvi in @group.Items select olvi)
                {
                    PostProcessOneRow(olvi.Index, i, olvi);
                    i++;
                }
            }
            else
            {
                foreach (OlvListItem olvi in Items)
                {
                    PostProcessOneRow(olvi.Index, i, olvi);
                    i++;
                }
            }
        }

        protected virtual void PostProcessOneRow(int rowIndex, int displayIndex, OlvListItem olvi)
        {
            if (UseAlternatingBackColors && View == View.Details)
            {
                olvi.BackColor = displayIndex%2 == 1 ? AlternateRowBackColorOrDefault : BackColor;
            }
            if (ShowImagesOnSubItems && !VirtualMode)
            {
                SetSubItemImages(rowIndex, olvi);
            }
            if (UseHyperlinks)
            {
                ApplyHyperlinkStyle(olvi);
            }
            TriggerFormatRowEvent(rowIndex, displayIndex, olvi);
        }

        [Obsolete("This method is no longer used. Override PostProcessOneRow() to achieve a similar result")]
        protected virtual void PrepareAlternateBackColors()
        {
            /* Empty */
        }

        [Obsolete("This method is not longer maintained and will be removed", false)]
        protected virtual void SetAllSubItemImages()
        {
            /* Empty */
        }

        protected virtual void SetSubItemImages(int rowIndex, OlvListItem item)
        {
            SetSubItemImages(rowIndex, item, false);
        }

        protected virtual void SetSubItemImages(int rowIndex, OlvListItem item, bool shouldClearImages)
        {
            if (!ShowImagesOnSubItems || OwnerDraw)
            {
                return;
            }
            for (var i = 1; i < item.SubItems.Count; i++)
            {
                SetSubItemImage(rowIndex, i, item.GetSubItem(i), shouldClearImages);
            }
        }

        public virtual void SetSubItemImage(int rowIndex, int subItemIndex, OlvListSubItem subItem, bool shouldClearImages)
        {
            var imageIndex = GetActualImageIndex(subItem.ImageSelector);
            if (shouldClearImages || imageIndex != -1)
            {
                NativeMethods.SetSubItemImage(this, rowIndex, subItemIndex, imageIndex);
            }
        }

        protected virtual void TakeOwnershipOfObjects()
        {
            if (_isOwnerOfObjects)
            {
                return;
            }
            _isOwnerOfObjects = true;
            _objects = EnumerableToArray(_objects, true);
        }

        protected virtual void TriggerFormatRowEvent(int rowIndex, int displayIndex, OlvListItem olvi)
        {
            var args = new FormatRowEventArgs
                           {
                               ListView = this,
                               RowIndex = rowIndex,
                               DisplayIndex = displayIndex,
                               Item = olvi,
                               UseCellFormatEvents = UseCellFormatEvents
                           };
            OnFormatRow(args);
            if (!args.UseCellFormatEvents || View != View.Details) { return; }
            /* If a cell isn't given its own color, it should use the color of the item.
             * However, there is a bug in the .NET framework where the cell are given
             * the color of the ListView instead. So we have to explicitly give each
             * cell the back color that it should have. */
            olvi.UseItemStyleForSubItems = false;
            var backColor = olvi.BackColor;
            for (var i = 0; i < Columns.Count; i++)
            {
                olvi.SubItems[i].BackColor = backColor;
            }
            /* Fire one event per cell */
            var args2 = new FormatCellEventArgs
                            {
                                ListView = this, 
                                RowIndex = rowIndex,
                                DisplayIndex = displayIndex, 
                                Item = olvi
                            };
            for (var i = 0; i < Columns.Count; i++)
            {
                args2.ColumnIndex = i;
                args2.Column = GetColumn(i);
                args2.SubItem = olvi.GetSubItem(i);
                OnFormatCell(args2);
            }
        }

        public virtual void Reset()
        {
            Clear();
            AllColumns.Clear();
            ClearObjects();
            PrimarySortColumn = null;
            SecondarySortColumn = null;
            ClearPersistentCheckState();
            ClearUrlVisited();
            ClearHotItem();
        }

        /* ISupportInitialize Members */
        void ISupportInitialize.BeginInit()
        {
            Frozen = true;
        }

        void ISupportInitialize.EndInit()
        {
            if (RowHeight != -1)
            {
                SmallImageList = SmallImageList;
                if (CheckBoxes)
                {
                    InitializeStateImageList();
                }
            }
            if (UseCustomSelectionColors)
            {
                EnableCustomSelectionColors();
            }
            if (UseSubItemCheckBoxes || (VirtualMode && CheckBoxes))
            {
                SetupSubItemCheckBoxes();
            }
            Frozen = false;
        }

        /* Image list manipulation */
        private void SetupBaseImageList()
        {
            /* If a row height hasn't been set, or an image list has been give which is the required size, just assign it */
            if (_rowHeight == -1 || View != View.Details || (_shadowedImageList != null && _shadowedImageList.ImageSize.Height == _rowHeight))
            {
                BaseSmallImageList = _shadowedImageList;
            }
            else
            {
                var width = (_shadowedImageList == null ? 16 : _shadowedImageList.ImageSize.Width);
                BaseSmallImageList = MakeResizedImageList(width, _rowHeight, _shadowedImageList);
            }
        }

        private ImageList MakeResizedImageList(int width, int height, ImageList source)
        {
            var il = new ImageList
                         {
                             ImageSize = new Size(width, height), 
                             ColorDepth = ColorDepth.Depth32Bit
                         };
            /* If there's nothing to copy, just return the new list */
            if (source == null)
            {
                return il;
            }
            il.TransparentColor = source.TransparentColor;
            il.ColorDepth = source.ColorDepth;
            /* Fill the imagelist with resized copies from the source */
            for (var i = 0; i < source.Images.Count; i++)
            {
                var bm = MakeResizedImage(width, height, source.Images[i], source.TransparentColor);
                il.Images.Add(bm);
            }
            /* Give each image the same key it has in the original */
            foreach (var key in source.Images.Keys)
            {
                il.Images.SetKeyName(source.Images.IndexOfKey(key), key);
            }
            return il;
        }

        private static Bitmap MakeResizedImage(int width, int height, Image image, Color transparent)
        {
            var bm = new Bitmap(width, height);
            var g = Graphics.FromImage(bm);
            g.Clear(transparent);
            var x = Math.Max(0, (bm.Size.Width - image.Size.Width)/2);
            var y = Math.Max(0, (bm.Size.Height - image.Size.Height)/2);
            g.DrawImage(image, x, y, image.Size.Width, image.Size.Height);
            return bm;
        }

        protected virtual void InitializeStateImageList()
        {
            if (DesignMode)
            {
                return;
            }
            if (StateImageList == null)
            {
                StateImageList = new ImageList
                                     {
                                         ImageSize = new Size(16, 16),
                                         ColorDepth = ColorDepth.Depth32Bit
                                     };
            }
            if (RowHeight != -1 && View == View.Details && StateImageList.ImageSize.Height != RowHeight)
            {
                StateImageList = new ImageList
                                     {
                                         ImageSize = new Size(16, RowHeight),
                                         ColorDepth = ColorDepth.Depth32Bit
                                     };
            }
            if (!CheckBoxes)
            {
                return;
            }
            /* The internal logic of ListView cycles through the state images when the primary
             * checkbox is clicked. So we have to get exactly the right number of images in the 
             * image list. */
            if (StateImageList.Images.Count == 0)
            {
                AddCheckStateBitmap(StateImageList, UncheckedKey, CheckBoxState.UncheckedNormal);
            }
            if (StateImageList.Images.Count <= 1)
            {
                AddCheckStateBitmap(StateImageList, CheckedKey, CheckBoxState.CheckedNormal);
            }
            if (TriStateCheckBoxes && StateImageList.Images.Count <= 2)
            {
                AddCheckStateBitmap(StateImageList, IndeterminateKey, CheckBoxState.MixedNormal);
            }
            else
            {
                if (StateImageList.Images.ContainsKey(IndeterminateKey))
                    StateImageList.Images.RemoveByKey(IndeterminateKey);
            }
        }

        public virtual void SetupSubItemCheckBoxes()
        {
            ShowImagesOnSubItems = true;
            if (SmallImageList == null || !SmallImageList.Images.ContainsKey(CheckedKey))
            {
                InitializeCheckBoxImages();
            }
        }

        protected virtual void InitializeCheckBoxImages()
        {
            /* Don't mess with the image list in design mode */
            if (DesignMode)
            {
                return;
            }
            var il = SmallImageList ?? new ImageList
                                           {
                                               ImageSize = new Size(16, 16),
                                               ColorDepth = ColorDepth.Depth32Bit
                                           };
            AddCheckStateBitmap(il, CheckedKey, CheckBoxState.CheckedNormal);
            AddCheckStateBitmap(il, UncheckedKey, CheckBoxState.UncheckedNormal);
            AddCheckStateBitmap(il, IndeterminateKey, CheckBoxState.MixedNormal);
            SmallImageList = il;
        }

        private void AddCheckStateBitmap(ImageList il, string key, CheckBoxState boxState)
        {
            var b = new Bitmap(il.ImageSize.Width, il.ImageSize.Height);
            var g = Graphics.FromImage(b);
            g.Clear(il.TransparentColor);
            var location = new Point(b.Width/2 - 5, b.Height/2 - 6);
            CheckBoxRenderer.DrawCheckBox(g, location, boxState);
            il.Images.Add(key, b);
        }

        /* Owner drawing */
        protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
            base.OnDrawColumnHeader(e);
        }

        protected override void OnDrawItem(DrawListViewItemEventArgs e)
        {
            if (View == View.Details)
            {
                e.DrawDefault = false;
            }
            else
            {
                if (ItemRenderer == null)
                {
                    e.DrawDefault = true;
                }
                else
                {
                    var row = ((OlvListItem)e.Item).RowObject;
                    e.DrawDefault = !ItemRenderer.RenderItem(e, e.Graphics, e.Bounds, row);
                }
            }
            if (e.DrawDefault)
            {
                base.OnDrawItem(e);
            }
        }

        protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
        {            
            /* Don't try to do owner drawing at design time */
            if (DesignMode)
            {
                e.DrawDefault = true;
                return;
            }
            /* Calculate where the subitem should be drawn */
            var r = e.Bounds;
            /* Get the special renderer for this column. If there isn't one, use the default draw mechanism. */
            var column = GetColumn(e.ColumnIndex);
            var renderer = column.Renderer ?? DefaultRenderer;
            /* Get a graphics context for the renderer to use.
             * But we have more complications. Virtual lists have a nasty habit of drawing column 0
             * whenever there is any mouse move events over a row, and doing it in an un-double-buffered manner,
             * which results in nasty flickers! There are also some unbuffered draw when a mouse is first
             * hovered over column 0 of a normal row. So, to avoid all complications,
             * we always manually double-buffer the drawing.
             * Except with Mono, which doesn't seem to handle double buffering at all :-( */
            var buffer = BufferedGraphicsManager.Current.Allocate(e.Graphics, r);
            var g = buffer.Graphics;
            g.TextRenderingHint = TextRenderingHint;
            g.SmoothingMode = SmoothingMode;
            /* Finally, give the renderer a chance to draw something */
            e.DrawDefault = !renderer.RenderSubItem(e, g, r, ((OlvListItem)e.Item).RowObject);
            if (!e.DrawDefault)
            {
                buffer.Render();
            }
            buffer.Dispose();
        }

        /* Hot row and cell handling */
        public virtual void ClearHotItem()
        {
            UpdateHotItem(new Point(-1, -1));
        }

        public virtual void RefreshHotItem()
        {
            UpdateHotItem(PointToClient(Cursor.Position));
        }

        protected virtual void UpdateHotItem(Point pt)
        {
            UpdateHotItem(OlvHitTest(pt.X, pt.Y));
        }

        protected virtual void UpdateHotItem(OlvListViewHitTestInfo hti)
        {
            if (!UseHotItem && !UseHyperlinks)
            {
                return;
            }
            var newHotRow = hti.RowIndex;
            var newHotColumn = hti.ColumnIndex;
            var newHotCellHitLocation = hti.HitTestLocation;
            var newHotCellHitLocationEx = hti.HitTestLocationEx;
            var newHotGroup = hti.Group;
            /* In non-details view, we treat any hit on a row as if it were a hit
             * on column 0 -- which (effectively) it is! */
            if (newHotRow >= 0 && View != View.Details)
            {
                newHotColumn = 0;
            }
            if (HotRowIndex == newHotRow &&
                HotColumnIndex == newHotColumn &&
                HotCellHitLocation == newHotCellHitLocation &&
                HotCellHitLocationEx == newHotCellHitLocationEx &&
                HotGroup == newHotGroup)
            {
                return;
            }
            /* Trigger the hotitem changed event */
            var args = new HotItemChangedEventArgs
                           {
                               HotCellHitLocation = newHotCellHitLocation,
                               HotCellHitLocationEx = newHotCellHitLocationEx,
                               HotColumnIndex = newHotColumn,
                               HotRowIndex = newHotRow,
                               HotGroup = newHotGroup,
                               OldHotCellHitLocation = HotCellHitLocation,
                               OldHotCellHitLocationEx = HotCellHitLocationEx,
                               OldHotColumnIndex = HotColumnIndex,
                               OldHotRowIndex = HotRowIndex,
                               OldHotGroup = HotGroup
                           };
            OnHotItemChanged(args);
            /* Update the state of the control */
            HotRowIndex = newHotRow;
            HotColumnIndex = newHotColumn;
            HotCellHitLocation = newHotCellHitLocation;
            HotCellHitLocationEx = newHotCellHitLocationEx;
            HotGroup = newHotGroup;
            /* If the event handler handled it complete, don't do anything else */
            if (args.Handled)
            {
                return;
            }
            BeginUpdate();
            try
            {
                Invalidate();
                if (args.OldHotRowIndex != -1)
                {
                    UnapplyHotItem(args.OldHotRowIndex);
                }
                if (HotRowIndex != -1)
                {
                    /* Virtual lists apply hot item style when fetching their rows */
                    if (VirtualMode)
                    {
                        RedrawItems(HotRowIndex, HotRowIndex, true);
                    }
                    else
                    {
                        UpdateHotRow(HotRowIndex, HotColumnIndex, HotCellHitLocation, hti.Item);
                    }
                }
                if (UseHotItem && HotItemStyle != null && HotItemStyle.Overlay != null)
                {
                    RefreshOverlays();
                }
            }
            finally
            {
                EndUpdate();
            }
        }

        protected virtual void UpdateHotRow(OlvListItem olvi)
        {
            UpdateHotRow(HotRowIndex, HotColumnIndex, HotCellHitLocation, olvi);
        }

        protected virtual void UpdateHotRow(int rowIndex, int columnIndex, HitTestLocation hitLocation, OlvListItem olvi)
        {
            if (rowIndex < 0 || columnIndex < 0)
            {
                return;
            }
            if (UseHyperlinks)
            {
                var column = GetColumn(columnIndex);
                var subItem = olvi.GetSubItem(columnIndex);
                if (column.Hyperlink && hitLocation == HitTestLocation.Text && !string.IsNullOrEmpty(subItem.Url))
                {
                    ApplyCellStyle(olvi, columnIndex, HyperlinkStyle.Over);
                    Cursor = HyperlinkStyle.OverCursor ?? Cursors.Default;
                }
                else
                {
                    Cursor = Cursors.Default;
                }
            }
            if (!UseHotItem) { return; }
            if (!olvi.Selected)
            {
                ApplyRowStyle(olvi, HotItemStyle);
            }
        }

        protected virtual void ApplyRowStyle(OlvListItem olvi, IItemStyle style)
        {
            if (style == null)
            {
                return;
            }
            if (FullRowSelect || View != View.Details)
            {
                if (style.Font != null)
                {
                    olvi.Font = style.Font;
                }
                if (style.FontStyle != FontStyle.Regular)
                {
                    olvi.Font = new Font(olvi.Font ?? Font, style.FontStyle);
                }
                if (!style.ForeColor.IsEmpty)
                {
                    if (olvi.UseItemStyleForSubItems)
                    {
                        olvi.ForeColor = style.ForeColor;
                    }
                    else
                    {
                        foreach (ListViewItem.ListViewSubItem x in olvi.SubItems)
                        {
                            x.ForeColor = style.ForeColor;
                        }
                    }
                }
                if (!style.BackColor.IsEmpty)
                {
                    if (olvi.UseItemStyleForSubItems)
                    {
                        olvi.BackColor = style.BackColor;
                    }
                    else
                    {
                        foreach (ListViewItem.ListViewSubItem x in olvi.SubItems)
                        {
                            x.BackColor = style.BackColor;
                        }
                    }
                }
            }
            else
            {
                olvi.UseItemStyleForSubItems = false;
                foreach (ListViewItem.ListViewSubItem x in olvi.SubItems)
                {
                    x.BackColor = style.BackColor.IsEmpty ? olvi.BackColor : style.BackColor;
                }
                ApplyCellStyle(olvi, 0, style);
            }
        }

        protected virtual void ApplyCellStyle(OlvListItem olvi, int columnIndex, IItemStyle style)
        {
            if (style == null)
            {
                return;
            }
            /* Don't apply formatting to subitems when not in Details view */
            if (View != View.Details && columnIndex > 0)
            {
                return;
            }
            olvi.UseItemStyleForSubItems = false;
            var subItem = olvi.SubItems[columnIndex];
            if (style.Font != null)
            {
                subItem.Font = style.Font;
            }
            if (style.FontStyle != FontStyle.Regular)
            {
                subItem.Font = new Font(subItem.Font ?? olvi.Font ?? Font, style.FontStyle);
            }
            if (!style.ForeColor.IsEmpty)
            {
                subItem.ForeColor = style.ForeColor;
            }
            if (!style.BackColor.IsEmpty)
            {
                subItem.BackColor = style.BackColor;
            }
        }

        protected virtual void UnapplyHotItem(int index)
        {
            Cursor = Cursors.Default;
            /* Virtual lists will apply the appropriate formatting when the row is fetched */
            if (VirtualMode)
            {
                if (index < VirtualListSize)
                {
                    RedrawItems(index, index, true);
                }
            }
            else
            {
                var olvi = GetItem(index);
                if (olvi != null)
                {
                    RefreshItem(olvi);
                }
            }
        }

        /* Drag and drop */
        protected override void OnItemDrag(ItemDragEventArgs e)
        {
            base.OnItemDrag(e);
            if (DragSource == null)
            {
                return;
            }
            var data = DragSource.StartDrag(this, e.Button, (OlvListItem)e.Item);
            if (data == null) { return; }
            var effect = DoDragDrop(data, DragSource.GetAllowedEffects(data));
            DragSource.EndDrag(data, effect);
        }

        protected override void OnDragEnter(DragEventArgs args)
        {
            base.OnDragEnter(args);
            if (DropSink != null)
            {
                DropSink.Enter(args);
            }
        }

        protected override void OnDragOver(DragEventArgs args)
        {
            base.OnDragOver(args);
            if (DropSink != null)
            {
                DropSink.Over(args);
            }
        }

        protected override void OnDragDrop(DragEventArgs args)
        {
            base.OnDragDrop(args);
            if (DropSink != null)
            {
                DropSink.Drop(args);
            }
        }

        protected override void OnDragLeave(EventArgs e)
        {
            base.OnDragLeave(e);
            if (DropSink != null)
            {
                DropSink.Leave();
            }
        }

        protected override void OnGiveFeedback(GiveFeedbackEventArgs args)
        {
            base.OnGiveFeedback(args);
            if (DropSink != null)
            {
                DropSink.GiveFeedback(args);
            }
        }

        protected override void OnQueryContinueDrag(QueryContinueDragEventArgs args)
        {
            base.OnQueryContinueDrag(args);
            if (DropSink != null)
            {
                DropSink.QueryContinue(args);
            }
        }

        /* Filtering */
        public virtual IModelFilter CreateColumnFilter()
        {
            var filters = Columns.Cast<OlvColumn>().Select(column => column.ValueBasedFilter).Where(filter => filter != null).ToList();
            return (filters.Count == 0) ? null : new CompositeAllFilter(filters);
        }

        protected virtual IEnumerable FilterObjects(IEnumerable originalObjects, IModelFilter aModelFilter, IListFilter aListFilter)
        {
            /* Being cautious */
            originalObjects = originalObjects ?? new ArrayList();
            /* Tell the world to filter the objects. If they do so, don't do anything else */
// ReSharper disable PossibleMultipleEnumeration
            var args = new FilterEventArgs(originalObjects);
            OnFilter(args);
            if (args.FilteredObjects != null)
            {
                return args.FilteredObjects;
            }
            /* Apply a filter to the list as a whole */
            if (aListFilter != null)
            {
                originalObjects = aListFilter.Filter(originalObjects);
            }
            /* Apply the object filter if there is one */
            if (aModelFilter != null)
            {
                var filteredObjects = new ArrayList();
                foreach (var model in originalObjects.Cast<object>().Where(aModelFilter.Filter))
                {
                    filteredObjects.Add(model);
                }
                originalObjects = filteredObjects;
            }
            return originalObjects;
// ReSharper restore PossibleMultipleEnumeration
        }

        public virtual void ResetColumnFiltering()
        {
            foreach (OlvColumn column in Columns)
            {
                column.ValuesChosenForFiltering.Clear();
            }
            UpdateColumnFiltering();
        }

        public virtual void UpdateColumnFiltering()
        {
            if (AdditionalFilter == null)
            {
                ModelFilter = CreateColumnFilter();
            }
            else
            {
                var columnFilter = CreateColumnFilter();
                if (columnFilter == null)
                {
                    ModelFilter = AdditionalFilter;
                }
                else
                {
                    var filters = new List<IModelFilter>
                                      {
                                          columnFilter, 
                                          AdditionalFilter
                                      };
                    ModelFilter = new CompositeAllFilter(filters);
                }
            }
        }

        protected virtual void UpdateFiltering()
        {
            BuildList(true);
        }

        /* Persistent check state */
        protected virtual CheckState GetPersistentCheckState(object model)
        {
            var state = CheckState.Unchecked;
            if (model != null)
            {
                CheckStateMap.TryGetValue(model, out state);
            }
            return state;
        }

        protected virtual CheckState SetPersistentCheckState(object model, CheckState state)
        {
            if (model == null)
            {
                return CheckState.Unchecked;
            }
            CheckStateMap[model] = state;
            return state;
        }

        protected virtual void ClearPersistentCheckState()
        {
            CheckStateMap = null;
        }
    }
}