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
using System.Linq;
using System.Text.RegularExpressions;
using System.Drawing;
using libolv.Filtering.Filters;

namespace libolv.Filtering.TextMatch
{
    public class TextMatchFilter : AbstractModelFilter
    {
        private RegexOptions? _regexOptions;
        private StringComparison _stringComparison = StringComparison.InvariantCultureIgnoreCase;
        private List<TextMatchingStrategy> _matchingStrategies = new List<TextMatchingStrategy>();

        public static TextMatchFilter Regex(ObjectListView olv, params string[] texts)
        {
            var filter = new TextMatchFilter(olv)
                             {
                                 RegexStrings = texts
                             };
            return filter;
        }

        public static TextMatchFilter Prefix(ObjectListView olv, params string[] texts)
        {
            var filter = new TextMatchFilter(olv)
                             {
                                 PrefixStrings = texts
                             };
            return filter;
        }

        public static TextMatchFilter Contains(ObjectListView olv, params string[] texts)
        {
            var filter = new TextMatchFilter(olv)
                             {
                                 ContainsStrings = texts
                             };
            return filter;
        }

        public TextMatchFilter(ObjectListView olv)
        {
            ListView = olv;
        }

        public TextMatchFilter(ObjectListView olv, string text)
        {
            ListView = olv;
            ContainsStrings = new[]
                                  {
                                      text
                                  };
        }

        public TextMatchFilter(ObjectListView olv, string text, StringComparison comparison)
        {
            ListView = olv;
            ContainsStrings = new[]
                                  {
                                      text
                                  };
            StringComparison = comparison;
        }

        /* Public properties */
        public OlvColumn[] Columns { get; set; }
        public OlvColumn[] AdditionalColumns { get; set; }

        public IEnumerable<string> ContainsStrings
        {
            get { return _matchingStrategies.Select(component => component.Text); }
            set
            {
                _matchingStrategies = new List<TextMatchingStrategy>();
                if (value == null) { return; }
                foreach (var text in value)
                {
                    _matchingStrategies.Add(new TextContainsMatchingStrategy(this, text));
                }
            }
        }

        public bool HasComponents
        {
            get { return _matchingStrategies.Count > 0; }
        }

        public ObjectListView ListView { get; set; }

        public IEnumerable<string> PrefixStrings
        {
            get { return _matchingStrategies.Select(component => component.Text); }
            set
            {
                _matchingStrategies = new List<TextMatchingStrategy>();
                if (value == null) { return; }
                foreach (var text in value)
                {
                    _matchingStrategies.Add(new TextBeginsMatchingStrategy(this, text));
                }
            }
        }

        public RegexOptions RegexOptions
        {
            get
            {
                if (!_regexOptions.HasValue)
                {
                    switch (StringComparison)
                    {
                        case StringComparison.CurrentCulture:
                            _regexOptions = RegexOptions.None;
                            break;

                        case StringComparison.CurrentCultureIgnoreCase:
                            _regexOptions = RegexOptions.IgnoreCase;
                            break;

                        case StringComparison.Ordinal:
                        case StringComparison.InvariantCulture:
                            _regexOptions = RegexOptions.CultureInvariant;
                            break;

                        case StringComparison.OrdinalIgnoreCase:
                        case StringComparison.InvariantCultureIgnoreCase:
                            _regexOptions = RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;
                            break;

                        default:
                            _regexOptions = RegexOptions.None;
                            break;
                    }
                }
                return _regexOptions.Value;
            }
            set { _regexOptions = value; }
        }

        public IEnumerable<string> RegexStrings
        {
            get { return _matchingStrategies.Select(component => component.Text); }
            set
            {
                _matchingStrategies = new List<TextMatchingStrategy>();
                if (value == null) return;
                foreach (var text in value)
                {
                    _matchingStrategies.Add(new TextRegexMatchingStrategy(this, text));
                }
            }
        }

        public StringComparison StringComparison
        {
            get { return _stringComparison; }
            set { _stringComparison = value; }
        }

        /* Implementation */
        protected virtual IEnumerable<OlvColumn> IterateColumns()
        {
            if (Columns == null)
            {
                foreach (OlvColumn column in ListView.Columns)
                {
                    yield return column;
                }
            }
            else
            {
                foreach (var column in Columns)
                {
                    yield return column;
                }
            }
            if (AdditionalColumns == null) { yield break; }
            foreach (var column in AdditionalColumns)
            {
                yield return column;
            }
        }

        /* Public interface */
        public override bool Filter(object modelObject)
        {
            if (ListView == null || !HasComponents)
            {
                return true;
            }
            return (from column in IterateColumns()
                    where column.IsVisible && column.Searchable
                    select column.GetStringValue(modelObject)
                    into cellText from filter in _matchingStrategies
                    where string.IsNullOrEmpty(filter.Text) || filter.MatchesText(cellText) select cellText).Any();
        }

        public IEnumerable<CharacterRange> FindAllMatchedRanges(string cellText)
        {
            var ranges = new List<CharacterRange>();
            foreach (var filter in _matchingStrategies.Where(filter => !string.IsNullOrEmpty(filter.Text)))
            {
                ranges.AddRange(filter.FindAllMatchedRanges(cellText));
            }
            return ranges;
        }

        public bool IsIncluded(OlvColumn column)
        {
            if (Columns == null)
            {
                return column.ListView == ListView;
            }
            return Columns.Any(x => x == column);
        }
    }
}