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

namespace ircScript.Controls.SyntaxHighlight.Controls.AutoComplete
{
    public class SelectingEventArgs : EventArgs
    {
        public AutoCompleteItem Item { get; internal set; }
        public bool Cancel { get; set; }
        public int SelectedIndex { get; set; }
        public bool Handled { get; set; }
    }
}
