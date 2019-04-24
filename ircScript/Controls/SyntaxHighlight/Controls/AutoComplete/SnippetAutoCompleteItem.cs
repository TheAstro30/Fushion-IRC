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

namespace ircScript.Controls.SyntaxHighlight.Controls.AutoComplete
{
    public sealed class SnippetAutoCompleteItem : AutoCompleteItem
    {
        public SnippetAutoCompleteItem(string snippet)
        {
            Text = snippet.Replace("\r", "");
            ToolTipTitle = "Code snippet:";
            ToolTipText = Text;
        }

        public override string ToString()
        {
            return MenuText ?? Text.Replace("\n", " ").Replace("^", "");
        }

        public override string GetTextForReplace()
        {
            return Text;
        }

        public override void OnSelected(AutoCompleteMenu popupMenu, SelectedEventArgs e)
        {
            e.Tb.BeginUpdate();
            e.Tb.Selection.BeginUpdate();
            /* Remember places */
            var p1 = popupMenu.Fragment.Start;
            var p2 = e.Tb.Selection.Start;
            /* Do auto indent */
            if (e.Tb.AutoIndent)
            {
                for (var iLine = p1.Line + 1; iLine <= p2.Line; iLine++)
                {
                    e.Tb.Selection.Start = new Place(0, iLine);
                    e.Tb.DoAutoIndent(iLine);
                }
            }
            e.Tb.Selection.Start = p1;
            /* Move caret position right and find char ^ */
            while (e.Tb.Selection.CharBeforeStart != '^')
            {
                if (!e.Tb.Selection.GoRightThroughFolded())
                {
                    break;
                }
            }
            /* Remove char ^ */
            e.Tb.Selection.GoLeft(true);
            e.Tb.InsertText("");
            e.Tb.Selection.EndUpdate();
            e.Tb.EndUpdate();
        }

        public override CompareResult Compare(string fragmentText)
        {
            if (Text.StartsWith(fragmentText, StringComparison.InvariantCultureIgnoreCase) && Text != fragmentText)
            {
                return CompareResult.Visible;
            }
            return CompareResult.Hidden;
        }
    }
}
