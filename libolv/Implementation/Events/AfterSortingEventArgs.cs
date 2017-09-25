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
using System.Windows.Forms;

namespace libolv.Implementation.Events
{
    public class AfterSortingEventArgs : EventArgs
    {
        public AfterSortingEventArgs(OlvColumn groupColumn, SortOrder groupOrder, OlvColumn column, SortOrder order, OlvColumn column2, SortOrder order2)
        {
            ColumnToGroupBy = groupColumn;
            GroupByOrder = groupOrder;
            ColumnToSort = column;
            SortOrder = order;
            SecondaryColumnToSort = column2;
            SecondarySortOrder = order2;
        }

        public AfterSortingEventArgs(BeforeSortingEventArgs args)
        {
            ColumnToGroupBy = args.ColumnToGroupBy;
            GroupByOrder = args.GroupByOrder;
            ColumnToSort = args.ColumnToSort;
            SortOrder = args.SortOrder;
            SecondaryColumnToSort = args.SecondaryColumnToSort;
            SecondarySortOrder = args.SecondarySortOrder;
        }

        public OlvColumn ColumnToGroupBy { get; private set; }
        public SortOrder GroupByOrder { get; private set; }
        public OlvColumn ColumnToSort { get; private set; }
        public SortOrder SortOrder { get; private set; }
        public OlvColumn SecondaryColumnToSort { get; private set; }
        public SortOrder SecondarySortOrder { get; private set; }
    }
}
