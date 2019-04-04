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

        private const int UmsgDonothide = 0x404;
        private const int UmsgOkhide = 0x405;
        private const int ScMinimize = 0xf020;
        private const int WmSyscommand = 0x112;
        private const int WmMdirestore = 0x223;
        private const int WmMdinext = 0x224;
        private const int WmNcactivate = 0x86;

        /* Public properties */
        public bool NoDeactivate { get; set; }

        /* Public activation mehtods */
        public void ShowWithoutActive()
        {
            var topWindow = GetTopWindow(Parent.Handle);
            SendMessage(topWindow, UmsgDonothide, IntPtr.Zero, IntPtr.Zero);
            Show();
            SendMessage(topWindow, UmsgOkhide, IntPtr.Zero, IntPtr.Zero);
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
            SendMessage(Parent.Handle, WmMdirestore, Handle, IntPtr.Zero);
        }

        /* Window procedure */
        protected override void WndProc(ref Message m)
        {
            if (NoDeactivate && m.Msg == WmNcactivate && m.WParam == IntPtr.Zero)
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
                    case UmsgOkhide:
                        NoDeactivate = false;
                        break;

                    case UmsgDonothide:
                        NoDeactivate = true;
                        break;

                    default:
                        if (m.Msg == WmSyscommand && m.WParam.ToInt32() == ScMinimize)
                        {
                            /* Detects if a form is minimized and stops the MDI client from restoring down all child windows */
                            SendMessage(Parent.Handle, WmMdinext, IntPtr.Zero, IntPtr.Zero);
                        }
                        base.WndProc(ref m);
                        break;
                }
            }
        }  
    }
}
