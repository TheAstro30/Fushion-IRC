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

namespace ircScript.Controls.SyntaxHighlight.Helpers.TextSource
{
    public class LinePushedEventArgs : EventArgs
    {
        public string SourceLineText { get; private set; }
        public int DisplayedLineIndex { get; private set; }
        public string DisplayedLineText { get; private set; }
        public string SavedText { get; set; }
        
        public LinePushedEventArgs(string sourceLineText, int displayedLineIndex, string displayedLineText)
        {
            SourceLineText = sourceLineText;
            DisplayedLineIndex = displayedLineIndex;
            DisplayedLineText = displayedLineText;
            SavedText = displayedLineText;
        }
    }
}
