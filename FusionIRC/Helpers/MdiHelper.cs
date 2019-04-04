/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using FusionIRC.Forms.Child;

namespace FusionIRC.Helpers
{
    public sealed class MdiHelper : NativeWindow, IComponent
    {
        /* MDI Helper class
           By: Jason James Newland
           ©2009 - 2011 - KangaSoft Software
           All Rights Reserved
         
           Removes scrollbars from MDI client
           Enables double-buffering for flicker-free graphic rendering
         */
        private const int SbBoth = 3;
        private const int WmMdinext = 0x224;
        private const int Wfnccalcsize = 131;

        private readonly Form _parentForm;
        private readonly MdiClient _mdiClient;

        [DllImport("user32.dll")]
        private static extern int ShowScrollBar(IntPtr hWnd, int wBar, int bShow);

        [DllImport("user32.dll", EntryPoint = "SendMessageA", CharSet = CharSet.Auto)]
        private static extern int SendMessage(int hWnd, int wMsg, int wParam, ref int lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessageA", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        public event EventHandler Disposed;

        /* Constructor */
        public MdiHelper(Form theParentForm)
        {
            Site = null;
            _parentForm = theParentForm;
            _mdiClient = null;
            if (!_parentForm.IsMdiContainer)
            {
                _parentForm.IsMdiContainer = true;
            }
            foreach (Control a in _parentForm.Controls)
            {
                _mdiClient = a as MdiClient;
                if (_mdiClient == null)
                {
                    continue;
                }
                ReleaseHandle();
                AssignHandle(_mdiClient.Handle);
                break;
            }
            /* Setup double-buffering */
            const ControlStyles styles = ControlStyles.OptimizedDoubleBuffer;
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            if (_mdiClient == null)
            {
                return;
            }
            var method = _mdiClient.GetType().GetMethod("SetStyle", flags);
            object[] param = { styles, true };
            /* Invoke double buffering */
            method.Invoke(_mdiClient, param);
        }

        public MdiClient MdiClientWnd
        {
            get { return _mdiClient; }
        }

        public ISite Site { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Wfnccalcsize:
                    ShowScrollBar(m.HWnd, SbBoth, 0);
                    break;
            }
            base.WndProc(ref m);
        }

        /* Child activation methods */
        public void ActivateChild(FrmChildWindow childForm)
        {
            var iRef = 0;
            FrmChildWindow frmTemp;
            if (_mdiClient == null) { return; }
            var iPos = _mdiClient.Controls.IndexOf(childForm);
            var iLastIdX = _mdiClient.Controls.Count - 1;
            if (iPos == iLastIdX)
            {
                frmTemp = (FrmChildWindow) _mdiClient.Controls[0];
            }
            else
            {
                frmTemp = (FrmChildWindow)_mdiClient.Controls[iPos + 1];
            }
            SendMessage(_mdiClient.Handle.ToInt32(), WmMdinext, frmTemp.Handle.ToInt32(), ref iRef);
        }

        public void ActivateNextMdiChild()
        {
            if (_mdiClient != null)
            {
                SendMessage(_mdiClient.Handle, WmMdinext, IntPtr.Zero, IntPtr.Zero);
            }
        }
        
        /* Private methods */
        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }
            lock (this)
            {
                if (Site != null && Site.Container != null) { Site.Container.Remove(this); }
                if (Disposed != null) { Disposed(this, EventArgs.Empty); }
            }
        }
    }
}
