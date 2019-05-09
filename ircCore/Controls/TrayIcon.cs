/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace ircCore.Controls
{
    public class TrayIcon : Form
    {
        /* Notification icon control/class (inherit on main form)
           By: Jason James Newland
           ©2012 - KangaSoft Software
           All Rights Reserved
         */
        private Image _renderBmp;

        private bool _trayAlwaysShowIcon;
        private FormWindowState _lastWindowState = FormWindowState.Normal;

        public NotifyIcon TrayNotifyIcon { get; set; }

        public bool TrayHideOnMinimize { get; set; }

        public bool TrayAlwaysShowIcon
        {
            get { return _trayAlwaysShowIcon; }
            set
            {
                _trayAlwaysShowIcon = value;
                TrayNotifyIcon.Visible = _trayAlwaysShowIcon;
            }
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

        private void OnTrayDoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = _lastWindowState;
            Activate();
        }
    }
}
