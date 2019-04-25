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
using ircScript.Controls.SyntaxHighlight.Helpers.TextRange;

namespace ircScript.Controls.SyntaxHighlight.Commands
{
    internal class RangeInfo
    {
        public Place Start { get; set; }
        public Place End { get; set; }

        public RangeInfo(Range r)
        {
            Start = r.Start;
            End = r.End;
        }

        internal int FromX
        {
            get
            {
                if (End.Line < Start.Line)
                {
                    return End.Char;
                }
                return End.Line > Start.Line ? Start.Char : Math.Min(End.Char, Start.Char);
            }
        }
    }
}
