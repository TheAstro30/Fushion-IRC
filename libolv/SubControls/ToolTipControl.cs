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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Security.Permissions;
using libolv.Implementation;
using libolv.Implementation.Events;

namespace libolv.SubControls
{
    public class ToolTipControl : NativeWindow
    {
        public enum StandardIcons
        {
            None = 0,
            Info = 1,
            Warning = 2,
            Error = 3,
            InfoLarge = 4,
            WarningLarge = 5,
            ErrorLarge = 6
        }

        private const int GwlStyle = -16;
        private const int WmGetfont = 0x31;
        private const int WmSetfont = 0x30;
        private const int WsBorder = 0x800000;
        private const int WsExTopmost = 8;

        private const int TtmAddtool = 0x432;
        private const int TtmDeltool = 0x433;
        private const int TtmGettipbkcolor = 0x400 + 22;
        private const int TtmGettiptextcolor = 0x400 + 23;
        private const int TtmGetdelaytime = 0x400 + 21;
        private const int TtmPop = 0x41c;
        private const int TtmSetdelaytime = 0x400 + 3;
        private const int TtmSetmaxtipwidth = 0x400 + 24;
        private const int TtmSettipbkcolor = 0x400 + 19;
        private const int TtmSettiptextcolor = 0x400 + 20;
        private const int TtmSettitle = 0x400 + 33;

        private const int TtfIdishwnd = 1;
        private const int TtfRtlreading = 4;
        private const int TtfSubclass = 0x10;

        private const int TtsNoprefix = 2;
        private const int TtsBalloon = 0x40;
        private const int TtsUsevisualstyle = 0x100;

        private const int TtnFirst = -520;

        public const int TtnShow = (TtnFirst - 1);
        public const int TtnPop = (TtnFirst - 2);
        public const int TtnLinkclick = (TtnFirst - 3);
        public const int TtnGetdispinfo = (TtnFirst - 10);

        private const int TtdtReshow = 1;
        private const int TtdtAutopop = 2;
        private const int TtdtInitial = 3;

        private bool _hasBorder = true;
        private string _title;
        private StandardIcons _standardIcon;
        private Font _font;
        private Hashtable _settings;

        /* Events */
        public event EventHandler<ToolTipShowingEventArgs> Showing;
        public event EventHandler<EventArgs> Pop;

        /* Properties */
        internal int WindowStyle
        {
            get { return NativeMethods.GetWindowLong(Handle, GwlStyle); }
            set { NativeMethods.SetWindowLong(Handle, GwlStyle, value); }
        }

        public bool IsBalloon
        {
            get { return (WindowStyle & TtsBalloon) == TtsBalloon; }
            set
            {
                if (IsBalloon == value)
                {
                    return;
                }
                var windowStyle = WindowStyle;
                if (value)
                {
                    windowStyle |= (TtsBalloon | TtsUsevisualstyle);
                    /* On XP, a border makes the ballon look wrong */
                    if (!ObjectListView.IsVistaOrLater)
                    {
                        windowStyle &= ~WsBorder;
                    }
                }
                else
                {
                    windowStyle &= ~(TtsBalloon | TtsUsevisualstyle);
                    if (!ObjectListView.IsVistaOrLater)
                    {
                        if (_hasBorder)
                        {
                            windowStyle |= WsBorder;
                        }
                        else
                        {
                            windowStyle &= ~WsBorder;
                        }
                    }
                }
                WindowStyle = windowStyle;
            }
        }

        public bool HasBorder
        {
            get { return _hasBorder; }
            set
            {
                if (_hasBorder == value)
                {
                    return;
                }
                _hasBorder = value;
                if (_hasBorder)
                {
                    WindowStyle |= WsBorder;
                    
                }
                else
                {
                    WindowStyle &= ~WsBorder;
                }
            }
        }

        public Color BackColor
        {
            get
            {
                var color = (int)NativeMethods.SendMessage(Handle, TtmGettipbkcolor, 0, 0);
                return ColorTranslator.FromWin32(color);
            }
            set
            {
                /* For some reason, setting the color fails on Vista and messes up later ops.
                 * So we don't even try to set it. */
                if (ObjectListView.IsVistaOrLater) { return; }
                var color = ColorTranslator.ToWin32(value);
                NativeMethods.SendMessage(Handle, TtmSettipbkcolor, color, 0);
            }
        }

        public Color ForeColor
        {
            get
            {
                var color = (int)NativeMethods.SendMessage(Handle, TtmGettiptextcolor, 0, 0);
                return ColorTranslator.FromWin32(color);
            }
            set
            {
                /* For some reason, setting the color fails on Vista and messes up later ops.
                 * So we don't even try to set it. */
                if (ObjectListView.IsVistaOrLater) { return; }
                var color = ColorTranslator.ToWin32(value);
                NativeMethods.SendMessage(Handle, TtmSettiptextcolor, color, 0);
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _title = string.Empty;
                }
                else if (value.Length >= 100)
                {
                    _title = value.Substring(0, 99);
                }
                else
                {
                    _title = value;
                }
                NativeMethods.SendMessageString(Handle, TtmSettitle, (int)_standardIcon, _title);
            }
        }

        public StandardIcons StandardIcon
        {
            get { return _standardIcon; }
            set
            {
                _standardIcon = value;
                NativeMethods.SendMessageString(Handle, TtmSettitle, (int)_standardIcon, _title);
            }
        }

        public Font Font
        {
            get
            {
                var hfont = NativeMethods.SendMessage(Handle, WmGetfont, 0, 0);
                return hfont == IntPtr.Zero ? Control.DefaultFont : Font.FromHfont(hfont);
            }
            set
            {
                var newFont = value ?? Control.DefaultFont;
                if (Equals(newFont, _font))
                {
                    return;
                }
                _font = newFont;
                var hfont = _font.ToHfont(); /* THINK: When should we delete this hfont? */
                NativeMethods.SendMessage(Handle, WmSetfont, hfont, 0);
            }
        }

        public int AutoPopDelay
        {
            get { return GetDelayTime(TtdtAutopop); }
            set { SetDelayTime(TtdtAutopop, value); }
        }

        public int InitialDelay
        {
            get { return GetDelayTime(TtdtInitial); }
            set { SetDelayTime(TtdtInitial, value); }
        }

        public int ReshowDelay
        {
            get { return GetDelayTime(TtdtReshow); }
            set { SetDelayTime(TtdtReshow, value); }
        }

        private int GetDelayTime(int which)
        {
            return (int)NativeMethods.SendMessage(Handle, TtmGetdelaytime, which, 0);
        }

        private void SetDelayTime(int which, int value)
        {
            NativeMethods.SendMessage(Handle, TtmSetdelaytime, which, value);
        }

        /* Commands */
        public void Create(IntPtr parentHandle)
        {
            if (Handle != IntPtr.Zero)
            {
                return;
            }
            var cp = new CreateParams
                         {
                             ClassName = "tooltips_class32",
                             Style = TtsNoprefix,
                             ExStyle = WsExTopmost,
                             Parent = parentHandle
                         };
            CreateHandle(cp);
            /* Ensure that multiline tooltips work correctly */
            SetMaxWidth();
        }

        public void PushSettings()
        {
            /* Ignore any nested calls */
            if (_settings != null)
            {
                return;
            }
            _settings = new Hashtable();
            _settings["IsBalloon"] = IsBalloon;
            _settings["HasBorder"] = HasBorder;
            _settings["BackColor"] = BackColor;
            _settings["ForeColor"] = ForeColor;
            _settings["Title"] = Title;
            _settings["StandardIcon"] = StandardIcon;
            _settings["AutoPopDelay"] = AutoPopDelay;
            _settings["InitialDelay"] = InitialDelay;
            _settings["ReshowDelay"] = ReshowDelay;
            _settings["Font"] = Font;
        }

        public void PopSettings()
        {
            if (_settings == null)
            {
                return;
            }
            IsBalloon = (bool)_settings["IsBalloon"];
            HasBorder = (bool)_settings["HasBorder"];
            BackColor = (Color)_settings["BackColor"];
            ForeColor = (Color)_settings["ForeColor"];
            Title = (string)_settings["Title"];
            StandardIcon = (StandardIcons)_settings["StandardIcon"];
            AutoPopDelay = (int)_settings["AutoPopDelay"];
            InitialDelay = (int)_settings["InitialDelay"];
            ReshowDelay = (int)_settings["ReshowDelay"];
            Font = (Font)_settings["Font"];
            _settings = null;
        }

        public void AddTool(IWin32Window window)
        {
            var lParam = MakeToolInfoStruct(window);
            NativeMethods.SendMessageTOOLINFO(Handle, TtmAddtool, 0, lParam);
        }

        public void PopToolTip(IWin32Window window)
        {
            NativeMethods.SendMessage(Handle, TtmPop, 0, 0);
        }

        public void RemoveToolTip(IWin32Window window)
        {
            var lParam = MakeToolInfoStruct(window);
            NativeMethods.SendMessageTOOLINFO(Handle, TtmDeltool, 0, lParam);
        }

        public void SetMaxWidth()
        {
            SetMaxWidth(SystemInformation.MaxWindowTrackSize.Width);
        }

        public void SetMaxWidth(int maxWidth)
        {
            NativeMethods.SendMessage(Handle, TtmSetmaxtipwidth, 0, maxWidth);
        }

        /* Implementation */
        private static NativeMethods.Toolinfo MakeToolInfoStruct(IWin32Window window)
        {
            var toolinfoTooltip = new NativeMethods.Toolinfo
                                      {
                                          hwnd = window.Handle,
                                          uFlags = TtfIdishwnd | TtfSubclass,
                                          uId = window.Handle,
                                          lpszText = (IntPtr)(-1)
                                      };
            return toolinfoTooltip;
        }

        protected virtual bool HandleNotify(ref Message msg)
        {
            /* THINK: What do we have to do here? Nothing it seems :) */
            return false;
        }

        public virtual bool HandleGetDispInfo(ref Message msg)
        {
            SetMaxWidth();
            var args = new ToolTipShowingEventArgs
                           {
                               ToolTipControl = this
                           };
            OnShowing(args);
            if (string.IsNullOrEmpty(args.Text))
            {
                return false;
            }
            ApplyEventFormatting(args);
            var dispInfo = (NativeMethods.Nmttdispinfo)msg.GetLParam(typeof (NativeMethods.Nmttdispinfo));
            dispInfo.lpszText = args.Text;
            dispInfo.hinst = IntPtr.Zero;
            if (args.RightToLeft == RightToLeft.Yes)
            {
                dispInfo.uFlags |= TtfRtlreading;
            }
            Marshal.StructureToPtr(dispInfo, msg.LParam, false);
            return true;
        }

        private void ApplyEventFormatting(ToolTipShowingEventArgs args)
        {
            if (!args.IsBalloon.HasValue &&
                !args.BackColor.HasValue &&
                !args.ForeColor.HasValue &&
                args.Title == null &&
                !args.StandardIcon.HasValue &&
                !args.AutoPopDelay.HasValue &&
                args.Font == null)
                return;

            PushSettings();
            if (args.IsBalloon.HasValue)
            {
                IsBalloon = args.IsBalloon.Value;
            }
            if (args.BackColor.HasValue)
            {
                BackColor = args.BackColor.Value;
            }
            if (args.ForeColor.HasValue)
            {
                ForeColor = args.ForeColor.Value;
            }
            if (args.StandardIcon.HasValue)
            {
                StandardIcon = args.StandardIcon.Value;
            }
            if (args.AutoPopDelay.HasValue)
            {
                AutoPopDelay = args.AutoPopDelay.Value;
            }
            if (args.Font != null)
            {
                Font = args.Font;
            }
            if (args.Title != null)
            {
                Title = args.Title;
            }
        }

        public virtual bool HandleLinkClick(ref Message msg)
        {
            return false;
        }

        public virtual bool HandlePop(ref Message msg)
        {
            PopSettings();
            return true;
        }

        public virtual bool HandleShow(ref Message msg)
        {
            return false;
        }

        protected virtual bool HandleReflectNotify(ref Message msg)
        {
            var nmheader = (NativeMethods.Nmheader)msg.GetLParam(typeof (NativeMethods.Nmheader));
            switch (nmheader.nhdr.code)
            {
                case TtnShow:
                    if (HandleShow(ref msg))
                    {
                        return true;
                    }
                    break;

                case TtnPop:
                    if (HandlePop(ref msg))
                    {
                        return true;
                    }
                    break;

                case TtnLinkclick:
                    if (HandleLinkClick(ref msg))
                    {
                        return true;
                    }
                    break;

                case TtnGetdispinfo:
                    if (HandleGetDispInfo(ref msg))
                    {
                        return true;
                    }
                    break;
            }

            return false;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message msg)
        {
            switch (msg.Msg)
            {
                case 0x4E: /* WM_NOTIFY */
                    if (!HandleNotify(ref msg))
                    {
                        return;
                    }
                    break;

                case 0x204E: /* WM_REFLECT_NOTIFY */
                    if (!HandleReflectNotify(ref msg))
                    {
                        return;
                    }
                    break;
            }
            base.WndProc(ref msg);
        }

        /* Events */
        protected virtual void OnShowing(ToolTipShowingEventArgs e)
        {
            if (Showing != null)
            {
                Showing(this, e);
            }
        }

        protected virtual void OnPop(EventArgs e)
        {
            if (Pop != null)
            {
                Pop(this, e);
            }
        }

    }
}