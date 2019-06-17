/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ircCore.Settings;

namespace ircCore.Controls
{
    public class ChildWindow : Form
    {
        /* Win32 API */
        [DllImport("user32.dll", EntryPoint = "SendMessageA", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetTopWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        private const int UmsgDoNotHide = 0x404;
        private const int UmsgOkHide = 0x405;
        private const int ScMinimize = 0xf020;
        private const int WmSysCommand = 0x112;
        private const int WmMdiRestore = 0x223;
        private const int WmMdiNext = 0x224;
        private const int WmNcActivate = 0x86;

        /* Public properties */
        public bool NoDeactivate { get; set; }

        public bool NoActivate { get; set; }

        public bool Maximized { get; set; }

        /* Public activation mehtods */
        public void ShowWithoutActive()
        {
            NoActivate = true;
            var topWindow = GetTopWindow(Parent.Handle);
            SendMessage(topWindow, UmsgDoNotHide, IntPtr.Zero, IntPtr.Zero);
            Show();
            SendMessage(topWindow, UmsgOkHide, IntPtr.Zero, IntPtr.Zero);
            var f = (ChildWindow)FromHandle(topWindow);
            if (f == null)
            {                
                return;
            }
            if (SettingsManager.Settings.Windows.ChildrenMaximized)
            {
                f.WindowState = FormWindowState.Maximized;
            }
            f.BringToFront();
            f.MyActivate();
        }

        public void MyActivate()
        {
            /* This method raises the original forms Activated event if you
               want other controls to be focused when the form regains focus */
            OnActivated(new EventArgs());
        }

        public override void Refresh()
        {
            OnResize(new EventArgs());
        }

        public void Restore()
        {
            SendMessage(Parent.Handle, WmMdiRestore, Handle, IntPtr.Zero);
        }

        /* Window procedure */
        protected override void WndProc(ref Message m)
        {
            if (NoDeactivate && m.Msg == WmNcActivate && m.WParam == IntPtr.Zero)
            {
                m.WParam = (IntPtr)1;
                BringWindowToTop(m.HWnd);
                base.WndProc(ref m);
                m.Result = IntPtr.Zero;
            }
            else
            {
                switch (m.Msg)
                {
                    case UmsgOkHide:
                        NoDeactivate = false;
                        break;

                    case UmsgDoNotHide:
                        NoDeactivate = true;
                        break;

                    default:
                        if (m.Msg == WmSysCommand && m.WParam.ToInt32() == ScMinimize)
                        {
                            /* Detects if a form is minimized and stops the MDI client from restoring down all child windows */
                            SendMessage(Parent.Handle, WmMdiNext, IntPtr.Zero, IntPtr.Zero);
                        }
                        base.WndProc(ref m);
                        break;
                }
            }
        }  
    }
}
