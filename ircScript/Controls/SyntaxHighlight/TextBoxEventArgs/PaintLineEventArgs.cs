﻿//
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
using System.Drawing;
using System.Windows.Forms;

namespace ircScript.Controls.SyntaxHighlight.TextBoxEventArgs
{
    public class PaintLineEventArgs : PaintEventArgs
    {
        public int LineIndex { get; private set; }
        public Rectangle LineRect { get; private set; }

        public PaintLineEventArgs(int iLine, Rectangle rect, Graphics gr, Rectangle clipRect) : base(gr, clipRect)
        {
            LineIndex = iLine;
            LineRect = rect;
        }
    }
}
