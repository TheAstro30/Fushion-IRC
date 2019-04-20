/* Syntax highlighter - by Uriel Guy
 * Original version 2005
 * This version 2019 - Jason James Newland
 */
using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ircScript.Controls.SyntaxHightlight.Helpers;

namespace ircScript.Controls.SyntaxHightlight.BaseControl
{
    public class SyntaxHighlight : RichTextBox
    {
        private class UndoRedoInfo
        {
            public readonly int CursorLocation;
            public readonly Win32.Point ScrollPos;
            public readonly string Text;

            public UndoRedoInfo(string text, Win32.Point scrollPos, int cursorLoc)
            {
                Text = text;
                ScrollPos = scrollPos;
                CursorLocation = cursorLoc;
            }
        }

        private readonly Stack _redoStack = new Stack();
        private readonly ArrayList _undoList = new ArrayList();

        private bool _isUndo;
        private UndoRedoInfo _lastInfo = new UndoRedoInfo("", new Win32.Point(), 0);
        private bool _parsing;
        private bool _bufferSet;        

        public SyntaxHighlight()
        {           
            HighlightDescriptors = new HighLightDescriptorCollection();
            Seperators = new SeperaratorCollection();
            MaxUndoRedoSteps = 50;
        }

        /* Properties */
        public bool CaseSensitive { get; set; } /* Determines if token recognition is case sensitive. */

        public int MaxUndoRedoSteps { get; set; }

        public SeperaratorCollection Seperators { get; private set; }

        public HighLightDescriptorCollection HighlightDescriptors { get; private set; }
        
        public new bool CanUndo
        {
            get { return _bufferSet ? _undoList.Count > 1 : _undoList.Count > 0; }
        }

        public new bool CanRedo
        {
            get { return _redoStack.Count > 0; }
        }

        public bool CanCopy
        {
            get { return SelectionLength > 0; }
        }

        public new string[] Lines
        {
            get { return base.Lines; }
            set
            {
                /* We override the default behaviour of Lines.Set to prevent undo from removing what was
                 * originally added */
                _bufferSet = true;
                base.Lines = value;
            }
        }

        /* Overrides */
        protected override void OnTextChanged(EventArgs e)
        {
            if (_parsing)
            {
                return;
            }
            _parsing = true;
            Win32.LockWindowUpdate(Handle);
            base.OnTextChanged(e);
            if (!_isUndo)
            {
                _redoStack.Clear();
                _undoList.Insert(0, _lastInfo);
                LimitUndo();
                _lastInfo = new UndoRedoInfo(Text, GetScrollPos(), SelectionStart);
            }
            /* Save scroll bar an cursor position, changeing the RTF moves the cursor and scrollbars to top position */
            var scrollPos = GetScrollPos();
            var cursorLoc = SelectionStart;
            /* Created with an estimate of how big the stringbuilder has to be... */
            var sb = new StringBuilder((int) (Text.Length*1.5 + 150));
            /* Add RTF header */
            sb.Append(@"{\rtf1\fbidis\ansi\ansicpg1255\deff0\deflang1037{\fonttbl{");
            /* Font table creation */
            var fontCounter = 0;
            var fonts = new Hashtable();
            AddFontToTable(sb, Font, ref fontCounter, fonts);
            foreach (HighlightDescriptor hd in HighlightDescriptors)
            {
                if ((hd.Font != null) && !fonts.ContainsKey(hd.Font.Name))
                {
                    AddFontToTable(sb, hd.Font, ref fontCounter, fonts);
                }
            }
            sb.Append("}\n");
            /* ColorTable */
            sb.Append(@"{\colortbl ;");
            var colors = new Hashtable();
            var colorCounter = 1;
            AddColorToTable(sb, ForeColor, ref colorCounter, colors);
            AddColorToTable(sb, BackColor, ref colorCounter, colors);
            foreach (HighlightDescriptor hd in HighlightDescriptors)
            {
                if (!colors.ContainsKey(hd.Color))
                {
                    AddColorToTable(sb, hd.Color, ref colorCounter, colors);
                }
            }
            /* Parsing text */
            sb.Append("}\n").Append(@"\viewkind4\uc1\pard\ltrpar");
            SetDefaultSettings(sb, colors, fonts);
            var sperators = Seperators.GetAsCharArray();
            /* Replacing "\" to "\\" for RTF... */
            var lines = Text.Replace("\\", "\\\\").Replace("{", "\\{").Replace("}", "\\}").Split('\n');
            for (var lineCounter = 0; lineCounter < lines.Length; lineCounter++)
            {
                if (lineCounter != 0)
                {
                    AddNewLine(sb);
                }
                var line = lines[lineCounter];
                var tokens = CaseSensitive ? line.Split(sperators) : line.ToUpper().Split(sperators);
                if (tokens.Length == 0)
                {
                    sb.Append(line);
                    AddNewLine(sb);
                    continue;
                }
                var tokenCounter = 0;
                for (var i = 0; i < line.Length;)
                {
                    var curChar = line[i];
                    if (Seperators.Contains(curChar))
                    {
                        sb.Append(curChar);
                        i++;
                    }
                    else
                    {
                        var curToken = tokens[tokenCounter++];
                        var bAddToken = true;
                        foreach (HighlightDescriptor hd in HighlightDescriptors)
                        {
                            var compareStr = CaseSensitive ? hd.Token : hd.Token.ToUpper();
                            var match = false;
                            /* Check if the highlight descriptor matches the current toker according to the DescriptoRecognision property. */
                            switch (hd.DescriptorRecognition)
                            {
                                case DescriptorRecognition.WholeWord:
                                    if (curToken == compareStr)
                                    {
                                        match = true;
                                    }
                                    break;

                                case DescriptorRecognition.StartsWith:
                                    if (curToken.StartsWith(compareStr))
                                    {
                                        match = true;
                                    }
                                    break;

                                case DescriptorRecognition.Contains:
                                    if (curToken.IndexOf(compareStr) != -1)
                                    {                                        
                                        match = true;
                                    }
                                    break;
                            }
                            if (!match)
                            {
                                /* If this token doesn't match chech the next one. */
                                continue;
                            }
                            /* Printing this token will be handled by the inner code, don't apply default settings... */
                            bAddToken = false;
                            /* Set colors to current descriptor settings. */
                            SetDescriptorSettings(sb, hd, colors, fonts);
                            /* Print text affected by this descriptor. */
                            switch (hd.DescriptorType)
                            {
                                case DescriptorType.Word:
                                    sb.Append(line.Substring(i, curToken.Length));
                                    SetDefaultSettings(sb, colors, fonts);
                                    i += curToken.Length;
                                    break;

                                case DescriptorType.ToEol:
                                    sb.Append(line.Remove(0, i));
                                    i = line.Length;
                                    SetDefaultSettings(sb, colors, fonts);
                                    break;

                                case DescriptorType.ToCloseToken:
                                    while ((line.IndexOf(hd.CloseToken, i) == -1) && (lineCounter < lines.Length))
                                    {
                                        sb.Append(line.Remove(0, i));
                                        lineCounter++;
                                        if (lineCounter < lines.Length)
                                        {
                                            AddNewLine(sb);
                                            line = lines[lineCounter];
                                            i = 0;
                                        }
                                        else
                                        {
                                            i = line.Length;
                                        }
                                    }
                                    if (line.IndexOf(hd.CloseToken, i) != -1)
                                    {
                                        sb.Append(line.Substring(i,
                                                                 line.IndexOf(hd.CloseToken, i) + hd.CloseToken.Length -
                                                                 i));
                                        line = line.Remove(0, line.IndexOf(hd.CloseToken, i) + hd.CloseToken.Length);
                                        tokenCounter = 0;
                                        tokens = CaseSensitive ? line.Split(sperators) : line.ToUpper().Split(sperators);
                                        SetDefaultSettings(sb, colors, fonts);
                                        i = 0;
                                    }
                                    break;
                            }
                            break;
                        }
                        if (bAddToken)
                        {
                            /* Print text with default settings... */
                            sb.Append(line.Substring(i, curToken.Length));
                            i += curToken.Length;
                        }
                    }
                }
            }
            Rtf = sb.ToString();
            /* Restore cursor and scrollbars location. */
            SelectionStart = cursorLoc;
            _parsing = false;
            SetScrollPos(scrollPos);
            Win32.LockWindowUpdate((IntPtr) 0);
            Invalidate();
        }

        protected override void OnVScroll(EventArgs e)
        {
            if (_parsing)
            {
                return;
            }
            base.OnVScroll(e);
        }

        /* Window procedure */
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Win32.WmPaint:
                    /* Don't draw the control while parsing to avoid flicker. */
                    if (_parsing)
                    {
                        return;
                    }
                    break;

                case Win32.WmKeydown:
                    if (((Keys) (int) m.WParam == Keys.Z) && ((Win32.GetKeyState(Win32.VkControl) & Win32.KsKeydown) != 0))
                    {
                        Undo();
                        return;
                    }
                    if (((Keys) (int) m.WParam == Keys.Y) && ((Win32.GetKeyState(Win32.VkControl) & Win32.KsKeydown) != 0))
                    {
                        Redo();
                        return;
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        /* Undo/redo */
        private void LimitUndo()
        {
            while (_undoList.Count > MaxUndoRedoSteps)
            {
                _undoList.RemoveAt(MaxUndoRedoSteps);
            }
        }

        public new void Undo()
        {
            if (!CanUndo)
            {
                return;
            }
            _isUndo = true;
            _redoStack.Push(new UndoRedoInfo(Text, GetScrollPos(), SelectionStart));
            var info = (UndoRedoInfo) _undoList[0];
            _undoList.RemoveAt(0);
            Text = info.Text;
            SelectionStart = info.CursorLocation;
            SetScrollPos(info.ScrollPos);
            _lastInfo = info;
            _isUndo = false;
        }

        public new void Redo()
        {
            if (!CanRedo)
            {
                return;
            }
            _isUndo = true;
            _undoList.Insert(0, new UndoRedoInfo(Text, GetScrollPos(), SelectionStart));
            LimitUndo();
            var info = (UndoRedoInfo) _redoStack.Pop();
            Text = info.Text;
            SelectionStart = info.CursorLocation;
            SetScrollPos(info.ScrollPos);
            _isUndo = false;
        }

        /* RTF building helper functions */
        private void SetDefaultSettings(StringBuilder sb, Hashtable colors, Hashtable fonts)
        {
            SetColor(sb, ForeColor, colors);
            SetFont(sb, Font, fonts);
            SetFontSize(sb, (int) Font.Size);
            EndTags(sb);
        }

        private static void SetDescriptorSettings(StringBuilder sb, HighlightDescriptor hd, Hashtable colors, Hashtable fonts)
        {
            SetColor(sb, hd.Color, colors);
            if (hd.Font != null)
            {
                SetFont(sb, hd.Font, fonts);
                SetFontSize(sb, (int) hd.Font.Size);
            }
            EndTags(sb);
        }

        private static void SetColor(StringBuilder sb, Color color, Hashtable colors)
        {
            sb.Append(@"\cf").Append(colors[color]);
        }

        //private void SetBackColor(StringBuilder sb, Color color, IDictionary colors)
        //{
        //    sb.Append(@"\cb").Append(colors[color]);
        //}

        private static void SetFont(StringBuilder sb, Font font, Hashtable fonts)
        {
            if (font == null) return;
            sb.Append(@"\f").Append(fonts[font.Name]);
        }

        private static void SetFontSize(StringBuilder sb, int size)
        {
            sb.Append(@"\fs").Append(size*2);
        }

        private static void AddNewLine(StringBuilder sb)
        {
            sb.Append("\\par\n");
        }

        private static void EndTags(StringBuilder sb)
        {
            sb.Append(' ');
        }

        private static void AddFontToTable(StringBuilder sb, Font font, ref int counter, Hashtable fonts)
        {
            sb.Append(@"\f").Append(counter).Append(@"\fnil\fcharset0").Append(font.Name).Append(";}");
            fonts.Add(font.Name, counter++);
        }

        private static void AddColorToTable(StringBuilder sb, Color color, ref int counter, Hashtable colors)
        {
            sb.Append(@"\red").Append(color.R).Append(@"\green").Append(color.G).Append(@"\blue")
                .Append(color.B).Append(";");
            colors.Add(color, counter++);
        }

        private unsafe Win32.Point GetScrollPos()
        {
            var res = new Win32.Point();
            var ptr = new IntPtr(&res);
            Win32.SendMessage(Handle, Win32.EmGetscrollpos, 0, ptr);
            return res;
        }

        private unsafe void SetScrollPos(Win32.Point point)
        {
            var ptr = new IntPtr(&point);
            Win32.SendMessage(Handle, Win32.EmSetscrollpos, 0, ptr);
        }
    }
}