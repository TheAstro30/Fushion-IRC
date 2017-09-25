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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using libolv.SubControls;

namespace libolv.Implementation
{
    internal static class NativeMethods
    {
        /* Constants */
        private const int LvmFirst = 0x1000;
        private const int LvmGetcountperpage = LvmFirst + 40;
        private const int LvmGetgroupinfo = LvmFirst + 149;
        private const int LvmGetgroupstate = LvmFirst + 92;
        private const int LvmGetheader = LvmFirst + 31;
        private const int LvmGettooltips = LvmFirst + 78;
        private const int LvmGettopindex = LvmFirst + 39;
        private const int LvmHittest = LvmFirst + 18;
        private const int LvmInsertgroup = LvmFirst + 145;
        private const int LvmRemoveallgroups = LvmFirst + 160;
        private const int LvmScroll = LvmFirst + 20;
        private const int LvmSetbkimage = LvmFirst + 0x8A;
        private const int LvmSetextendedlistviewstyle = LvmFirst + 54;
        private const int LvmSetgroupinfo = LvmFirst + 147;
        private const int LvmSetgroupmetrics = LvmFirst + 155;
        private const int LvmSetimagelist = LvmFirst + 3;
        private const int LvmSetitem = LvmFirst + 76;
        private const int LvmSetitemcount = LvmFirst + 47;
        private const int LvmSetitemstate = LvmFirst + 43;
        private const int LvmSetselectedcolumn = LvmFirst + 140;
        private const int LvmSettooltips = LvmFirst + 74;
        private const int LvmSubitemhittest = LvmFirst + 57;
        private const int LvsExSubitemimages = 0x0002;

        private const int LvifImage = 0x0002;
        private const int LvbkifSourceHbitmap = 0x1;
        private const int LvbkifStyleTile = 0x10;
        private const int LvbkifTypeWatermark = 0x10000000;

        private const int LvsicfNoscroll = 2;

        private const int HdmFirst = 0x1200;
        private const int HdmHittest = HdmFirst + 6;
        private const int HdmGetitemrect = HdmFirst + 7;
        private const int HdmGetitem = HdmFirst + 11;
        private const int HdmSetitem = HdmFirst + 12;

        private const int HdiFormat = 0x0004;
        private const int HdiImage = 0x0020;

        private const int HdfBitmapOnRight = 0x1000;
        private const int HdfImage = 0x0800;
        private const int HdfSortup = 0x0400;
        private const int HdfSortdown = 0x0200;

        private const int SbHorz = 0;
        private const int SbVert = 1;

        private const int SifPos = 0x0004;

        private const int IldTransparent = 0x1;
        private const int IldBlend25 = 0x2;

        private const int SwpNosize = 1;
        private const int SwpNomove = 2;
        private const int SwpNozorder = 4;
        private const int SwpNoredraw = 8;
        private const int SwpNoactivate = 16;
        public const int SwpFramechanged = 32;

        private const int SwpZorderonly = SwpNosize | SwpNomove | SwpNoredraw | SwpNoactivate;
        private const int SwpSizeonly = SwpNomove | SwpNoredraw | SwpNozorder | SwpNoactivate;

        /* Structures */
        [StructLayout(LayoutKind.Sequential)]
        public struct HdItem
        {
            public int mask;
            public int cxy;
            public IntPtr pszText;
            public IntPtr hbm;
            public int cchTextMax;
            public int fmt;
            public IntPtr lParam;
            public int iImage;
            public int iOrder;
            public int type;
            public IntPtr pvFilter;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class HdHitTestInfo
        {
            public int pt_x;
            public int pt_y;
            public int flags;
            public int iItem;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class HdLayout
        {
            public IntPtr prc;
            public IntPtr pwpos;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct Lvbkimage
        {
            public int ulFlags;
            public IntPtr hBmp;
            [MarshalAs(UnmanagedType.LPTStr)] public string pszImage;
            public int cchImageMax;
            public int xOffset;
            public int yOffset;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct Lvcolumn
        {
            public int mask;
            public int fmt;
            public int cx;
            [MarshalAs(UnmanagedType.LPTStr)] public string pszText;
            public int cchTextMax;
            public int iSubItem;
            /* These are available in Common Controls >= 0x0300 */
            public int iImage;
            public int iOrder;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct Lvfindinfo
        {
            public int flags;
            public string psz;
            public IntPtr lParam;
            public int ptX;
            public int ptY;
            public int vkDirection;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Lvgroup
        {
            public uint cbSize;
            public uint mask;

            [MarshalAs(UnmanagedType.LPTStr)] public string pszHeader;
            public int cchHeader;

            [MarshalAs(UnmanagedType.LPTStr)] public string pszFooter;
            public int cchFooter;

            public int iGroupId;
            public uint stateMask;
            public uint state;
            public uint uAlign;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Lvgroup2
        {
            public uint cbSize;
            public uint mask;

            [MarshalAs(UnmanagedType.LPTStr)] public string pszHeader;
            public uint cchHeader;

            [MarshalAs(UnmanagedType.LPTStr)] public string pszFooter;
            public int cchFooter;

            public int iGroupId;
            public uint stateMask;
            public uint state;
            public uint uAlign;

            [MarshalAs(UnmanagedType.LPTStr)] public string pszSubtitle;
            public uint cchSubtitle;

            [MarshalAs(UnmanagedType.LPTStr)] public string pszTask;
            public uint cchTask;

            [MarshalAs(UnmanagedType.LPTStr)] public string pszDescriptionTop;
            public uint cchDescriptionTop;

            [MarshalAs(UnmanagedType.LPTStr)] public string pszDescriptionBottom;
            public uint cchDescriptionBottom;

            public int iTitleImage;
            public int iExtendedImage;
            public int iFirstItem; /* Read only */
            public int cItems; /* Read only */

            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszSubsetTitle; /* NULL if group is not subset */

            public uint cchSubsetTitle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Lvgroupmetrics
        {
            public uint cbSize;
            public uint mask;
            public uint Left;
            public uint Top;
            public uint Right;
            public uint Bottom;
            public int crLeft;
            public int crTop;
            public int crRight;
            public int crBottom;
            public int crHeader;
            public int crFooter;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct Lvhittestinfo
        {
            public int pt_x;
            public int pt_y;
            public int flags;
            public int iItem;
            public int iSubItem;
            public int iGroup;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct Lvitem
        {
            public int mask;
            public int iItem;
            public int iSubItem;
            public int state;
            public int stateMask;

            [MarshalAs(UnmanagedType.LPTStr)] public string pszText;
            public int cchTextMax;

            public int iImage;
            public IntPtr lParam;
            /* These are available in Common Controls >= 0x0300 */
            public int iIndent;
            /* These are available in Common Controls >= 0x056 */
            public int iGroupId;
            public int cColumns;
            public IntPtr puColumns;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Nmhdr
        {
            public IntPtr hwndFrom;
            public IntPtr idFrom;
            public int code;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Nmcustomdraw
        {
            public Nmhdr nmcd;
            public int dwDrawStage;
            public IntPtr hdc;
            public Rect rc;
            public IntPtr dwItemSpec;
            public int uItemState;
            public IntPtr lItemlParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Nmheader
        {
            public Nmhdr nhdr;
            public int iItem;
            public int iButton;
            public IntPtr pHDITEM;
        }

        private const int MaxLinkidText = 48;
        private const int LMaxUrlLength = 2048 + 32 + 4;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct Litem
        {
            public uint mask;
            public int iLink;
            public uint state;
            public uint stateMask;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxLinkidText)]
            public string szID;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LMaxUrlLength)]
            public string szUrl;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Nmlistview
        {
            public Nmhdr hdr;
            public int iItem;
            public int iSubItem;
            public int uNewState;
            public int uOldState;
            public int uChanged;
            public IntPtr lParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Nmlvcustomdraw
        {
            public Nmcustomdraw nmcd;
            public int clrText;
            public int clrTextBk;
            public int iSubItem;
            public int dwItemType;
            public int clrFace;
            public int iIconEffect;
            public int iIconPhase;
            public int iPartId;
            public int iStateId;
            public Rect rcText;
            public uint uAlign;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Nmlvfinditem
        {
            public Nmhdr hdr;
            public int iStart;
            public Lvfindinfo lvfi;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Nmlvgetinfotip
        {
            public Nmhdr hdr;
            public int dwFlags;
            public string pszText;
            public int cchTextMax;
            public int iItem;
            public int iSubItem;
            public IntPtr lParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Nmlvgroup
        {
            public Nmhdr hdr;
            public int iGroupId; /* which group is changing */
            public uint uNewState; /* LVGS_xxx flags */
            public uint uOldState;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Nmlvlink
        {
            public Nmhdr hdr;
            public Litem link;
            public int iItem;
            public int iSubItem;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Nmlvscroll
        {
            public Nmhdr hdr;
            public int dx;
            public int dy;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct Nmttdispinfo
        {
            public Nmhdr hdr;

            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpszText;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szText;

            public IntPtr hinst;
            public int uFlags;
            public IntPtr lParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class Scrollinfo
        {
            public int cbSize = Marshal.SizeOf(typeof (Scrollinfo));
            public int fMask;
            public int nMin;
            public int nMax;
            public int nPage;
            public int nPos;
            public int nTrackPos;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class Toolinfo
        {
            public int cbSize = Marshal.SizeOf(typeof (Toolinfo));
            public int uFlags;
            public IntPtr hwnd;
            public IntPtr uId;
            public Rect rect;
            public IntPtr hinst = IntPtr.Zero;
            public IntPtr lpszText;
            public IntPtr lParam = IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Windowpos
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Lvitemindex
        {
            public int iItem;
            public int iGroup;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct InteropPoint
        {
            public int x;
            public int y;
        }

        /* Entry points */
        /* Various flavours of SendMessage: plain vanilla, and passing references to various structures */
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageLVItem(IntPtr hWnd, int msg, int wParam, ref Lvitem lvi);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref Lvhittestinfo ht);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageRECT(IntPtr hWnd, int msg, int wParam, ref Rect r);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessageHDItem(IntPtr hWnd, int msg, int wParam, ref HdItem hdi);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageHDHITTESTINFO(IntPtr hWnd, int msg, IntPtr wParam, [In, Out] HdHitTestInfo lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageTOOLINFO(IntPtr hWnd, int msg, int wParam, Toolinfo lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageLVBKIMAGE(IntPtr hWnd, int msg, int wParam, ref Lvbkimage lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageString(IntPtr hWnd, int msg, int wParam, string lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageIUnknown(IntPtr hWnd, int msg,
                                                        [MarshalAs(UnmanagedType.IUnknown)] object wParam, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref Lvgroup lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref Lvgroup2 lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref Lvgroupmetrics lParam);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr objectHandle);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool GetClientRect(IntPtr hWnd, ref Rectangle r);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool GetScrollInfo(IntPtr hWnd, int fnBar, Scrollinfo scrollInfo);

        [DllImport("user32.dll", EntryPoint = "GetUpdateRect", CharSet = CharSet.Auto)]
        private static extern int GetUpdateRectInternal(IntPtr hWnd, ref Rectangle r, bool eraseBackground);

        [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
        private static extern bool ImageList_Draw(IntPtr himl, int i, IntPtr hdcDst, int x, int y, int fStyle);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
        public static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto)]
        public static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", EntryPoint = "ValidateRect", CharSet = CharSet.Auto)]
        private static extern IntPtr ValidatedRectInternal(IntPtr hWnd, ref Rectangle r);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int SetBkColor(IntPtr hDc, int clr);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int SetTextColor(IntPtr hDc, int crColor);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr obj);

        [DllImport("uxtheme.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr SetWindowTheme(IntPtr hWnd, string subApp, string subIdList);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool InvalidateRect(IntPtr hWnd, int ignored, bool erase);

        /* Implementation */
        public static bool SetBackgroundImage(ListView lv, Image image, bool isWatermark, bool isTiled, int xOffset, int yOffset)
        {
            /* We have to clear any pre-existing background image, otherwise the attempt to set the image will fail.
             * We don't know which type may already have been set, so we just clear both the watermark and the image. */
            var lvbkimage = new Lvbkimage
                                {
                                    ulFlags = LvbkifTypeWatermark
                                };
            lvbkimage.ulFlags = LvbkifSourceHbitmap;
            var result = SendMessageLVBKIMAGE(lv.Handle, LvmSetbkimage, 0, ref lvbkimage);
            var bm = image as Bitmap;
            if (bm != null)
            {
                lvbkimage.hBmp = bm.GetHbitmap();
                lvbkimage.ulFlags = isWatermark
                                        ? LvbkifTypeWatermark
                                        : (isTiled ? LvbkifSourceHbitmap | LvbkifStyleTile : LvbkifSourceHbitmap);
                lvbkimage.xOffset = xOffset;
                lvbkimage.yOffset = yOffset;
                result = SendMessageLVBKIMAGE(lv.Handle, LvmSetbkimage, 0, ref lvbkimage);
            }
            return (result != IntPtr.Zero);
        }

        public static bool DrawImageList(Graphics g, ImageList il, int index, int x, int y, bool isSelected)
        {
            var flags = IldTransparent;
            if (isSelected)
            {
                flags |= IldBlend25;
            }
            var result = ImageList_Draw(il.Handle, index, g.GetHdc(), x, y, flags);
            g.ReleaseHdc();
            return result;
        }

        public static void ForceSubItemImagesExStyle(ListView list)
        {
            SendMessage(list.Handle, LvmSetextendedlistviewstyle, LvsExSubitemimages, LvsExSubitemimages);
        }

        public static void SetItemCount(ListView list, int count)
        {
            SendMessage(list.Handle, LvmSetitemcount, count, LvsicfNoscroll);
        }

        public static void SetExtendedStyle(ListView list, int style, int styleMask)
        {
            SendMessage(list.Handle, LvmSetextendedlistviewstyle, style, styleMask);
        }

        public static int GetCountPerPage(ListView list)
        {
            return (int)SendMessage(list.Handle, LvmGetcountperpage, 0, 0);
        }

        public static void SetSubItemImage(ListView list, int itemIndex, int subItemIndex, int imageIndex)
        {
            var lvItem = new Lvitem
                             {
                                 mask = LvifImage, 
                                 iItem = itemIndex, 
                                 iSubItem = subItemIndex,
                                 iImage = imageIndex
                             };
            SendMessageLVItem(list.Handle, LvmSetitem, 0, ref lvItem);
        }

        public static void SetColumnImage(ListView list, int columnIndex, SortOrder order, int imageIndex)
        {
            var hdrCntl = GetHeaderControl(list);
            if (hdrCntl.ToInt32() == 0)
            {
                return;
            }
            var item = new HdItem
                           {
                               mask = HdiFormat
                           };
            SendMessageHDItem(hdrCntl, HdmGetitem, columnIndex, ref item);
            item.fmt &= ~(HdfSortup | HdfSortdown | HdfImage | HdfBitmapOnRight);
            if (HasBuiltinSortIndicators())
            {
                if (order == SortOrder.Ascending)
                {
                    item.fmt |= HdfSortup;
                }
                if (order == SortOrder.Descending)
                {
                    item.fmt |= HdfSortdown;
                }
            }
            else
            {
                item.mask |= HdiImage;
                item.fmt |= (HdfImage | HdfBitmapOnRight);
                item.iImage = imageIndex;
            }
            SendMessageHDItem(hdrCntl, HdmSetitem, columnIndex, ref item);            
        }

        public static bool HasBuiltinSortIndicators()
        {
            return OSFeature.Feature.GetVersionPresent(OSFeature.Themes) != null;
        }

        /// <returns>A rectangle</returns>
        public static Rectangle GetUpdateRect(Control cntl)
        {
            var r = new Rectangle();
            GetUpdateRectInternal(cntl.Handle, ref r, false);
            return r;
        }

        public static void ValidateRect(Control cntl, Rectangle r)
        {
            ValidatedRectInternal(cntl.Handle, ref r);
        }

        public static void SelectAllItems(ListView list)
        {
            SetItemState(list, -1, 2, 2);
        }

        public static void DeselectAllItems(ListView list)
        {
            SetItemState(list, -1, 2, 0);
        }

        public static void SetItemState(ListView list, int itemIndex, int mask, int value)
        {
            var lvItem = new Lvitem
                             {
                                 stateMask = mask,
                                 state = value
                             };
            SendMessageLVItem(list.Handle, LvmSetitemstate, itemIndex, ref lvItem);
        }

        public static bool Scroll(ListView list, int dx, int dy)
        {
            return SendMessage(list.Handle, LvmScroll, dx, dy) != IntPtr.Zero;
        }

        public static IntPtr GetHeaderControl(ListView list)
        {
            return SendMessage(list.Handle, LvmGetheader, 0, 0);
        }

        public static Point GetColumnSides(ObjectListView lv, int columnIndex)
        {
            var hdr = GetHeaderControl(lv);
            if (hdr == IntPtr.Zero)
            {
                return new Point(-1, -1);
            }
            var r = new Rect();
            SendMessageRECT(hdr, HdmGetitemrect, columnIndex, ref r);
            return new Point(r.left, r.right);
        }

        public static Point GetScrolledColumnSides(ListView lv, int columnIndex)
        {
            var hdr = GetHeaderControl(lv);
            if (hdr == IntPtr.Zero)
            {
                return new Point(-1, -1);
            }
            var r = new Rect();
            SendMessageRECT(hdr, HdmGetitemrect, columnIndex, ref r);
            var scrollH = GetScrollPosition(lv, true);
            return new Point(r.left - scrollH, r.right - scrollH);
        }

        public static int GetColumnUnderPoint(IntPtr handle, Point pt)
        {
            const int hhtOnheader = 2;
            const int hhtOndivider = 4;
            return HeaderControlHitTest(handle, pt, hhtOnheader | hhtOndivider);
        }

        private static int HeaderControlHitTest(IntPtr handle, Point pt, int flag)
        {
            var testInfo = new HdHitTestInfo
                               {
                                   pt_x = pt.X,
                                   pt_y = pt.Y
                               };
            SendMessageHDHITTESTINFO(handle, HdmHittest, IntPtr.Zero, testInfo);
            return (testInfo.flags & flag) != 0 ? testInfo.iItem : -1;
        }

        public static int GetDividerUnderPoint(IntPtr handle, Point pt)
        {
            const int hhtOndivider = 4;
            return HeaderControlHitTest(handle, pt, hhtOndivider);
        }

        public static int GetScrollPosition(ListView lv, bool horizontalBar)
        {
            var fnBar = (horizontalBar ? SbHorz : SbVert);
            var scrollInfo = new Scrollinfo
                                 {
                                     fMask = SifPos
                                 };
            return GetScrollInfo(lv.Handle, fnBar, scrollInfo) ? scrollInfo.nPos : -1;
        }

        public static bool ChangeZOrder(IWin32Window toBeMoved, IWin32Window reference)
        {
            return SetWindowPos(toBeMoved.Handle, reference.Handle, 0, 0, 0, 0, SwpZorderonly);
        }

        public static bool MakeTopMost(IWin32Window toBeMoved)
        {
            var hwndTopmost = (IntPtr)(-1);
            return SetWindowPos(toBeMoved.Handle, hwndTopmost, 0, 0, 0, 0, SwpZorderonly);
        }

        public static bool ChangeSize(IWin32Window toBeMoved, int width, int height)
        {
            return SetWindowPos(toBeMoved.Handle, IntPtr.Zero, 0, 0, width, height, SwpSizeonly);
        }

        public static void ShowWithoutActivate(IWin32Window win)
        {
            const int swShowna = 8;
            ShowWindow(win.Handle, swShowna);
        }

        public static void SetSelectedColumn(ListView objectListView, ColumnHeader value)
        {
            SendMessage(objectListView.Handle, LvmSetselectedcolumn, (value == null) ? -1 : value.Index, 0);
        }

        public static int GetTopIndex(ListView lv)
        {
            return (int)SendMessage(lv.Handle, LvmGettopindex, 0, 0);
        }

        public static IntPtr GetTooltipControl(ListView lv)
        {
            return SendMessage(lv.Handle, LvmGettooltips, 0, 0);
        }

        public static IntPtr SetTooltipControl(ListView lv, ToolTipControl tooltip)
        {
            return SendMessage(lv.Handle, LvmSettooltips, 0, tooltip.Handle);
        }

        public static bool HasHorizontalScrollBar(ListView lv)
        {
            const int gwlStyle = -16;
            const int wsHscroll = 0x00100000;
            return (GetWindowLong(lv.Handle, gwlStyle) & wsHscroll) != 0;
        }

        public static int GetWindowLong(IntPtr hWnd, int nIndex)
        {
            return IntPtr.Size == 4 ? (int)GetWindowLong32(hWnd, nIndex) : (int)(long)GetWindowLongPtr64(hWnd, nIndex);
        }

        public static int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong)
        {
            return IntPtr.Size == 4
                       ? (int)SetWindowLongPtr32(hWnd, nIndex, dwNewLong)
                       : (int)(long)SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        }

        public static int GetGroupInfo(ObjectListView olv, int groupId, ref Lvgroup2 group)
        {
            return (int)SendMessage(olv.Handle, LvmGetgroupinfo, groupId, ref group);
        }

        public static GroupState GetGroupState(ObjectListView olv, int groupId, GroupState mask)
        {
            return (GroupState)SendMessage(olv.Handle, LvmGetgroupstate, groupId, (int)mask);
        }

        public static int InsertGroup(ObjectListView olv, Lvgroup2 group)
        {
            return (int)SendMessage(olv.Handle, LvmInsertgroup, -1, ref group);
        }

        public static int SetGroupInfo(ObjectListView olv, int groupId, Lvgroup2 group)
        {
            return (int)SendMessage(olv.Handle, LvmSetgroupinfo, groupId, ref group);
        }

        public static int SetGroupMetrics(ObjectListView olv, Lvgroupmetrics metrics)
        {
            return (int)SendMessage(olv.Handle, LvmSetgroupmetrics, 0, ref metrics);
        }

        public static int ClearGroups(VirtualObjectListView virtualObjectListView)
        {
            return (int)SendMessage(virtualObjectListView.Handle, LvmRemoveallgroups, 0, 0);
        }

        public static int SetGroupImageList(ObjectListView olv, ImageList il)
        {
            const int lvsilGroupheader = 3;
            var handle = IntPtr.Zero;
            if (il != null)
            {
                handle = il.Handle;
            }
            return (int)SendMessage(olv.Handle, LvmSetimagelist, lvsilGroupheader, handle);
        }

        public static int HitTest(ObjectListView olv, ref Lvhittestinfo hittest)
        {
            return (int)SendMessage(olv.Handle, olv.View == View.Details ? LvmSubitemhittest : LvmHittest, -1, ref hittest);
        }
    }
}
