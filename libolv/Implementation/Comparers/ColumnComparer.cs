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
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace libolv.Implementation.Comparers
{
    public class ColumnComparer : IComparer, IComparer<OlvListItem>
    {
        private readonly OlvColumn _column;
        private readonly SortOrder _sortOrder;
        private readonly ColumnComparer _secondComparer;

        public ColumnComparer(OlvColumn col, SortOrder order)
        {
            _column = col;
            _sortOrder = order;
        }

        public ColumnComparer(OlvColumn col, SortOrder order, OlvColumn col2, SortOrder order2) : this(col, order)
        {
            /* There is no point in secondary sorting on the same column */
            if (col != col2)
            {
                _secondComparer = new ColumnComparer(col2, order2);
            }
        }

        public int Compare(object x, object y)
        {
            return Compare((OlvListItem)x, (OlvListItem)y);
        }

        public int Compare(OlvListItem x, OlvListItem y)
        {
            if (_sortOrder == SortOrder.None)
            {
                return 0;
            }
            int result;
            var x1 = _column.GetValue(x.RowObject);
            var y1 = _column.GetValue(y.RowObject);
            /* Handle nulls. Null values come last */
            var xIsNull = (x1 == null || x1 == DBNull.Value);
            var yIsNull = (y1 == null || y1 == DBNull.Value);
            if (xIsNull || yIsNull)
            {
                result = xIsNull && yIsNull ? 0 : (xIsNull ? -1 : 1);
            }
            else
            {
                result = CompareValues(x1, y1);
            }
            if (_sortOrder == SortOrder.Descending)
            {
                result = 0 - result;
            }
            /* If the result was equality, use the secondary comparer to resolve it */
            if (result == 0 && _secondComparer != null)
            {
                result = _secondComparer.Compare(x, y);
            }
            return result;
        }

        public int CompareValues(object x, object y)
        {
            /* Force case insensitive compares on strings */
            var xAsString = x as String;
            if (xAsString != null)
            {
                return String.Compare(xAsString, (String)y, StringComparison.CurrentCultureIgnoreCase);
            }
            var comparable = x as IComparable;
            return comparable != null ? comparable.CompareTo(y) : 0;
        }
    }
}
