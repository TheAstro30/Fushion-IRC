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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using libolv.Implementation;
using libolv.Rendering.Overlays;

namespace libolv.DragDrop.DropSink
{
    public class SimpleDropSink : AbstractDropSink
    {
        private bool _acceptExternal = true;
        private bool _autoScroll = true;
        private int _dropTargetIndex = -1;
        private DropTargetLocation _dropTargetLocation;
        private int _dropTargetSubItemIndex = -1;
        private bool _useDefaultCursors = true;

        private readonly Timer _timer;
        private int _scrollAmount;
        private bool _originalFullRowSelect;
        private ModelDropEventArgs _dropEventArgs;

        /* Events */
        public event EventHandler<OlvDropEventArgs> CanDrop;
        public event EventHandler<OlvDropEventArgs> Dropped;
        public event EventHandler<ModelDropEventArgs> ModelCanDrop;
        public event EventHandler<ModelDropEventArgs> ModelDropped;

        public SimpleDropSink()
        {
            _timer = new Timer
                         {
                             Interval = 250
                         };
            _timer.Tick += TimerTick;
            CanDropOnItem = true;
            FeedbackColor = Color.FromArgb(180, Color.MediumBlue);
            Billboard = new BillboardOverlay();
        }

        /* Public properties */
        public DropTargetLocation AcceptableLocations { get; set; }

        public bool AcceptExternal
        {
            get { return _acceptExternal; }
            set { _acceptExternal = value; }
        }

        public bool AutoScroll
        {
            get { return _autoScroll; }
            set { _autoScroll = value; }
        }

        public BillboardOverlay Billboard { get; set; }

        public bool CanDropBetween
        {
            get { return (AcceptableLocations & DropTargetLocation.BetweenItems) == DropTargetLocation.BetweenItems; }
            set
            {
                if (value)
                {
                    AcceptableLocations |= DropTargetLocation.BetweenItems;
                }
                else
                {
                    AcceptableLocations &= ~DropTargetLocation.BetweenItems;
                }
            }
        }

        public bool CanDropOnBackground
        {
            get { return (AcceptableLocations & DropTargetLocation.Background) == DropTargetLocation.Background; }
            set
            {
                if (value)
                {
                    AcceptableLocations |= DropTargetLocation.Background;
                }
                else
                {
                    AcceptableLocations &= ~DropTargetLocation.Background;
                }
            }
        }

        public bool CanDropOnItem
        {
            get { return (AcceptableLocations & DropTargetLocation.Item) == DropTargetLocation.Item; }
            set
            {
                if (value)
                {
                    AcceptableLocations |= DropTargetLocation.Item;
                }
                else
                {
                    AcceptableLocations &= ~DropTargetLocation.Item;
                }
            }
        }

        public bool CanDropOnSubItem
        {
            get { return (AcceptableLocations & DropTargetLocation.SubItem) == DropTargetLocation.SubItem; }
            set
            {
                if (value)
                {
                    AcceptableLocations |= DropTargetLocation.SubItem;
                }
                else
                {
                    AcceptableLocations &= ~DropTargetLocation.SubItem;
                }
            }
        }

        public int DropTargetIndex
        {
            get { return _dropTargetIndex; }
            set
            {
                if (_dropTargetIndex == value)
                {
                    return;
                }
                _dropTargetIndex = value;
                ListView.Invalidate();
            }
        }

        public OlvListItem DropTargetItem
        {
            get { return ListView.GetItem(DropTargetIndex); }
        }

        public DropTargetLocation DropTargetLocation
        {
            get { return _dropTargetLocation; }
            set
            {
                if (_dropTargetLocation == value)
                {
                    return;
                }
                _dropTargetLocation = value;
                ListView.Invalidate();
            }
        }

        public int DropTargetSubItemIndex
        {
            get { return _dropTargetSubItemIndex; }
            set
            {
                if (_dropTargetSubItemIndex == value)
                {
                    return;
                }
                _dropTargetSubItemIndex = value;
                ListView.Invalidate();
            }
        }

        public Color FeedbackColor { get; set; }

        public bool IsAltDown
        {
            get { return (KeyState & 32) == 32; }
        }

        public bool IsAnyModifierDown
        {
            get { return (KeyState & (4 + 8 + 32)) != 0; }
        }

        public bool IsControlDown
        {
            get { return (KeyState & 8) == 8; }
        }

        public bool IsLeftMouseButtonDown
        {
            get { return (KeyState & 1) == 1; }
        }

        public bool IsMiddleMouseButtonDown
        {
            get { return (KeyState & 16) == 16; }
        }

        public bool IsRightMouseButtonDown
        {
            get { return (KeyState & 2) == 2; }
        }

        public bool IsShiftDown
        {
            get { return (KeyState & 4) == 4; }
        }

        public int KeyState { get; set; }

        public bool UseDefaultCursors
        {
            get { return _useDefaultCursors; }
            set { _useDefaultCursors = value; }
        }

        /* DropSink Interface */

        protected override void Cleanup()
        {
            DropTargetLocation = DropTargetLocation.None;
            ListView.FullRowSelect = _originalFullRowSelect;
            Billboard.Text = null;
        }

        public override void DrawFeedback(Graphics g, Rectangle bounds)
        {
            g.SmoothingMode = ObjectListView.SmoothingMode;
            switch (DropTargetLocation)
            {
                case DropTargetLocation.Background:
                    DrawFeedbackBackgroundTarget(g, bounds);
                    break;

                case DropTargetLocation.Item:
                    DrawFeedbackItemTarget(g, bounds);
                    break;

                case DropTargetLocation.AboveItem:
                    DrawFeedbackAboveItemTarget(g, bounds);
                    break;

                case DropTargetLocation.BelowItem:
                    DrawFeedbackBelowItemTarget(g, bounds);
                    break;
            }
            if (Billboard != null)
            {
                Billboard.Draw(ListView, g, bounds);
            }
        }

        public override void Drop(DragEventArgs args)
        {
            _dropEventArgs.DragEventArgs = args;
            TriggerDroppedEvent(args);
            _timer.Stop();
            Cleanup();
        }

        public override void Enter(DragEventArgs args)
        {
            /* 
             * When FullRowSelect is true, we have two problems:
             * 1) GetItemRect(ItemOnly) returns the whole row rather than just the icon/text, which messes
             *    up our calculation of the drop rectangle.
             * 2) during the drag, the Timer events will not fire! This is the major problem, since without
             *    those events we can't autoscroll. 
             * 
             * The first problem we can solve through coding, but the second is more difficult. 
             * We avoid both problems by turning off FullRowSelect during the drop operation.
             */
            _originalFullRowSelect = ListView.FullRowSelect;
            ListView.FullRowSelect = false;
            /* Setup our drop event args block */
            _dropEventArgs = new ModelDropEventArgs
                                 {
                                     DropSink = this,
                                     ListView = ListView,
                                     DragEventArgs = args,
                                     DataObject = args.Data
                                 };
            var olvData = args.Data as OlvDataObject;
            if (olvData != null)
            {
                _dropEventArgs.SourceListView = olvData.ListView;
                _dropEventArgs.SourceModels = olvData.ModelObjects;
            }
            Over(args);
        }

        public override void GiveFeedback(GiveFeedbackEventArgs args)
        {
            args.UseDefaultCursors = UseDefaultCursors;
        }

        public override void Over(DragEventArgs args)
        {
            _dropEventArgs.DragEventArgs = args;
            KeyState = args.KeyState;
            var pt = ListView.PointToClient(new Point(args.X, args.Y));
            args.Effect = CalculateDropAction(args, pt);
            CheckScrolling(pt);
        }

        /* Events */

        protected virtual void TriggerDroppedEvent(DragEventArgs args)
        {
            _dropEventArgs.Handled = false;
            /* If the source is an ObjectListView, trigger the ModelDropped event */
            if (_dropEventArgs.SourceListView != null)
            {
                OnModelDropped(_dropEventArgs);
            }
            if (!_dropEventArgs.Handled)
            {
                OnDropped(_dropEventArgs);
            }
        }

        protected virtual void OnCanDrop(OlvDropEventArgs args)
        {
            if (CanDrop != null)
            {
                CanDrop(this, args);
            }
        }

        protected virtual void OnDropped(OlvDropEventArgs args)
        {
            if (Dropped != null)
            {
                Dropped(this, args);
            }
        }

        protected virtual void OnModelCanDrop(ModelDropEventArgs args)
        {
            /* Don't allow drops from other list, if that's what's configured */
            if (!AcceptExternal && args.SourceListView != null && args.SourceListView != ListView)
            {
                args.Effect = DragDropEffects.None;
                args.DropTargetLocation = DropTargetLocation.None;
                args.InfoMessage = "This list doesn't accept drops from other lists";
                return;
            }
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

        /* Implementation */

        private void TimerTick(object sender, EventArgs e)
        {
            HandleTimerTick();
        }

        protected virtual void HandleTimerTick()
        {
            /* If the mouse has been released, stop scrolling.
             * This is only necessary if the mouse is released outside of the control. 
             * If the mouse is released inside the control, we would receive a Drop event. */
            if ((IsLeftMouseButtonDown && (Control.MouseButtons & MouseButtons.Left) != MouseButtons.Left) ||
                (IsMiddleMouseButtonDown && (Control.MouseButtons & MouseButtons.Middle) != MouseButtons.Middle) ||
                (IsRightMouseButtonDown && (Control.MouseButtons & MouseButtons.Right) != MouseButtons.Right))
            {
                _timer.Stop();
                Cleanup();
                return;
            }
            /* Auto scrolling will continune while the mouse is close to the ListView */
            const int gracePerimeter = 30;
            var pt = ListView.PointToClient(Cursor.Position);
            var r2 = ListView.ClientRectangle;
            r2.Inflate(gracePerimeter, gracePerimeter);
            if (r2.Contains(pt))
            {
                ListView.LowLevelScroll(0, _scrollAmount);
            }
        }

        protected virtual void CalculateDropTarget(OlvDropEventArgs args, Point pt)
        {
            const int smallValue = 3;
            var location = DropTargetLocation.None;
            var targetIndex = -1;
            var targetSubIndex = 0;
            if (CanDropOnBackground)
            {
                location = DropTargetLocation.Background;
            }
            /* Which item is the mouse over?
             * If it is not over any item, it's over the background. */
            var info = ListView.OlvHitTest(pt.X, pt.Y);
            if (info.Item != null && CanDropOnItem)
            {
                location = DropTargetLocation.Item;
                targetIndex = info.Item.Index;
                if (info.SubItem != null && CanDropOnSubItem)
                {
                    targetSubIndex = info.Item.SubItems.IndexOf(info.SubItem);
                }
            }
            /* Check to see if the mouse is "between" rows.
             * ("between" is somewhat loosely defined) */
            if (CanDropBetween && ListView.GetItemCount() > 0)
            {
                /* If the mouse is over an item, check to see if it is near the top or bottom */
                if (location == DropTargetLocation.Item)
                {
                    if (info.Item != null && pt.Y - smallValue <= info.Item.Bounds.Top)
                    {
                        location = DropTargetLocation.AboveItem;
                    }
                    if (info.Item != null && pt.Y + smallValue >= info.Item.Bounds.Bottom)
                    {
                        location = DropTargetLocation.BelowItem;
                    }
                }
                else
                {
                    /* Is there an item a little below the mouse?
                     * If so, we say the drop point is above that row */
                    info = ListView.OlvHitTest(pt.X, pt.Y + smallValue);
                    if (info.Item != null)
                    {
                        targetIndex = info.Item.Index;
                        location = DropTargetLocation.AboveItem;
                    }
                    else
                    {
                        /* Is there an item a little above the mouse? */
                        info = ListView.OlvHitTest(pt.X, pt.Y - smallValue);
                        if (info.Item != null)
                        {
                            targetIndex = info.Item.Index;
                            location = DropTargetLocation.BelowItem;
                        }
                    }
                }
            }
            args.DropTargetLocation = location;
            args.DropTargetIndex = targetIndex;
            args.DropTargetSubItemIndex = targetSubIndex;
        }

        public virtual DragDropEffects CalculateDropAction(DragEventArgs args, Point pt)
        {
            CalculateDropTarget(_dropEventArgs, pt);
            _dropEventArgs.MouseLocation = pt;
            _dropEventArgs.InfoMessage = null;
            _dropEventArgs.Handled = false;
            if (_dropEventArgs.SourceListView != null)
            {
                _dropEventArgs.TargetModel = ListView.GetModelObject(_dropEventArgs.DropTargetIndex);
                OnModelCanDrop(_dropEventArgs);
            }
            if (!_dropEventArgs.Handled)
            {
                OnCanDrop(_dropEventArgs);
            }
            UpdateAfterCanDropEvent(_dropEventArgs);
            return _dropEventArgs.Effect;
        }

        public DragDropEffects CalculateStandardDropActionFromKeys()
        {
            return IsControlDown ? (IsShiftDown ? DragDropEffects.Link : DragDropEffects.Copy) : DragDropEffects.Move;
        }

        protected virtual void CheckScrolling(Point pt)
        {
            if (!AutoScroll)
            {
                return;
            }
            var r = ListView.ContentRectangle;
            var rowHeight = ListView.RowHeightEffective;
            var close = rowHeight;
            /* In Tile view, using the whole row height is too much */
            if (ListView.View == View.Tile)
            {
                close /= 2;
            }
            if (pt.Y <= (r.Top + close))
            {
                /* Scroll faster if the mouse is closer to the top */
                _timer.Interval = ((pt.Y <= (r.Top + close/2)) ? 100 : 350);
                _timer.Start();
                _scrollAmount = -rowHeight;
            }
            else
            {
                if (pt.Y >= (r.Bottom - close))
                {
                    _timer.Interval = ((pt.Y >= (r.Bottom - close/2)) ? 100 : 350);
                    _timer.Start();
                    _scrollAmount = rowHeight;
                }
                else
                {
                    _timer.Stop();
                }
            }
        }

        protected virtual void UpdateAfterCanDropEvent(OlvDropEventArgs args)
        {
            DropTargetIndex = args.DropTargetIndex;
            DropTargetLocation = args.DropTargetLocation;
            DropTargetSubItemIndex = args.DropTargetSubItemIndex;
            if (Billboard == null)
            {
                return;
            }
            var pt = args.MouseLocation;
            pt.Offset(5, 5);
            if (Billboard.Text == _dropEventArgs.InfoMessage && Billboard.Location == pt)
            {
                return;
            }
            Billboard.Text = _dropEventArgs.InfoMessage;
            Billboard.Location = pt;
            ListView.Invalidate();
        }

        /* Rendering */

        protected virtual void DrawFeedbackBackgroundTarget(Graphics g, Rectangle bounds)
        {
            const float penWidth = 12.0f;
            var r = bounds;
            r.Inflate((int)-penWidth/2, (int)-penWidth/2);
            using (var p = new Pen(Color.FromArgb(128, FeedbackColor), penWidth))
            {
                using (var path = GetRoundedRect(r, 30.0f))
                {
                    g.DrawPath(p, path);
                }
            }
        }

        protected virtual void DrawFeedbackItemTarget(Graphics g, Rectangle bounds)
        {
            if (DropTargetItem == null)
            {
                return;
            }
            var r = CalculateDropTargetRectangle(DropTargetItem, DropTargetSubItemIndex);
            r.Inflate(1, 1);
            var diameter = (float)r.Height/3;
            using (var path = GetRoundedRect(r, diameter))
            {
                using (var b = new SolidBrush(Color.FromArgb(48, FeedbackColor)))
                {
                    g.FillPath(b, path);
                }
                using (var p = new Pen(FeedbackColor, 3.0f))
                {
                    g.DrawPath(p, path);
                }
            }
        }

        protected virtual void DrawFeedbackAboveItemTarget(Graphics g, Rectangle bounds)
        {
            if (DropTargetItem == null)
            {
                return;
            }
            var r = CalculateDropTargetRectangle(DropTargetItem, DropTargetSubItemIndex);
            DrawBetweenLine(g, r.Left, r.Top, r.Right, r.Top);
        }

        protected virtual void DrawFeedbackBelowItemTarget(Graphics g, Rectangle bounds)
        {
            if (DropTargetItem == null)
            {
                return;
            }
            var r = CalculateDropTargetRectangle(DropTargetItem, DropTargetSubItemIndex);
            DrawBetweenLine(g, r.Left, r.Bottom, r.Right, r.Bottom);
        }

        protected GraphicsPath GetRoundedRect(Rectangle rect, float diameter)
        {
            var path = new GraphicsPath();
            var arc = new RectangleF(rect.X, rect.Y, diameter, diameter);
            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected virtual Rectangle CalculateDropTargetRectangle(OlvListItem item, int subItem)
        {
            if (subItem > 0)
            {
                return item.SubItems[subItem].Bounds;
            }
            var r = ListView.CalculateCellTextBounds(item, subItem);
            /* Allow for indent */
            if (item.IndentCount > 0)
            {
                var indentWidth = ListView.SmallImageSize.Width;
                r.X += (indentWidth*item.IndentCount);
                r.Width -= (indentWidth*item.IndentCount);
            }
            return r;
        }

        protected virtual void DrawBetweenLine(Graphics g, int x1, int y1, int x2, int y2)
        {
            using (Brush b = new SolidBrush(FeedbackColor))
            {
                var x = x1;
                var y = y1;
                using (var gp = new GraphicsPath())
                {
                    gp.AddLine(
                        x, y + 5,
                        x, y - 5);
                    gp.AddBezier(
                        x, y - 6,
                        x + 3, y - 2,
                        x + 6, y - 1,
                        x + 11, y);
                    gp.AddBezier(
                        x + 11, y,
                        x + 6, y + 1,
                        x + 3, y + 2,
                        x, y + 6);
                    gp.CloseFigure();
                    g.FillPath(b, gp);
                }
                x = x2;
                y = y2;
                using (var gp = new GraphicsPath())
                {
                    gp.AddLine(
                        x, y + 6,
                        x, y - 6);
                    gp.AddBezier(
                        x, y - 7,
                        x - 3, y - 2,
                        x - 6, y - 1,
                        x - 11, y);
                    gp.AddBezier(
                        x - 11, y,
                        x - 6, y + 1,
                        x - 3, y + 2,
                        x, y + 7);
                    gp.CloseFigure();
                    g.FillPath(b, gp);
                }
            }
            using (var p = new Pen(FeedbackColor, 3.0f))
            {
                g.DrawLine(p, x1, y1, x2, y2);
            }
        }
    }
}
