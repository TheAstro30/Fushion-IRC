//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE.
//
//  License: GNU Lesser General Public License (LGPLv3)
//
//  Email: pavel_torgashov@ukr.net
//
//  Copyright (C) Pavel Torgashov, 2011-2016.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ircScript.Controls.SyntaxHighlight.Commands;
using ircScript.Controls.SyntaxHighlight.Forms.Hotkeys;
using ircScript.Controls.SyntaxHighlight.Helpers.TextRange;

namespace ircScript.Controls.SyntaxHighlight.Controls.AutoComplete
{
    [ToolboxItem(false)]
    public sealed class AutocompleteListView : UserControl
    {
        private const int HoveredItemIndex = -1;
        private readonly FastColoredTextBox _tb;
        private readonly Timer _timer = new Timer();
        internal ToolTip ToolTip = new ToolTip();
        internal List<AutoCompleteItem> VisibleItems;
        private int _focussedItemIndex;

        private int _oldItemCount;
        private IEnumerable<AutoCompleteItem> _sourceItems = new List<AutoCompleteItem>();

        internal AutocompleteListView(FastColoredTextBox tb)
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            Font = new Font(FontFamily.GenericSansSerif, 9);
            VisibleItems = new List<AutoCompleteItem>();
            VerticalScroll.SmallChange = ItemHeight;
            MaximumSize = new Size(Size.Width, 180);
            ToolTip.ShowAlways = false;
            AppearInterval = 500;
            _timer.Tick += TimerTick;
            SelectedColor = Color.Orange;
            HoveredColor = Color.Red;
            ToolTipDuration = 3000;
            ToolTip.Popup += ToolTip_Popup;

            _tb = tb;

            tb.KeyDown += TextBoxKeyDown;
            tb.SelectionChanged += TextBoxSelectionChanged;
            tb.KeyPressed += TextBoxKeyPressed;

            var form = tb.FindForm();
            if (form != null)
            {
                form.LocationChanged += delegate { SafetyClose(); };
                form.ResizeBegin += delegate { SafetyClose(); };
                form.FormClosing += delegate { SafetyClose(); };
                form.LostFocus += delegate { SafetyClose(); };
            }
            tb.LostFocus += (o, e) =>
                                {
                                    if (Menu == null || Menu.IsDisposed)
                                    {
                                        return;
                                    }
                                    if (!Menu.Focused)
                                    {
                                        SafetyClose();
                                    }
                                };

            tb.Scroll += delegate { SafetyClose(); };

            VisibleChanged += (o, e) =>
                                  {
                                      if (Visible)
                                          DoSelectedVisible();
                                  };
        }

        private int ItemHeight
        {
            get { return Font.Height + 2; }
        }

        private AutoCompleteMenu Menu
        {
            get { return Parent as AutoCompleteMenu; }
        }

        internal bool AllowTabKey { get; set; }
        public ImageList ImageList { get; set; }

        internal int AppearInterval
        {
            get { return _timer.Interval; }
            set { _timer.Interval = value; }
        }

        internal int ToolTipDuration { get; set; }
        internal Size MaxToolTipSize { get; set; }

        internal bool AlwaysShowTooltip
        {
            get { return ToolTip.ShowAlways; }
            set { ToolTip.ShowAlways = value; }
        }

        public Color SelectedColor { get; set; }
        public Color HoveredColor { get; set; }

        public int FocussedItemIndex
        {
            get { return _focussedItemIndex; }
            set
            {
                if (_focussedItemIndex == value)
                {
                    return;
                }
                _focussedItemIndex = value;
                if (FocussedItemIndexChanged != null)
                {
                    FocussedItemIndexChanged(this, EventArgs.Empty);
                }
            }
        }

        public AutoCompleteItem FocussedItem
        {
            get
            {
                if (FocussedItemIndex >= 0 && _focussedItemIndex < VisibleItems.Count)
                {
                    return VisibleItems[_focussedItemIndex];
                }
                return null;
            }
            set { FocussedItemIndex = VisibleItems.IndexOf(value); }
        }

        public int Count
        {
            get { return VisibleItems.Count; }
        }

        public event EventHandler FocussedItemIndexChanged;

        private void ToolTip_Popup(object sender, PopupEventArgs e)
        {
            if (MaxToolTipSize.Height > 0 && MaxToolTipSize.Width > 0)
            {
                e.ToolTipSize = MaxToolTipSize;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (ToolTip != null)
            {
                ToolTip.Popup -= ToolTip_Popup;
                ToolTip.Dispose();
            }
            if (_tb != null)
            {
                _tb.KeyDown -= TextBoxKeyDown;
                _tb.KeyPressed -= TextBoxKeyPressed;
                _tb.SelectionChanged -= TextBoxSelectionChanged;
            }

            if (_timer != null)
            {
                _timer.Stop();
                _timer.Tick -= TimerTick;
                _timer.Dispose();
            }
            base.Dispose(disposing);
        }

        private void SafetyClose()
        {
            if (Menu != null && !Menu.IsDisposed)
            {
                Menu.Close();
            }
        }

        private void TextBoxKeyPressed(object sender, KeyPressEventArgs e)
        {
            var backspaceORdel = e.KeyChar == '\b' || e.KeyChar == 0xff;
            if (Menu.Visible && !backspaceORdel)
            {
                DoAutocomplete(false);
            }
            else
            {
                ResetTimer(_timer);
            }
        }

        private void TimerTick(object sender, EventArgs e)
        {
            _timer.Stop();
            DoAutocomplete(false);
        }

        private static void ResetTimer(Timer timer)
        {
            timer.Stop();
            timer.Start();
        }

        internal void DoAutocomplete()
        {
            DoAutocomplete(false);
        }

        internal void DoAutocomplete(bool forced)
        {
            if (!Menu.Enabled)
            {
                Menu.Close();
                return;
            }
            VisibleItems.Clear();
            FocussedItemIndex = 0;
            VerticalScroll.Value = 0;
            /* Some magic for update scrolls */
            AutoScrollMinSize -= new Size(1, 0);
            AutoScrollMinSize += new Size(1, 0);
            /* Get fragment around caret */
            var fragment = _tb.Selection.GetFragment(Menu.SearchPattern);
            var text = fragment.Text;
            /* Calc screen point for popup menu */
            var point = _tb.PlaceToPoint(fragment.End);
            point.Offset(2, _tb.CharHeight);
            if (forced || (text.Length >= Menu.MinFragmentLength
                           && _tb.Selection.IsEmpty /*pops up only if selected range is empty*/
                           &&
                           (_tb.Selection.Start > fragment.Start || text.Length == 0
                           /* Pops up only if caret is after first letter */)))
            {
                Menu.Fragment = fragment;
                var foundSelected = false;
                /* Build popup menu */
                foreach (var item in _sourceItems)
                {
                    item.Parent = Menu;
                    var res = item.Compare(text);
                    if (res != CompareResult.Hidden)
                    {
                        VisibleItems.Add(item);
                    }
                    if (res != CompareResult.VisibleAndSelected || foundSelected)
                    {
                        continue;
                    }
                    foundSelected = true;
                    FocussedItemIndex = VisibleItems.Count - 1;
                }
                if (foundSelected)
                {
                    AdjustScroll();
                    DoSelectedVisible();
                }
            }
            /* Show popup menu */
            if (Count > 0)
            {
                if (!Menu.Visible)
                {
                    var args = new CancelEventArgs();
                    Menu.OnOpening(args);
                    if (!args.Cancel)
                    {
                        Menu.Show(_tb, point);
                    }
                }
                DoSelectedVisible();
                Invalidate();
            }
            else
            {
                Menu.Close();
            }
        }

        private void TextBoxSelectionChanged(object sender, EventArgs e)
        {
            if (!Menu.Visible)
            {
                return;
            }
            var needClose = false;
            if (!_tb.Selection.IsEmpty)
            {
                needClose = true;
            }
            else if (!Menu.Fragment.Contains(_tb.Selection.Start))
            {
                if (_tb.Selection.Start.Line == Menu.Fragment.End.Line &&
                    _tb.Selection.Start.Char == Menu.Fragment.End.Char + 1)
                {
                    /* User press key at end of fragment */
                    var c = _tb.Selection.CharBeforeStart;
                    if (!Regex.IsMatch(c.ToString(), Menu.SearchPattern)) //check char
                    {
                        needClose = true;
                    }
                }
                else
                {
                    needClose = true;
                }
            }
            if (needClose)
            {
                Menu.Close();
            }
        }

        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            var tb = sender as FastColoredTextBox;
            if (Menu.Visible)
            {
                if (ProcessKey(e.KeyCode, e.Modifiers))
                {
                    e.Handled = true;
                }
            }
            if (Menu.Visible)
            {
                return;
            }
            if (tb != null && (tb.HotkeysMapping.ContainsKey(e.KeyData) &&
                               tb.HotkeysMapping[e.KeyData] == FctbAction.AutocompleteMenu))
            {
                DoAutocomplete();
                e.Handled = true;
            }
            else
            {
                if (e.KeyCode == Keys.Escape && _timer.Enabled)
                {
                    _timer.Stop();
                }
            }
        }

        private void AdjustScroll()
        {
            if (_oldItemCount == VisibleItems.Count)
            {
                return;
            }
            var needHeight = ItemHeight*VisibleItems.Count + 1;
            Height = Math.Min(needHeight, MaximumSize.Height);
            Menu.CalcSize();
            AutoScrollMinSize = new Size(0, needHeight);
            _oldItemCount = VisibleItems.Count;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            AdjustScroll();
            var itemHeight = ItemHeight;
            var startI = VerticalScroll.Value/itemHeight - 1;
            var finishI = (VerticalScroll.Value + ClientSize.Height)/itemHeight + 1;
            startI = Math.Max(startI, 0);
            finishI = Math.Min(finishI, VisibleItems.Count);
            int y;
            const int leftPadding = 18;
            for (var i = startI; i < finishI; i++)
            {
                y = i*itemHeight - VerticalScroll.Value;
                var item = VisibleItems[i];
                if (item.BackColor != Color.Transparent)
                {
                    using (var brush = new SolidBrush(item.BackColor))
                    {
                        e.Graphics.FillRectangle(brush, 1, y, ClientSize.Width - 1 - 1, itemHeight - 1);
                    }
                }
                if (ImageList != null && VisibleItems[i].ImageIndex >= 0)
                {
                    e.Graphics.DrawImage(ImageList.Images[item.ImageIndex], 1, y);
                }
                if (i == FocussedItemIndex)
                {
                    using (
                        var selectedBrush = new LinearGradientBrush(new Point(0, y - 3), new Point(0, y + itemHeight),
                                                                    Color.Transparent, SelectedColor))
                    {
                        using (var pen = new Pen(SelectedColor))
                        {
                            e.Graphics.FillRectangle(selectedBrush,
                                                     leftPadding, y,
                                                     ClientSize.Width -
                                                     1 - leftPadding,
                                                     itemHeight - 1);
                            e.Graphics.DrawRectangle(pen, leftPadding, y,
                                                     ClientSize.Width -
                                                     1 - leftPadding,
                                                     itemHeight - 1);
                        }
                    }
                }
                if (i == HoveredItemIndex)
                {
                    using (var pen = new Pen(HoveredColor))
                    {
                        e.Graphics.DrawRectangle(pen, leftPadding, y, ClientSize.Width - 1 - leftPadding, itemHeight - 1);
                    }
                }
                using (var brush = new SolidBrush(item.ForeColor != Color.Transparent ? item.ForeColor : ForeColor))
                {
                    e.Graphics.DrawString(item.ToString(), Font, brush, leftPadding, y);
                }
            }
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            FocussedItemIndex = PointToItemIndex(e.Location);
            DoSelectedVisible();
            Invalidate();
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            FocussedItemIndex = PointToItemIndex(e.Location);
            Invalidate();
            OnSelecting();
        }

        internal void OnSelecting()
        {
            if (FocussedItemIndex < 0 || FocussedItemIndex >= VisibleItems.Count)
            {
                return;
            }
            _tb.TextSource.Manager.BeginAutoUndoCommands();
            try
            {
                var item = FocussedItem;
                var args = new SelectingEventArgs
                               {
                                   Item = item,
                                   SelectedIndex = FocussedItemIndex
                               };

                Menu.OnSelecting(args);
                if (args.Cancel)
                {
                    FocussedItemIndex = args.SelectedIndex;
                    Invalidate();
                    return;
                }
                if (!args.Handled)
                {
                    var fragment = Menu.Fragment;
                    DoAutocomplete(item, fragment);
                }
                Menu.Close();
                var args2 = new SelectedEventArgs
                                {
                                    Item = item,
                                    Tb = Menu.Fragment.TextBox
                                };
                item.OnSelected(Menu, args2);
                Menu.OnSelected(args2);
            }
            finally
            {
                _tb.TextSource.Manager.EndAutoUndoCommands();
            }
        }

        private static void DoAutocomplete(AutoCompleteItem item, Range fragment)
        {
            var newText = item.GetTextForReplace();
            /* Replace text of fragment */
            var tb = fragment.TextBox;
            tb.BeginAutoUndo();
            tb.TextSource.Manager.ExecuteCommand(new SelectCommand(tb.TextSource));
            if (tb.Selection.ColumnSelectionMode)
            {
                var start = tb.Selection.Start;
                var end = tb.Selection.End;
                start.Char = fragment.Start.Char;
                end.Char = fragment.End.Char;
                tb.Selection.Start = start;
                tb.Selection.End = end;
            }
            else
            {
                tb.Selection.Start = fragment.Start;
                tb.Selection.End = fragment.End;
            }
            tb.InsertText(newText);
            tb.TextSource.Manager.ExecuteCommand(new SelectCommand(tb.TextSource));
            tb.EndAutoUndo();
            tb.Focus();
        }

        private int PointToItemIndex(Point p)
        {
            return (p.Y + VerticalScroll.Value)/ItemHeight;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            ProcessKey(keyData, Keys.None);
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private bool ProcessKey(Keys keyData, Keys keyModifiers)
        {
            if (keyModifiers == Keys.None)
            {
                switch (keyData)
                {
                    case Keys.Down:
                        SelectNext(+1);
                        return true;

                    case Keys.PageDown:
                        SelectNext(+10);
                        return true;

                    case Keys.Up:
                        SelectNext(-1);
                        return true;

                    case Keys.PageUp:
                        SelectNext(-10);
                        return true;

                    case Keys.Enter:
                        OnSelecting();
                        return true;

                    case Keys.Tab:
                        if (!AllowTabKey)
                        {
                            break;
                        }
                        OnSelecting();
                        return true;

                    case Keys.Escape:
                        Menu.Close();
                        return true;
                }
            }
            return false;
        }

        public void SelectNext(int shift)
        {
            FocussedItemIndex = Math.Max(0, Math.Min(FocussedItemIndex + shift, VisibleItems.Count - 1));
            DoSelectedVisible();
            Invalidate();
        }

        private void DoSelectedVisible()
        {
            if (FocussedItem != null)
            {
                SetToolTip(FocussedItem);
            }
            var y = FocussedItemIndex*ItemHeight - VerticalScroll.Value;
            if (y < 0)
            {
                VerticalScroll.Value = FocussedItemIndex*ItemHeight;
            }
            if (y > ClientSize.Height - ItemHeight)
            {
                VerticalScroll.Value = Math.Min(VerticalScroll.Maximum,
                                                FocussedItemIndex*ItemHeight - ClientSize.Height + ItemHeight);
            }
            /* Some magic for update scrolls */
            AutoScrollMinSize -= new Size(1, 0);
            AutoScrollMinSize += new Size(1, 0);
        }

        private void SetToolTip(AutoCompleteItem autoCompleteItem)
        {
            var title = autoCompleteItem.ToolTipTitle;
            var text = autoCompleteItem.ToolTipText;
            if (string.IsNullOrEmpty(title))
            {
                ToolTip.ToolTipTitle = null;
                ToolTip.SetToolTip(this, null);
                return;
            }
            if (Parent == null)
            {
                return;
            }
            IWin32Window window = Parent;
            var location = (PointToScreen(Location).X + MaxToolTipSize.Width + 105) <
                           Screen.FromControl(Parent).WorkingArea.Right
                               ? new Point(Right + 5, 0)
                               : new Point(Left - 105 - MaximumSize.Width, 0);
            if (string.IsNullOrEmpty(text))
            {
                ToolTip.ToolTipTitle = null;
                ToolTip.Show(title, window, location.X, location.Y, ToolTipDuration);
            }
            else
            {
                ToolTip.ToolTipTitle = title;
                ToolTip.Show(text, window, location.X, location.Y, ToolTipDuration);
            }
        }

        public void SetAutocompleteItems(ICollection<string> items)
        {
            var list = new List<AutoCompleteItem>(items.Count);
            list.AddRange(items.Select(item => new AutoCompleteItem(item)));
            SetAutocompleteItems(list);
        }

        public void SetAutocompleteItems(IEnumerable<AutoCompleteItem> items)
        {
            _sourceItems = items;
        }
    }
}