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
namespace ircScript.Controls.SyntaxHighlight.Helpers.TextRange
{
    public struct RangeRect
    {
        public RangeRect(int startLine, int startChar, int endLine, int endChar)
        {
            StartLine = startLine;
            StartChar = startChar;
            EndLine = endLine;
            EndChar = endChar;
        }

        public int StartLine;
        public int StartChar;
        public int EndLine;
        public int EndChar;
    }
}
