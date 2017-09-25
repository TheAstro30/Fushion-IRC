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
using System.Collections.Generic;
using System.Drawing;

namespace libolv.Filtering.TextMatch
{
    internal class TextContainsMatchingStrategy : TextMatchingStrategy
    {
        public TextContainsMatchingStrategy(TextMatchFilter filter, string text)
        {
            TextFilter = filter;
            Text = text;
        }

        public override bool MatchesText(string cellText)
        {
            return cellText.IndexOf(Text, StringComparison) != -1;
        }

        public override IEnumerable<CharacterRange> FindAllMatchedRanges(string cellText)
        {
            var ranges = new List<CharacterRange>();
            var matchIndex = cellText.IndexOf(Text, StringComparison);
            while (matchIndex != -1)
            {
                ranges.Add(new CharacterRange(matchIndex, Text.Length));
                matchIndex = cellText.IndexOf(Text, matchIndex + Text.Length, StringComparison);
            }
            return ranges;
        }
    }
}
