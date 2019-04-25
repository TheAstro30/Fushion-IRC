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

namespace ircScript.Controls.SyntaxHighlight.TextBoxEventArgs
{
    public class AutoIndentEventArgs : EventArgs
    {
        public int Line { get; internal set; }
        public int TabLength { get; internal set; }
        public string LineText { get; internal set; }
        public string PrevLineText { get; internal set; }
        public int Shift { get; set; }
        public int ShiftNextLines { get; set; }
        public int AbsoluteIndentation { get; set; }

        public AutoIndentEventArgs(int iLine, string lineText, string prevLineText, int tabLength, int currentIndentation)
        {
            Line = iLine;
            LineText = lineText;
            PrevLineText = prevLineText;
            TabLength = tabLength;
            AbsoluteIndentation = currentIndentation;
        }
    }
}
