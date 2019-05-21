/*
 *	Created/modified in 2011 by Simon Baer
 *	
 *  Based on the Code Project article by Nicolas Wälti:
 *  http://www.codeproject.com/KB/cpp/PopupNotifier.aspx
 * 
 *  Licensed under the Code Project Open License (CPOL).
 */
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ircCore.Properties;

namespace ircCore.Controls.Notification
{
    internal class FrmPopup : Form
    {
        private int _heightOfTitle;
        private bool _mouseOnClose;
        private bool _mouseOnLink;
        private bool _mouseOnOptions;

        /* GDI objects */
        private LinearGradientBrush _brushBody;
        private Brush _brushButtonHover;
        private Brush _brushContent;
        private Brush _brushForeColor;
        private LinearGradientBrush _brushHeader;
        private Brush _brushLinkHover;
        private Brush _brushTitle;
        private bool _gdiInitialized;
        private Pen _penBorder;
        private Pen _penButtonBorder;
        private Pen _penContent;
        private Rectangle _rcBody;
        private Rectangle _rcForm;
        private Rectangle _rcHeader;

        /* Events Raised */
        public event EventHandler LinkClick;
        public event EventHandler CloseClick;

        internal event EventHandler ContextMenuOpened;
        internal event EventHandler ContextMenuClosed;

        /* Properties */
        public new PopupNotifier Parent { get; set; }

        private RectangleF RectContentText
        {
            get
            {
                if (Parent.Image != null)
                {
                    return new RectangleF(
                        Parent.ImagePadding.Left + Parent.ImageSize.Width + Parent.ImagePadding.Right +
                        Parent.ContentPadding.Left,
                        Parent.HeaderHeight + Parent.TitlePadding.Top + _heightOfTitle + Parent.TitlePadding.Bottom +
                        Parent.ContentPadding.Top,
                        Width - Parent.ImagePadding.Left - Parent.ImageSize.Width - Parent.ImagePadding.Right -
                        Parent.ContentPadding.Left - Parent.ContentPadding.Right - 16 - 5,
                        Height - Parent.HeaderHeight - Parent.TitlePadding.Top - _heightOfTitle -
                        Parent.TitlePadding.Bottom - Parent.ContentPadding.Top - Parent.ContentPadding.Bottom - 1);
                }
                return new RectangleF(
                    Parent.ContentPadding.Left,
                    Parent.HeaderHeight + Parent.TitlePadding.Top + _heightOfTitle + Parent.TitlePadding.Bottom +
                    Parent.ContentPadding.Top,
                    Width - Parent.ContentPadding.Left - Parent.ContentPadding.Right - 16 - 5,
                    Height - Parent.HeaderHeight - Parent.TitlePadding.Top - _heightOfTitle -
                    Parent.TitlePadding.Bottom - Parent.ContentPadding.Top - Parent.ContentPadding.Bottom - 1);
            }
        }

        private Rectangle RectClose
        {
            get { return new Rectangle(Width - 5 - 16, Parent.HeaderHeight + 3, 16, 16); }
        }

        private Rectangle RectOptions
        {
            get { return new Rectangle(Width - 5 - 16, Parent.HeaderHeight + 3 + 16 + 5, 16, 16); }
        }

        private static int AddValueMax255(int input, int add)
        {
            return (input + add < 256) ? input + add : 255;
        }

        private static int DedValueMin0(int input, int ded)
        {
            return (input - ded > 0) ? input - ded : 0;
        }

        private Color GetDarkerColor(Color color)
        {
            return Color.FromArgb(255, DedValueMin0(color.R, Parent.GradientPower),
                                  DedValueMin0(color.G, Parent.GradientPower),
                                  DedValueMin0(color.B, Parent.GradientPower));
        }

        private Color GetLighterColor(Color color)
        {
            return Color.FromArgb(255, AddValueMax255(color.R, Parent.GradientPower),
                                  AddValueMax255(color.G, Parent.GradientPower),
                                  AddValueMax255(color.B, Parent.GradientPower));
        }

        /* Constructor */
        public FrmPopup(PopupNotifier parent)
        {
            Parent = parent;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            ShowInTaskbar = false;

            VisibleChanged += PopupVisibleChanged;
        }

        /* Overrides */
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                /* make sure Top Most property on form is set to false otherwise this doesn't work */
                const int wsExTopmost = 0x00000008;
                var cp = base.CreateParams;
                cp.ExStyle |= wsExTopmost;
                return cp;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (Parent.ShowCloseButton)
            {
                _mouseOnClose = RectClose.Contains(e.X, e.Y);
            }
            if (Parent.ShowOptionsButton)
            {
                _mouseOnOptions = RectOptions.Contains(e.X, e.Y);
            }
            _mouseOnLink = RectContentText.Contains(e.X, e.Y);
            Invalidate();
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (RectClose.Contains(e.X, e.Y) && (CloseClick != null))
                {
                    CloseClick(this, EventArgs.Empty);
                }
                if (RectContentText.Contains(e.X, e.Y) && (LinkClick != null))
                {
                    LinkClick(this, EventArgs.Empty);
                }
                if (RectOptions.Contains(e.X, e.Y) && (Parent.OptionsMenu != null))
                {
                    if (ContextMenuOpened != null)
                    {
                        ContextMenuOpened(this, EventArgs.Empty);
                    }
                    Parent.OptionsMenu.Show(this,
                                            new Point(RectOptions.Right - Parent.OptionsMenu.Width, RectOptions.Bottom));
                    Parent.OptionsMenu.Closed += OptionsMenuClosed;
                }
            }
            base.OnMouseMove(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!_gdiInitialized)
            {
                AllocateGdiObjects();
            }
            /* Draw window */
            e.Graphics.FillRectangle(_brushBody, _rcBody);
            e.Graphics.FillRectangle(_brushHeader, _rcHeader);
            e.Graphics.DrawRectangle(_penBorder, _rcForm);
            if (Parent.ShowGrip)
            {
                var b = Resources.popupGrip;
                e.Graphics.DrawImage(b, ((Width - b.Width) / 2), ((Parent.HeaderHeight - 3) / 2));
            }
            if (Parent.ShowCloseButton)
            {
                if (_mouseOnClose)
                {
                    e.Graphics.FillRectangle(_brushButtonHover, RectClose);
                    e.Graphics.DrawRectangle(_penButtonBorder, RectClose);
                }
                e.Graphics.DrawLine(_penContent, RectClose.Left + 4, RectClose.Top + 4, RectClose.Right - 4,
                                    RectClose.Bottom - 4);
                e.Graphics.DrawLine(_penContent, RectClose.Left + 4, RectClose.Bottom - 4, RectClose.Right - 4,
                                    RectClose.Top + 4);
            }
            if (Parent.ShowOptionsButton)
            {
                if (_mouseOnOptions)
                {
                    e.Graphics.FillRectangle(_brushButtonHover, RectOptions);
                    e.Graphics.DrawRectangle(_penButtonBorder, RectOptions);
                }
                e.Graphics.FillPolygon(_brushForeColor,
                                       new[]
                                           {
                                               new Point(RectOptions.Left + 4, RectOptions.Top + 6),
                                               new Point(RectOptions.Left + 12, RectOptions.Top + 6),
                                               new Point(RectOptions.Left + 8, RectOptions.Top + 4 + 6)
                                           });
            }
            /* Draw icon */
            if (Parent.Image != null)
            {
                e.Graphics.DrawImage(Parent.Image, Parent.ImagePadding.Left,
                                     Parent.HeaderHeight + Parent.ImagePadding.Top, Parent.ImageSize.Width,
                                     Parent.ImageSize.Height);
            }
            if (Parent.IsRightToLeft)
            {
                _heightOfTitle = (int)e.Graphics.MeasureString("A", Parent.TitleFont).Height;
                /* The value 30 is because of x close icon */
                var titleX2 = Width - 30;
                /* Draw title right to left */
                var headerFormat = new StringFormat(StringFormatFlags.DirectionRightToLeft);
                e.Graphics.DrawString(Parent.TitleText, Parent.TitleFont, _brushTitle, titleX2,
                                      Parent.HeaderHeight + Parent.TitlePadding.Top, headerFormat);
                /* Draw content text, optionally with a bold part */
                Cursor = _mouseOnLink ? Cursors.Hand : Cursors.Default;
                using (var brushText = _mouseOnLink ? _brushLinkHover : _brushContent)
                {
                    var contentFormat = new StringFormat(StringFormatFlags.DirectionRightToLeft);
                    e.Graphics.DrawString(Parent.ContentText, Parent.ContentFont, brushText, RectContentText, contentFormat);
                }
            }
            else
            {
                /* Calculate height of title */
                _heightOfTitle = (int)e.Graphics.MeasureString("A", Parent.TitleFont).Height;
                var titleX = Parent.TitlePadding.Left;
                if (Parent.Image != null)
                {
                    titleX += Parent.ImagePadding.Left + Parent.ImageSize.Width + Parent.ImagePadding.Right;
                }
                e.Graphics.DrawString(Parent.TitleText, Parent.TitleFont, _brushTitle, titleX,
                                      Parent.HeaderHeight + Parent.TitlePadding.Top);
                /* Draw content text, optionally with a bold part */
                Cursor = _mouseOnLink ? Cursors.Hand : Cursors.Default;
                var brushText = _mouseOnLink ? _brushLinkHover : _brushContent;
                e.Graphics.DrawString(Parent.ContentText, Parent.ContentFont, brushText, RectContentText);                
            }
            base.OnPaint(e);
        }

        /* Callbacks */
        private void PopupVisibleChanged(object sender, EventArgs e)
        {
            if (!Visible)
            {
                return;
            }
            _mouseOnClose = false;
            _mouseOnLink = false;
            _mouseOnOptions = false;
        }

        private void OptionsMenuClosed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            Parent.OptionsMenu.Closed -= OptionsMenuClosed;

            if (ContextMenuClosed != null)
            {
                ContextMenuClosed(this, EventArgs.Empty);
            }
        }

        /* Private methods */
        private void AllocateGdiObjects()
        {
            _rcBody = new Rectangle(0, 0, Width, Height);
            _rcHeader = new Rectangle(0, 0, Width, Parent.HeaderHeight);
            _rcForm = new Rectangle(0, 0, Width - 1, Height - 1);

            _brushBody = new LinearGradientBrush(_rcBody, Parent.BodyColor, GetLighterColor(Parent.BodyColor),
                                                LinearGradientMode.Vertical);
            _brushHeader = new LinearGradientBrush(_rcHeader, Parent.HeaderColor, GetDarkerColor(Parent.HeaderColor),
                                                  LinearGradientMode.Vertical);
            _brushButtonHover = new SolidBrush(Parent.ButtonHoverColor);
            _penButtonBorder = new Pen(Parent.ButtonBorderColor);
            _penContent = new Pen(Parent.ContentColor, 2);
            _penBorder = new Pen(Parent.BorderColor);
            _brushForeColor = new SolidBrush(ForeColor);
            _brushLinkHover = new SolidBrush(Parent.ContentHoverColor);
            _brushContent = new SolidBrush(Parent.ContentColor);
            _brushTitle = new SolidBrush(Parent.TitleColor);
            _gdiInitialized = true;
        }

        private void DisposeGdiObjects()
        {
            if (!_gdiInitialized)
            {
                return;
            }
            _brushBody.Dispose();
            _brushHeader.Dispose();
            _brushButtonHover.Dispose();
            _penButtonBorder.Dispose();
            _penContent.Dispose();
            _penBorder.Dispose();
            _brushForeColor.Dispose();
            _brushLinkHover.Dispose();
            _brushContent.Dispose();
            _brushTitle.Dispose();
        }

        /* IDisposable */
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeGdiObjects();
            }
            base.Dispose(disposing);
        }
    }
}