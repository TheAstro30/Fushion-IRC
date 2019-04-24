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

using ircScript.Controls.SyntaxHighlight.Helpers;
using ircScript.Controls.SyntaxHighlight.Helpers.TextSource;

namespace ircScript.Controls.SyntaxHighlight.Commands
{
    public class InsertTextCommand : UndoableCommand
    {
        public string InsertedText;

        public InsertTextCommand(TextSource textSource, string insertedText) : base(textSource)
        {
            InsertedText = insertedText;
        }

        public override void Undo()
        {
            TextSource.CurrentTextBox.Selection.Start = Sel.Start;
            TextSource.CurrentTextBox.Selection.End = LastSel.Start;
            TextSource.OnTextChanging();
            ClearSelectedCommand.ClearSelected(TextSource);
            base.Undo();
        }

        public override void Execute()
        {
            TextSource.OnTextChanging(ref InsertedText);
            InsertText(InsertedText, TextSource);
            base.Execute();
        }

        internal static void InsertText(string insertedText, TextSource ts)
        {
            var tb = ts.CurrentTextBox;
            try
            {
                tb.Selection.BeginUpdate();
                var cc = '\x0';
                if (ts.Count == 0)
                {
                    InsertCharCommand.InsertLine(ts);
                    tb.Selection.Start = Place.Empty;
                }
                tb.ExpandBlock(tb.Selection.Start.Line);
                var len = insertedText.Length;
                for (var i = 0; i < len; i++)
                {
                    var c = insertedText[i];
                    if (c == '\r' && (i >= len - 1 || insertedText[i + 1] != '\n'))
                    {
                        InsertCharCommand.InsertChar('\n', ref cc, ts);
                    }
                    else
                    {
                        InsertCharCommand.InsertChar(c, ref cc, ts);
                    }
                }
                ts.NeedRecalc(new TextSource.TextChangedEventArgs(0, 1));
            }
            finally
            {
                tb.Selection.EndUpdate();
            }
        }

        public override UndoableCommand Clone()
        {
            return new InsertTextCommand(TextSource, InsertedText);
        }
    }
}
