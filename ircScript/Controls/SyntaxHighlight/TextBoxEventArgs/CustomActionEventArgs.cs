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
using ircScript.Controls.SyntaxHighlight.Forms.Hotkeys;

namespace ircScript.Controls.SyntaxHighlight.TextBoxEventArgs
{
    public class CustomActionEventArgs : EventArgs
    {
        public FctbAction Action { get; private set; }

        public CustomActionEventArgs(FctbAction action)
        {
            Action = action;
        }
    }
}
