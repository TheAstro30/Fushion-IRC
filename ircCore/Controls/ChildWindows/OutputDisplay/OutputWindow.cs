/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ircCore.Controls.ChildWindows.Helpers;
using ircCore.Controls.ChildWindows.OutputDisplay.Helpers;
using ircCore.Controls.ChildWindows.OutputDisplay.Structures;
using ircCore.Settings.Theming;
using ircCore.Utils.Serialization;

namespace ircCore.Controls.ChildWindows.OutputDisplay
{
    /* Main output window control */
    public sealed class OutputWindow : OutputRenderer
    {
        private readonly bool _isDesignMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);

        private readonly Regex _regExUrl =
            new Regex(
                "((www\\.|www\\d\\.|(https?|shttp|ftp|irc):((//)|(\\\\\\\\)))+[\\w\\d:#@%/;$()~_?\\+-=\\\\\\.&]*)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly VScrollBar _vScroll;
        private Font _font;

        private bool _showScrollBar;
        private int _maximumLines;
        private int _averageCharacterWidth;
        private int _windowWidth;
        private int _scrollValue;
        private bool _scrolledToBottom;

        private string _url;
        private string _word;

        private readonly Timer _wrapUpdate;
        private readonly Timer _update;
        private bool _incomingText;

        private readonly bool _initialized;

        /* Public events */
        public event Action<string> OnLineAdded;
        public event Action<string> OnUrlDoubleClicked;
        public event Action<string> OnSpecialWordDoubleClicked;

        public event Action<string> OnWordUnderMouse;

        public event Action OnWindowDoubleClicked;
        public event Action OnWindowRightClicked;

        /* Constructor */
        public OutputWindow()
        {
            /* Double buffering */
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            /* Initial fore/back colors */
            ForeColor = SystemColors.WindowText;
            BackColor = SystemColors.Window;
            /* Initial property default settings */
            Font = new Font("Lucida Console", 10);
            TextData = new TextData();
            LineSpacingStyle = LineSpacingStyle.Single;
            ShowLineMarker = true;
            LineMarkerColor = Color.Red;
            MaximumLines = 500;
            ShowScrollBar = true;
            WordWrap = true;
            AllowCopySelection = true;
            /* Create scroll bar */
            _vScroll = new VScrollBar
                           {
                               Width = 18,
                               Dock = DockStyle.Right,
                               Minimum = 0,
                               Maximum = 0,
                               LargeChange = 1,
                               Visible = _showScrollBar
                           };
            _vScroll.Scroll += VerticalScrollUpdate;
            Controls.Add(_vScroll);
            /* Wrap update timer */
            _wrapUpdate = new Timer {Interval = 5};
            _wrapUpdate.Tick += TimerReWrap;
            /* Addline wait to finish adding timer */
            _update = new Timer {Interval = 5};
            _update.Tick += TimerUpdate;
            _initialized = true;           
        }

        /* Public properties */
        public override Font Font
        {
            get { return _font; }
            set
            {
                _font = new Font(value.Name, value.Size, value.Style);
                AdjustWidth(_initialized);
            }
        }

        public bool ShowScrollBar
        {
            get { return _showScrollBar; }
            set
            {
                _showScrollBar = value;
                if (_vScroll == null)
                {
                    return;
                }
                _vScroll.Visible = _showScrollBar;
                AdjustWidth(true);
            }
        }

        public bool WordWrap { get; set; }
        public bool AllowCopySelection { get; set; }
        public bool UserResize { get; set; }

        public bool AllowSpecialWordDoubleClick { get; set; } /* Used for double-clicking of nicks/channel names in text */

        public int MaximumLines
        {
            get
            {
                return _maximumLines;
            }
            set
            {
                _maximumLines = value > 0 ? value : 1;
            }
        }

        public int ScrollTo
        {
            get
            {
                return _scrollValue;
            }
            set
            {
                if (TextData.Lines.Count - 1 == -1)
                {
                    return;
                }
                _scrollValue = value;
                _vScroll.Value = _scrollValue;
                _scrolledToBottom = _scrollValue == TextData.WrappedLinesCount - 1;
                Invalidate();
            }
        }        

        /* Control overrides */        
        protected override void OnResize(EventArgs e)
        {
            if (_isDesignMode)
            {
                return;
            }            
            /* Adjust the vertical scrollbar's large change based on client window size - we also need to re-wrap the line data */
            AdjustWidth(true);
            Invalidate();
            base.OnResize(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            /* Where the magic happens ;) - simplified to a separate class */
            if (!Visible || _isDesignMode)
            {
                return;
            }
            if (!UserResize && _wrapUpdate.Enabled)
            {                
                /* Because of the way the control wraps/rewraps lines on resize, there is a slight jump if the parent form is maximized -
                 * this gets around that issue (but will flicker if the end user is resizing the form) */
                return;
            }
            /* Set initial graphics modes - not sure they actually make any difference to the overall speed... */
            e.Graphics.InterpolationMode = InterpolationMode.Low;
            e.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.None;            
            e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            /* Set client rectangle */
            var clientRect = new Rectangle(0, 0, ClientRectangle.Width - _vScroll.Width, ClientRectangle.Height);
            /* Render out drawing data */            
            Render(e.Graphics, _font, clientRect, _scrollValue);
        }

        /* Mouse events */
        protected override void OnMouseDown(MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (!AllowCopySelection)
                    {
                        return;
                    }
                    int lineIndex;
                    int wrapIndex;
                    int startY;
                    var currentLine = Character.GetLineNumber(TextData, e.Y, ClientRectangle.Height, _scrollValue,
                                                              _font.Height,
                                                              LineSpacing, LinePadding, out lineIndex, out wrapIndex,
                                                              out startY);
                    if (lineIndex == -1)
                    {
                        return; /* Nothing to mark */
                    }                    
                    var bmp = new Bitmap(ClientRectangle.Width - _vScroll.Width, ClientRectangle.Height,
                                         PixelFormat.Format24bppRgb);
                    using (var gSrc = CreateGraphics())
                    {
                        gSrc.InterpolationMode = InterpolationMode.Low;
                        gSrc.SmoothingMode = SmoothingMode.HighSpeed;
                        gSrc.PixelOffsetMode = PixelOffsetMode.None;
                        gSrc.CompositingQuality = CompositingQuality.HighSpeed;
                        gSrc.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                        using (var gDest = Graphics.FromImage(bmp))
                        {
                            var hdcSrc = gSrc.GetHdc();
                            var hdcDest = gDest.GetHdc();
                            /* Copy current graphics object to marking bmp */
                            Functions.BitBlt(hdcDest, 0, 0, bmp.Width, bmp.Height, hdcSrc, 0, 0, Functions.SrcCopy);
                            var wrapData = TextData.Wrapped[lineIndex].Lines[wrapIndex];
                            int position;
                            int startX;
                            var bold = wrapData.IsBold;
                            var underLine = wrapData.IsUnderline;
                            var italic = wrapData.IsItalic;                            
                            Character.ReturnChar(gSrc, wrapData, e.X, wrapIndex > 0, IndentWidth, _font, out position,
                                                 out startX, ref bold, ref underLine, ref italic);

                            MarkingData = new MarkingData
                                              {
                                                  MarkScrolledToBottom = _scrolledToBottom,
                                                  MarkBitmap = bmp,
                                                  MarkStartLine = currentLine,
                                                  MarkBackwardStart = currentLine,
                                                  MarkEndLine = currentLine,
                                                  MarkStartXPos = startX,
                                                  MarkOriginalXPos = startX,
                                                  MarkStartCharPos = position,
                                                  MarkEndCharPos = position,
                                                  IsBold = bold,
                                                  IsUnderline = underLine,
                                                  IsItalic = italic
                                              };
                            Invalidate();
                            /* Release resources */
                            gSrc.ReleaseHdc(hdcSrc);
                            gDest.ReleaseHdc(hdcDest);
                        }
                    }
                    break;

                case MouseButtons.Right:
                    if (OnWindowRightClicked != null)
                    {
                        OnWindowRightClicked();
                    }
                    break;
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && MarkingData != null)
            {
                /* Reset scrollbar (if scrolled to bottom originally) */
                if (MarkingData.MarkScrolledToBottom)
                {
                    _scrolledToBottom = true;
                    _scrollValue = TextData.WrappedLinesCount - 1;
                    UpdateScrollBar();
                }
                /* Do the actual copying of data to clipboard */
                if (MarkingData.MarkStartCharPos > -1 && MarkingData.MouseStartedMoving)
                {
                    var sb = new StringBuilder();
                    LineIndexData id;
                    WrapData.WrapLineData wrapData;                
                    if (MarkingData.MarkEndLine > MarkingData.MarkStartLine)
                    {
                        var iLines = MarkingData.MarkEndLine - MarkingData.MarkStartLine;
                        for (var i = 0; i <= iLines; i++)
                        {
                            id = TextData.GetWrappedIndexByLineNumber(MarkingData.MarkStartLine + i);
                            if (id == null)
                            {
                                continue;
                            }
                            wrapData = TextData.Wrapped[id.ActualLineNumber].Lines[id.WrappedLineNumber];
                            int iMarkCharEnd;
                            if (i == 0)
                            {
                                /* First line (text is got from markStartCharPos) */
                                iMarkCharEnd = wrapData.Text.Length - 1;
                                if (MarkingData.MarkStartCharPos <= iMarkCharEnd)
                                {
                                    sb.Append(wrapData.Text.Substring(MarkingData.MarkStartCharPos,
                                                                      (iMarkCharEnd - MarkingData.MarkStartCharPos) + 1));
                                }
                            }
                            else
                            {
                                /* Start pos is irrelevant */
                                if (i < iLines)
                                {
                                    iMarkCharEnd = wrapData.Text.Length;
                                }
                                else
                                {
                                    iMarkCharEnd = MarkingData.MarkEndCharPos + 1;
                                }
                                /* Now we need to know if this is the first line */
                                if (id.WrappedLineNumber == 0)
                                {
                                    sb.Append(string.Format("\r\n{0}", wrapData.Text.Substring(0, iMarkCharEnd)));
                                }
                                else
                                {
                                    /* We check if the line is broken at the start or it was broken by space */
                                    sb.Append(wrapData.BrokenAtStart
                                                  ? wrapData.Text.Substring(0, iMarkCharEnd)
                                                  : string.Format(" {0}", wrapData.Text.Substring(0, iMarkCharEnd)));
                                }
                            }
                        }
                        /* Set data to clipboard */
                        InternalClipboard.AddText(sb.ToString());
                    }
                    else
                    {
                        /* Single line only */                        
                        if (MarkingData.MarkEndCharPos >= MarkingData.MarkStartCharPos)
                        {
                            id = TextData.GetWrappedIndexByLineNumber(MarkingData.MarkStartLine);
                            if (id != null)
                            {
                                wrapData = TextData.Wrapped[id.ActualLineNumber].Lines[id.WrappedLineNumber];
                                if (MarkingData.MarkEndCharPos <= wrapData.Text.Length - 1)
                                {
                                    /* Set data to clipboard */
                                    InternalClipboard.AddText(wrapData.Text.Substring(MarkingData.MarkStartCharPos,
                                                                                           (MarkingData.MarkEndCharPos -
                                                                                            MarkingData.MarkStartCharPos) +
                                                                                           1));
                                }
                            }
                        }
                    }
                }
                /* Finish up */
                MarkingData.Dispose();
                MarkingData = null;
                Cursor = Cursors.Default;
                Invalidate();
            }
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int lineIndex;
            int wrapIndex;
            int startY;
            var currentLine = Character.GetLineNumber(TextData, e.Y, ClientRectangle.Height, _scrollValue, _font.Height, LineSpacing, LinePadding, out lineIndex, out wrapIndex, out startY);            
            if (lineIndex == -1)
            {
                /* Nothing under mouse - clear all word detection variables */
                _url = string.Empty;
                _word = string.Empty;
                AllowSpecialWordDoubleClick = false;
                Cursor = Cursors.Default;
                return;
            }
            using (var g = CreateGraphics())
            {
                g.InterpolationMode = InterpolationMode.Low;
                g.SmoothingMode = SmoothingMode.HighSpeed;
                g.PixelOffsetMode = PixelOffsetMode.None;
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                int position;
                int startX;
                var wrapData = TextData.Wrapped[lineIndex].Lines[wrapIndex];
                var bold = false;
                var underLine = false;
                var italic = false;                
                if (MarkingData != null)
                {                    
                    /* Selection marking */                                                                                    
                    MarkingData.MouseStartedMoving = true;
                    int i;                    
                    if (!MarkingData.MarkReverse)
                    {
                        /* Currently going forwards */                        
                        Character.ReturnChar(g, wrapData, e.X, wrapIndex > 0, IndentWidth, _font, out position,
                                             out startX, ref bold, ref underLine, ref italic);
                        if (MarkingData.MarkStartCharPos == -1 & position == 0)
                        {
                            MarkingData.MarkStartCharPos = 0;
                        }    
                        MarkingData.MarkEndLine = currentLine;
                        MarkingData.MarkEndCharPos = position;
                        if (MarkingData.MarkEndCharPos < MarkingData.MarkStartCharPos && MarkingData.MarkEndLine == MarkingData.MarkStartLine && MarkingData.MarkBackwardStart == MarkingData.MarkEndLine)
                        {
                            /* We are now going backwards in the line */                             
                            i = MarkingData.MarkStartCharPos;
                            MarkingData.MarkStartCharPos = position;
                            MarkingData.MarkEndCharPos = i != wrapData.Text.Length - 1 ? i - 1 : i;
                            MarkingData.MarkStartXPos = startX;
                            MarkingData.MarkReverse = true;                            
                        }
                        else if (MarkingData.MarkEndLine < MarkingData.MarkStartLine && MarkingData.MarkBackwardStart == MarkingData.MarkStartLine)
                        {
                            /* Shifted up to the next line above */                            
                            i = MarkingData.MarkStartLine;
                            MarkingData.MarkStartLine = MarkingData.MarkEndLine;
                            MarkingData.MarkEndLine = i;                            
                            i = MarkingData.MarkStartCharPos - 1;
                            MarkingData.MarkStartCharPos = MarkingData.MarkEndCharPos;
                            MarkingData.MarkEndCharPos = i;
                            MarkingData.MarkStartXPos = startX;                            
                            MarkingData.MarkReverse = true;                            
                        }                        
                    }
                    else
                    {              
                        /* Currently going backwards */                        
                        Character.ReturnChar(g, wrapData, e.X, wrapIndex > 0, IndentWidth, _font, out position,
                                             out startX, ref MarkingData.IsBold, ref MarkingData.IsUnderline,
                                             ref MarkingData.IsItalic);
                        MarkingData.MarkStartLine = currentLine;
                        MarkingData.MarkStartCharPos = position;
                        MarkingData.MarkStartXPos = startX;
                        if (MarkingData.MarkStartCharPos > MarkingData.MarkEndCharPos && MarkingData.MarkStartLine == MarkingData.MarkEndLine && MarkingData.MarkBackwardStart == MarkingData.MarkStartLine)
                        {                            
                            /* Now reversed direction on current line */                            
                            i = position;
                            MarkingData.MarkStartCharPos = MarkingData.MarkEndCharPos + 1;
                            MarkingData.MarkEndCharPos = i;
                            MarkingData.MarkStartXPos = MarkingData.MarkOriginalXPos;                            
                            MarkingData.MarkReverse = false;                            
                        }
                        else if (MarkingData.MarkEndLine < MarkingData.MarkStartLine && MarkingData.MarkBackwardStart == MarkingData.MarkEndLine)
                        {                            
                            i = MarkingData.MarkStartLine;
                            MarkingData.MarkStartLine = MarkingData.MarkEndLine;
                            MarkingData.MarkEndLine = i;                            
                            i = MarkingData.MarkEndCharPos+1;                            
                            MarkingData.MarkEndCharPos = MarkingData.MarkStartCharPos+1;
                            MarkingData.MarkStartCharPos = i;
                            MarkingData.MarkStartXPos = MarkingData.MarkOriginalXPos;
                            MarkingData.MarkReverse = false;                            
                        }                        
                    }                    
                    Invalidate();
                }
                else
                {
                    /* Get current word under mouse */
                    var c = Character.ReturnChar(g, wrapData, e.X, wrapIndex > 0, IndentWidth, _font, out position,
                                                 out startX, ref bold, ref underLine, ref italic);
                    var wordUnderMouse = c != '-'
                                                ? Character.ReturnWord(g, TextData.Wrapped[lineIndex], wrapIndex, position)
                                                : string.Empty;
                    if (!string.IsNullOrEmpty(wordUnderMouse))
                    {
                        /* Now we verify if the word is a hyper link */
                        if (_regExUrl.Match(wordUnderMouse).Success)
                        {
                            Cursor = Cursors.Hand;
                            _url = wordUnderMouse;
                        }
                        else
                        {
                            Cursor = Cursors.Default;
                            _url = string.Empty;
                            _word = wordUnderMouse;
                            if (OnWordUnderMouse != null)
                            {
                                OnWordUnderMouse(_word);
                            }
                        }
                    }
                    else
                    {
                        Cursor = Cursors.Default;
                        _url = string.Empty;
                        _word = null;
                    }
                }
            }
            base.OnMouseMove(e);
        }
        
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            MarkingData = null; /* Make sure this is nulled */
            if (!string.IsNullOrEmpty(_url))
            {                
                if (OnUrlDoubleClicked != null)
                {
                    OnUrlDoubleClicked(_url);
                }                
            }
            else
            {
                if (AllowSpecialWordDoubleClick && !string.IsNullOrEmpty(_word))
                {
                    if (OnSpecialWordDoubleClicked != null)
                    {
                        OnSpecialWordDoubleClicked(_word);
                    }
                    return;
                }
                if (OnWindowDoubleClicked != null)
                {
                    OnWindowDoubleClicked();
                }
            }
            base.OnMouseDoubleClick(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (!_showScrollBar)
            {
                return;
            }
            var lines = TextData.WrappedLinesCount - 1;
            if (e.Delta < 0)
            {
                /* Scroll down */
                _scrollValue += 3;                
                if (_scrollValue > lines)
                {
                    _scrollValue = lines;
                }
            }
            else
            {
                /* Scroll up */
                _scrollValue -= 3;
                if (_scrollValue < 0) { _scrollValue = 0; }
            }            
            if (_scrollValue < 0) { _scrollValue = 0; }
            _scrolledToBottom = _scrollValue == lines;            
            _vScroll.Value = _scrollValue;
            Invalidate();
        }

        /* Methods */
        public void AddLineMarker()
        {
            AddLineMarker(false);
        }

        public void AddLineMarker(bool removePrevious)
        {
            if (removePrevious)
            {
                var t = GetLineMarker();
                if (t != -1)
                {
                    TextData.Wrapped.RemoveAt(t);
                    TextData.Lines.RemoveAt(t);
                }
            }
            var text = new TextData.Text
                           {
                               DefaultColor = ThemeManager.CurrentTheme.Colors[1], 
                               Line = "-",
                               IsLineMarker = true
                           };
            AddLine(text);
        }

        public void AddLine(int defaultColor, string text)
        {
            if (string.IsNullOrEmpty(text))
            {                
                return; /* Nothing to do */
            }                     
            var t = new TextData.Text
                        {
                            DefaultColor = ThemeManager.CurrentTheme.Colors[defaultColor % 15],
                            Line = text
                        };
            AddLine(t);
        }

        public void AddLine(TextData.Text t)
        {
            /* Add the raw text to our structure first; this may or may not be used as the control develops */
            _incomingText = true;
            var lines = TextData.WrappedLinesCount - 1;
            var currentLines = lines > 0 ? lines : 0;
            TextData.Lines.Add(t);
            WrapData w;
            using (var g = CreateGraphics())
            {
                Wrapping.WordWrap(g, t.DefaultColor, BackColor, WordWrap, IndentWidth, t.Line, _windowWidth, _font, out w);
            }
            if (w.Lines.Count == 0)
            {
                return; /* It's unlikely ... */
            }
            w.IsLineMarker = t.IsLineMarker;
            TextData.Wrapped.Add(w);
            /* Set scrolled to bottom value */
            if (_scrollValue == currentLines)
            {
                _scrolledToBottom = true;
                if (MarkingData == null)
                {
                    _scrollValue = TextData.WrappedLinesCount - 1;
                    if (TextData.Lines.Count > _maximumLines)
                    {
                        /* Only trim the buffer if the window isn't scrolled above bottom (or marking) - or else lines appear to move up */
                        TrimBuffer();
                    }
                }
            }
            //else if (text != "-")
            //{
            else if (!t.IsLineMarker)
            {
                _scrolledToBottom = false;
                SystemSounds.Beep.Play();
            }
            if (OnLineAdded != null && !t.IsLineMarker)
            {
                OnLineAdded(t.Line); /* This can be used to output to a log file */
            }
            if (MarkingData != null)
            {
                /* Do not refresh the screen as marking is in progress */
                return;
            }
            /* Now refresh the hDC and draw out our newly added line ;) */
            if (_update.Enabled)
            {
                return;
            }
            _update.Enabled = true;
        }

        public void LoadBuffer(string file)
        {
            /* Load the file and fill the window buffer/re-wrap */
            if (!File.Exists(file))
            {
                return;
            }
            _vScroll.Minimum = 0;
            _vScroll.Maximum = 0;
            _vScroll.LargeChange = 1;
            var tmp = new TextData();
            if (!BinarySerialize<TextData>.Load(file, ref tmp))
            {
                /* Failed to load so we leave the window blank, obviously */
                return;
            }
            TextData = tmp;
            TextData.LoadBuffer = true;
            /* Add a separator line at the bottom */
            AddLineMarker();
            /* Reset scrollbar */
            _scrollValue = TextData.WrappedLinesCount - 1;
            if (_scrollValue < 0)
            {
                _scrollValue = 0;
            }
            _scrolledToBottom = true;
            _vScroll.Maximum = _scrollValue + _vScroll.LargeChange - 1;
            _vScroll.Value = _scrollValue;
            /* Make sure to re-wrap just incase the window size is different to what was cached */
            _wrapUpdate.Enabled = true;
        }

        public void SaveBuffer(string file)
        {
            var t = GetLineMarker();
            if (t != -1)
            {
                TextData.Wrapped.RemoveAt(t);
                TextData.Lines.RemoveAt(t);
            }
            TextData.WindowWidth = ClientRectangle.Width;
            /* Save the buffer output to file */
            BinarySerialize<TextData>.Save(file, TextData);
        }

        public void AdjustWidth(bool adjustClientWidth)
        {
            using (var g = CreateGraphics())
            {
                _averageCharacterWidth = TextMeasurement.MeasureStringWidth(g, _font, "*");
                IndentWidth = _averageCharacterWidth * 2;
            }
            TextHeight = _font.Height * LineSpacing;
            if (!adjustClientWidth) { return; }
            var i = ClientRectangle.Width - (_showScrollBar ? _vScroll.Width + _averageCharacterWidth : _averageCharacterWidth) - 2;
            if (i == _windowWidth)
            {
                return;
            }
            _windowWidth = i; /* Update average characters that will fit in the client area */
            _wrapUpdate.Enabled = true;
        }

        public void Clear()
        {
            TextData = new TextData();
            _scrollValue = 0;
            _scrolledToBottom = true;
            _vScroll.Minimum = 0;
            _vScroll.Maximum = 0;
            _vScroll.LargeChange = 1;
            Invalidate();
        }

        public void MouseWheelScroll(MouseEventArgs e)
        {
            OnMouseWheel(e);
        }

        /* Private methods */
        private int GetLineMarker()
        {
            for (var i = 0; i <= TextData.Lines.Count - 1; i++ )
            {
                if (TextData.Lines[i].IsLineMarker)
                {
                    return i;
                }
            }
            return -1;
            //return TextData.Lines.FirstOrDefault(tx => tx.IsLineMarker);
        }

        private void TrimBuffer()
        {
            while (TextData.Lines.Count > _maximumLines)
            {
                /* Simplest solution is to remove both the first line and all wrapped lines associated with it */
                TextData.Wrapped.RemoveAt(0);
                TextData.Lines.RemoveAt(0);
            }
            if (_scrolledToBottom)
            {
                _scrollValue = TextData.WrappedLinesCount - 1;
            }
        }        

        private void VerticalScrollUpdate(object sender, ScrollEventArgs e)
        {
            _scrollValue = e.NewValue;
            _scrolledToBottom = _scrollValue == TextData.WrappedLinesCount - 1;
            Invalidate();
        }

        private void UpdateScrollBar()
        {
            if (_scrollValue < 0)
            {
                /* It shouldn't be... */
                _scrollValue = 0;
            }
            var lines = TextData.WrappedLinesCount - 1;
            if (lines < 0)
            {
                return;
            }
            if (_scrollValue > lines)
            {
                /* This sometimes happens if scrolled up and resizing the window... */
                _scrollValue = lines;
            }
            _vScroll.Maximum = _scrolledToBottom
                                   ? _scrollValue + _vScroll.LargeChange - 1
                                   : lines + (_vScroll.LargeChange - 1);
            _vScroll.Value = _scrollValue;
        }

        /* Timer events */
        private void TimerReWrap(object sender, EventArgs e)
        {            
            _wrapUpdate.Enabled = false;
            if (TextData.LoadBuffer && TextData.WindowWidth == ClientRectangle.Width)
            {
                TextData.LoadBuffer = false;
                return;
            }
            /* Re-wrap */
            TextData.Wrapped.Clear();
            using (var g = CreateGraphics())
            {
                foreach (var l in TextData.Lines)
                {
                    WrapData w;                    
                    Wrapping.WordWrap(g, l.DefaultColor, BackColor, WordWrap, IndentWidth, l.Line, _windowWidth, _font, out w);
                    if (w.Lines.Count == 0)
                    {
                        return;
                    }
                    w.IsLineMarker = l.IsLineMarker;                    
                    TextData.Wrapped.Add(w); /* It's unlikely ... */
                }
            }
            /* Update the vertical scroll bar's max, current value, etc */
            var lines = TextData.WrappedLinesCount - 1;
            if (TextHeight > 0)
            {
                var largeChange = ClientRectangle.Height / TextHeight;
                _vScroll.LargeChange = largeChange;
                _vScroll.Maximum = (lines) + largeChange;
            }
            if (_scrolledToBottom)
            {
                _scrollValue = lines;
            }
            UpdateScrollBar();
            Invalidate();
        }

        private void TimerUpdate(object sender, EventArgs e)
        {
            if (!_incomingText)
            {
                _update.Enabled = false;
                UpdateScrollBar();
                Invalidate();
            }
            else
            {
                _incomingText = false;
            }
        }
    }
}