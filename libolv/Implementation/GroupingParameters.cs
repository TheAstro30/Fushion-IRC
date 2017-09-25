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
using System.Windows.Forms;

namespace libolv.Implementation
{
    public class GroupingParameters
    {
        public GroupingParameters(ObjectListView olv, OlvColumn groupByColumn, SortOrder groupByOrder,
                                  OlvColumn column, SortOrder order, OlvColumn secondaryColumn, SortOrder secondaryOrder,
                                  string titleFormat, string titleSingularFormat, bool sortItemsByPrimaryColumn)
        {
            ListView = olv;
            GroupByColumn = groupByColumn;
            GroupByOrder = groupByOrder;
            PrimarySort = column;
            PrimarySortOrder = order;
            SecondarySort = secondaryColumn;
            SecondarySortOrder = secondaryOrder;
            SortItemsByPrimaryColumn = sortItemsByPrimaryColumn;
            TitleFormat = titleFormat;
            TitleSingularFormat = titleSingularFormat;
        }

        /* Properties */
        public ObjectListView ListView { get; set; }
        public OlvColumn GroupByColumn { get; set; }
        public SortOrder GroupByOrder { get; set; }
        public IComparer<OlvGroup> GroupComparer { get; set; }
        public IComparer<OlvListItem> ItemComparer { get; set; }
        public OlvColumn PrimarySort { get; set; }
        public SortOrder PrimarySortOrder { get; set; }
        public OlvColumn SecondarySort { get; set; }
        public SortOrder SecondarySortOrder { get; set; }
        public string TitleFormat { get; set; }
        public string TitleSingularFormat { get; set; }
        public bool SortItemsByPrimaryColumn { get; set; }
    }
}
