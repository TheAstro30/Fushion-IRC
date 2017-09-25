/* Object List View
 * Copyright (C) 2006-2012 Phillip Piper
 * Refactored by Jason James Newland - 2014
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * If you wish to use this code in a closed source application, please contact phillip_piper@bigfoot.com.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace libolv.Filtering.TextMatch
{
    internal class TextRegexMatchingStrategy : TextMatchingStrategy
    {
        private static readonly Regex InvalidRegexMarker = new Regex(".*");

        private Regex _regex;

        public TextRegexMatchingStrategy(TextMatchFilter filter, string text)
        {
            TextFilter = filter;
            Text = text;
        }
        public RegexOptions RegexOptions
        {
            get { return TextFilter.RegexOptions; }
        }

        protected Regex Regex
        {
            get
            {
                if (_regex == null)
                {
                    try
                    {
                        _regex = new Regex(Text, RegexOptions);
                    }
                    catch (ArgumentException)
                    {
                        _regex = InvalidRegexMarker;
                    }
                }
                return _regex;
            }
            set { _regex = value; }
        }

        protected bool IsRegexInvalid
        {
            get { return Regex == InvalidRegexMarker; }
        }

        public override bool MatchesText(string cellText)
        {
            return IsRegexInvalid || Regex.Match(cellText).Success;
        }

        public override IEnumerable<CharacterRange> FindAllMatchedRanges(string cellText)
        {
            var ranges = new List<CharacterRange>();
            if (!IsRegexInvalid)
            {
                ranges.AddRange(from Match match in Regex.Matches(cellText) where match.Length > 0 select new CharacterRange(match.Index, match.Length));
            }
            return ranges;
        }
    }
}
