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
    public class SelectCommand : UndoableCommand
    {
        public SelectCommand(TextSource textSource) : base(textSource)
        {
            /* Empty */
        }

        public override void Execute()
        {
            /* Remember selection */
            LastSel = new RangeInfo(TextSource.CurrentTextBox.Selection);
        }

        public override void Undo()
        {
            /* Restore selection */
            TextSource.CurrentTextBox.Selection = new Range(TextSource.CurrentTextBox, LastSel.Start, LastSel.End);
        }

        public override UndoableCommand Clone()
        {
            var result = new SelectCommand(TextSource);
            if (LastSel != null)
            {
                result.LastSel = new RangeInfo(new Range(TextSource.CurrentTextBox, LastSel.Start, LastSel.End));
            }
            return result;
        }
    }
}
