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
    public class ReplaceMultipleTextCommand : UndoableCommand
    {
        public class ReplaceRange
        {
            public Range ReplacedRange { get; set; }
            public String ReplaceText { get; set; }
        }

        private readonly List<string> _prevText = new List<string>();
        private readonly List<ReplaceRange> _ranges;

        public ReplaceMultipleTextCommand(TextSource textSource, List<ReplaceRange> ranges) : base(textSource)
        {
            /* Sort ranges by place */
            ranges.Sort((r1, r2) =>
                            {
                                if (r1.ReplacedRange.Start.Line == r2.ReplacedRange.Start.Line)
                                    return r1.ReplacedRange.Start.Char.CompareTo(r2.ReplacedRange.Start.Char);
                                return r1.ReplacedRange.Start.Line.CompareTo(r2.ReplacedRange.Start.Line);
                            });
            _ranges = ranges;
            LastSel = Sel = new RangeInfo(textSource.CurrentTextBox.Selection);
        }

        public override void Undo()
        {
            var tb = TextSource.CurrentTextBox;
            TextSource.OnTextChanging();
            tb.Selection.BeginUpdate();
            for (var i = 0; i < _ranges.Count; i++)
            {
                tb.Selection.Start = _ranges[i].ReplacedRange.Start;
                for (var j = 0; j < _ranges[i].ReplaceText.Length; j++)
                {
                    tb.Selection.GoRight(true);
                }
                ClearSelectedCommand.ClearSelected(TextSource);
                var prevTextIndex = _ranges.Count - 1 - i;
                InsertTextCommand.InsertText(_prevText[prevTextIndex], TextSource);
                TextSource.OnTextChanged(_ranges[i].ReplacedRange.Start.Line, _ranges[i].ReplacedRange.Start.Line);
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
            for (var i = _ranges.Count - 1; i >= 0; i--)
            {
                tb.Selection.Start = _ranges[i].ReplacedRange.Start;
                tb.Selection.End = _ranges[i].ReplacedRange.End;
                _prevText.Add(tb.Selection.Text);
                ClearSelectedCommand.ClearSelected(TextSource);
                InsertTextCommand.InsertText(_ranges[i].ReplaceText, TextSource);
                TextSource.OnTextChanged(_ranges[i].ReplacedRange.Start.Line, _ranges[i].ReplacedRange.End.Line);
            }
            tb.Selection.EndUpdate();
            TextSource.NeedRecalc(new TextSource.TextChangedEventArgs(0, 1));
            LastSel = new RangeInfo(tb.Selection);
        }

        public override UndoableCommand Clone()
        {
            return new ReplaceMultipleTextCommand(TextSource, new List<ReplaceRange>(_ranges));
        }
    }
}