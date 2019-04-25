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
using System.Collections.Generic;

namespace ircScript.Controls.SyntaxHighlight.TextBoxEventArgs
{
    public class LineRemovedEventArgs : EventArgs
    {
        public int Index { get; private set; }
        public int Count { get; private set; }
        public List<int> RemovedLineUniqueIds { get; private set; }

        public LineRemovedEventArgs(int index, int count, List<int> removedLineIds)
        {
            Index = index;
            Count = count;
            RemovedLineUniqueIds = removedLineIds;
        }
    }
}
