/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ircCore.Controls.ChildWindows.Input;
using ircCore.Controls.ChildWindows.Nicklist;
using ircCore.Controls.ChildWindows.OutputDisplay;

namespace FusionIRC.Forms
{
    /* This class is our "main" chat window for console, channel, query and DCC chats - one class for all */
    public enum ChildWindowType
    {
        Console = 0,
        Channel = 1,
        Private = 2,
        DccChat = 3
    }

    public class FrmChildWindow : Form
    {        
        /* Win32 API */
        [DllImport("user32.dll", EntryPoint = "SendMessageA", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetTopWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        protected const int UmsgDonothide = 0x404;
        protected const int UmsgOkhide = 0x405;
        protected const int ScMinimize = 0xf020;
        protected const int WmSyscommand = 0x112;
        protected const int WmMdirestore = 0x223;
        protected const int WmMdinext = 0x224;
        protected const int WmNcactivate = 0x86;

        /* Public properties */
        public bool NoDeactivate { get; set; }

        public ChildWindowType WindowType { get; private set; }
        public OutputWindow Output { get; set; }
        public InputWindow Input { get; set; }
        public Nicklist Nicklist { get; set; }

        /* Constructor */
        public FrmChildWindow(ChildWindowType type)
        {
            /* Constructor where we pass what type of window this is - then we know what controls to create ;) */
            WindowType = type;
            Output = new OutputWindow(); //make sure to set proper properties and things here
            Input = new InputWindow(); //make sure to set proper properties and things here
        }

        /* Public activation mehtods */
        public void ShowWithoutActive()
        {
            var topWindow = GetTopWindow(Parent.Handle);
            SendMessage(topWindow, UmsgDonothide, IntPtr.Zero, IntPtr.Zero);
            Show();
            SendMessage(topWindow, UmsgOkhide, IntPtr.Zero, IntPtr.Zero);
            var f = (FrmChildWindow)FromHandle(topWindow);
            if (f == null) { return; }
            //if (IrcMain.Settings.WinMaximized)
            //{
            //    f.WindowState = FormWindowState.Maximized;
            //}
            f.BringToFront();
            f.MyActivate();
        }

        public void MyActivate()
        {
            /* This method raises the original forms Activated event if you
               want other controls to be focused when the form regains focus */
            OnActivated(new EventArgs());
        }

        public void Restore()
        {
            SendMessage(Parent.Handle, WmMdirestore, Handle, IntPtr.Zero);
        }

        /* Overrides */
        protected override void OnResize(EventArgs e)
        {
            /* Adjust our controls based on window type */
            base.OnResize(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Output.Dispose();
                Input.Dispose();
                if (Nicklist != null)
                {
                    Nicklist.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        protected override void WndProc(ref Message m)
        {
            if (NoDeactivate && m.Msg == WmNcactivate && m.WParam == IntPtr.Zero)
            {
                m.WParam = (IntPtr)1;
                BringWindowToTop(m.HWnd);
                base.WndProc(ref m);
                m.Result = IntPtr.Zero;
            }
            else switch (m.Msg)
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
