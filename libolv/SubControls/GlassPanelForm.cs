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
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using libolv.Implementation;
using libolv.Rendering.Overlays;

namespace libolv.SubControls
{
    internal sealed class GlassPanelForm : Form
    {
        internal IOverlay Overlay;

        private ObjectListView _objectListView;
        private bool _isDuringResizeSequence;
        private bool _isGlassShown;
        private bool _wasGlassShownBeforeResize;

        /* Cache these so we can unsubscribe from events even when the OLV has been disposed. */
        private Form _myOwner;
        private Form _mdiOwner;
        private List<Control> _ancestors;
        private MdiClient _mdiClient;

        public GlassPanelForm()
        {
            Name = "GlassPanelForm";
            Text = "GlassPanelForm";
            ClientSize = new Size(0, 0);
            ControlBox = false;
            FormBorderStyle = FormBorderStyle.None;
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.Manual;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.None;

            SetStyle(ControlStyles.Selectable, false);

            Opacity = 0.5f;
            BackColor = Color.FromArgb(255, 254, 254, 254);
            TransparencyKey = BackColor;
            HideGlass();
            NativeMethods.ShowWithoutActivate(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Unbind();
            }
            base.Dispose(disposing);
        }

        /* Properties */
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x20; /* WS_EX_TRANSPARENT */
                cp.ExStyle |= 0x80; /* WS_EX_TOOLWINDOW */
                return cp;
            }
        }

        /* Commands */     
        public void Bind(ObjectListView olv, IOverlay overlay)
        {
            if (_objectListView != null)
            {
                Unbind();
            }
            _objectListView = olv;
            Overlay = overlay;
            _mdiClient = null;
            _mdiOwner = null;
            if (_objectListView == null)
            {
                return;
            }
            /* NOTE: If you listen to any events here, you *must* stop listening in Unbind() */
            _objectListView.Disposed += ObjectListViewDisposed;
            _objectListView.LocationChanged += ObjectListViewLocationChanged;
            _objectListView.SizeChanged += ObjectListViewSizeChanged;
            _objectListView.VisibleChanged += ObjectListViewVisibleChanged;
            _objectListView.ParentChanged += ObjectListViewParentChanged;
            /* Collect our ancestors in the widget hierachy */
            if (_ancestors == null)
            {
                _ancestors = new List<Control>();
            }
            var parent = _objectListView.Parent;
            while (parent != null)
            {
                _ancestors.Add(parent);
                parent = parent.Parent;
            }
            /* Listen for changes in the hierachy */
            foreach (var ancestor in _ancestors)
            {
                ancestor.ParentChanged += ObjectListViewParentChanged;
                var tabControl = ancestor as TabControl;
                if (tabControl != null)
                {
                    tabControl.Selected += TabControlSelected;
                }
            }
            /* Listen for changes in our owning form */
            Owner = _objectListView.FindForm();
            _myOwner = Owner;
            if (Owner != null)
            {
                Owner.LocationChanged += OwnerLocationChanged;
                Owner.SizeChanged += OwnerSizeChanged;
                Owner.ResizeBegin += OwnerResizeBegin;
                Owner.ResizeEnd += OwnerResizeEnd;
                if (Owner.TopMost)
                {
                    /* We can't do TopMost = true; since that will activate the panel,
                     * taking focus away from the owner of the listview */
                    NativeMethods.MakeTopMost(this);
                }
                /* We need special code to handle MDI */
                _mdiOwner = Owner.MdiParent;
                if (_mdiOwner != null)
                {
                    _mdiOwner.LocationChanged += OwnerLocationChanged;
                    _mdiOwner.SizeChanged += OwnerSizeChanged;
                    _mdiOwner.ResizeBegin += OwnerResizeBegin;
                    _mdiOwner.ResizeEnd += OwnerResizeEnd;
                    /* Find the MDIClient control, which houses all MDI children */
                    foreach (Control c in _mdiOwner.Controls)
                    {
                        _mdiClient = c as MdiClient;
                        if (_mdiClient != null)
                        {
                            break;
                        }
                    }
                    if (_mdiClient != null)
                    {
                        _mdiClient.ClientSizeChanged += MyMdiClientClientSizeChanged;
                    }
                }
            }
            UpdateTransparency();
        }

        private void MyMdiClientClientSizeChanged(object sender, EventArgs e)
        {
            RecalculateBounds();
            Invalidate();
        }

        public void HideGlass()
        {
            if (!_isGlassShown)
            {
                return;
            }
            _isGlassShown = false;
            Bounds = new Rectangle(-10000, -10000, 1, 1);
        }

        public void ShowGlass()
        {
            if (_isGlassShown || _isDuringResizeSequence)
            {
                return;
            }
            _isGlassShown = true;
            RecalculateBounds();
        }

        public void Unbind()
        {
            if (_objectListView != null)
            {
                _objectListView.Disposed -= ObjectListViewDisposed;
                _objectListView.LocationChanged -= ObjectListViewLocationChanged;
                _objectListView.SizeChanged -= ObjectListViewSizeChanged;
                _objectListView.VisibleChanged -= ObjectListViewVisibleChanged;
                _objectListView.ParentChanged -= ObjectListViewParentChanged;
                _objectListView = null;
            }
            if (_ancestors != null)
            {
                foreach (var parent in _ancestors)
                {
                    parent.ParentChanged -= ObjectListViewParentChanged;
                    var tabControl = parent as TabControl;
                    if (tabControl != null)
                    {
                        tabControl.Selected -= TabControlSelected;
                    }
                }
                _ancestors = null;
            }
            if (_myOwner != null)
            {
                _myOwner.LocationChanged -= OwnerLocationChanged;
                _myOwner.SizeChanged -= OwnerSizeChanged;
                _myOwner.ResizeBegin -= OwnerResizeBegin;
                _myOwner.ResizeEnd -= OwnerResizeEnd;
                _myOwner = null;
            }
            if (_mdiOwner != null)
            {
                _mdiOwner.LocationChanged -= OwnerLocationChanged;
                _mdiOwner.SizeChanged -= OwnerSizeChanged;
                _mdiOwner.ResizeBegin -= OwnerResizeBegin;
                _mdiOwner.ResizeEnd -= OwnerResizeEnd;
                _mdiOwner = null;
            }
            if (_mdiClient == null) { return; }
            _mdiClient.ClientSizeChanged -= MyMdiClientClientSizeChanged;
            _mdiClient = null;
        }

        /* Event Handlers */

        private void ObjectListViewDisposed(object sender, EventArgs e)
        {
            Unbind();
        }

        private void OwnerResizeBegin(object sender, EventArgs e)
        {
            /* When the top level window is being resized, we just want to hide
             * the overlay window. When the resizing finishes, we want to show
             * the overlay window, if it was shown before the resize started. */
            _isDuringResizeSequence = true;
            _wasGlassShownBeforeResize = _isGlassShown;
        }

        private void OwnerResizeEnd(object sender, EventArgs e)
        {
            _isDuringResizeSequence = false;
            if (_wasGlassShownBeforeResize)
            {
                ShowGlass();
            }
        }

        private void OwnerLocationChanged(object sender, EventArgs e)
        {
            if (_mdiOwner != null)
            {
                HideGlass();
            }
            else
            {
                RecalculateBounds();
            }
        }

        private void OwnerSizeChanged(object sender, EventArgs e)
        {
            HideGlass();
        }

        private void ObjectListViewLocationChanged(object sender, EventArgs e)
        {
            if (_isGlassShown)
            {
                RecalculateBounds();
            }
        }

        private static void ObjectListViewSizeChanged(object sender, EventArgs e)
        {
            /* This event is triggered in all sorts of places, and not always when the size changes. */
            //if (isGlassShown) {
            //    Size = objectListView.ClientSize;
            //}
        }

        private void TabControlSelected(object sender, TabControlEventArgs e)
        {
            HideGlass();
        }

        private void ObjectListViewParentChanged(object sender, EventArgs e)
        {
            var olv = _objectListView;
            var overlay = Overlay;
            Unbind();
            Bind(olv, overlay);
        }

        private void ObjectListViewVisibleChanged(object sender, EventArgs e)
        {
            if (_objectListView.Visible)
            {
                ShowGlass();
            }
            else
                HideGlass();
        }

        /* Implementation */
        protected override void OnPaint(PaintEventArgs e)
        {
            if (_objectListView == null || Overlay == null)
            {
                return;
            }
            var g = e.Graphics;
            g.TextRenderingHint = ObjectListView.TextRenderingHint;
            g.SmoothingMode = ObjectListView.SmoothingMode;
            /* If we are part of an MDI app, make sure we don't draw outside the bounds */
            if (_mdiClient != null)
            {
                var r = _mdiClient.RectangleToScreen(_mdiClient.ClientRectangle);
                var r2 = _objectListView.RectangleToClient(r);
                g.SetClip(r2, System.Drawing.Drawing2D.CombineMode.Intersect);
            }
            Overlay.Draw(_objectListView, g, _objectListView.ClientRectangle);
        }

        private void RecalculateBounds()
        {
            if (!_isGlassShown)
            {
                return;
            }
            var rect = _objectListView.ClientRectangle;
            rect.X = 0;
            rect.Y = 0;
            Bounds = _objectListView.RectangleToScreen(rect);
        }

        internal void UpdateTransparency()
        {
            var transparentOverlay = Overlay as ITransparentOverlay;
            Opacity = transparentOverlay == null
                          ? _objectListView.OverlayTransparency/255.0f
                          : transparentOverlay.Transparency/255.0f;
        }

        protected override void WndProc(ref Message m)
        {
            const int wmNchittest = 132;
            const int httransparent = -1;
            switch (m.Msg)
            {
                /* Ignore all mouse interactions */
                case wmNchittest:
                    m.Result = (IntPtr)httransparent;
                    break;
            }
            base.WndProc(ref m);
        }
    }
}
