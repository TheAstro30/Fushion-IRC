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
using System.Collections.Generic;
using System;
using ircScript.Controls.SyntaxHighlight.Styles;

namespace ircScript.Controls.SyntaxHighlight.Highlight.Descriptor
{
    public class SyntaxDescriptor: IDisposable
    {
        public char LeftBracket = '(';
        public char RightBracket = ')';
        public char LeftBracket2 = '{';
        public char RightBracket2 = '}';

        public BracketsHighlightStrategy BracketsHighlightStrategy = BracketsHighlightStrategy.Strategy2;
        public readonly List<Style> Styles = new List<Style>();
        public readonly List<RuleDesc> Rules = new List<RuleDesc>();
        public readonly List<FoldingDesc> Foldings = new List<FoldingDesc>();

        public void Dispose()
        {
            foreach (var style in Styles)
            {
                style.Dispose();
            }
        }
    }
}
