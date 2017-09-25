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
using System.Windows.Forms;

namespace libolv.Implementation.Comparers
{
    public class OlvGroupComparer : IComparer<OlvGroup>
    {
        private readonly SortOrder _sortOrder;

        public OlvGroupComparer(SortOrder order)
        {
            _sortOrder = order;
        }

        public int Compare(OlvGroup x, OlvGroup y)
        {
            /* If we can compare the sort values, do that.
             * Otherwise do a case insensitive compare on the group header. */
            int result;
            if (x.SortValue != null && y.SortValue != null)
            {
                result = x.SortValue.CompareTo(y.SortValue);
            }
            else
            {
                result = String.Compare(x.Header, y.Header, StringComparison.CurrentCultureIgnoreCase);
            }
            if (_sortOrder == SortOrder.Descending)
            {
                result = 0 - result;
            }
            return result;
        }        
    }
}
