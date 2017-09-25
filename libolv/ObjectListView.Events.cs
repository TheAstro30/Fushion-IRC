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
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using libolv.DragDrop.DropSink;
using libolv.Implementation;
using libolv.Implementation.Events;

namespace libolv
{
    public delegate void CellEditEventHandler(object sender, CellEditEventArgs e);

    public partial class ObjectListView
    {
        public enum CellEditActivateMode
        {
            None = 0,
            SingleClick = 1,
            DoubleClick = 2,
            F2Only = 3
        }

        public enum ColumnSelectBehaviour
        {
            None = 0,
            InlineMenu = 1,
            Submenu = 2,
            ModelDialog = 3,
        }

        private int _lastValidatingEvent;
        private bool _hasIdleHandler; /* has an Idle handler already been installed? */
        private bool _hasResizeColumnsHandler; /* has an idle handler been installed which will handle column resizing? */

        [Category("ObjectListView"),
         Description("This event is triggered after the control has done a search-by-typing action.")]
        public event EventHandler<AfterSearchingEventArgs> AfterSearching;

        [Category("ObjectListView"),
         Description("This event is triggered after the items in the list have been sorted.")]
        public event EventHandler<AfterSortingEventArgs> AfterSorting;

        [Category("ObjectListView"),
         Description("This event is triggered before the control does a search-by-typing action.")]
        public event EventHandler<BeforeSearchingEventArgs> BeforeSearching;

        [Category("ObjectListView"), Description("This event is triggered before the items in the list are sorted.")]
        public event EventHandler<BeforeSortingEventArgs> BeforeSorting;

        [Category("ObjectListView"), Description("This event is triggered after the groups are created.")]
        public event EventHandler<CreateGroupsEventArgs> AfterCreatingGroups;

        [Category("ObjectListView"), Description("This event is triggered before the groups are created.")]
        public event EventHandler<CreateGroupsEventArgs> BeforeCreatingGroups;

        [Category("ObjectListView"),
         Description("This event is triggered when the groups are just about to be created.")]
        public event EventHandler<CreateGroupsEventArgs> AboutToCreateGroups;

        [Category("ObjectListView"),
         Description("Can the user drop the currently dragged items at the current mouse location?")]
        public event EventHandler<OlvDropEventArgs> CanDrop;

        [Category("ObjectListView"), Description("This event is triggered cell edit operation is finishing.")]
        public event CellEditEventHandler CellEditFinishing;

        [Category("ObjectListView"), Description("This event is triggered when cell edit is about to begin.")]
        public event CellEditEventHandler CellEditStarting;

        [Category("ObjectListView"),
         Description(
             "This event is triggered when a cell editor is about to lose focus and its new contents need to be validated."
             )]
        public event CellEditEventHandler CellEditValidating;

        [Category("ObjectListView"), Description("This event is triggered when the user left clicks a cell.")]
        public event EventHandler<CellClickEventArgs> CellClick;

        [Category("ObjectListView"), Description("This event is triggered when the mouse is over a cell.")]
        public event EventHandler<CellOverEventArgs> CellOver;

        [Category("ObjectListView"), Description("This event is triggered when the user right clicks a cell.")]
        public event EventHandler<CellRightClickEventArgs> CellRightClick;

        [Category("ObjectListView"), Description("This event is triggered when a cell needs a tool tip.")]
        public event EventHandler<ToolTipShowingEventArgs> CellToolTipShowing;

        [Category("ObjectListView"),
         Description("This event is triggered when a checkbox is checked/unchecked on a subitem.")]
        public event EventHandler<SubItemCheckingEventArgs> SubItemChecking;

        [Category("ObjectListView"), Description("This event is triggered when the user right clicks a column header.")]
        public event ColumnRightClickEventHandler ColumnRightClick;

        [Category("ObjectListView"),
         Description("This event is triggered when the user dropped items onto the control.")]
        public event EventHandler<OlvDropEventArgs> Dropped;

        [Category("ObjectListView"),
         Description("This event is triggered when the control needs to filter its collection of objects.")]
        public event EventHandler<FilterEventArgs> Filter;

        [Category("ObjectListView"), Description("This event is triggered when a cell needs to be formatted.")]
        public event EventHandler<FormatCellEventArgs> FormatCell;

        [Category("ObjectListView"), Description("This event is triggered when frozeness of the control changes.")]
        public event EventHandler<FreezeEventArgs> Freezing;

        [Category("ObjectListView"), Description("This event is triggered when a row needs to be formatted.")]
        public event EventHandler<FormatRowEventArgs> FormatRow;

        [Category("ObjectListView"), Description("This event is triggered when a group is about to collapse or expand.")
        ]
        public event EventHandler<GroupExpandingCollapsingEventArgs> GroupExpandingCollapsing;

        [Category("ObjectListView"), Description("This event is triggered when a group changes state.")]
        public event EventHandler<GroupStateChangedEventArgs> GroupStateChanged;

        [Category("ObjectListView"), Description("This event is triggered when a header needs a tool tip.")]
        public event EventHandler<ToolTipShowingEventArgs> HeaderToolTipShowing;

        [Category("ObjectListView"), Description("This event is triggered when the hot item changed.")]
        public event EventHandler<HotItemChangedEventArgs> HotItemChanged;

        [Category("ObjectListView"), Description("This event is triggered when a hyperlink cell is clicked.")]
        public event EventHandler<HyperlinkClickedEventArgs> HyperlinkClicked;

        [Category("ObjectListView"), Description("This event is triggered when the task text of a group is clicked.")]
        public event EventHandler<GroupTaskClickedEventArgs> GroupTaskClicked;

        [Category("ObjectListView"),
         Description("This event is triggered when the control needs to know if a given cell contains a hyperlink.")]
        public event EventHandler<IsHyperlinkEventArgs> IsHyperlink;

        [Category("ObjectListView"),
         Description("This event is triggered when objects are about to be added to the control")]
        public event EventHandler<ItemsAddingEventArgs> ItemsAdding;

        [Category("ObjectListView"),
         Description("This event is triggered when the contents of the control have changed.")]
        public event EventHandler<ItemsChangedEventArgs> ItemsChanged;

        [Category("ObjectListView"), Description("This event is triggered when the contents of the control changes.")]
        public event EventHandler<ItemsChangingEventArgs> ItemsChanging;

        [Category("ObjectListView"), Description("This event is triggered when objects are removed from the control.")]
        public event EventHandler<ItemsRemovingEventArgs> ItemsRemoving;

        [Category("ObjectListView"),
         Description("Can the dragged collection of model objects be dropped at the current mouse location")]
        public event EventHandler<ModelDropEventArgs> ModelCanDrop;

        [Category("ObjectListView"),
         Description("A collection of model objects from a ObjectListView has been dropped on this control")]
        public event EventHandler<ModelDropEventArgs> ModelDropped;

        [Category("ObjectListView"),
         Description(
             "This event is triggered once per user action that changes the selection state of one or more rows.")]
        public event EventHandler SelectionChanged;

        [Category("ObjectListView"),
         Description("This event is triggered when the contents of the ObjectListView has scrolled.")]
        public event EventHandler<ScrollEventArgs> Scroll;

        /* OnEvents */

        protected virtual void OnAboutToCreateGroups(CreateGroupsEventArgs e)
        {
            if (AboutToCreateGroups != null)
            {
                AboutToCreateGroups(this, e);
            }
        }

        protected virtual void OnBeforeCreatingGroups(CreateGroupsEventArgs e)
        {
            if (BeforeCreatingGroups != null)
            {
                BeforeCreatingGroups(this, e);
            }
        }

        protected virtual void OnAfterCreatingGroups(CreateGroupsEventArgs e)
        {
            if (AfterCreatingGroups != null)
            {
                AfterCreatingGroups(this, e);
            }
        }

        protected virtual void OnAfterSearching(AfterSearchingEventArgs e)
        {
            if (AfterSearching != null)
            {
                AfterSearching(this, e);
            }
        }

        protected virtual void OnAfterSorting(AfterSortingEventArgs e)
        {
            if (AfterSorting != null)
            {
                AfterSorting(this, e);
            }
        }

        protected virtual void OnBeforeSearching(BeforeSearchingEventArgs e)
        {
            if (BeforeSearching != null)
            {
                BeforeSearching(this, e);
            }
        }

        protected virtual void OnBeforeSorting(BeforeSortingEventArgs e)
        {
            if (BeforeSorting != null)
            {
                BeforeSorting(this, e);
            }
        }

        protected virtual void OnCanDrop(OlvDropEventArgs args)
        {
            if (CanDrop != null)
            {
                CanDrop(this, args);
            }
        }

        protected virtual void OnCellClick(CellClickEventArgs args)
        {
            if (CellClick != null)
            {
                CellClick(this, args);
            }
        }

        protected virtual void OnCellOver(CellOverEventArgs args)
        {
            if (CellOver != null)
            {
                CellOver(this, args);
            }
        }

        protected virtual void OnCellRightClick(CellRightClickEventArgs args)
        {
            if (CellRightClick != null)
            {
                CellRightClick(this, args);
            }
        }

        protected virtual void OnCellToolTip(ToolTipShowingEventArgs args)
        {
            if (CellToolTipShowing != null)
            {
                CellToolTipShowing(this, args);
            }
        }

        protected virtual void OnSubItemChecking(SubItemCheckingEventArgs args)
        {
            if (SubItemChecking != null)
            {
                SubItemChecking(this, args);
            }
        }

        protected virtual void OnColumnRightClick(ColumnClickEventArgs e)
        {
            if (ColumnRightClick != null)
            {
                ColumnRightClick(this, e);
            }
        }

        protected virtual void OnDropped(OlvDropEventArgs args)
        {
            if (Dropped != null)
            {
                Dropped(this, args);
            }
        }

        protected virtual void OnFilter(FilterEventArgs e)
        {
            if (Filter != null)
            {
                Filter(this, e);
            }
        }

        protected virtual void OnFormatCell(FormatCellEventArgs args)
        {
            if (FormatCell != null)
            {
                FormatCell(this, args);
            }
        }

        protected virtual void OnFormatRow(FormatRowEventArgs args)
        {
            if (FormatRow != null)
            {
                FormatRow(this, args);
            }
        }

        protected virtual void OnFreezing(FreezeEventArgs args)
        {
            if (Freezing != null)
            {
                Freezing(this, args);
            }
        }

        protected virtual void OnGroupExpandingCollapsing(GroupExpandingCollapsingEventArgs args)
        {
            if (GroupExpandingCollapsing != null)
            {
                GroupExpandingCollapsing(this, args);
            }
        }

        protected virtual void OnGroupStateChanged(GroupStateChangedEventArgs args)
        {
            if (GroupStateChanged != null)
            {
                GroupStateChanged(this, args);
            }
        }

        protected virtual void OnHeaderToolTip(ToolTipShowingEventArgs args)
        {
            if (HeaderToolTipShowing != null)
            {
                HeaderToolTipShowing(this, args);
            }
        }

        protected virtual void OnHotItemChanged(HotItemChangedEventArgs e)
        {
            if (HotItemChanged != null)
            {
                HotItemChanged(this, e);
            }
        }

        protected virtual void OnHyperlinkClicked(HyperlinkClickedEventArgs e)
        {
            if (HyperlinkClicked != null)
            {
                HyperlinkClicked(this, e);
            }
        }

        protected virtual void OnGroupTaskClicked(GroupTaskClickedEventArgs e)
        {
            if (GroupTaskClicked != null)
            {
                GroupTaskClicked(this, e);
            }
        }

        protected virtual void OnIsHyperlink(IsHyperlinkEventArgs e)
        {
            if (IsHyperlink != null)
            {
                IsHyperlink(this, e);
            }
        }

        protected virtual void OnItemsAdding(ItemsAddingEventArgs e)
        {
            if (ItemsAdding != null)
            {
                ItemsAdding(this, e);
            }
        }

        protected virtual void OnItemsChanged(ItemsChangedEventArgs e)
        {
            if (ItemsChanged != null)
            {
                ItemsChanged(this, e);
            }
        }

        protected virtual void OnItemsChanging(ItemsChangingEventArgs e)
        {
            if (ItemsChanging != null)
            {
                ItemsChanging(this, e);
            }
        }

        protected virtual void OnItemsRemoving(ItemsRemovingEventArgs e)
        {
            if (ItemsRemoving != null)
            {
                ItemsRemoving(this, e);
            }
        }

        protected virtual void OnModelCanDrop(ModelDropEventArgs args)
        {
            if (ModelCanDrop != null)
            {
                ModelCanDrop(this, args);
            }
        }

        protected virtual void OnModelDropped(ModelDropEventArgs args)
        {
            if (ModelDropped != null)
            {
                ModelDropped(this, args);
            }
        }

        protected virtual void OnSelectionChanged(EventArgs e)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, e);
            }
        }

        protected virtual void OnScroll(ScrollEventArgs e)
        {
            if (Scroll != null)
            {
                Scroll(this, e);
            }
        }

        protected virtual void OnCellEditStarting(CellEditEventArgs e)
        {
            if (CellEditStarting != null)
            {
                CellEditStarting(this, e);
            }
        }

        protected virtual void OnCellEditorValidating(CellEditEventArgs e)
        {
            /* Hack. ListView is an imperfect control container. It does not manage validation
             * perfectly. If the ListView is part of a TabControl, and the cell editor loses
             * focus by the user clicking on another tab, the TabControl processes the click
             * and switches tabs, even if this Validating event cancels. This results in the
             * strange situation where the cell editor is active, but isn't visible. When the
             * user switches back to the tab with the ListView, composite controls like spin
             * controls, DateTimePicker and ComboBoxes do not work properly. Specifically,
             * keyboard input still works fine, but the controls do not respond to mouse
             * input. SO, if the validation fails, we have to specifically give focus back to
             * the cell editor. (this is the Select() call in the code below). 
             * But (there is always a 'but'), doing that changes the focus so the cell editor
             * triggers another Validating event -- which fails again. From the user's point
             * of view, they click away from the cell editor, and the validating code
             * complains twice. So we only trigger a Validating event if more than 0.1 seconds
             * has elapsed since the last validate event.
             * I know it's a hack. I'm very open to hear a neater solution.
             * Also, this timed response stops us from sending a series of validation events
             * if the user clicks and holds on the OLV scroll bar. */            
            if ((Environment.TickCount - _lastValidatingEvent) < 100)
            {
                e.Cancel = true;
            }
            else
            {
                _lastValidatingEvent = Environment.TickCount;
                if (CellEditValidating != null)
                {
                    CellEditValidating(this, e);
                }
            }
            _lastValidatingEvent = Environment.TickCount;
        }

        protected virtual void OnCellEditFinishing(CellEditEventArgs e)
        {
            if (CellEditFinishing != null)
            {
                CellEditFinishing(this, e);
            }
        }

        protected virtual void HandleApplicationIdle(object sender, EventArgs e)
        {
            /* Remove the handler before triggering the event */
            Application.Idle -= HandleApplicationIdle;
            _hasIdleHandler = false;
            OnSelectionChanged(new EventArgs());
        }

        protected virtual void HandleApplicationIdleResizeColumns(object sender, EventArgs e)
        {
            /* Remove the handler before triggering the event */
            Application.Idle -= HandleApplicationIdleResizeColumns;
            _hasResizeColumnsHandler = false;
            ResizeFreeSpaceFillingColumns();
        }

        protected virtual bool HandleBeginScroll(ref Message m)
        {
            var nmlvscroll = (NativeMethods.Nmlvscroll)m.GetLParam(typeof(NativeMethods.Nmlvscroll));
            if (nmlvscroll.dx != 0)
            {
                var scrollPositionH = NativeMethods.GetScrollPosition(this, true);
                var args = new ScrollEventArgs(ScrollEventType.EndScroll, scrollPositionH - nmlvscroll.dx,
                                               scrollPositionH, ScrollOrientation.HorizontalScroll);
                OnScroll(args);
                /* Force any empty list msg to redraw when the list is scrolled horizontally */
                if (GetItemCount() == 0)
                {
                    Invalidate();
                }
            }
            if (nmlvscroll.dy != 0)
            {
                var scrollPositionV = NativeMethods.GetScrollPosition(this, false);
                var args = new ScrollEventArgs(ScrollEventType.EndScroll, scrollPositionV - nmlvscroll.dy,
                                               scrollPositionV, ScrollOrientation.VerticalScroll);
                OnScroll(args);
            }
            return false;
        }

        protected virtual bool HandleEndScroll(ref Message m)
        {
            if (!IsVistaOrLater && MouseButtons == MouseButtons.Left && GridLines)
            {
                Invalidate();
                Update();
            }
            return false;
        }

        protected virtual bool HandleLinkClick(ref Message m)
        {
            var nmlvlink = (NativeMethods.Nmlvlink)m.GetLParam(typeof(NativeMethods.Nmlvlink));
            /* Find the group that was clicked and trigger an event */
            foreach (var x in OlvGroups.Where(x => x.GroupId == nmlvlink.iSubItem))
            {
                OnGroupTaskClicked(new GroupTaskClickedEventArgs(x));
                return true;
            }
            return false;
        }

        protected virtual void HandleCellToolTipShowing(object sender, ToolTipShowingEventArgs e)
        {
            BuildCellEvent(e, PointToClient(Cursor.Position));
            if (e.Item == null) { return; }
            e.Text = GetCellToolTip(e.ColumnIndex, e.RowIndex);
            OnCellToolTip(e);
        }

        internal void HeaderToolTipShowingCallback(object sender, ToolTipShowingEventArgs e)
        {
            HandleHeaderToolTipShowing(sender, e);
        }

        protected virtual void HandleHeaderToolTipShowing(object sender, ToolTipShowingEventArgs e)
        {
            e.ColumnIndex = HeaderControl.ColumnIndexUnderCursor;
            if (e.ColumnIndex < 0)
            {
                return;
            }
            e.RowIndex = -1;
            e.Model = null;
            e.Column = GetColumn(e.ColumnIndex);
            e.Text = GetHeaderToolTip(e.ColumnIndex);
            e.ListView = this;
            OnHeaderToolTip(e);
        }

        protected virtual void HandleColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (!PossibleFinishCellEditing())
            {
                return;
            }
            /* Toggle the sorting direction on successive clicks on the same column */
            if (PrimarySortColumn != null && e.Column == PrimarySortColumn.Index)
            {
                PrimarySortOrder = (PrimarySortOrder == SortOrder.Descending
                                        ? SortOrder.Ascending
                                        : SortOrder.Descending);
            }
            else
            {
                PrimarySortOrder = SortOrder.Ascending;
            }
            BeginUpdate();
            try
            {
                Sort(e.Column);
            }
            finally
            {
                EndUpdate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _lastMouseDownClickCount = e.Clicks;
            base.OnMouseDown(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (!Created)
            {
                return;
            }
            UpdateHotItem(new Point(-1, -1));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!Created)
            {
                return;
            }            
            var args = new CellOverEventArgs();
            BuildCellEvent(args, e.Location);
            OnCellOver(args);
            MouseMoveHitTest = args.HitTest;
            if (!args.Handled)
            {
                UpdateHotItem(args.HitTest);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (!Created)
            {
                return;
            }
            if (e.Button == MouseButtons.Right)
            {
                OnRightMouseUp(e);
                return;
            }
            /* Tell the world about a cell click. If someone handles it, don't do anything else */
            var args = new CellClickEventArgs();
            BuildCellEvent(args, e.Location);
            args.ClickCount = _lastMouseDownClickCount;
            OnCellClick(args);
            if (args.Handled)
            {
                return;
            }
            /* Did the user click a hyperlink? */
            if (UseHyperlinks &&
                args.HitTest.HitTestLocation == HitTestLocation.Text &&
                args.SubItem != null &&
                !string.IsNullOrEmpty(args.SubItem.Url))
            {
                /* We have to delay the running of this process otherwise we can generate
                 * a series of MouseUp events (don't ask me why) */
                BeginInvoke((MethodInvoker)(() => ProcessHyperlinkClicked(args)));
            }
            /* No one handled it so check to see if we should start editing. */
            if (!ShouldStartCellEdit(e))
            {
                return;
            }
            /* We only start the edit if the user clicked on the image or text. */
            if (args.HitTest.HitTestLocation == HitTestLocation.Nothing)
            {
                return;
            }
            /* We don't edit the primary column by single clicks -- only subitems. */
            if (CellEditActivation == CellEditActivateMode.SingleClick && args.ColumnIndex <= 0)
            {
                return;
            }
            /* Don't start a cell edit operation when the user clicks on the background of a checkbox column -- it just
             * looks wrong. If the user clicks on the actual checkbox, changing the checkbox state is handled elsewhere. */
            if (args.Column != null && args.Column.CheckBoxes)
            {
                return;
            }
            EditSubItem(args.Item, args.ColumnIndex);
        }

        protected virtual void ProcessHyperlinkClicked(CellClickEventArgs e)
        {
            var args = new HyperlinkClickedEventArgs
                           {
                               HitTest = e.HitTest,
                               ListView = this,
                               Location = new Point(-1, -1),
                               Item = e.Item,
                               SubItem = e.SubItem,
                               Model = e.Model,
                               ColumnIndex = e.ColumnIndex,
                               Column = e.Column,
                               RowIndex = e.RowIndex,
                               ModifierKeys = ModifierKeys,
                               Url = e.SubItem.Url
                           };
            OnHyperlinkClicked(args);
            if (!args.Handled)
            {
                StandardHyperlinkClickedProcessing(args);
            }
        }

        protected virtual void StandardHyperlinkClickedProcessing(HyperlinkClickedEventArgs args)
        {
            var originalCursor = Cursor;
            try
            {
                Cursor = Cursors.WaitCursor;
                System.Diagnostics.Process.Start(args.Url);
            }
            catch (Win32Exception)
            {
                SystemSounds.Beep.Play();
                /* ignore it */
            }
            finally
            {
                Cursor = originalCursor;
            }
            MarkUrlVisited(args.Url);
            RefreshHotItem();
        }

        protected virtual void OnRightMouseUp(MouseEventArgs e)
        {
            var args = new CellRightClickEventArgs();
            BuildCellEvent(args, e.Location);
            OnCellRightClick(args);
            if (args.Handled) { return; }
            if (args.MenuStrip != null)
            {
                args.MenuStrip.Show(this, args.Location);
            }
        }

        private void BuildCellEvent(CellEventArgs args, Point location)
        {
            var hitTest = OlvHitTest(location.X, location.Y);
            args.HitTest = hitTest;
            args.ListView = this;
            args.Location = location;
            args.Item = hitTest.Item;
            args.SubItem = hitTest.SubItem;
            args.Model = hitTest.RowObject;
            args.ColumnIndex = hitTest.ColumnIndex;
            args.Column = hitTest.Column;
            if (hitTest.Item != null)
            {
                args.RowIndex = hitTest.Item.Index;
            }
            args.ModifierKeys = ModifierKeys;
            /* In non-details view, we want any hit on an item to act as if it was a hit
             * on column 0 -- which, effectively, it was. */
            if (args.Item == null || args.ListView.View == View.Details) { return; }
            args.ColumnIndex = 0;
            args.Column = args.ListView.GetColumn(0);
            args.SubItem = args.Item.GetSubItem(0);
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            if (SelectionEventsSuspended)
            {
                return;
            }
            base.OnSelectedIndexChanged(e);
            /* If we haven't already scheduled an event, schedule it to be triggered
             * By using idle time, we will wait until all select events for the same
             * user action have finished before triggering the event. */
            if (_hasIdleHandler) { return; }
            _hasIdleHandler = true;
            RunWhenIdle(HandleApplicationIdle);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Invoke((MethodInvoker)OnControlCreated);
        }

        protected virtual void OnControlCreated()
        {
            /* Force the header control to be created when the listview handle is */
            var hc = HeaderControl;
            hc.WordWrap = HeaderWordWrap;
            /* Make sure any overlays that are set on the hot item style take effect */
            HotItemStyle = HotItemStyle;
            /* Arrange for any group images to be installed after the control is created */
            NativeMethods.SetGroupImageList(this, GroupImageList);
            UseExplorerTheme = UseExplorerTheme;
            RememberDisplayIndicies();
            SetGroupSpacing();
        }
    }
}
