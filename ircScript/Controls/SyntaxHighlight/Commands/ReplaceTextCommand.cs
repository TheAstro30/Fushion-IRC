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
using System;
using System.Collections.Generic;
using ircScript.Controls.SyntaxHighlight.Helpers;
using ircScript.Controls.SyntaxHighlight.Helpers.TextSource;

namespace ircScript.Controls.SyntaxHighlight.Commands
{
    public class ReplaceTextCommand : UndoableCommand
    {
        private readonly List<string> _prevText = new List<string>();
        private readonly List<Range> _ranges;
        private string _insertedText;

        public ReplaceTextCommand(TextSource textSource, List<Range> ranges, string insertedText) : base(textSource)
        {
            /* Sort ranges by place */
            ranges.Sort(
                (r1, r2) =>
                r1.Start.Line == r2.Start.Line
                    ? r1.Start.Char.CompareTo(r2.Start.Char)
                    : r1.Start.Line.CompareTo(r2.Start.Line));
            _ranges = ranges;
            _insertedText = insertedText;
            LastSel = Sel = new RangeInfo(textSource.CurrentTextBox.Selection);
        }

        public override void Undo()
        {
            var tb = TextSource.CurrentTextBox;
            TextSource.OnTextChanging();
            tb.BeginUpdate();
            tb.Selection.BeginUpdate();
            for (var i = 0; i < _ranges.Count; i++)
            {
                tb.Selection.Start = _ranges[i].Start;
                for (var j = 0; j < _insertedText.Length; j++)
                {
                    tb.Selection.GoRight(true);
                } 
                ClearSelected(TextSource);
                InsertTextCommand.InsertText(_prevText[_prevText.Count - i - 1], TextSource);
            }
            tb.Selection.EndUpdate();
            tb.EndUpdate();
            if (_ranges.Count > 0)
            {
                TextSource.OnTextChanged(_ranges[0].Start.Line, _ranges[_ranges.Count - 1].End.Line);
            }
            TextSource.NeedRecalc(new TextSource.TextChangedEventArgs(0, 1));
        }

        public override void Execute()
        {
            var tb = TextSource.CurrentTextBox;
            _prevText.Clear();
            TextSource.OnTextChanging(ref _insertedText);
            tb.Selection.BeginUpdate();
            tb.BeginUpdate();
            for (var i = _ranges.Count - 1; i >= 0; i--)
            {
                tb.Selection.Start = _ranges[i].Start;
                tb.Selection.End = _ranges[i].End;
                _prevText.Add(tb.Selection.Text);
                ClearSelected(TextSource);
                if (_insertedText != "")
                {
                    InsertTextCommand.InsertText(_insertedText, TextSource);
                }
            }
            if (_ranges.Count > 0)
            {
                TextSource.OnTextChanged(_ranges[0].Start.Line, _ranges[_ranges.Count - 1].End.Line);
            }
            tb.EndUpdate();
            tb.Selection.EndUpdate();
            TextSource.NeedRecalc(new TextSource.TextChangedEventArgs(0, 1));
            LastSel = new RangeInfo(tb.Selection);
        }

        public override UndoableCommand Clone()
        {
            return new ReplaceTextCommand(TextSource, new List<Range>(_ranges), _insertedText);
        }

        internal static void ClearSelected(TextSource ts)
        {
            var tb = ts.CurrentTextBox;
            tb.Selection.Normalize();
            var start = tb.Selection.Start;
            var end = tb.Selection.End;
            var fromLine = Math.Min(end.Line, start.Line);
            var toLine = Math.Max(end.Line, start.Line);
            var fromChar = tb.Selection.FromX;
            var toChar = tb.Selection.ToX;
            if (fromLine < 0)
            {
                return;
            }
            if (fromLine == toLine)
            {
                ts[fromLine].RemoveRange(fromChar, toChar - fromChar);
            }
            else
            {
                ts[fromLine].RemoveRange(fromChar, ts[fromLine].Count - fromChar);
                ts[toLine].RemoveRange(0, toChar);
                ts.RemoveLine(fromLine + 1, toLine - fromLine - 1);
                InsertCharCommand.MergeLines(fromLine, ts);
            }
        }
    }
}