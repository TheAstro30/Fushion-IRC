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
using ircScript.Controls.SyntaxHighlight.Helpers.Lines;
using ircScript.Controls.SyntaxHighlight.Helpers.TextSource;
using Char = ircScript.Controls.SyntaxHighlight.Helpers.Char;

namespace ircScript.Controls.SyntaxHighlight.Commands
{
    public class InsertCharCommand : UndoableCommand
    {
        public char C;
        private char _deletedChar = '\x0';

        public InsertCharCommand(TextSource textSource, char c) : base(textSource)
        {
            C = c;
        }

        public override void Undo()
        {
            TextSource.OnTextChanging();
            switch (C)
            {
                case '\n':
                    MergeLines(Sel.Start.Line, TextSource);
                    break;

                case '\r':
                    break;

                case '\b':
                    TextSource.CurrentTextBox.Selection.Start = LastSel.Start;
                    var cc = '\x0';
                    if (_deletedChar != '\x0')
                    {
                        TextSource.CurrentTextBox.ExpandBlock(TextSource.CurrentTextBox.Selection.Start.Line);
                        InsertChar(_deletedChar, ref cc, TextSource);
                    }
                    break;

                case '\t':
                    TextSource.CurrentTextBox.ExpandBlock(Sel.Start.Line);
                    for (var i = Sel.FromX; i < LastSel.FromX; i++)
                    {
                        TextSource[Sel.Start.Line].RemoveAt(Sel.Start.Char);
                    }
                    TextSource.CurrentTextBox.Selection.Start = Sel.Start;
                    break;

                default:
                    TextSource.CurrentTextBox.ExpandBlock(Sel.Start.Line);
                    TextSource[Sel.Start.Line].RemoveAt(Sel.Start.Char);
                    TextSource.CurrentTextBox.Selection.Start = Sel.Start;
                    break;
            }
            TextSource.NeedRecalc(new TextSource.TextChangedEventArgs(Sel.Start.Line, Sel.Start.Line));
            base.Undo();
        }

        public override void Execute()
        {
            TextSource.CurrentTextBox.ExpandBlock(TextSource.CurrentTextBox.Selection.Start.Line);
            var s = C.ToString();
            TextSource.OnTextChanging(ref s);
            if (s.Length == 1)
            {
                C = s[0];
            }
            if (String.IsNullOrEmpty(s))
            {
                throw new ArgumentOutOfRangeException();
            }
            if (TextSource.Count == 0)
            {
                InsertLine(TextSource);
            }
            InsertChar(C, ref _deletedChar, TextSource);
            TextSource.NeedRecalc(new TextSource.TextChangedEventArgs(TextSource.CurrentTextBox.Selection.Start.Line,
                                                              TextSource.CurrentTextBox.Selection.Start.Line));
            base.Execute();
        }

        internal static void InsertChar(char c, ref char deletedChar, TextSource ts)
        {
            var tb = ts.CurrentTextBox;
            switch (c)
            {
                case '\n':
                    if (!ts.CurrentTextBox.AllowInsertRemoveLines)
                    {
                        throw new ArgumentOutOfRangeException("c", @"Cannot insert this character in ColumnRange mode");
                    }
                    if (ts.Count == 0)
                    {
                        InsertLine(ts);
                    }
                    InsertLine(ts);
                    break;

                case '\r':
                    break;

                case '\b': /* Backspace */
                    if (tb.Selection.Start.Char == 0 && tb.Selection.Start.Line == 0)
                    {
                        return;
                    }
                    if (tb.Selection.Start.Char == 0)
                    {
                        if (!ts.CurrentTextBox.AllowInsertRemoveLines)
                        {                          
                            throw new ArgumentOutOfRangeException("c", @"Cannot insert this character in ColumnRange mode");
                        }
                        if (tb.LineInfos[tb.Selection.Start.Line - 1].VisibleState != VisibleState.Visible)
                        {
                            tb.ExpandBlock(tb.Selection.Start.Line - 1);
                        }
                        deletedChar = '\n';
                        MergeLines(tb.Selection.Start.Line - 1, ts);
                    }
                    else
                    {
                        deletedChar = ts[tb.Selection.Start.Line][tb.Selection.Start.Char - 1].C;
                        ts[tb.Selection.Start.Line].RemoveAt(tb.Selection.Start.Char - 1);
                        tb.Selection.Start = new Place(tb.Selection.Start.Char - 1, tb.Selection.Start.Line);
                    }
                    break;

                case '\t':
                    var spaceCountNextTabStop = tb.TabLength - (tb.Selection.Start.Char%tb.TabLength);
                    if (spaceCountNextTabStop == 0)
                    {
                        spaceCountNextTabStop = tb.TabLength;
                    }
                    for (var i = 0; i < spaceCountNextTabStop; i++)
                    {
                        ts[tb.Selection.Start.Line].Insert(tb.Selection.Start.Char, new Char(' '));
                    }
                    tb.Selection.Start = new Place(tb.Selection.Start.Char + spaceCountNextTabStop,
                                                   tb.Selection.Start.Line);
                    break;

                default:
                    ts[tb.Selection.Start.Line].Insert(tb.Selection.Start.Char, new Char(c));
                    tb.Selection.Start = new Place(tb.Selection.Start.Char + 1, tb.Selection.Start.Line);
                    break;
            }
        }

        internal static void InsertLine(TextSource ts)
        {
            var tb = ts.CurrentTextBox;
            if (!tb.Multiline && tb.LinesCount > 0)
            {
                return;
            }
            if (ts.Count == 0)
            {
                ts.InsertLine(0, ts.CreateLine());
            }
            else
            {
                BreakLines(tb.Selection.Start.Line, tb.Selection.Start.Char, ts);
            }
            tb.Selection.Start = new Place(0, tb.Selection.Start.Line + 1);
            ts.NeedRecalc(new TextSource.TextChangedEventArgs(0, 1));
        }

        internal static void MergeLines(int i, TextSource ts)
        {
            var tb = ts.CurrentTextBox;
            if (i + 1 >= ts.Count)
            {
                return;
            }
            tb.ExpandBlock(i);
            tb.ExpandBlock(i + 1);
            var pos = ts[i].Count;
            if (ts[i + 1].Count == 0)
            {
                ts.RemoveLine(i + 1);
            }
            else
            {
                ts[i].AddRange(ts[i + 1]);
                ts.RemoveLine(i + 1);
            }
            tb.Selection.Start = new Place(pos, i);
            ts.NeedRecalc(new TextSource.TextChangedEventArgs(0, 1));
        }

        internal static void BreakLines(int iLine, int pos, TextSource ts)
        {
            var newLine = ts.CreateLine();
            for (var i = pos; i < ts[iLine].Count; i++)
            {
                newLine.Add(ts[iLine][i]);
            }
            ts[iLine].RemoveRange(pos, ts[iLine].Count - pos);
            ts.InsertLine(iLine + 1, newLine);
        }

        public override UndoableCommand Clone()
        {
            return new InsertCharCommand(TextSource, C);
        }
    }
}