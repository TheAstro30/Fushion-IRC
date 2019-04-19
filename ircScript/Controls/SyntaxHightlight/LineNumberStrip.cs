/* Originally part of Able Opus C# Controls
 * Available at:
 * https://sourceforge.net/projects/bsfccontrollibr/ 
 */
using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace ircScript.Controls.SyntaxHightlight
{
    public enum LineNumberStyle
    {
        None = 0,
        OffsetColors = 1,
        Boxed = 2
    }

    public class LineNumberStrip : Control
    {
        private const float FontModifier = 0.09f;
        private const int DrawingOffset = 1;

        private readonly BufferedGraphicsContext _bufferContext = BufferedGraphicsManager.Current;
        private readonly RichTextBox _richTextBox;
        private BufferedGraphics _bufferedGraphics;
        private int _dragDistance;
        private Brush _fontBrush;
        private float _fontHeight;
        private bool _hideWhenNoLines;
        private int _lastLineCount;
        private int _lastYPos = -1;
        private int _numPadding = 3;
        private Brush _offsetBrush = new SolidBrush(Color.DarkSlateGray);
        private Pen _penBoxedLine = Pens.LightGray;
        private int _scrollingLineIncrement = 5;
        private bool _speedBump;
        private LineNumberStyle _style;

        /* Public properties */
        public bool DockToRight
        {
            get { return (Dock == DockStyle.Right); }
            set { Dock = (value) ? DockStyle.Right : DockStyle.Left; }
        }

        [Category("Layout")]
        [Description(
            "Gets or sets the spacing from the left and right of the numbers to the let and right of the control")]
        public int NumberPadding
        {
            get { return _numPadding; }
            set
            {
                _numPadding = value;
                if (_richTextBox != null)
                {
                    SetControlWidth();
                }
            }
        }

        [Category("Appearance")]
        public LineNumberStyle Style
        {
            get { return _style; }
            set
            {
                _style = value;
                Invalidate(false);
            }
        }

        [Category("Appearance")]
        public Color BoxedLineColor
        {
            get { return _penBoxedLine.Color; }
            set
            {
                _penBoxedLine = new Pen(value);
                Invalidate(false);
            }
        }

        [Category("Appearance")]
        public Color OffsetColor
        {
            get { return new Pen(_offsetBrush).Color; }
            set
            {
                _offsetBrush = new SolidBrush(value);
                Invalidate(false);
            }
        }

        [Category("Behavior")]
        public bool HideWhenNoLines
        {
            get { return _hideWhenNoLines; }
            set { _hideWhenNoLines = value; }
        }

        [Browsable(false)]
        public override DockStyle Dock
        {
            get { return base.Dock; }
            set { base.Dock = value; }
        }

        [Category("Behavior")]
        public int ScrollSpeed
        {
            get { return _scrollingLineIncrement; }
            set { _scrollingLineIncrement = value; }
        }

        /* Constructor - MUST pass a RTB control */
        public LineNumberStrip(RichTextBox plainTextBox)
        {
            _richTextBox = plainTextBox;
            plainTextBox.TextChanged += RichTextBoxTextChanged;
            plainTextBox.FontChanged += RichTextBoxFontChanged;
            plainTextBox.VScroll += RichTextBoxVScroll;

            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            Size = new Size(10, 10);
            base.BackColor = Color.White;
            base.Dock = DockStyle.Left;
            OffsetColor = Color.PapayaWhip;
            Style = LineNumberStyle.OffsetColors;

            _fontBrush = new SolidBrush(base.ForeColor);

            SetFontHeight();
            UpdateBackBuffer();
            SendToBack();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (!e.Button.Equals(MouseButtons.Left) || _scrollingLineIncrement == 0)
            {
                return;
            }
            _lastYPos = Cursor.Position.Y;
            Cursor = Cursors.NoMoveVert;
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            SetControlWidth();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            Cursor = Cursors.Default;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!e.Button.Equals(MouseButtons.Left) || _scrollingLineIncrement == 0)
            {
                return;
            }
            _dragDistance += Cursor.Position.Y - _lastYPos;
            if (_dragDistance > _fontHeight)
            {
                var selectionStart = _richTextBox.GetFirstCharIndexFromLine(NextLineDown);
                _richTextBox.Select(selectionStart, 0);
                _dragDistance = 0;
            }
            else if (_dragDistance < _fontHeight*-1)
            {
                var selectionStart = _richTextBox.GetFirstCharIndexFromLine(NextLineUp);
                _richTextBox.Select(selectionStart, 0);
                _dragDistance = 0;
            }
            _lastYPos = Cursor.Position.Y;
        }

        /* Functions */
        private void UpdateBackBuffer()
        {
            if (Width <= 0)
            {
                return;
            }
            _bufferContext.MaximumBuffer = new Size(Width + 1, Height + 1);
            _bufferedGraphics = _bufferContext.Allocate(CreateGraphics(), ClientRectangle);
        }

        private int GetPositionOfRtbLine(int lineNumber)
        {
            /* This method keeps the painted text aligned with the text in the corisponding
             * textbox perfectly. GetFirstCharIndexFromLine will yeild -1 if line not
             * present. GetPositionFromCharIndex will yeild an empty point to char index -1.
             * To explicitly say that line is not present return -1. */
            var index = _richTextBox.GetFirstCharIndexFromLine(lineNumber);
            var pos = _richTextBox.GetPositionFromCharIndex(index);
            return index.Equals(-1) ? -1 : pos.Y;
        }

        private void SetFontHeight()
        {
            /* Shrink the font for minor compensation */
            Font = new Font(_richTextBox.Font.FontFamily, _richTextBox.Font.Size -
                                                          FontModifier, _richTextBox.Font.Style);
            _fontHeight = _bufferedGraphics.Graphics.MeasureString("123ABC", Font).Height;
        }

        private void SetControlWidth()
        {
            /* Make the line numbers virtually invisble when no lines present */
            if (_richTextBox.Lines.Length.Equals(0) && _hideWhenNoLines)
            {
                Width = 0;
            }
            else
            {
                Width = WidthOfWidestLineNumber + _numPadding*2;
            }
            Invalidate(false);
        }

        /* Event handlers */
        private void RichTextBoxFontChanged(object sender, EventArgs e)
        {
            SetFontHeight();
            SetControlWidth();
        }

        private void RichTextBoxTextChanged(object sender, EventArgs e)
        {
            /* If word wrap is enabled do not check for line changes as new lines
             * from word wrapping will not raise the line changed event\
             * Last line count is always equal to current when words are wrapped */
            if (_richTextBox.WordWrap || !_lastLineCount.Equals(_richTextBox.Lines.Length))
            {
                SetControlWidth();
            }
            _lastLineCount = _richTextBox.Lines.Length;
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            _fontBrush = new SolidBrush(ForeColor);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateBackBuffer();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            _bufferedGraphics.Graphics.Clear(BackColor);
            var firstIndex = _richTextBox.GetCharIndexFromPosition(Point.Empty);
            var firstLine = _richTextBox.GetLineFromCharIndex(firstIndex);
            var bottomLeft = new Point(0, ClientRectangle.Height);
            var lastIndex = _richTextBox.GetCharIndexFromPosition(bottomLeft);
            var lastLine = _richTextBox.GetLineFromCharIndex(lastIndex);
            for (var i = firstLine; i <= lastLine + 1; i++)
            {
                var charYPos = GetPositionOfRtbLine(i);
                if (charYPos.Equals(-1)) continue;
                float yPos = GetPositionOfRtbLine(i) + DrawingOffset;
                if (_style.Equals(LineNumberStyle.OffsetColors))
                {
                    if (i%2 == 0)
                    {
                        _bufferedGraphics.Graphics.FillRectangle(_offsetBrush, 0, yPos, Width,
                                                                 _fontHeight*FontModifier*10);
                    }
                }
                else if (_style.Equals(LineNumberStyle.Boxed))
                {
                    var endPos = new PointF(Width, yPos + _fontHeight - DrawingOffset*3);
                    var startPos = new PointF(0, yPos + _fontHeight - DrawingOffset*3);
                    _bufferedGraphics.Graphics.DrawLine(_penBoxedLine, startPos, endPos);
                }
                var stringPos = new PointF(_numPadding, yPos);
                var line = (i + 1).ToString(CultureInfo.InvariantCulture);
                /* i + 1 to start the line numbers at 1 instead of 0 */
                _bufferedGraphics.Graphics.DrawString(line, Font, _fontBrush, stringPos);
            }
            _bufferedGraphics.Render(pevent.Graphics);
        }

        private void RichTextBoxVScroll(object sender, EventArgs e)
        {
            /* Decrease the paint calls by one half when there is more than 3000 lines */
            if (_richTextBox.Lines.Length > 3000 && _speedBump)
            {
                _speedBump = !_speedBump;
                return;
            }
            Invalidate(false);
        }

        /* Private properties */
        private int NextLineDown
        {
            get
            {
                var yPos = _richTextBox.ClientSize.Height + (int) (_fontHeight*ScrollSpeed + 0.5f);
                var topPos = new Point(0, yPos);
                var index = _richTextBox.GetCharIndexFromPosition(topPos);
                return _richTextBox.GetLineFromCharIndex(index);
            }
        }

        private int NextLineUp
        {
            get
            {
                var topPos = new Point(0, (int) (_fontHeight*(ScrollSpeed*-1) + -0.5f));
                var index = _richTextBox.GetCharIndexFromPosition(topPos);
                return _richTextBox.GetLineFromCharIndex(index);
            }
        }

        private int WidthOfWidestLineNumber
        {
            get
            {
                if (_bufferedGraphics.Graphics != null)
                {
                    var strNumber = (_richTextBox.Lines.Length).ToString(CultureInfo.InvariantCulture);
                    var size = _bufferedGraphics.Graphics.MeasureString(strNumber, _richTextBox.Font);
                    return (int) (size.Width + 0.5);
                }
                return 1;
            }
        }
    }
}