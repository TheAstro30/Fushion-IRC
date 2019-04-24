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
using System.Text.RegularExpressions;
using ircScript.Controls.SyntaxHighlight.Styles;

namespace ircScript.Controls.SyntaxHighlight.Highlight.Descriptor
{
    public class RuleDesc
    {
        private Regex _regex;

        public string Pattern { get; set; }
        public Style Style { get; set; }

        public RegexOptions Options = RegexOptions.None;
        
        public Regex Regex
        {
            get { return _regex ?? (_regex = new Regex(Pattern, SyntaxHighlighter.RegexCompiledOption | Options)); }
        }
    }
}
