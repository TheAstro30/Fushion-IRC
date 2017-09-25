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
using libolv.Implementation.Comparers;

namespace libolv.Implementation.VirtualGroups
{
    public class FastListGroupingStrategy : AbstractVirtualGroups
    {
        private List<int> _indexToGroupMap;

        public override IList<OlvGroup> GetGroups(GroupingParameters parmameters)
        {
            /* There is a lot of overlap between this method and ObjectListView.MakeGroups()
             * Any changes made here may need to be reflected there
             * This strategy can only be used on FastObjectListViews */
            var folv = (FastObjectListView)parmameters.ListView;
            /* Separate the list view items into groups, using the group key as the descrimanent */
            var objectCount = 0;
            var map = new NullableDictionary<object, List<object>>();
            foreach (var model in folv.FilteredObjects)
            {
                var key = parmameters.GroupByColumn.GetGroupKey(model);
                if (!map.ContainsKey(key))
                {
                    map[key] = new List<object>();
                }
                map[key].Add(model);
                objectCount++;
            }
            /* Sort the items within each group
             * TODO: Give parameters a ModelComparer property */
            var primarySortColumn = parmameters.SortItemsByPrimaryColumn
                                        ? parmameters.ListView.GetColumn(0)
                                        : parmameters.PrimarySort;
            var sorter = new ModelObjectComparer(primarySortColumn, parmameters.PrimarySortOrder,
                                                 parmameters.SecondarySort,
                                                 parmameters.SecondarySortOrder);
            foreach (var key in map.Keys)
            {
                map[key].Sort(sorter);
            }
            /* Make a list of the required groups */
            var groups = new List<OlvGroup>();
            foreach (var key in map.Keys)
            {
                var title = parmameters.GroupByColumn.ConvertGroupKeyToTitle(key);
                if (!string.IsNullOrEmpty(parmameters.TitleFormat))
                {
                    var count = map[key].Count;
                    var format = (count == 1 ? parmameters.TitleSingularFormat : parmameters.TitleFormat);
                    try
                    {
                        title = string.Format(format, title, count);
                    }
                    catch (FormatException)
                    {
                        title = "Invalid group format: " + format;
                    }
                }
                var lvg = new OlvGroup(title)
                              {
                                  Collapsible = folv.HasCollapsibleGroups,
                                  Key = key,
                                  SortValue = key as IComparable,
                                  Contents = map[key].ConvertAll(folv.IndexOf),
                                  VirtualItemCount = map[key].Count
                              };
                if (parmameters.GroupByColumn.GroupFormatter != null)
                {
                    parmameters.GroupByColumn.GroupFormatter(lvg, parmameters);
                }
                groups.Add(lvg);
            }
            /* Sort the groups */
            if (parmameters.GroupByOrder != SortOrder.None)
            {
                groups.Sort(parmameters.GroupComparer ?? new OlvGroupComparer(parmameters.GroupByOrder));
            }
            /* Build an array that remembers which group each item belongs to. */
            _indexToGroupMap = new List<int>(objectCount);
            _indexToGroupMap.AddRange(new int[objectCount]);
            for (var i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                var members = (List<int>)group.Contents;
                foreach (var j in members)
                {
                    _indexToGroupMap[j] = i;
                }
            }
            return groups;
        }

        public override int GetGroupMember(OlvGroup group, int indexWithinGroup)
        {
            return (int)group.Contents[indexWithinGroup];
        }

        public override int GetGroup(int itemIndex)
        {
            return _indexToGroupMap[itemIndex];
        }

        public override int GetIndexWithinGroup(OlvGroup group, int itemIndex)
        {
            return group.Contents.IndexOf(itemIndex);
        }
    }
}
