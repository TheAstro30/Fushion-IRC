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
using System.Collections;

namespace libolv.Filtering.Filters
{
    public class TailFilter : AbstractListFilter
    {
        public TailFilter()
        {
            /* Empty */
        }

        public TailFilter(int numberOfObjects)
        {
            Count = numberOfObjects;
        }

        public int Count { get; set; }

        public override IEnumerable Filter(IEnumerable modelObjects)
        {
            if (Count <= 0)
            {
                return modelObjects;
            }
            var list = ObjectListView.EnumerableToArray(modelObjects, false);
            if (Count > list.Count)
            {
                return list;
            }
            var tail = new object[Count];
            list.CopyTo(list.Count - Count, tail, 0, Count);
            return new ArrayList(tail);
        }
    }
}
