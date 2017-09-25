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
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows.Forms.VisualStyles;
using System.Drawing.Drawing2D;
using System.Security.Permissions;
using libolv.Implementation;
using libolv.Properties;
using libolv.Rendering.Styles;

namespace libolv.SubControls
{
    public class HeaderControl : NativeWindow
    {
        private ToolTipControl _toolTip;
        private int _columnShowingTip = -1;
        private bool _cachedNeedsCustomDraw;
        private IntPtr _fontHandle;

        public HeaderControl(ObjectListView olv)
        {
            ListView = olv;
            AssignHandle(NativeMethods.GetHeaderControl(olv));
        }

        /* Properties */
        public int ColumnIndexUnderCursor
        {
            get
            {
                var pt = ListView.PointToClient(Cursor.Position);
                pt.X += NativeMethods.GetScrollPosition(ListView, true);
                return NativeMethods.GetColumnUnderPoint(Handle, pt);
            }
        }

        public new IntPtr Handle
        {
            get { return NativeMethods.GetHeaderControl(ListView); }
        }

        protected bool IsCursorOverLockedDivider
        {
            get
            {
                var pt = ListView.PointToClient(Cursor.Position);
                pt.X += NativeMethods.GetScrollPosition(ListView, true);
                var dividerIndex = NativeMethods.GetDividerUnderPoint(Handle, pt);
                if (dividerIndex < 0 || dividerIndex >= ListView.Columns.Count)
                {
                    return false;
                }
                var column = ListView.GetColumn(dividerIndex);
                return column.IsFixedWidth || column.FillsFreeSpace;
            }
        }

        protected ObjectListView ListView { get; set; }

        public int MaximumHeight
        {
            get { return ListView.HeaderMaximumHeight; }
        }

        public ToolTipControl ToolTip
        {
            get
            {
                if (_toolTip == null)
                {
                    CreateToolTip();
                }
                return _toolTip;
            }
            protected set { _toolTip = value; }
        }

        public bool WordWrap { get; set; }

        /* Commands */
        protected int CalculateHeight(Graphics g)
        {
            var flags = TextFormatFlags;
            var columnUnderCursor = ColumnIndexUnderCursor;
            float height = 0.0f;
            for (var i = 0; i < ListView.Columns.Count; i++)
            {
                var column = ListView.GetColumn(i);
                height = Math.Max(height, CalculateColumnHeight(g, column, flags, columnUnderCursor == i, i));
            }
            return MaximumHeight == -1 ? (int)height : Math.Min(MaximumHeight, (int)height);
        }

        private float CalculateColumnHeight(Graphics g, OlvColumn column, TextFormatFlags flags, bool isHot, int i)
        {
            var f = CalculateFont(column, isHot, false);
            if (column.IsHeaderVertical)
            {
                return TextRenderer.MeasureText(g, column.Text, f, new Size(10000, 10000), flags).Width;
            }
            const int fudge = 9; /* 9 is a magic constant that makes it perfectly match XP behavior */
            if (!WordWrap)
            {
                return f.Height + fudge;
            }
            var r = GetItemRect(i);
            r.Width -= 6; /* Match the "tweaking" done in CustomRender */
            if (HasNonThemedSortIndicator(column))
            {
                r.Width -= 16;
            }
            if (column.HasHeaderImage)
            {
                r.Width -= column.ImageList.ImageSize.Width + 3;
            }
            if (HasFilterIndicator(column))
            {
                r.Width -= CalculateFilterIndicatorWidth(r);
            }
            SizeF size = TextRenderer.MeasureText(g, column.Text, f, new Size(r.Width, 100), flags);
            return size.Height + fudge;
        }

        protected bool HasSortIndicator(OlvColumn column)
        {
            if (!ListView.ShowSortIndicators) { return false; }
            return column == ListView.LastSortColumn && ListView.LastSortOrder != SortOrder.None;
        }

        protected bool HasFilterIndicator(OlvColumn column)
        {
            return (ListView.UseFiltering && ListView.UseFilterIndicator && column.HasFilterIndicator);
        }

        protected bool HasNonThemedSortIndicator(OlvColumn column)
        {
            if (!ListView.ShowSortIndicators) { return false; }
            return VisualStyleRenderer.IsSupported
                       ? !VisualStyleRenderer.IsElementDefined(VisualStyleElement.Header.SortArrow.SortedUp) &&
                         HasSortIndicator(column)
                       : HasSortIndicator(column);
        }

        public Rectangle GetItemRect(int itemIndex)
        {
            const int hdmFirst = 0x1200;
            const int hdmGetitemrect = hdmFirst + 7;
            var r = new NativeMethods.Rect();
            NativeMethods.SendMessageRECT(Handle, hdmGetitemrect, itemIndex, ref r);
            return Rectangle.FromLTRB(r.left, r.top, r.right, r.bottom);
        }

        public void Invalidate()
        {
            NativeMethods.InvalidateRect(Handle, 0, true);
        }

        /* Tooltip */
        protected virtual void CreateToolTip()
        {
            ToolTip = new ToolTipControl();
            ToolTip.Create(Handle);
            ToolTip.AddTool(this);
            ToolTip.Showing += ListView.HeaderToolTipShowingCallback;
        }

        /* Windows messaging */
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            const int wmDestroy = 2;
            const int wmSetcursor = 0x20;
            const int wmNotify = 0x4E;
            const int wmMousemove = 0x200;
            const int hdmFirst = 0x1200;
            const int hdmLayout = (hdmFirst + 5);
            switch (m.Msg)
            {
                case wmSetcursor:
                    if (!HandleSetCursor(ref m))
                    {
                        return;
                    }
                    break;

                case wmNotify:
                    if (!HandleNotify(ref m))
                    {
                        return;
                    }
                    break;

                case wmMousemove:
                    if (!HandleMouseMove(ref m))
                    {
                        return;
                    }
                    break;

                case hdmLayout:
                    if (!HandleLayout(ref m))
                    {
                        return;
                    }
                    break;

                case wmDestroy:
                    if (!HandleDestroy(ref m))
                    {
                        return;
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        protected bool HandleSetCursor(ref Message m)
        {
            if (IsCursorOverLockedDivider)
            {
                m.Result = (IntPtr)1; /* Don't change the cursor */
                return false;
            }
            return true;
        }

        protected bool HandleMouseMove(ref Message m)
        {
            var columnIndex = ColumnIndexUnderCursor;
            /* If the mouse has moved to a different header, pop the current tip (if any)
             * For some reason, references ToolTip when in design mode, causes the 
             * columns to not be resizable by dragging the divider in the Designer. No idea why. */
            if (columnIndex != _columnShowingTip && !ListView.IsDesignMode)
            {
                ToolTip.PopToolTip(this);
                _columnShowingTip = columnIndex;
            }
            return true;
        }

        protected bool HandleNotify(ref Message m)
        {
            /* Can this ever happen? JPP 2009-05-22 */
            if (m.LParam == IntPtr.Zero)
            {
                return false;
            }
            var nmhdr = (NativeMethods.Nmhdr)m.GetLParam(typeof (NativeMethods.Nmhdr));
            switch (nmhdr.code)
            {
                case ToolTipControl.TtnShow:
                    return ToolTip.HandleShow(ref m);

                case ToolTipControl.TtnPop:
                    return ToolTip.HandlePop(ref m);

                case ToolTipControl.TtnGetdispinfo:
                    return ToolTip.HandleGetDispInfo(ref m);
            }
            return false;
        }

        internal virtual bool HandleHeaderCustomDraw(ref Message m)
        {
            const int cdrfNewfont = 2;
            const int cdrfSkipdefault = 4;
            const int cdrfNotifypostpaint = 0x10;
            const int cdrfNotifyitemdraw = 0x20;

            const int cddsPrepaint = 1;
            const int cddsPostpaint = 2;
            const int cddsItem = 0x00010000;
            const int cddsItemprepaint = (cddsItem | cddsPrepaint);
            const int cddsItempostpaint = (cddsItem | cddsPostpaint);

            var nmcustomdraw = (NativeMethods.Nmcustomdraw)m.GetLParam(typeof (NativeMethods.Nmcustomdraw));            
            switch (nmcustomdraw.dwDrawStage)
            {
                case cddsPrepaint:
                    _cachedNeedsCustomDraw = NeedsCustomDraw();
                    m.Result = (IntPtr)cdrfNotifyitemdraw;
                    return true;

                case cddsItemprepaint:
                    var columnIndex = nmcustomdraw.dwItemSpec.ToInt32();
                    var column = ListView.GetColumn(columnIndex);
                    if (_cachedNeedsCustomDraw)
                    {
                        using (var g = Graphics.FromHdc(nmcustomdraw.hdc))
                        {
                            g.TextRenderingHint = ObjectListView.TextRenderingHint;
                            CustomDrawHeaderCell(g, columnIndex, nmcustomdraw.uItemState);
                        }
                        m.Result = (IntPtr)cdrfSkipdefault;
                    }
                    else
                    {
                        const int cdisSelected = 1;
                        var isPressed = ((nmcustomdraw.uItemState & cdisSelected) == cdisSelected);
                        var f = CalculateFont(column, columnIndex == ColumnIndexUnderCursor, isPressed);
                        _fontHandle = f.ToHfont();
                        NativeMethods.SelectObject(nmcustomdraw.hdc, _fontHandle);
                        m.Result = (IntPtr)(cdrfNewfont | cdrfNotifypostpaint);
                    }
                    return true;

                case cddsItempostpaint:
                    if (_fontHandle != IntPtr.Zero)
                    {
                        NativeMethods.DeleteObject(_fontHandle);
                        _fontHandle = IntPtr.Zero;
                    }
                    break;
            }
            return false;
        }

        protected bool HandleLayout(ref Message m)
        {
            if (ListView.HeaderStyle == ColumnHeaderStyle.None)
            {
                return true;
            }
            var hdlayout = (NativeMethods.HdLayout)m.GetLParam(typeof (NativeMethods.HdLayout));
            var rect = (NativeMethods.Rect)Marshal.PtrToStructure(hdlayout.prc, typeof (NativeMethods.Rect));
            var wpos = (NativeMethods.Windowpos)Marshal.PtrToStructure(hdlayout.pwpos, typeof (NativeMethods.Windowpos));
            using (var g = ListView.CreateGraphics())
            {
                g.TextRenderingHint = ObjectListView.TextRenderingHint;
                var height = CalculateHeight(g);
                wpos.hwnd = Handle;
                wpos.hwndInsertAfter = IntPtr.Zero;
                wpos.flags = NativeMethods.SwpFramechanged;
                wpos.x = rect.left;
                wpos.y = rect.top;
                wpos.cx = rect.right - rect.left;
                wpos.cy = height;
                rect.top = height;
                Marshal.StructureToPtr(rect, hdlayout.prc, false);
                Marshal.StructureToPtr(wpos, hdlayout.pwpos, false);
            }

            ListView.BeginInvoke((MethodInvoker)delegate
                                                         {
                                                             Invalidate();
                                                             ListView.Invalidate();
                                                         });
            return false;
        }

        protected bool HandleDestroy(ref Message m)
        {
            if (ToolTip != null)
            {
                ToolTip.Showing -= ListView.HeaderToolTipShowingCallback;
            }
            return false;
        }

        /* Rendering */
        protected bool NeedsCustomDraw()
        {
            if (WordWrap)
            {
                return true;
            }
            return !ListView.HeaderUsesThemes &&
                   (NeedsCustomDraw(ListView.HeaderFormatStyle) ||
                    ListView.Columns.Cast<OlvColumn>().Any(
                        column =>
                        column.HasHeaderImage || !column.ShowTextInHeader || column.IsHeaderVertical ||
                        HasFilterIndicator(column) || column.TextAlign != column.HeaderTextAlign ||
                        NeedsCustomDraw(column.HeaderFormatStyle)));
        }

        private bool NeedsCustomDraw(HeaderFormatStyle style)
        {
            if (style == null)
            {
                return false;
            }
            return (NeedsCustomDraw(style.Normal) ||
                    NeedsCustomDraw(style.Hot) ||
                    NeedsCustomDraw(style.Pressed));
        }

        private bool NeedsCustomDraw(HeaderStateStyle style)
        {
            if (style == null)
            {
                return false;
            }
            /* If we want fancy colors or frames, we have to custom draw. Oddly enough, we 
             * can handle font changes without custom drawing. */
            if (!style.BackColor.IsEmpty)
            {
                return true;
            }
            return style.FrameWidth > 0f && !style.FrameColor.IsEmpty ||
                   !style.ForeColor.IsEmpty && style.ForeColor != Color.Black;
        }

        protected void CustomDrawHeaderCell(Graphics g, int columnIndex, int itemState)
        {
            var r = GetItemRect(columnIndex);
            var column = ListView.GetColumn(columnIndex);
            const int cdisSelected = 1;
            var isPressed = ((itemState & cdisSelected) == cdisSelected);
            /* Calculate which style should be used for the header */
            var stateStyle = CalculateStyle(column, columnIndex == ColumnIndexUnderCursor, isPressed);
            /* If there is an owner drawn delegate installed, give it a chance to draw the header */
            if (column.HeaderDrawing != null)
            {
                if (!column.HeaderDrawing(g, r, columnIndex, column, isPressed, stateStyle))
                {
                    return;
                }
            }
            /* Draw the background */
            if (ListView.HeaderUsesThemes &&
                VisualStyleRenderer.IsSupported &&
                VisualStyleRenderer.IsElementDefined(VisualStyleElement.Header.Item.Normal))
            {
                DrawThemedBackground(g, r, columnIndex, isPressed);
            }
            else
            {
                DrawUnthemedBackground(g, r, columnIndex, isPressed, stateStyle);
            }
            /* Draw the sort indicator if this column has one */
            if (HasSortIndicator(column))
            {
                if (ListView.HeaderUsesThemes &&
                    VisualStyleRenderer.IsSupported &&
                    VisualStyleRenderer.IsElementDefined(VisualStyleElement.Header.SortArrow.SortedUp))
                {
                    DrawThemedSortIndicator(g, r);
                }
                else
                {
                    r = DrawUnthemedSortIndicator(g, r);
                }
            }
            if (HasFilterIndicator(column))
            {
                r = DrawFilterIndicator(g, r);
            }
            /* Finally draw the text */
            DrawHeaderImageAndText(g, r, column, stateStyle);
        }

        protected void DrawUnthemedBackground(Graphics g, Rectangle r, int columnIndex, bool isSelected, HeaderStateStyle stateStyle)
        {
            if (stateStyle.BackColor.IsEmpty)
            {
                /* I know we're supposed to be drawing the unthemed background, but let's just see if we
                 * can draw something more interesting than the dull raised block */
                if (VisualStyleRenderer.IsSupported &&
                    VisualStyleRenderer.IsElementDefined(VisualStyleElement.Header.Item.Normal))
                {
                    DrawThemedBackground(g, r, columnIndex, isSelected);
                }
                else
                {
                    ControlPaint.DrawBorder3D(g, r, Border3DStyle.RaisedInner);
                }
            }
            else
            {
                using (Brush b = new SolidBrush(stateStyle.BackColor))
                {
                    g.FillRectangle(b, r);
                }
            }
            /* Draw the frame if the style asks for one */
            if (stateStyle.FrameColor.IsEmpty || !(stateStyle.FrameWidth > 0f)) { return; }
            RectangleF r2 = r;
            r2.Inflate(-stateStyle.FrameWidth, -stateStyle.FrameWidth);
            g.DrawRectangle(new Pen(stateStyle.FrameColor, stateStyle.FrameWidth), r2.X, r2.Y, r2.Width, r2.Height);
        }

        protected void DrawThemedBackground(Graphics g, Rectangle r, int columnIndex, bool isSelected)
        {
            var part = 1; /* normal item */
            if (columnIndex == 0 && VisualStyleRenderer.IsElementDefined(VisualStyleElement.Header.ItemLeft.Normal))
            {
                part = 2; /* left item */
            }
            if (columnIndex == ListView.Columns.Count - 1 && VisualStyleRenderer.IsElementDefined(VisualStyleElement.Header.ItemRight.Normal))
            {
                part = 3; /* right item */
            }
            var state = 1; /* normal state */
            if (isSelected)
            {
                state = 3; /* pressed */
            }
            else if (columnIndex == ColumnIndexUnderCursor)
            {
                state = 2; /* hot */
            }
            var renderer = new VisualStyleRenderer("HEADER", part, state);
            renderer.DrawBackground(g, r);
        }

        protected void DrawThemedSortIndicator(Graphics g, Rectangle r)
        {
            VisualStyleRenderer renderer2 = null;
            if (ListView.LastSortOrder == SortOrder.Ascending)
            {
                renderer2 = new VisualStyleRenderer(VisualStyleElement.Header.SortArrow.SortedUp);
            }
            if (ListView.LastSortOrder == SortOrder.Descending)
            {
                renderer2 = new VisualStyleRenderer(VisualStyleElement.Header.SortArrow.SortedDown);
            }
            if (renderer2 == null) { return; }
            var sz = renderer2.GetPartSize(g, ThemeSizeType.True);
            var pt = renderer2.GetPoint(PointProperty.Offset);
            /* GetPoint() should work, but if it doesn't, put the arrow in the top middle */
            if (pt.X == 0 && pt.Y == 0)
            {
                pt = new Point(r.X + (r.Width/2) - (sz.Width/2), r.Y);
            }
            renderer2.DrawBackground(g, new Rectangle(pt, sz));
        }

        protected Rectangle DrawUnthemedSortIndicator(Graphics g, Rectangle r)
        {
            /* No theme support for sort indicators. So, we draw a triangle at the right edge
             * of the column header. */
            const int triangleHeight = 16;
            const int triangleWidth = 16;
            const int midX = triangleWidth/2;
            const int midY = (triangleHeight/2) - 1;
            const int deltaX = midX - 2;
            const int deltaY = deltaX/2;

            var triangleLocation = new Point(r.Right - triangleWidth - 2, r.Top + (r.Height - triangleHeight)/2);
            var pts = new[] {triangleLocation, triangleLocation, triangleLocation};

            if (ListView.LastSortOrder == SortOrder.Ascending)
            {
                pts[0].Offset(midX - deltaX, midY + deltaY);
                pts[1].Offset(midX, midY - deltaY - 1);
                pts[2].Offset(midX + deltaX, midY + deltaY);
            }
            else
            {
                pts[0].Offset(midX - deltaX, midY - deltaY);
                pts[1].Offset(midX, midY + deltaY);
                pts[2].Offset(midX + deltaX, midY - deltaY);
            }

            g.FillPolygon(Brushes.SlateGray, pts);
            r.Width = r.Width - triangleWidth;
            return r;
        }

        protected Rectangle DrawFilterIndicator(Graphics g, Rectangle r)
        {
            var width = CalculateFilterIndicatorWidth(r);
            if (width <= 0)
            {
                return r;
            }
            Image indicator = Resources.ColumnFilterIndicator;
            var x = r.Right - width;
            var y = r.Top + (r.Height - indicator.Height)/2;
            g.DrawImageUnscaled(indicator, x, y);
            r.Width -= width;
            return r;
        }

        private static int CalculateFilterIndicatorWidth(Rectangle r)
        {
            return Resources.ColumnFilterIndicator == null || r.Width < 48
                       ? 0
                       : Resources.ColumnFilterIndicator.Width + 1;
        }

        protected void DrawHeaderImageAndText(Graphics g, Rectangle r, OlvColumn column, HeaderStateStyle stateStyle)
        {
            var flags = TextFormatFlags;
            flags |= TextFormatFlags.VerticalCenter;
            if (column.HeaderTextAlign == HorizontalAlignment.Center)
            {
                flags |= TextFormatFlags.HorizontalCenter;
            }
            if (column.HeaderTextAlign == HorizontalAlignment.Right)
            {
                flags |= TextFormatFlags.Right;
            }
            var f = ListView.HeaderUsesThemes ? ListView.Font : stateStyle.Font ?? ListView.Font;
            var color = ListView.HeaderUsesThemes ? Color.Black : stateStyle.ForeColor;
            if (color.IsEmpty)
            {
                color = Color.Black;
            }
            /* Tweak the text rectangle a little to improve aethestics */
            r.Inflate(-3, 0);
            r.Y -= 2;
            const int imageTextGap = 3;
            if (column.IsHeaderVertical)
            {
                DrawVerticalText(g, r, column, f, color);
            }
            else
            {
                /* Does the column have a header image and is there space for it? */
                if (column.HasHeaderImage && r.Width > column.ImageList.ImageSize.Width*2)
                {
                    DrawImageAndText(g, r, column, flags, f, color, imageTextGap);
                }
                else
                {
                    DrawText(g, r, column, flags, f, color);
                }
            }
        }

        private static void DrawText(IDeviceContext g, Rectangle r, OlvColumn column, TextFormatFlags flags, Font f, Color color)
        {
            if (column.ShowTextInHeader)
            {
                TextRenderer.DrawText(g, column.Text, f, r, color, Color.Transparent, flags);
            }
        }

        private static void DrawImageAndText(Graphics g, Rectangle r, OlvColumn column, TextFormatFlags flags, Font f, Color color, int imageTextGap)
        {
            var textRect = r;
            textRect.X += (column.ImageList.ImageSize.Width + imageTextGap);
            textRect.Width -= (column.ImageList.ImageSize.Width + imageTextGap);

            var textSize = Size.Empty;
            if (column.ShowTextInHeader)
            {
                textSize = TextRenderer.MeasureText(g, column.Text, f, textRect.Size, flags);
            }
            var imageY = r.Top + ((r.Height - column.ImageList.ImageSize.Height)/2);
            var imageX = textRect.Left;
            if (column.HeaderTextAlign == HorizontalAlignment.Center)
            {
                imageX = textRect.Left + ((textRect.Width - textSize.Width)/2);
            }
            if (column.HeaderTextAlign == HorizontalAlignment.Right)
            {
                imageX = textRect.Right - textSize.Width;
            }
            imageX -= (column.ImageList.ImageSize.Width + imageTextGap);

            column.ImageList.Draw(g, imageX, imageY, column.ImageList.Images.IndexOfKey(column.HeaderImageKey));

            DrawText(g, textRect, column, flags, f, color);
        }

        private static void DrawVerticalText(Graphics g, Rectangle r, OlvColumn column, Font f, Color color)
        {
            try
            {
                /* Create a matrix transformation that will rotate the text 90 degrees vertically
                 * AND place the text in the middle of where it was previously. [Think of tipping
                 * a box over by its bottom left edge -- you have to move it back a bit so it's
                 * in the same place as it started] */
                var m = new Matrix();
                m.RotateAt(-90, new Point(r.X, r.Bottom));
                m.Translate(0, r.Height);
                g.Transform = m;
                var fmt = new StringFormat(StringFormatFlags.NoWrap)
                              {
                                  Alignment = StringAlignment.Near,
                                  LineAlignment = column.HeaderTextAlignAsStringAlignment
                              };
                /* The drawing is rotated 90 degrees, so switch our text boundaries */
                var textRect = r;
                textRect.Width = r.Height;
                textRect.Height = r.Width;
                using (Brush b = new SolidBrush(color))
                {
                    g.DrawString(column.Text, f, b, textRect, fmt);
                }
            }
            finally
            {
                g.ResetTransform();
            }
        }

        protected HeaderStateStyle CalculateStyle(OlvColumn column, bool isHot, bool isPressed)
        {
            var headerStyle = column.HeaderFormatStyle ?? ListView.HeaderFormatStyle ?? new HeaderFormatStyle();
            if (ListView.IsDesignMode)
            {
                return headerStyle.Normal;
            }
            if (isPressed)
            {
                return headerStyle.Pressed;
            }
            return isHot ? headerStyle.Hot : headerStyle.Normal;
        }

        protected Font CalculateFont(OlvColumn column, bool isHot, bool isPressed)
        {
            var stateStyle = CalculateStyle(column, isHot, isPressed);
            return stateStyle.Font ?? ListView.Font;
        }

        protected TextFormatFlags TextFormatFlags
        {
            get
            {
                var flags = TextFormatFlags.EndEllipsis |
                            TextFormatFlags.NoPrefix |
                            TextFormatFlags.WordEllipsis |
                            TextFormatFlags.PreserveGraphicsTranslateTransform;
                if (WordWrap)
                {
                    flags |= TextFormatFlags.WordBreak;
                }
                else
                {
                    flags |= TextFormatFlags.SingleLine;
                }
                if (ListView.RightToLeft == RightToLeft.Yes)
                {
                    flags |= TextFormatFlags.RightToLeft;
                }
                return flags;
            }
        }
    }
}
