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

namespace ircScript.Controls.SyntaxHighlight.Commands
{
    public class MultiRangeCommand : UndoableCommand
    {
        private readonly UndoableCommand _cmd;
        private readonly List<UndoableCommand> _commandsByRanges = new List<UndoableCommand>();
        private readonly Range _range;

        public MultiRangeCommand(UndoableCommand command) : base(command.TextSource)
        {
            _cmd = command;
            _range = TextSource.CurrentTextBox.Selection.Clone();
        }

        public override void Execute()
        {
            _commandsByRanges.Clear();
            var prevSelection = _range.Clone();
            var iChar = -1;
            var iStartLine = prevSelection.Start.Line;
            var iEndLine = prevSelection.End.Line;
            TextSource.CurrentTextBox.Selection.ColumnSelectionMode = false;
            TextSource.CurrentTextBox.Selection.BeginUpdate();
            TextSource.CurrentTextBox.BeginUpdate();
            TextSource.CurrentTextBox.AllowInsertRemoveLines = false;
            try
            {
                if (_cmd is InsertTextCommand)
                {
                    ExecuteInsertTextCommand(ref iChar, (_cmd as InsertTextCommand).InsertedText);
                }
                else if (_cmd is InsertCharCommand && (_cmd as InsertCharCommand).C != '\x0' &&
                         (_cmd as InsertCharCommand).C != '\b') /* If not DEL or BACKSPACE */
                {
                    ExecuteInsertTextCommand(ref iChar, (_cmd as InsertCharCommand).C.ToString());
                }
                else
                {
                    ExecuteCommand(ref iChar);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                /* Empty */
            }
            finally
            {
                TextSource.CurrentTextBox.AllowInsertRemoveLines = true;
                TextSource.CurrentTextBox.EndUpdate();
                TextSource.CurrentTextBox.Selection = _range;
                if (iChar >= 0)
                {
                    TextSource.CurrentTextBox.Selection.Start = new Place(iChar, iStartLine);
                    TextSource.CurrentTextBox.Selection.End = new Place(iChar, iEndLine);
                }
                TextSource.CurrentTextBox.Selection.ColumnSelectionMode = true;
                TextSource.CurrentTextBox.Selection.EndUpdate();
            }
        }

        private void ExecuteInsertTextCommand(ref int iChar, string text)
        {
            var lines = text.Split('\n');
            var iLine = 0;
            foreach (var r in _range.GetSubRanges(true))
            {
                var line = TextSource.CurrentTextBox[r.Start.Line];
                var lineIsEmpty = r.End < r.Start && line.StartSpacesCount == line.Count;
                if (!lineIsEmpty)
                {
                    var insertedText = lines[iLine%lines.Length];
                    if (r.End < r.Start && insertedText != "")
                    {
                        /* Add forwarding spaces */
                        insertedText = new string(' ', r.Start.Char - r.End.Char) + insertedText;
                        r.Start = r.End;
                    }
                    TextSource.CurrentTextBox.Selection = r;
                    var c = new InsertTextCommand(TextSource, insertedText);
                    c.Execute();
                    if (TextSource.CurrentTextBox.Selection.End.Char > iChar)
                    {
                        iChar = TextSource.CurrentTextBox.Selection.End.Char;
                    }
                    _commandsByRanges.Add(c);
                }
                iLine++;
            }
        }

        private void ExecuteCommand(ref int iChar)
        {
            foreach (var r in _range.GetSubRanges(false))
            {
                TextSource.CurrentTextBox.Selection = r;
                var c = _cmd.Clone();
                c.Execute();
                if (TextSource.CurrentTextBox.Selection.End.Char > iChar)
                {
                    iChar = TextSource.CurrentTextBox.Selection.End.Char;
                }
                _commandsByRanges.Add(c);
            }
        }

        public override void Undo()
        {
            TextSource.CurrentTextBox.BeginUpdate();
            TextSource.CurrentTextBox.Selection.BeginUpdate();
            try
            {
                for (var i = _commandsByRanges.Count - 1; i >= 0; i--)
                {
                    _commandsByRanges[i].Undo();
                }
            }
            finally
            {
                TextSource.CurrentTextBox.Selection.EndUpdate();
                TextSource.CurrentTextBox.EndUpdate();
            }
            TextSource.CurrentTextBox.Selection = _range.Clone();
            TextSource.CurrentTextBox.OnTextChanged(_range);
            TextSource.CurrentTextBox.OnSelectionChanged();
            TextSource.CurrentTextBox.Selection.ColumnSelectionMode = true;
        }

        public override UndoableCommand Clone()
        {
            throw new NotImplementedException();
        }
    }
}