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
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using libolv.Implementation;
using libolv.Implementation.Events;
using libolv.SubControls;

namespace libolv
{
    public partial class ObjectListView
    {
        private int _timeLastCharEvent;
        private string _lastSearchString;
        private readonly IntPtr _minusOne = new IntPtr(-1);

        private bool _isAfterItemPaint;
        private List<OlvListItem> _drawnItems;

        private int _prePaintLevel;

        private bool _isInWmPaintEvent; /* is a WmPaint event currently being handled? */
        private bool _shouldDoCustomDrawing; /* should the list do its custom drawing? */
        private bool _isMarqueSelecting; /* Is a marque selection in progress? */

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 2: /* WM_DESTROY */
                    if (!HandleDestroy(ref m))
                    {
                        base.WndProc(ref m);
                    }
                    break;

                case 0x0F: /* WM_PAINT */
                    if (!HandlePaint(ref m))
                        base.WndProc(ref m);
                    break;
                    
                case 0x46: /* WM_WINDOWPOSCHANGING */
                    if (PossibleFinishCellEditing() && !HandleWindowPosChanging(ref m))
                    {
                        base.WndProc(ref m);
                    }
                    break;

                case 0x4E: /* WM_NOTIFY */
                    if (!HandleNotify(ref m))
                    {
                        base.WndProc(ref m);
                    }
                    break;

                case 0x0100: /* WM_KEY_DOWN */
                    if (!HandleKeyDown(ref m))
                    {
                        base.WndProc(ref m);
                    }
                    break;

                case 0x0102: /* WM_CHAR */
                    if (!HandleChar(ref m))
                    {
                        base.WndProc(ref m);
                    }
                    break;

                case 0x0200: /* WM_MOUSEMOVE */
                    if (!HandleMouseMove(ref m))
                    {
                        base.WndProc(ref m);
                    }
                    break;

                case 0x0201: /* WM_LBUTTONDOWN */
                    if (PossibleFinishCellEditing() && !HandleLButtonDown(ref m))
                    {
                        base.WndProc(ref m);
                    }
                    break;

                case 0x202: /* WM_LBUTTONUP */
                    if (PossibleFinishCellEditing() && !HandleLButtonUp(ref m))
                    {
                        base.WndProc(ref m);
                    }
                    break;

                case 0x0203: /* WM_LBUTTONDBLCLK */
                    if (PossibleFinishCellEditing() && !HandleLButtonDoubleClick(ref m))
                    {
                        base.WndProc(ref m);
                    }
                    break;

                case 0x0204: /* WM_RBUTTONDOWN */
                    if (PossibleFinishCellEditing() && !HandleRButtonDown(ref m))
                    {
                        base.WndProc(ref m);
                    }
                    break;

                case 0x0206: /* WM_RBUTTONDBLCLK */
                    if (PossibleFinishCellEditing() && !HandleRButtonDoubleClick(ref m))
                    {
                        base.WndProc(ref m);
                    }
                    break;

                case 0x204E: /* WM_REFLECT_NOTIFY */
                    if (!HandleReflectNotify(ref m))
                    {
                        base.WndProc(ref m);
                    }
                    break;

                case 0x114: /* WM_HSCROLL */
                case 0x115: /* WM_VSCROLL */
                    if (PossibleFinishCellEditing())
                    {
                        base.WndProc(ref m);
                    }
                    break;

                case 0x20A: /* WM_MOUSEWHEEL */
                case 0x20E: /* WM_MOUSEHWHEEL */
                    if (PossibleFinishCellEditing())
                    {
                        base.WndProc(ref m);
                    }
                    break;

                case 0x7B: /* WM_CONTEXTMENU */
                    if (!HandleContextMenu(ref m))
                    {
                        base.WndProc(ref m);
                    }
                    break;
                    
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        protected virtual bool HandleChar(ref Message m)
        {
            /* Trigger a normal KeyPress event, which listeners can handle if they want.
             * Handling the event stops ObjectListView's fancy search-by-typing. */
            if (ProcessKeyEventArgs(ref m))
            {
                return true;
            }
            const int millisecondsBetweenKeypresses = 1000;
            /* What character did the user type and was it part of a longer string? */
            var character = (char)m.WParam.ToInt32(); /* TODO: Will this work on 64 bit or MBCS? */
            if (character == (char)Keys.Back)
            {
                /* Backspace forces the next key to be considered the start of a new search */
                _timeLastCharEvent = 0;
                return true;
            }
            if (Environment.TickCount < (_timeLastCharEvent + millisecondsBetweenKeypresses))
            {
                _lastSearchString += character;
            }
            else
            {
                _lastSearchString = character.ToString(CultureInfo.InvariantCulture);
            }
            /* If this control is showing checkboxes, we want to ignore single space presses,
             * since they are used to toggle the selected checkboxes. */
            if (CheckBoxes && _lastSearchString == " ")
            {
                _timeLastCharEvent = 0;
                return true;
            }
            /* Where should the search start? */
            var start = 0;
            var focused = FocusedItem;
            if (focused != null)
            {
                start = GetDisplayOrderOfItemIndex(focused.Index);
                /* If the user presses a single key, we search from after the focused item,
                 * being careful not to march past the end of the list */
                if (_lastSearchString.Length == 1)
                {
                    start += 1;
                    if (start == GetItemCount())
                    {
                        start = 0;
                    }
                }
            }
            /* Give the world a chance to fiddle with or completely avoid the searching process */
            var args = new BeforeSearchingEventArgs(_lastSearchString, start);
            OnBeforeSearching(args);
            if (args.Canceled)
            {
                return true;
            }
            /* The parameters of the search may have been changed */
            var searchString = args.StringToFind;
            start = args.StartSearchFrom;
            /* Do the actual search */
            var found = FindMatchingRow(searchString, start, SearchDirectionHint.Down);
            if (found >= 0)
            {
                /* Select and focus on the found item */
                BeginUpdate();
                try
                {
                    SelectedIndices.Clear();
                    ListViewItem lvi = GetNthItemInDisplayOrder(found);
                    lvi.Selected = true;
                    lvi.Focused = true;
                    EnsureVisible(lvi.Index);
                }
                finally
                {
                    EndUpdate();
                }
            }
            /* Tell the world that a search has occurred */
            var args2 = new AfterSearchingEventArgs(searchString, found);
            OnAfterSearching(args2);
            if (!args2.Handled)
            {
                if (found < 0)
                {
                    SystemSounds.Beep.Play();
                }
            }
            /* When did this event occur? */
            _timeLastCharEvent = Environment.TickCount;
            return true;
        }

        protected virtual bool HandleContextMenu(ref Message m)
        {
            /* Don't try to handle context menu commands at design time. */
            if (DesignMode)
            {
                return false;
            }
            /* If the context menu command was generated by the keyboard, LParam will be -1.
             * We don't want to process these. */
            if (m.LParam == _minusOne)
            {
                return false;
            }
            /* If the context menu came from somewhere other than the header control,
             * we also don't want to ignore it */
            if (m.WParam != HeaderControl.Handle)
            {
                return false;
            }
            /* OK. Looks like a right click in the header */
            if (!PossibleFinishCellEditing())
            {
                return true;
            }
            var columnIndex = HeaderControl.ColumnIndexUnderCursor;
            return HandleHeaderRightClick(columnIndex);
        }

        protected virtual bool HandleCustomDraw(ref Message m)
        {
            const int cddsPrepaint = 1;
            const int cddsPostpaint = 2;
            const int cddsItem = 0x00010000;
            const int cddsSubitem = 0x00020000;
            const int cddsItemprepaint = (cddsItem | cddsPrepaint);
            const int cddsItempostpaint = (cddsItem | cddsPostpaint);
            const int cddsSubitemprepaint = (cddsSubitem | cddsItemprepaint);
            const int cddsSubitempostpaint = (cddsSubitem | cddsItempostpaint);
            const int cdrfNotifypostpaint = 0x10;
            const int cdrfNotifyposterase = 0x40;
            /* There is a bug in owner drawn virtual lists which causes lots of custom draw messages
             * to be sent to the control *outside* of a WmPaint event. AFAIK, these custom draw events
             * are spurious and only serve to make the control flicker annoyingly.
             * So, we ignore messages that are outside of a paint event. */
            if (!_isInWmPaintEvent)
            {
                return true;
            }
            /* One more complication! Sometimes with owner drawn virtual lists, the act of drawing
             * the overlays triggers a second attempt to paint the control -- which makes an annoying
             * flicker. So, we only do the custom drawing once per WmPaint event. */
            if (!_shouldDoCustomDrawing)
            {
                return true;
            }
            var nmcustomdraw = (NativeMethods.Nmlvcustomdraw)m.GetLParam(typeof(NativeMethods.Nmlvcustomdraw));            
            /* Ignore drawing of group items */
            if (nmcustomdraw.dwItemType == 1)
            {
                return true;
            }
            switch (nmcustomdraw.nmcd.dwDrawStage)
            {
                case cddsPrepaint:
                    /* Remember which items were drawn during this paint cycle */
                    if (_prePaintLevel == 0)
                    {
                        _drawnItems = new List<OlvListItem>();
                    }
                    /* If there are any items, we have to wait until at least one has been painted
                     * before we draw the overlays. If there aren't any items, there will never be any
                     * item paint events, so we can draw the overlays whenever */
                    _isAfterItemPaint = (GetItemCount() == 0);
                    _prePaintLevel++;
                    base.WndProc(ref m);
                    /* Make sure that we get postpaint notifications */
                    m.Result = (IntPtr)((int)m.Result | cdrfNotifypostpaint | cdrfNotifyposterase);
                    return true;

                case cddsPostpaint:
                    _prePaintLevel--;
                    /* When in group view, we have two problems. On XP, the control sends
                     * a whole heap of PREPAINT/POSTPAINT messages before drawing any items.
                     * We have to wait until after the first item paint before we draw overlays.
                     * On Vista, we have a different problem. On Vista, the control nests calls
                     * to PREPAINT and POSTPAINT. We only want to draw overlays on the outermost
                     * POSTPAINT. */
                    if (_prePaintLevel == 0 && (_isMarqueSelecting || _isAfterItemPaint))
                    {
                        _shouldDoCustomDrawing = false;
                        /* Draw our overlays after everything has been drawn */
                        using (var g = Graphics.FromHdc(nmcustomdraw.nmcd.hdc))
                        {
                            DrawAllDecorations(g, _drawnItems);
                        }
                    }
                    break;

                case cddsItemprepaint:
                    /* When in group view on XP, the control send a whole heap of PREPAINT/POSTPAINT
                     * messages before drawing any items.
                     * We have to wait until after the first item paint before we draw overlays */
                    _isAfterItemPaint = true;
                    /* This scheme of catching custom draw msgs works fine, except
                     * for Tile view. Something in .NET's handling of Tile view causes lots
                     * of invalidates and erases. So, we just ignore completely
                     * .NET's handling of Tile view and let the underlying control
                     * do its stuff. Strangely, if the Tile view is
                     * completely owner drawn, those erasures don't happen. */
                    if (View == View.Tile)
                    {
                        if (OwnerDraw && ItemRenderer != null)
                        {
                            base.WndProc(ref m);
                        }
                    }
                    else
                    {
                        base.WndProc(ref m);
                    }
                    m.Result = (IntPtr)((int)m.Result | cdrfNotifypostpaint | cdrfNotifyposterase);
                    return true;

                case cddsItempostpaint:
                    /* Remember which items have been drawn so we can draw any decorations for them
                     * once all other painting is finished */
                    if (Columns.Count > 0)
                    {
                        var olvi = GetItem((int)nmcustomdraw.nmcd.dwItemSpec);
                        if (olvi != null)
                        {
                            _drawnItems.Add(olvi);
                        }
                    }
                    break;

                case cddsSubitemprepaint:
                    /* There is a bug in the .NET framework which appears when column 0 of an owner drawn listview
                     * is dragged to another column position.
                     * The bounds calculation always returns the left edge of column 0 as being 0.
                     * The effects of this bug become apparent
                     * when the listview is scrolled horizontally: the control can think that column 0
                     * is no longer visible (the horizontal scroll position is subtracted from the bounds, giving a
                     * rectangle that is offscreen). In those circumstances, column 0 is not redraw because
                     * the control thinks it is not visible and so does not trigger a DrawSubItem event.
                     * To fix this problem, we have to detected the situation -- owner drawing column 0 in any column except 0 --
                     * trigger our own DrawSubItem, and then prevent the default processing from occuring.
                     * Are we owner drawing column 0 when it's in any column except 0? */
                    if (!OwnerDraw)
                    {
                        return false;
                    }
                    var columnIndex = nmcustomdraw.iSubItem;
                    if (columnIndex != 0)
                    {
                        return false;
                    }
                    var displayIndex = Columns[0].DisplayIndex;
                    if (displayIndex == 0)
                    {
                        return false;
                    }
                    var rowIndex = (int)nmcustomdraw.nmcd.dwItemSpec;
                    var item = GetItem(rowIndex);
                    if (item == null)
                    {
                        return false;
                    }
                    /* OK. We have the error condition, so lets do what the .NET framework should do.
                     * Trigger an event to draw column 0 when it is not at display index 0 */
                    using (var g = Graphics.FromHdc(nmcustomdraw.nmcd.hdc))
                    {
                        /* Correctly calculate the bounds of cell 0 */
                        var r = item.GetSubItemBounds(0);
                        /* We can hardcode "0" here since we know we are only doing this for column 0 */
                        var args = new DrawListViewSubItemEventArgs(g, r, item,
                                                                    item.SubItems[0], rowIndex,
                                                                    0,
                                                                    Columns[0],
                                                                    (ListViewItemStates)
                                                                    nmcustomdraw.nmcd.
                                                                        uItemState);
                        OnDrawSubItem(args);
                        /* If the event handler wants to do the default processing (i.e. DrawDefault = true), we are stuck.
                         * There is no way we can force the default drawing because of the
                         * bug in .NET we are trying to get around. */
                        System.Diagnostics.Trace.Assert(!args.DrawDefault, "Default drawing is impossible in this situation");
                    }
                    m.Result = (IntPtr)4;
                    return true;

                case cddsSubitempostpaint:
                    break;
            }
            return false;
        }

        protected virtual bool HandleDestroy(ref Message m)
        {
            /* Recreate the header control when the listview control is destroyed */
            BeginInvoke((MethodInvoker)delegate
                                           {
                                               _headerControl = null;
                                               HeaderControl.WordWrap = HeaderWordWrap;
                                           });

            /* When the underlying control is destroyed, we need to recreate
             * and reconfigure its tooltip */
            if (_cellToolTip == null)
            {
                return false;
            }
            _cellToolTip.PushSettings();
            base.WndProc(ref m);
            BeginInvoke((MethodInvoker)delegate
                                           {
                                               UpdateCellToolTipHandle();
                                               _cellToolTip.PopSettings();
                                           });
            return true;
        }

        protected virtual bool HandleFindItem(ref Message m)
        {
            /* NOTE: As far as I can see, this message is never actually sent to the control, making this
             * method redundant! */
            const int lvfiString = 0x0002;
            var findInfo = (NativeMethods.Lvfindinfo)m.GetLParam(typeof(NativeMethods.Lvfindinfo));
            /* We can only handle string searches */
            if ((findInfo.flags & lvfiString) != lvfiString)
            {
                return false;
            }
            var start = m.WParam.ToInt32();
            m.Result = (IntPtr)FindMatchingRow(findInfo.psz, start, SearchDirectionHint.Down);
            return true;
        }

        public virtual int FindMatchingRow(string text, int start, SearchDirectionHint direction)
        {
            /* We also can't do anything if we don't have data */
            int rowCount = GetItemCount();
            if (rowCount == 0)
            {
                return -1;
            }
            /* Which column are we going to use for our comparing? */
            var column = GetColumn(0);
            if (IsSearchOnSortColumn && View == View.Details && PrimarySortColumn != null)
            {
                column = PrimarySortColumn;
            }
            /* Do two searches if necessary to find a match. The second search is the wrap-around part of searching */
            int i;
            if (direction == SearchDirectionHint.Down)
            {
                i = FindMatchInRange(text, start, rowCount - 1, column);
                if (i == -1 && start > 0)
                {
                    i = FindMatchInRange(text, 0, start - 1, column);
                }
            }
            else
            {
                i = FindMatchInRange(text, start, 0, column);
                if (i == -1 && start != rowCount)
                {
                    i = FindMatchInRange(text, rowCount - 1, start + 1, column);
                }
            }
            return i;
        }

        protected virtual int FindMatchInRange(string text, int first, int last, OlvColumn column)
        {
            if (first <= last)
            {
                for (var i = first; i <= last; i++)
                {
                    var data = column.GetStringValue(GetNthItemInDisplayOrder(i).RowObject);
                    if (data.StartsWith(text, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return i;
                    }
                }
            }
            else
            {
                for (var i = first; i >= last; i--)
                {
                    var data = column.GetStringValue(GetNthItemInDisplayOrder(i).RowObject);
                    if (data.StartsWith(text, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        
        protected virtual bool HandleGroupInfo(ref Message m)
        {
            var nmlvgroup = (NativeMethods.Nmlvgroup)m.GetLParam(typeof(NativeMethods.Nmlvgroup));
            /* Ignore state changes that aren't related to selection, focus or collapsedness */
            const uint interestingStates = (uint)(GroupState.LvgsCollapsed | GroupState.LvgsFocused | GroupState.LvgsSelected);
            if ((nmlvgroup.uOldState & interestingStates) == (nmlvgroup.uNewState & interestingStates))
            {
                return false;
            }
            foreach (GroupStateChangedEventArgs args in from @group in OlvGroups
                                                        where @group.GroupId == nmlvgroup.iGroupId
                                                        select new GroupStateChangedEventArgs(@group,
                                                                                             (GroupState)nmlvgroup.uOldState,
                                                                                             (GroupState)nmlvgroup.uNewState))
            {
                OnGroupStateChanged(args);
                break;
            }
            return false;
        }

        protected virtual bool HandleKeyDown(ref Message m)
        {
            /* If this is a checkbox list, toggle the selected rows when the user presses Space */
            if (CheckBoxes && m.WParam.ToInt32() == (int)Keys.Space && SelectedIndices.Count > 0)
            {
                ToggleSelectedRowCheckBoxes();
                return true;
            }
            /* Remember the scroll position so we can decide if the listview has scrolled in the
             * handling of the event. */
            var scrollPositionH = NativeMethods.GetScrollPosition(this, true);
            var scrollPositionV = NativeMethods.GetScrollPosition(this, false);
            base.WndProc(ref m);
            /* It's possible that the processing in base.WndProc has actually destroyed this control */
            if (IsDisposed)
            {
                return true;
            }
            /* If the keydown processing changed the scroll position, trigger a Scroll event */
            var newScrollPositionH = NativeMethods.GetScrollPosition(this, true);
            var newScrollPositionV = NativeMethods.GetScrollPosition(this, false);
            if (scrollPositionH != newScrollPositionH)
            {
                var args = new ScrollEventArgs(ScrollEventType.EndScroll,
                                               scrollPositionH, newScrollPositionH,
                                               ScrollOrientation.HorizontalScroll);
                OnScroll(args);
                RefreshHotItem();
            }
            if (scrollPositionV != newScrollPositionV)
            {
                var args = new ScrollEventArgs(ScrollEventType.EndScroll,
                                               scrollPositionV, newScrollPositionV,
                                               ScrollOrientation.VerticalScroll);
                OnScroll(args);
                RefreshHotItem();
            }
            return true;
        }

        private void ToggleSelectedRowCheckBoxes()
        {
            /* This doesn't actually toggle all rows. It toggles the first row, and
             * all other rows get the check state of that first row. */
            var primaryModel = GetItem(SelectedIndices[0]).RowObject;
            ToggleCheckObject(primaryModel);
            var state = GetCheckState(primaryModel);
            if (!state.HasValue) { return; }
            foreach (var x in SelectedObjects)
            {
                SetObjectCheckedness(x, state.Value);
            }
        }

        protected virtual bool HandleLButtonDown(ref Message m)
        {
            /* We have to intercept this low level message rather than the more natural
             * overridding of OnMouseDown, since ListCtrl's internal mouse down behavior
             * is to select (or deselect) rows when the mouse is released. We don't
             * want the selection to change when the user checks or unchecks a checkbox, so if the
             * mouse down event was to check/uncheck, we have to hide this mouse
             * down event from the control. */
            var x = m.LParam.ToInt32() & 0xFFFF;
            var y = (m.LParam.ToInt32() >> 16) & 0xFFFF;
            return ProcessLButtonDown(OlvHitTest(x, y));
        }

        protected virtual bool ProcessLButtonDown(OlvListViewHitTestInfo hti)
        {
            if (hti.Item == null)
            {
                return false;
            }
            /* If they didn't click checkbox, we can just return */
            if (View != View.Details || hti.HitTestLocation != HitTestLocation.CheckBox)
            {
                return false;
            }
            /* Did they click a sub item checkbox? */
            if (hti.Column.Index > 0)
            {
                if (hti.Column.IsEditable)
                {
                    ToggleSubItemCheckBox(hti.RowObject, hti.Column);
                }
                return true;
            }
            /* They must have clicked the primary checkbox */
            ToggleCheckObject(hti.RowObject);
            /* If they change the checkbox of a selecte row, all the rows in the selection
             * should be given the same state */
            if (hti.Item.Selected)
            {
                var state = GetCheckState(hti.RowObject);
                if (state.HasValue)
                {
                    foreach (var x in SelectedObjects)
                    {
                        SetObjectCheckedness(x, state.Value);
                    }
                }
            }
            return true;
        }

        protected virtual bool HandleLButtonUp(ref Message m)
        {
            if (MouseMoveHitTest == null)
            {
                return false;
            }
            /* Are they trying to expand/collapse a group? */
            if (MouseMoveHitTest.HitTestLocation == HitTestLocation.GroupExpander)
            {
                if (TriggerGroupExpandCollapse(MouseMoveHitTest.Group))
                {
                    return true;
                }
            }
            if (IsVistaOrLater && HasCollapsibleGroups)
            {
                base.DefWndProc(ref m);
            }
            return false;
        }

        protected virtual bool TriggerGroupExpandCollapse(OlvGroup group)
        {
            var args = new GroupExpandingCollapsingEventArgs(group);
            OnGroupExpandingCollapsing(args);
            return args.Canceled;
        }

        protected virtual bool HandleRButtonDown(ref Message m)
        {
            var x = m.LParam.ToInt32() & 0xFFFF;
            var y = (m.LParam.ToInt32() >> 16) & 0xFFFF;
            return ProcessRButtonDown(OlvHitTest(x, y));
        }

        protected virtual bool ProcessRButtonDown(OlvListViewHitTestInfo hti)
        {
            if (hti.Item == null)
            {
                return false;
            }
            /* Ignore clicks on checkboxes */
            return (View == View.Details && hti.HitTestLocation == HitTestLocation.CheckBox);
        }

        protected virtual bool HandleLButtonDoubleClick(ref Message m)
        {
            var x = m.LParam.ToInt32() & 0xFFFF;
            var y = (m.LParam.ToInt32() >> 16) & 0xFFFF;
            return ProcessLButtonDoubleClick(OlvHitTest(x, y));
        }

        protected virtual bool ProcessLButtonDoubleClick(OlvListViewHitTestInfo hti)
        {
            /* If the user double clicked on a checkbox, ignore it */
            return (hti.HitTestLocation == HitTestLocation.CheckBox);
        }

        protected virtual bool HandleRButtonDoubleClick(ref Message m)
        {
            var x = m.LParam.ToInt32() & 0xFFFF;
            var y = (m.LParam.ToInt32() >> 16) & 0xFFFF;
            return ProcessRButtonDoubleClick(OlvHitTest(x, y));
        }

        protected virtual bool ProcessRButtonDoubleClick(OlvListViewHitTestInfo hti)
        {
            /* If the user double clicked on a checkbox, ignore it */
            return (hti.HitTestLocation == HitTestLocation.CheckBox);
        }

        protected virtual bool HandleMouseMove(ref Message m)
        {
            return false;
        }

        protected virtual bool HandleReflectNotify(ref Message m)
        {
            const int nmClick = -2;
            const int nmDblclk = -3;
            const int nmRdblclk = -6;
            const int nmCustomdraw = -12;
            const int nmReleasedcapture = -16;
            const int lvnFirst = -100;
            const int lvnItemchanged = lvnFirst - 1;
            const int lvnItemchanging = lvnFirst - 0;
            const int lvnHottrack = lvnFirst - 21;
            const int lvnMarqueebegin = lvnFirst - 56;
            const int lvnGetinfotip = lvnFirst - 58;
            const int lvnGetdispinfo = lvnFirst - 77;
            const int lvnBeginscroll = lvnFirst - 80;
            const int lvnEndscroll = lvnFirst - 81;
            const int lvnLinkclick = lvnFirst - 84;
            const int lvnGroupinfo = lvnFirst - 88; // undocumented
            const int lvifState = 8;

            var isMsgHandled = false;
            /* TODO: Don't do any logic in this method. Create separate methods for each message */
            var nmhdr = (NativeMethods.Nmhdr)m.GetLParam(typeof(NativeMethods.Nmhdr));            
            switch (nmhdr.code)
            {
                case nmClick:
                    /* The standard ListView does some strange stuff here when the list has checkboxes.
                     * If you shift click on non-primary columns when FullRowSelect is true, the 
                     * checkedness of the selected rows changes. 
                     * We avoid all that by just saying we've handled this message. */                    
                    isMsgHandled = true;
                    OnClick(EventArgs.Empty);
                    break;

                case lvnBeginscroll:
                    isMsgHandled = HandleBeginScroll(ref m);
                    break;

                case lvnEndscroll:
                    isMsgHandled = HandleEndScroll(ref m);
                    break;

                case lvnLinkclick:
                    isMsgHandled = HandleLinkClick(ref m);
                    break;

                case lvnMarqueebegin:
                    _isMarqueSelecting = true;
                    break;

                case lvnGetinfotip:
                    /* When virtual lists are in SmallIcon view, they generates tooltip message with invalid item indicies. */
                    var nmGetInfoTip = (NativeMethods.Nmlvgetinfotip)m.GetLParam(typeof(NativeMethods.Nmlvgetinfotip));
                    isMsgHandled = nmGetInfoTip.iItem >= GetItemCount();
                    break;

                case nmReleasedcapture:
                    _isMarqueSelecting = false;
                    Invalidate();
                    break;

                case nmCustomdraw:
                    isMsgHandled = HandleCustomDraw(ref m);
                    break;

                case nmDblclk:
                    /* The default behavior of a .NET ListView with checkboxes is to toggle the checkbox on
                     * double-click. That's just silly, if you ask me :) */
                    if (CheckBoxes)
                    {
                        /* How do we make ListView not do that silliness? We could just ignore the message
                         * but the last part of the base code sets up state information, and without that
                         * state, the ListView doesn't trigger MouseDoubleClick events. So we fake a
                         * right button double click event, which sets up the same state, but without
                         * toggling the checkbox. */
                        nmhdr.code = nmRdblclk;
                        Marshal.StructureToPtr(nmhdr, m.LParam, false);
                    }
                    break;

                case lvnItemchanged:
                    var nmlistviewPtr2 = (NativeMethods.Nmlistview)m.GetLParam(typeof(NativeMethods.Nmlistview));
                    if ((nmlistviewPtr2.uChanged & lvifState) != 0)
                    {
                        var currentValue = CalculateCheckState(nmlistviewPtr2.uOldState);
                        var newCheckValue = CalculateCheckState(nmlistviewPtr2.uNewState);
                        if (currentValue != newCheckValue)
                        {
                            /* Remove the state indicies so that we don't trigger the OnItemChecked method
                             * when we call our base method after exiting this method */
                            nmlistviewPtr2.uOldState = (nmlistviewPtr2.uOldState & 0x0FFF);
                            nmlistviewPtr2.uNewState = (nmlistviewPtr2.uNewState & 0x0FFF);
                            Marshal.StructureToPtr(nmlistviewPtr2, m.LParam, false);
                        }
                    }
                    break;

                case lvnItemchanging:
                    var nmlistviewPtr = (NativeMethods.Nmlistview)m.GetLParam(typeof(NativeMethods.Nmlistview));
                    if ((nmlistviewPtr.uChanged & lvifState) != 0)
                    {
                        var currentValue = CalculateCheckState(nmlistviewPtr.uOldState);
                        var newCheckValue = CalculateCheckState(nmlistviewPtr.uNewState);
                        if (currentValue != newCheckValue)
                        {
                            /* Prevent the base method from seeing the state change,
                             * since we handled it elsewhere */
                            nmlistviewPtr.uChanged &= ~lvifState;
                            Marshal.StructureToPtr(nmlistviewPtr, m.LParam, false);
                        }
                    }
                    break;

                case lvnHottrack:
                case lvnGetdispinfo:
                    break;

                case lvnGroupinfo:
                    isMsgHandled = HandleGroupInfo(ref m);
                    break;
            }
            return isMsgHandled;
        }

        private static CheckState CalculateCheckState(int state)
        {
            switch ((state & 0xf000) >> 12)
            {
                case 1:
                    return CheckState.Unchecked;

                case 2:
                    return CheckState.Checked;

                case 3:
                    return CheckState.Indeterminate;

                default:
                    return CheckState.Checked;
            }
        }

        protected bool HandleNotify(ref Message m)
        {
            var isMsgHandled = false;

            const int nmCustomdraw = -12;
            const int hdnFirst = (0 - 300);
            const int hdnItemchanginga = (hdnFirst - 0);
            const int hdnItemchangingw = (hdnFirst - 20);
            const int hdnItemclicka = (hdnFirst - 2);
            const int hdnItemclickw = (hdnFirst - 22);
            const int hdnDividerdblclicka = (hdnFirst - 5);
            const int hdnDividerdblclickw = (hdnFirst - 25);
            const int hdnBegintracka = (hdnFirst - 6);
            const int hdnBegintrackw = (hdnFirst - 26);
            const int hdnEndtracka = (hdnFirst - 7);
            const int hdnEndtrackw = (hdnFirst - 27);
            const int hdnTracka = (hdnFirst - 8);
            const int hdnTrackw = (hdnFirst - 28);
            /* Handle the notification, remembering to handle both ANSI and Unicode versions */
            var nmheader = (NativeMethods.Nmheader)m.GetLParam(typeof(NativeMethods.Nmheader));            
            switch (nmheader.nhdr.code)
            {
                case nmCustomdraw:
                    if (!OwnerDrawnHeader)
                    {
                        isMsgHandled = HeaderControl.HandleHeaderCustomDraw(ref m);
                    }
                    break;

                case hdnItemclicka:
                case hdnItemclickw:
                    if (!PossibleFinishCellEditing())
                    {
                        m.Result = (IntPtr)1; /* prevent the change from happening */
                        isMsgHandled = true;
                    }
                    break;

                case hdnDividerdblclicka:
                case hdnDividerdblclickw:
                case hdnBegintracka:
                case hdnBegintrackw:
                    if (!PossibleFinishCellEditing())
                    {
                        m.Result = (IntPtr)1; /* prevent the change from happening */
                        isMsgHandled = true;
                        break;
                    }
                    if (nmheader.iItem >= 0 && nmheader.iItem < Columns.Count)
                    {
                        var column = GetColumn(nmheader.iItem);
                        /* Space filling columns can't be dragged or double-click resized */
                        if (column.FillsFreeSpace)
                        {
                            m.Result = (IntPtr)1; /* prevent the change from happening */
                            isMsgHandled = true;
                        }
                    }
                    break;

                case hdnEndtracka:
                case hdnEndtrackw:
                    break;

                case hdnTracka:
                case hdnTrackw:
                    if (nmheader.iItem >= 0 && nmheader.iItem < Columns.Count)
                    {
                        var hditem = (NativeMethods.HdItem)Marshal.PtrToStructure(nmheader.pHDITEM, typeof(NativeMethods.HdItem));
                        var column = GetColumn(nmheader.iItem);
                        if (hditem.cxy < column.MinimumWidth)
                        {
                            hditem.cxy = column.MinimumWidth;
                        }
                        else if (column.MaximumWidth != -1 && hditem.cxy > column.MaximumWidth)
                        {
                            hditem.cxy = column.MaximumWidth;
                        }
                        Marshal.StructureToPtr(hditem, nmheader.pHDITEM, false);
                    }
                    break;

                case hdnItemchanginga:
                case hdnItemchangingw:
                    nmheader = (NativeMethods.Nmheader)m.GetLParam(typeof(NativeMethods.Nmheader));
                    if (nmheader.iItem >= 0 && nmheader.iItem < Columns.Count)
                    {
                        var hditem = (NativeMethods.HdItem)Marshal.PtrToStructure(nmheader.pHDITEM, typeof(NativeMethods.HdItem));
                        var column = GetColumn(nmheader.iItem);
                        /* Check the mask to see if the width field is valid, and if it is, make sure it's within range */
                        if ((hditem.mask & 1) == 1)
                        {
                            if (hditem.cxy < column.MinimumWidth || (column.MaximumWidth != -1 && hditem.cxy > column.MaximumWidth))
                            {
                                m.Result = (IntPtr)1; /* prevent the change from happening */
                                isMsgHandled = true;
                            }
                        }
                    }
                    break;

                case ToolTipControl.TtnShow:
                    if (CellToolTip.Handle == nmheader.nhdr.hwndFrom)
                    {
                        isMsgHandled = CellToolTip.HandleShow(ref m);
                    }
                    break;

                case ToolTipControl.TtnPop:
                    if (CellToolTip.Handle == nmheader.nhdr.hwndFrom)
                    {
                        isMsgHandled = CellToolTip.HandlePop(ref m);
                    }
                    break;

                case ToolTipControl.TtnGetdispinfo:
                    if (CellToolTip.Handle == nmheader.nhdr.hwndFrom)
                    {
                        isMsgHandled = CellToolTip.HandleGetDispInfo(ref m);
                    }
                    break;
            }
            return isMsgHandled;
        }

        protected virtual void CreateCellToolTip()
        {
            _cellToolTip = new ToolTipControl();
            _cellToolTip.AssignHandle(NativeMethods.GetTooltipControl(this));
            _cellToolTip.Showing += HandleCellToolTipShowing;
            _cellToolTip.SetMaxWidth();
            NativeMethods.MakeTopMost(_cellToolTip);
        }

        protected virtual void UpdateCellToolTipHandle()
        {
            if (_cellToolTip != null && _cellToolTip.Handle == IntPtr.Zero)
            {
                _cellToolTip.AssignHandle(NativeMethods.GetTooltipControl(this));
            }
        }

        protected virtual bool HandlePaint(ref Message m)
        {
            /* We only want to custom draw the control within WmPaint message and only
             * once per paint event. We use these bools to insure  */
            _isInWmPaintEvent = true;
            _shouldDoCustomDrawing = true;
            _prePaintLevel = 0;
            ShowOverlays();
            base.WndProc(ref m);
            HandlePostPaint();
            _isInWmPaintEvent = false;
            return true;
        }

        protected virtual void HandlePostPaint()
        {
            /* This message is no longer necessary, but we keep it for compatibility */
        }

        protected virtual bool HandleWindowPosChanging(ref Message m)
        {
            const int swpNosize = 1;
            var pos = (NativeMethods.Windowpos)m.GetLParam(typeof(NativeMethods.Windowpos));
            if ((pos.flags & swpNosize) == 0)
            {
                if (pos.cx < Bounds.Width)
                {
                    /* only when shrinking -- pos.cx is the window width, not the client area width,
                     * so we have to subtract the border widths */
                    ResizeFreeSpaceFillingColumns(pos.cx - (Bounds.Width - ClientSize.Width));
                }
            }
            return false;
        }
    }
}
