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
using ircScript.Controls.SyntaxHighlight.Helpers.TextSource;

namespace ircScript.Controls.SyntaxHighlight.Commands
{
    public abstract class UndoableCommand : Command
    {
        internal bool AutoUndo;
        internal RangeInfo LastSel;
        internal RangeInfo Sel;

        protected UndoableCommand(TextSource textSource)
        {
            TextSource = textSource;
            Sel = new RangeInfo(textSource.CurrentTextBox.Selection);
        }

        public virtual void Undo()
        {
            OnTextChanged(true);
        }

        public override void Execute()
        {
            LastSel = new RangeInfo(TextSource.CurrentTextBox.Selection);
            OnTextChanged(false);
        }

        protected virtual void OnTextChanged(bool invert)
        {
            var b = Sel.Start.Line < LastSel.Start.Line;
            if (invert)
            {
                TextSource.OnTextChanged(Sel.Start.Line, b ? Sel.Start.Line : LastSel.Start.Line);
            }
            else
            {
                TextSource.OnTextChanged(b ? Sel.Start.Line : LastSel.Start.Line, LastSel.Start.Line);
            }
        }

        public abstract UndoableCommand Clone();
    }
}