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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using libolv.Properties;
using libolv.Utilities;

namespace libolv
{
    public partial class ObjectListView
    {
        private string _menuLabelSortAscending = "Sort ascending by '{0}'";
        private string _menuLabelSortDescending = "Sort descending by '{0}'";
        private string _menuLabelGroupBy = "Group by '{0}'";
        private string _menuLabelLockGroupingOn = "Lock grouping on '{0}'";
        private string _menuLabelUnlockGroupingOn = "Unlock grouping from '{0}'";
        private string _menuLabelTurnOffGroups = "Turn off groups";
        private string _menuLabelUnsort = "Unsort";
        private string _menuLabelColumns = "Columns";
        private string _menuLabelSelectColumns = "Select Columns...";

        private bool _contextMenuStaysOpen;
        
        public static Bitmap SortAscendingImage = Resources.SortAscending;
        public static Bitmap SortDescendingImage = Resources.SortDescending;

        protected virtual bool HandleHeaderRightClick(int columnIndex)
        {
            var eventArgs = new ColumnClickEventArgs(columnIndex);
            OnColumnRightClick(eventArgs);
            /* TODO: Allow users to say they have handled this event */
            return ShowHeaderRightClickMenu(columnIndex, Cursor.Position);
        }

        protected virtual bool ShowHeaderRightClickMenu(int columnIndex, Point pt)
        {
            var m = MakeHeaderRightClickMenu(columnIndex);
            if (m.Items.Count > 0)
            {
                m.Show(pt);
                return true;
            }
            return false;
        }

        protected virtual ToolStripDropDown MakeHeaderRightClickMenu(int columnIndex)
        {
            ToolStripDropDown m = new ContextMenuStrip();
            if (columnIndex >= 0 && UseFiltering && ShowFilterMenuOnRightClick)
            {
                m = MakeFilteringMenu(m, columnIndex);
            }
            if (columnIndex >= 0 && ShowCommandMenuOnRightClick)
            {
                m = MakeColumnCommandMenu(m, columnIndex);
            }
            if (SelectColumnsOnRightClickBehaviour != ColumnSelectBehaviour.None)
            {
                m = MakeColumnSelectMenu(m);
            }
            return m;
        }

        [Obsolete("Use HandleHeaderRightClick(int) instead")]
        protected virtual bool HandleHeaderRightClick()
        {
            return false;
        }

        [Obsolete("Use ShowHeaderRightClickMenu instead")]
        protected virtual void ShowColumnSelectMenu(Point pt)
        {
            var m = MakeColumnSelectMenu(new ContextMenuStrip());
            m.Show(pt);
        }

        [Obsolete("Use ShowHeaderRightClickMenu instead")]
        protected virtual void ShowColumnCommandMenu(int columnIndex, Point pt)
        {
            var m = MakeColumnCommandMenu(new ContextMenuStrip(), columnIndex);
            if (SelectColumnsOnRightClick)
            {
                if (m.Items.Count > 0)
                {
                    m.Items.Add(new ToolStripSeparator());
                }
                MakeColumnSelectMenu(m);
            }
            m.Show(pt);
        }

        [Category("Labels - ObjectListView"), DefaultValue("Sort ascending by '{0}'"), Localizable(true)]
        public string MenuLabelSortAscending
        {
            get { return _menuLabelSortAscending; }
            set { _menuLabelSortAscending = value; }
        }

        [Category("Labels - ObjectListView"), DefaultValue("Sort descending by '{0}'"), Localizable(true)]
        public string MenuLabelSortDescending
        {
            get { return _menuLabelSortDescending; }
            set { _menuLabelSortDescending = value; }
        }
        
        [Category("Labels - ObjectListView"), DefaultValue("Group by '{0}'"), Localizable(true)]
        public string MenuLabelGroupBy
        {
            get { return _menuLabelGroupBy; }
            set { _menuLabelGroupBy = value; }
        }

        [Category("Labels - ObjectListView"), DefaultValue("Lock grouping on '{0}'"), Localizable(true)]
        public string MenuLabelLockGroupingOn
        {
            get { return _menuLabelLockGroupingOn; }
            set { _menuLabelLockGroupingOn = value; }
        }

        [Category("Labels - ObjectListView"), DefaultValue("Unlock grouping from '{0}'"), Localizable(true)]
        public string MenuLabelUnlockGroupingOn
        {
            get { return _menuLabelUnlockGroupingOn; }
            set { _menuLabelUnlockGroupingOn = value; }
        }        

        [Category("Labels - ObjectListView"), DefaultValue("Turn off groups"), Localizable(true)]
        public string MenuLabelTurnOffGroups
        {
            get { return _menuLabelTurnOffGroups; }
            set { _menuLabelTurnOffGroups = value; }
        }
        
        [Category("Labels - ObjectListView"), DefaultValue("Unsort"), Localizable(true)]
        public string MenuLabelUnsort
        {
            get { return _menuLabelUnsort; }
            set { _menuLabelUnsort = value; }
        }
      
        [Category("Labels - ObjectListView"), DefaultValue("Columns"), Localizable(true)]
        public string MenuLabelColumns
        {
            get { return _menuLabelColumns; }
            set { _menuLabelColumns = value; }
        }

        [Category("Labels - ObjectListView"), DefaultValue("Select Columns..."), Localizable(true)]
        public string MenuLabelSelectColumns
        {
            get { return _menuLabelSelectColumns; }
            set { _menuLabelSelectColumns = value; }
        }

        public virtual ToolStripDropDown MakeColumnCommandMenu(ToolStripDropDown strip, int columnIndex)
        {
            var column = GetColumn(columnIndex);
            if (column == null)
            {
                return strip;
            }
            if (strip.Items.Count > 0)
            {
                strip.Items.Add(new ToolStripSeparator());
            }
            var label = string.Format(MenuLabelSortAscending, column.Text);
            if (column.Sortable && !string.IsNullOrEmpty(label))
            {
                strip.Items.Add(label, SortAscendingImage, (sender, args) => Sort(column, SortOrder.Ascending));
            }
            label = string.Format(MenuLabelSortDescending, column.Text);
            if (column.Sortable && !string.IsNullOrEmpty(label))
            {
                strip.Items.Add(label, SortDescendingImage, (sender, args) => Sort(column, SortOrder.Descending));
            }
            if (CanShowGroups)
            {
                label = string.Format(MenuLabelGroupBy, column.Text);
                if (column.Groupable && !string.IsNullOrEmpty(label))
                {
                    strip.Items.Add(label, null, delegate
                                                     {
                                                         ShowGroups = true;
                                                         PrimarySortColumn = column;
                                                         PrimarySortOrder = SortOrder.Ascending;
                                                         BuildList();
                                                     });
                }
            }
            if (ShowGroups)
            {
                if (AlwaysGroupByColumn == column)
                {
                    label = string.Format(MenuLabelUnlockGroupingOn, column.Text);
                    if (!string.IsNullOrEmpty(label))
                    {
                        strip.Items.Add(label, null, delegate
                                                         {
                                                             AlwaysGroupByColumn = null;
                                                             AlwaysGroupBySortOrder = SortOrder.None;
                                                             BuildList();
                                                         });
                    }
                }
                else
                {
                    label = string.Format(MenuLabelLockGroupingOn, column.Text);
                    if (column.Groupable && !string.IsNullOrEmpty(label))
                    {
                        strip.Items.Add(label, null, delegate
                                                         {
                                                             ShowGroups = true;
                                                             AlwaysGroupByColumn = column;
                                                             AlwaysGroupBySortOrder =
                                                                 SortOrder.Ascending;
                                                             BuildList();
                                                         });
                    }
                }
                label = string.Format(MenuLabelTurnOffGroups, column.Text);
                if (!string.IsNullOrEmpty(label))
                {
                    strip.Items.Add(label, null, delegate
                                                     {
                                                         ShowGroups = false;
                                                         BuildList();
                                                     });
                }
            }
            else
            {
                label = string.Format(MenuLabelUnsort, column.Text);
                if (column.Sortable && !string.IsNullOrEmpty(label) && PrimarySortOrder != SortOrder.None)
                {
                    strip.Items.Add(label, null, (sender, args) => Unsort());
                }
            }
            return strip;
        }

        public virtual ToolStripDropDown MakeColumnSelectMenu(ToolStripDropDown strip)
        {
            System.Diagnostics.Debug.Assert(SelectColumnsOnRightClickBehaviour != ColumnSelectBehaviour.None);
            /* Append a separator if the menu isn't empty and the last item isn't already a separator */
            if (strip.Items.Count > 0 && (!(strip.Items[strip.Items.Count - 1] is ToolStripSeparator)))
            {
                strip.Items.Add(new ToolStripSeparator());
            }
            if (AllColumns.Count > 0 && AllColumns[0].LastDisplayIndex == -1)
            {
                RememberDisplayIndicies();
            }
            if (SelectColumnsOnRightClickBehaviour == ColumnSelectBehaviour.ModelDialog)
            {
                strip.Items.Add(MenuLabelSelectColumns, null, (sender, args) => (new ColumnSelectionForm()).OpenOn(this));
            }
            if (SelectColumnsOnRightClickBehaviour == ColumnSelectBehaviour.Submenu)
            {
                var menu = new ToolStripMenuItem(MenuLabelColumns);
                menu.DropDownItemClicked += ColumnSelectMenuItemClicked;
                strip.Items.Add(menu);
                AddItemsToColumnSelectMenu(menu.DropDownItems);
            }
            if (SelectColumnsOnRightClickBehaviour == ColumnSelectBehaviour.InlineMenu)
            {
                strip.ItemClicked += ColumnSelectMenuItemClicked;
                strip.Closing += ColumnSelectMenuClosing;
                AddItemsToColumnSelectMenu(strip.Items);
            }
            return strip;
        }

        protected void AddItemsToColumnSelectMenu(ToolStripItemCollection items)
        {
            /* Sort columns by display order */
            var columns = new List<OlvColumn>(AllColumns);
            columns.Sort((x, y) => (x.LastDisplayIndex - y.LastDisplayIndex));
            /* Build menu from sorted columns */
            foreach (var col in columns)
            {
                var mi = new ToolStripMenuItem(col.Text)
                             {
                                 Checked = col.IsVisible,
                                 Tag = col,
                                 Enabled = !col.IsVisible || col.CanBeHidden
                             };
                /* The 'Index' property returns -1 when the column is not visible, so if the
                 * column isn't visible we have to enable the item. Also the first column can't be turned off */
                items.Add(mi);
            }
        }

        private void ColumnSelectMenuItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            _contextMenuStaysOpen = false;
            var menuItemClicked = e.ClickedItem as ToolStripMenuItem;
            if (menuItemClicked == null)
            {
                return;
            }
            var col = menuItemClicked.Tag as OlvColumn;
            if (col == null)
            {
                return;
            }
            menuItemClicked.Checked = !menuItemClicked.Checked;
            col.IsVisible = menuItemClicked.Checked;
            _contextMenuStaysOpen = SelectColumnsMenuStaysOpen;
            BeginInvoke(new MethodInvoker(RebuildColumns));
        }
        
        private void ColumnSelectMenuClosing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            e.Cancel = _contextMenuStaysOpen && e.CloseReason == ToolStripDropDownCloseReason.ItemClicked;
            _contextMenuStaysOpen = false;
        }

        public virtual ToolStripDropDown MakeFilteringMenu(ToolStripDropDown strip, int columnIndex)
        {
            var column = GetColumn(columnIndex);
            if (column == null)
            {
                return strip;
            }
            var strategy = FilterMenuBuildStrategy;
            return strategy == null ? strip : strategy.MakeFilterMenu(strip, this, column);
        }

        protected override void OnColumnReordered(ColumnReorderedEventArgs e)
        {
            base.OnColumnReordered(e);
            /* The internal logic of the .NET code behind a ENDDRAG event means that,
             * at this point, the DisplayIndex's of the columns are not yet as they are
             * going to be. So we have to invoke a method to run later that will remember
             * what the real DisplayIndex's are. */
            BeginInvoke(new MethodInvoker(RememberDisplayIndicies));
        }

        private void RememberDisplayIndicies()
        {
            /* Remember the display indexes so we can put them back at a later date */
            foreach (var x in AllColumns)
            {
                x.LastDisplayIndex = x.DisplayIndex;
            }
        }

        protected virtual void HandleColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            if (!UpdateSpaceFillingColumnsWhenDraggingColumnDivider || GetColumn(e.ColumnIndex).FillsFreeSpace) { return; }
            /* If the width of a column is increasing, resize any space filling columns allowing the extra
             * space that the new column width is going to consume */
            var oldWidth = GetColumn(e.ColumnIndex).Width;
            if (e.NewWidth > oldWidth)
            {
                ResizeFreeSpaceFillingColumns(ClientSize.Width - (e.NewWidth - oldWidth));
            }
            else
            {
                ResizeFreeSpaceFillingColumns();
            }
        }

        protected virtual void HandleColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if (!GetColumn(e.ColumnIndex).FillsFreeSpace)
            {
                ResizeFreeSpaceFillingColumns();
            }
        }

        protected virtual void HandleLayout(object sender, LayoutEventArgs e)
        {
            /* We have to delay executing the recalculation of the columns, since virtual lists
             * get terribly confused if we resize the column widths during this event. */
            if (_hasResizeColumnsHandler) { return; }
            _hasResizeColumnsHandler = true;
            RunWhenIdle(HandleApplicationIdleResizeColumns);
        }

        private void RunWhenIdle(EventHandler eventHandler)
        {
            Application.Idle += eventHandler;
            if (!CanUseApplicationIdle)
            {
                SynchronizationContext.Current.Post(x => Application.RaiseIdle(EventArgs.Empty), null);
            }
        }

        protected virtual void ResizeFreeSpaceFillingColumns()
        {
            ResizeFreeSpaceFillingColumns(ClientSize.Width);
        }

        protected virtual void ResizeFreeSpaceFillingColumns(int freeSpace)
        {
            /* It's too confusing to dynamically resize columns at design time. */
            if (DesignMode)
            {
                return;
            }
            if (Frozen)
            {
                return;
            }
            /* Calculate the free space available */
            int totalProportion = 0;
            var spaceFillingColumns = new List<OlvColumn>();
            for (var i = 0; i < Columns.Count; i++)
            {
                var col = GetColumn(i);
                if (col.FillsFreeSpace)
                {
                    spaceFillingColumns.Add(col);
                    totalProportion += col.FreeSpaceProportion;
                }
                else
                {
                    freeSpace -= col.Width;
                }
            }
            freeSpace = Math.Max(0, freeSpace);
            /* Any space filling column that would hit it's Minimum or Maximum
             * width must be treated as a fixed column. */
            foreach (var col in spaceFillingColumns.ToArray())
            {
                var newWidth = (freeSpace * col.FreeSpaceProportion) / totalProportion;
                if (col.MinimumWidth != -1 && newWidth < col.MinimumWidth)
                {
                    newWidth = col.MinimumWidth;
                }
                else if (col.MaximumWidth != -1 && newWidth > col.MaximumWidth)
                {
                    newWidth = col.MaximumWidth;
                }
                else
                {
                    newWidth = 0;
                }
                if (newWidth <= 0) { continue; }
                col.Width = newWidth;
                freeSpace -= newWidth;
                totalProportion -= col.FreeSpaceProportion;
                spaceFillingColumns.Remove(col);
            }
            /* Distribute the free space between the columns */
            foreach (var col in spaceFillingColumns)
            {
                col.Width = (freeSpace * col.FreeSpaceProportion) / totalProportion;
            }
        }
    }
}
