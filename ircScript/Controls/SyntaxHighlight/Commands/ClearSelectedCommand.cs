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
using ircScript.Controls.SyntaxHighlight.Helpers;
using ircScript.Controls.SyntaxHighlight.Helpers.TextSource;

namespace ircScript.Controls.SyntaxHighlight.Commands
{
    public class ClearSelectedCommand : UndoableCommand
    {
        private string _deletedText;

        public ClearSelectedCommand(TextSource textSource) : base(textSource)
        {
            /* Empty */
        }

        public override void Undo()
        {
            TextSource.CurrentTextBox.Selection.Start = new Place(Sel.FromX, Math.Min(Sel.Start.Line, Sel.End.Line));
            TextSource.OnTextChanging();
            InsertTextCommand.InsertText(_deletedText, TextSource);
            TextSource.OnTextChanged(Sel.Start.Line, Sel.End.Line);
            TextSource.CurrentTextBox.Selection.Start = Sel.Start;
            TextSource.CurrentTextBox.Selection.End = Sel.End;
        }

        public override void Execute()
        {
            var tb = TextSource.CurrentTextBox;
            string temp = null;
            TextSource.OnTextChanging(ref temp);
            if (temp == "")
            {
                throw new ArgumentOutOfRangeException();
            }
            _deletedText = tb.Selection.Text;
            ClearSelected(TextSource);
            LastSel = new RangeInfo(tb.Selection);
            TextSource.OnTextChanged(LastSel.Start.Line, LastSel.Start.Line);
        }

        internal static void ClearSelected(TextSource ts)
        {
            var tb = ts.CurrentTextBox;
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
            tb.Selection.Start = new Place(fromChar, fromLine);
            ts.NeedRecalc(new TextSource.TextChangedEventArgs(fromLine, toLine));
        }

        public override UndoableCommand Clone()
        {
            return new ClearSelectedCommand(TextSource);
        }
    }
}