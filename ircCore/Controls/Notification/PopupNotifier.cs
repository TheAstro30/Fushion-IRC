/*
 *	Created/modified in 2011 by Simon Baer
 *	
 *  Based on the Code Project article by Nicolas Wälti:
 *  http://www.codeproject.com/KB/cpp/PopupNotifier.aspx
 * 
 *  Licensed under the Code Project Open License (CPOL).
 */
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace ircCore.Controls.Notification
{
    [DefaultEvent("Click")]
    public class PopupNotifier : Component
    {
        private readonly FrmPopup _frmPopup;
        private readonly Timer _tmrAnimation;
        private readonly Timer _tmrWait;
        private bool _disposed;

        private bool _isAppearing = true;
        private double _maxOpacity;
        private int _maxPosition;
        private bool _mouseIsOn;
        private double _opacityStart;
        private double _opacityStop;
        private int _posStart;
        private int _posStop;
        private int _realAnimationDuration;
        private Stopwatch _sw;
        private Size _imageSize = new Size(0, 0);

        /* Events raised */
        public event Action<object, EventArgs> Click;
        public event Action<object, EventArgs> Close;
        public event Action<object, EventArgs> Appear;
        public event Action<object, EventArgs> Disappear;

        /* Public properties */
        public Color HeaderColor { get; set; }
        public Color BodyColor { get; set; }
        public Color TitleColor { get; set; }
        public Color ContentColor { get; set; }
        public Color BorderColor { get; set; }
        public Color ButtonBorderColor { get; set; }
        public Color ButtonHoverColor { get; set; }
        public Color ContentHoverColor { get; set; }
        public int GradientPower { get; set; }
        public Font ContentFont { get; set; }
        public Font TitleFont { get; set; }

        public Size ImageSize
        {
            get
            {
                if (_imageSize.Width == 0)
                {
                    return Image != null ? Image.Size : new Size(0, 0);
                }
                return _imageSize;
            }
            set { _imageSize = value; }
        }

        public Image Image { get; set; }
        public bool ShowGrip { get; set; }
        public bool Scroll { get; set; }
        public string ContentText { get; set; }
        public string TitleText { get; set; }
        public Padding TitlePadding { get; set; }
        public Padding ContentPadding { get; set; }
        public Padding ImagePadding { get; set; }
        public int HeaderHeight { get; set; }
        public bool ShowCloseButton { get; set; }
        public bool ShowOptionsButton { get; set; }
        public ContextMenuStrip OptionsMenu { get; set; }
        public int Delay { get; set; }
        public int AnimationDuration { get; set; }
        public int AnimationInterval { get; set; }
        public Size Size { get; set; }
        public int PositionY { get; set; }
        public bool IsRightToLeft { get; set; }

        /* Constructor */
        public PopupNotifier()
        {
            HeaderColor = SystemColors.ControlDark;
            BodyColor = SystemColors.Control;
            TitleColor = Color.Gray;
            ContentColor = SystemColors.ControlText;
            BorderColor = SystemColors.WindowFrame;
            ButtonBorderColor = SystemColors.WindowFrame;
            ButtonHoverColor = SystemColors.Highlight;
            ContentHoverColor = SystemColors.HotTrack;
            GradientPower = 50;
            ContentFont = SystemFonts.DialogFont;
            TitleFont = SystemFonts.CaptionFont;
            ShowGrip = true;
            Scroll = true;
            TitlePadding = new Padding(0);
            ContentPadding = new Padding(0);
            ImagePadding = new Padding(0);
            HeaderHeight = 9;
            ShowCloseButton = true;
            ShowOptionsButton = false;
            Delay = 3000;
            AnimationInterval = 10;
            AnimationDuration = 1000;
            Size = new Size(400, 100);

            _frmPopup = new FrmPopup(this)
                            {
                                TopMost = true,
                                FormBorderStyle = FormBorderStyle.None,
                                StartPosition = FormStartPosition.Manual,
                            };

            _frmPopup.MouseEnter += PopupMouseEnter;
            _frmPopup.MouseLeave += PopupMouseLeave;
            _frmPopup.CloseClick += PopupCloseClick;
            _frmPopup.LinkClick += PopupLinkClicked;
            _frmPopup.ContextMenuOpened += frmPopup_ContextMenuOpened;
            _frmPopup.ContextMenuClosed += PopupContextMenuClosed;
            _frmPopup.VisibleChanged += PopupVisibleChanged;

            _tmrAnimation = new Timer();
            _tmrAnimation.Tick += AnimationTick;

            _tmrWait = new Timer();
            _tmrWait.Tick += WaitTick;
        }

        /* Public methods */
        public void ResetImageSize()
        {
            _imageSize = Size.Empty;
        }

        public void Popup()
        {
            if (_disposed)
            {
                return;
            }
            if (!_frmPopup.Visible)
            {
                _frmPopup.Size = Size;
                if (Scroll)
                {
                    _posStart = Screen.PrimaryScreen.WorkingArea.Bottom;
                    _posStop = Screen.PrimaryScreen.WorkingArea.Bottom - _frmPopup.Height - PositionY;
                }
                else
                {
                    _posStart = Screen.PrimaryScreen.WorkingArea.Bottom - _frmPopup.Height - PositionY;
                    _posStop = Screen.PrimaryScreen.WorkingArea.Bottom - _frmPopup.Height - PositionY;
                }
                _opacityStart = 0;
                _opacityStop = 1;

                _frmPopup.Opacity = _opacityStart;
                _frmPopup.Location = new Point(Screen.PrimaryScreen.WorkingArea.Right - _frmPopup.Size.Width - 1,
                                               _posStart);
                _frmPopup.Visible = true;
                _isAppearing = true;

                _tmrWait.Interval = Delay;
                _tmrAnimation.Interval = AnimationInterval;
                _realAnimationDuration = AnimationDuration;
                _tmrAnimation.Start();
                _sw = Stopwatch.StartNew();
            }
            else
            {
                if (!_isAppearing)
                {
                    _frmPopup.Size = Size;
                    if (Scroll)
                    {
                        _posStart = _frmPopup.Top;
                        _posStop = Screen.PrimaryScreen.WorkingArea.Bottom - _frmPopup.Height - PositionY;
                    }
                    else
                    {
                        _posStart = Screen.PrimaryScreen.WorkingArea.Bottom - _frmPopup.Height - PositionY;
                        _posStop = Screen.PrimaryScreen.WorkingArea.Bottom - _frmPopup.Height - PositionY;
                    }
                    _opacityStart = _frmPopup.Opacity;
                    _opacityStop = 1;
                    _isAppearing = true;
                    _realAnimationDuration = Math.Max((int) _sw.ElapsedMilliseconds, 1);
                    _sw.Restart();
                }
                _frmPopup.Invalidate();
            }
        }

        public void Hide()
        {
            _tmrAnimation.Stop();
            _tmrWait.Stop();
            _frmPopup.Hide();
        }

        /* Private methods */
        private bool ShouldSerializeImageSize()
        {
            return (!_imageSize.Equals(Size.Empty));
        }

        private void ResetTitlePadding()
        {
            TitlePadding = Padding.Empty;
        }

        private bool ShouldSerializeTitlePadding()
        {
            return (!TitlePadding.Equals(Padding.Empty));
        }

        private void ResetContentPadding()
        {
            ContentPadding = Padding.Empty;
        }

        private bool ShouldSerializeContentPadding()
        {
            return (!ContentPadding.Equals(Padding.Empty));
        }

        private void ResetImagePadding()
        {
            ImagePadding = Padding.Empty;
        }

        private bool ShouldSerializeImagePadding()
        {
            return (!ImagePadding.Equals(Padding.Empty));
        }

        private void PopupContextMenuClosed(object sender, EventArgs e)
        {
            if (_mouseIsOn)
            {
                return;
            }
            _tmrWait.Interval = Delay;
            _tmrWait.Start();
        }

        private void frmPopup_ContextMenuOpened(object sender, EventArgs e)
        {
            _tmrWait.Stop();
        }

        /* Callbacks */
        private void PopupLinkClicked(object sender, EventArgs e)
        {
            if (Click != null)
            {
                Click(this, EventArgs.Empty);
            }
        }

        private void PopupCloseClick(object sender, EventArgs e)
        {
            Hide();
            if (Close != null)
            {
                Close(this, EventArgs.Empty);
            }
        }

        private void PopupVisibleChanged(object sender, EventArgs e)
        {
            if (_frmPopup.Visible)
            {
                if (Appear != null)
                {
                    Appear(this, EventArgs.Empty);
                }
            }
            else
            {
                if (Disappear != null)
                {
                    Disappear(this, EventArgs.Empty);
                }
            }
        }

        private void PopupMouseLeave(object sender, EventArgs e)
        {
            if (_frmPopup.Visible && (OptionsMenu == null || !OptionsMenu.Visible))
            {
                _tmrWait.Interval = Delay;
                _tmrWait.Start();
            }
            _mouseIsOn = false;
        }

        private void PopupMouseEnter(object sender, EventArgs e)
        {
            if (!_isAppearing)
            {
                _frmPopup.Top = _maxPosition;
                _frmPopup.Opacity = _maxOpacity;
                _tmrAnimation.Stop();
            }
            _tmrWait.Stop();
            _mouseIsOn = true;
        }

        /* Timers */
        private void AnimationTick(object sender, EventArgs e)
        {
            var elapsed = _sw.ElapsedMilliseconds;
            var posCurrent = (int)(_posStart + ((_posStop - _posStart) * elapsed / _realAnimationDuration));
            var neg = (_posStop - _posStart) < 0;
            if ((neg && posCurrent < _posStop) || (!neg && posCurrent > _posStop))
            {
                posCurrent = _posStop;
            }
            var opacityCurrent = _opacityStart + ((_opacityStop - _opacityStart) * elapsed / _realAnimationDuration);
            neg = (_opacityStop - _opacityStart) < 0;
            if ((neg && opacityCurrent < _opacityStop) || (!neg && opacityCurrent > _opacityStop))
            {
                opacityCurrent = _opacityStop;
            }
            _frmPopup.Top = posCurrent;
            _frmPopup.Opacity = opacityCurrent;
            /* animation has ended */
            if (elapsed <= _realAnimationDuration)
            {
                return;
            }
            _sw.Reset();
            _tmrAnimation.Stop();
            if (_isAppearing)
            {
                if (Scroll)
                {
                    _posStart = Screen.PrimaryScreen.WorkingArea.Bottom - _frmPopup.Height;
                    _posStop = Screen.PrimaryScreen.WorkingArea.Bottom;
                }
                else
                {
                    _posStart = Screen.PrimaryScreen.WorkingArea.Bottom - _frmPopup.Height;
                    _posStop = Screen.PrimaryScreen.WorkingArea.Bottom - _frmPopup.Height;
                }
                _opacityStart = 1;
                _opacityStop = 0;
                _realAnimationDuration = AnimationDuration;
                _isAppearing = false;
                _maxPosition = _frmPopup.Top;
                _maxOpacity = _frmPopup.Opacity;
                if (!_mouseIsOn)
                {
                    _tmrWait.Stop();
                    _tmrWait.Start();
                }
            }
            else
            {
                _frmPopup.Hide();
            }
        }

        private void WaitTick(object sender, EventArgs e)
        {
            _tmrWait.Stop();
            _tmrAnimation.Interval = AnimationInterval;
            _tmrAnimation.Start();
            _sw.Restart();
        }

        /* IDisposable */
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing && _frmPopup != null)
                {
                    _frmPopup.Dispose();
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }
    }
}