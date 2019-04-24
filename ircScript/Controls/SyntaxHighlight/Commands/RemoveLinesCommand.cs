//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE.
//
//  License: GNU Lesser General Public License (LGPLv3)
//
//  Email: pavel_torgashov@ukr.net
//
//  Copyright (C) Pavel Torgashov, 2011-2016.
using System.Collections.Generic;
using ircScript.Controls.SyntaxHighlight.Helpers;
using ircScript.Controls.SyntaxHighlight.Helpers.TextSource;

namespace ircScript.Controls.SyntaxHighlight.Commands
{
    public class RemoveLinesCommand : UndoableCommand
    {
        private readonly List<int> _iLines;
        private readonly List<string> _prevText = new List<string>();

        public RemoveLinesCommand(TextSource textSource, List<int> iLines) : base(textSource)
        {
            /* Sort iLines */
            iLines.Sort();
            _iLines = iLines;
            LastSel = Sel = new RangeInfo(textSource.CurrentTextBox.Selection);
        }

        public override void Undo()
        {
            var tb = TextSource.CurrentTextBox;
            TextSource.OnTextChanging();
            tb.Selection.BeginUpdate();
            for (var i = 0; i < _iLines.Count; i++)
            {
                var iLine = _iLines[i];
                tb.Selection.Start = iLine < TextSource.Count ? new Place(0, iLine) : new Place(TextSource[TextSource.Count - 1].Count, TextSource.Count - 1);
                InsertCharCommand.InsertLine(TextSource);
                tb.Selection.Start = new Place(0, iLine);
                var text = _prevText[_prevText.Count - i - 1];
                InsertTextCommand.InsertText(text, TextSource);
                TextSource[iLine].IsChanged = true;
                if (iLine < TextSource.Count - 1)
                {
                    TextSource[iLine + 1].IsChanged = true;
                }
                else
                {
                    TextSource[iLine - 1].IsChanged = true;
                }
                if (text.Trim() != string.Empty)
                {
                    TextSource.OnTextChanged(iLine, iLine);
                }
            }
            tb.Selection.EndUpdate();
            TextSource.NeedRecalc(new TextSource.TextChangedEventArgs(0, 1));
        }

        public override void Execute()
        {
            var tb = TextSource.CurrentTextBox;
            _prevText.Clear();
            TextSource.OnTextChanging();
            tb.Selection.BeginUpdate();
            for (var i = _iLines.Count - 1; i >= 0; i--)
            {
                var iLine = _iLines[i];
                _prevText.Add(TextSource[iLine].Text); /* Backward */
                TextSource.RemoveLine(iLine);
            }
            tb.Selection.Start = new Place(0, 0);
            tb.Selection.EndUpdate();
            TextSource.NeedRecalc(new TextSource.TextChangedEventArgs(0, 1));
            LastSel = new RangeInfo(tb.Selection);
        }

        public override UndoableCommand Clone()
        {
            return new RemoveLinesCommand(TextSource, new List<int>(_iLines));
        }
    }
}