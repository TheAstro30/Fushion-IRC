/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ircCore.Controls.Notification;
using ircCore.Properties;

namespace ircCore.Controls
{
    public class TrayIcon : Form
    {
        /* Notification icon control/class (inherit on main form)
           By: Jason James Newland
           ©2012/2019 - KangaSoft Software
           All Rights Reserved
         */
        internal struct FlashWInfo
        {
            public UInt32 Size;
            public IntPtr Hwnd;
            public FlashWindowExOption Flags;
            public UInt32 Count;
            public UInt32 Timeout;
        }

        [Flags]
        internal enum FlashWindowExOption : uint
        {  
            FlashwStop = 0,
            FlashwCaption = 1,
            FlashwTray = 2,
            FlashwAll = 3,
            FlashwTimer = 4,
            FlashwTimerNoFg = 12
        }

        private Image _renderBmp;

        private int _positionY;

        private bool _trayAlwaysShowIcon;
        private FormWindowState _lastWindowState = FormWindowState.Normal;

        private readonly List<PopupNotifier> _popups = new List<PopupNotifier>();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FlashWInfo pwfi);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();

        /* Public properties */
        public bool Inactive { get; set; }

        public NotifyIcon TrayNotifyIcon { get; set; }

        public bool TrayHideOnMinimize { get; set; }

        public bool TrayShowNotifications { get; set; }

        public bool TrayAlwaysShowIcon
        {
            get { return _trayAlwaysShowIcon; }
            set
            {
                _trayAlwaysShowIcon = value;
                TrayNotifyIcon.Visible = _trayAlwaysShowIcon;
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            if (GetForegroundWindow() != Handle || Inactive)
            {
                return;
            }
            Inactive = false;
            base.OnActivated(e);
        }

        protected override void OnDeactivate(EventArgs e)
        {
            Inactive = true;
            base.OnDeactivate(e);
        }

        public override Image BackgroundImage
        {
            get
            {
                return _renderBmp;
            }
            set
            {
                /* This significantly speeds up the drawing speed of large (resolution) images - answer:
                 * https://blogs.msdn.microsoft.com/mhendersblog/2005/10/12/painting-performance-and-form-background-images/
                 * the idea is to resize the image to a smaller resolution (usually just the primary screen resolution) */
                if (value == null)
                {
                    _renderBmp = null;
                    return;
                }
                var baseImage = value;
                _renderBmp = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);
                using (var g = Graphics.FromImage(_renderBmp))
                {
                    g.InterpolationMode = InterpolationMode.High;
                    g.DrawImage(baseImage, 0, 0, Width, Height);
                }
            }
        }

        /* Constructor */
        public TrayIcon()
        {
            TrayNotifyIcon = new NotifyIcon();
            TrayNotifyIcon.MouseDoubleClick += OnTrayDoubleClick;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            TrayNotifyIcon.Visible = false;
            TrayNotifyIcon.Dispose();
            base.OnFormClosed(e);
        }

        protected override void OnResize(EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized && TrayHideOnMinimize)
            {
                if (!_trayAlwaysShowIcon)
                {
                    TrayNotifyIcon.Visible = true;
                }
                Hide();
                base.OnResize(e);
                return;
            }
            if (!_trayAlwaysShowIcon && TrayNotifyIcon.Visible)
            {
                TrayNotifyIcon.Visible = false;
            }
            if (WindowState != FormWindowState.Minimized)
            {
                _lastWindowState = WindowState;
            }
            base.OnResize(e);
        }

        /* Public methods */
        public void FlashWindow()
        {
            if (!Inactive || !Visible)
            {
                return;
            }
            var fInfo = new FlashWInfo
                            {
                                Hwnd = Handle,
                                Flags = FlashWindowExOption.FlashwAll | FlashWindowExOption.FlashwTimerNoFg,
                                Count = UInt32.MaxValue,
                                Timeout = 0
                            };
            fInfo.Size = Convert.ToUInt32(Marshal.SizeOf(fInfo));
            FlashWindowEx(ref fInfo);
        }

        public void ShowNotificationPopup(string title, string text, int height)
        {
            if (!TrayShowNotifications || !TrayNotifyIcon.Visible)
            {
                return;
            }
            var p = new PopupNotifier
                        {
                            TitleFont = new Font("Segeo UI Semibold", 10),
                            ContentFont = new Font("Segeo UI", 9),
                            Size = new Size(230, height),
                            Image = Resources.fusion.ToBitmap(),
                            TitleText = title,
                            ContentText = text,
                            AnimationDuration = 350,
                            Delay = 5000
                        };
            if (_popups.Count > 0)
            {
                _positionY += p.Size.Height;
            }
            p.PositionY = _positionY;
            _popups.Add(p);
            p.Disappear += PopupDisappear;
            p.Popup();
        }

        /* Callbacks */
        private void PopupDisappear(object sender, EventArgs e)
        {
            var p = (PopupNotifier)sender;
            if (p == null)
            {
                return;
            }
            _popups.Remove(p);
            p.Dispose();
            if (_popups.Count > 0)
            {
                _positionY -= p.Size.Height;
            }
        }

        private void OnTrayDoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = _lastWindowState;
            Activate();
        }   
    }
}
