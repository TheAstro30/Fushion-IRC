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
using System.Windows.Forms;
using ircScript.Controls.SyntaxHighlight.Helpers;

namespace ircScript.Controls.SyntaxHighlight.TextBoxEventArgs
{
    public class ToolTipNeededEventArgs : EventArgs
    {
        public Place Place { get; private set; }
        public string HoveredWord { get; private set; }
        public string ToolTipTitle { get; set; }
        public string ToolTipText { get; set; }
        public ToolTipIcon ToolTipIcon { get; set; }

        public ToolTipNeededEventArgs(Place place, string hoveredWord)
        {
            HoveredWord = hoveredWord;
            Place = place;
        }
    }
}
