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
using System.Windows.Forms;
using System.Collections;
using System.Drawing;
using libolv.Implementation;
using libolv.SubControls;

namespace libolv.Filtering
{
    public class FilterMenuBuilder
    {
        public static string ApplyLabel = "Apply";
        public static string ClearAllFiltersLabel = "Clear All Filters";
        public static string FilteringLabel = "Filtering";
        public static string SelectAllLabel = "Select All";

        public static Bitmap ClearFilteringImage = Properties.Resources.ClearFiltering;
        public static Bitmap FilteringImage = Properties.Resources.Filtering;

        private bool _treatNullAsDataValue = true;
        private int _maxObjectsToConsider = 10000;
        private bool _alreadyInHandleItemChecked;

        /* Public properties */
        public bool TreatNullAsDataValue
        {
            get { return _treatNullAsDataValue; }
            set { _treatNullAsDataValue = value; }
        }

        public int MaxObjectsToConsider
        {
            get { return _maxObjectsToConsider; }
            set { _maxObjectsToConsider = value; }
        }

        public virtual ToolStripDropDown MakeFilterMenu(ToolStripDropDown strip, ObjectListView listView, OlvColumn column)
        {
            if (strip == null) { throw new ArgumentNullException("strip"); } 
            if (listView == null) { throw new ArgumentNullException("listView"); }
            if (column == null) { throw new ArgumentNullException("column"); }

            if (!column.UseFiltering || column.ClusteringStrategy == null || listView.Objects == null)
            {
                return strip;
            }
            var clusters = Cluster(column.ClusteringStrategy, listView, column);
            if (clusters.Count > 0)
            {
                SortClusters(column.ClusteringStrategy, clusters);
                strip.Items.Add(CreateFilteringMenuItem(column, clusters));
            }
            return strip;
        }

        protected virtual List<ICluster> Cluster(IClusteringStrategy strategy, ObjectListView listView, OlvColumn column)
        {
            /* Build a map that correlates cluster key to clusters */
            var map = new NullableDictionary<object, ICluster>();
            var count = 0;
            foreach (var model in listView.ObjectsForClustering)
            {
                ClusterOneModel(strategy, map, model);
                if (count++ > MaxObjectsToConsider)
                {
                    break;
                }
            }
            /* Now that we know exactly how many items are in each cluster, create a label for it */
            foreach (var cluster in map.Values)
            {
                cluster.DisplayLabel = strategy.GetClusterDisplayLabel(cluster);
            }
            return new List<ICluster>(map.Values);
        }

        private void ClusterOneModel(IClusteringStrategy strategy, NullableDictionary<object, ICluster> map, object model)
        {
            var clusterKey = strategy.GetClusterKey(model);
            /* If the returned value is an IEnumerable, that means the given model can belong to more than one cluster */
            var keyEnumerable = clusterKey as IEnumerable;
            if (clusterKey is string || keyEnumerable == null)
            {
                keyEnumerable = new[]
                                    {
                                        clusterKey
                                    };
            }
            /* Deal with nulls and DBNulls */
            var nullCorrected = new ArrayList();
            foreach (var key in keyEnumerable)
            {
                if (key == null || key == DBNull.Value)
                {
                    if (TreatNullAsDataValue)
                    {
// ReSharper disable AssignNullToNotNullAttribute
                        nullCorrected.Add(null);
// ReSharper restore AssignNullToNotNullAttribute
                    }
                }
                else nullCorrected.Add(key);
            }
            /* Group by key */
            foreach (var key in nullCorrected)
            {
                if (map.ContainsKey(key))
                {
                    map[key].Count += 1;
                }
                else
                {
                    map[key] = strategy.CreateCluster(key);
                }
            }
        }

        protected virtual void SortClusters(IClusteringStrategy strategy, List<ICluster> clusters)
        {
            clusters.Sort();
        }

        protected virtual ToolStripMenuItem CreateFilteringMenuItem(OlvColumn column, List<ICluster> clusters)
        {
            var checkedList = new ToolStripCheckedListBox
                                  {
                                      Tag = column
                                  };
            foreach (var cluster in clusters)
            {
                checkedList.AddItem(cluster, column.ValuesChosenForFiltering.Contains(cluster.ClusterKey));
            }
            if (!string.IsNullOrEmpty(SelectAllLabel))
            {
                var checkedCount = checkedList.CheckedItems.Count;
                if (checkedCount == 0)
                {
                    checkedList.AddItem(SelectAllLabel, CheckState.Unchecked);
                }
                else
                {
                    checkedList.AddItem(SelectAllLabel,
                                        checkedCount == clusters.Count ? CheckState.Checked : CheckState.Indeterminate);
                }
            }
            checkedList.ItemCheck += HandleItemCheckedWrapped;
            var col = column;
            var clearAll = new ToolStripMenuItem(ClearAllFiltersLabel, ClearFilteringImage, (sender, args) => ClearAllFilters(col));
            var apply = new ToolStripMenuItem(ApplyLabel, FilteringImage, (sender, args) => EnactFilter(checkedList, column));
            var subMenu = new ToolStripMenuItem(FilteringLabel, null, new ToolStripItem[]
                                                                          {
                                                                              clearAll,
                                                                              new ToolStripSeparator(),
                                                                              checkedList, apply
                                                                          });
            return subMenu;
        }

        private void HandleItemCheckedWrapped(object sender, ItemCheckEventArgs e)
        {
            if (_alreadyInHandleItemChecked)
            {
                return;
            }
            try
            {
                _alreadyInHandleItemChecked = true;
                HandleItemChecked(sender, e);
            }
            finally
            {
                _alreadyInHandleItemChecked = false;
            }
        }

        protected virtual void HandleItemChecked(object sender, ItemCheckEventArgs e)
        {
            var checkedList = sender as ToolStripCheckedListBox;
            if (checkedList == null) { return; }
            var column = checkedList.Tag as OlvColumn;
            if (column == null) { return; }
            var listView = column.ListView as ObjectListView;
            if (listView == null) { return; }
            /* Deal with the "Select All" item if there is one */
            var selectAllIndex = checkedList.Items.IndexOf(SelectAllLabel);
            if (selectAllIndex >= 0)
            {
                HandleSelectAllItem(e, checkedList, selectAllIndex);
            }
        }

        protected virtual void HandleSelectAllItem(ItemCheckEventArgs e, ToolStripCheckedListBox checkedList, int selectAllIndex)
        {
            /* Did they check/uncheck the "Select All"? */
            if (e.Index == selectAllIndex)
            {
                switch (e.NewValue)
                {
                    case CheckState.Checked:
                        checkedList.CheckAll();
                        break;

                    case CheckState.Unchecked:
                        checkedList.UncheckAll();
                        break;
                }
                return;
            }
            /* OK. The user didn't check/uncheck SelectAll. Now we have to update it's
             * checkedness to reflect the state of everything else
             * If all clusters are checked, we check the Select All.
             * If no clusters are checked, the uncheck the Select All.
             * For everything else, Select All is set to indeterminate.
             * How many items are currenty checked? */
            var count = checkedList.CheckedItems.Count;
            /* First complication.
             * The value of the Select All itself doesn't count */
            if (checkedList.GetItemCheckState(selectAllIndex) != CheckState.Unchecked)
            {
                count -= 1;
            }
            /* Another complication.
             * CheckedItems does not yet know about the item the user has just
             * clicked, so we have to adjust the count of checked items to what
             * it is going to be */
            if (e.NewValue != e.CurrentValue)
            {
                switch (e.NewValue)
                {
                    case CheckState.Checked:
                        count += 1;
                        break;

                    default:
                        count -= 1;
                        break;
                }
            }
            /* Update the state of the Select All item */
            switch (count)
            {
                case 0:
                    checkedList.SetItemState(selectAllIndex, CheckState.Unchecked);
                    break;

                default:
                    checkedList.SetItemState(selectAllIndex,
                                             count == checkedList.Items.Count - 1
                                                 ? CheckState.Checked
                                                 : CheckState.Indeterminate);
                    break;
            }
        }

        protected virtual void ClearAllFilters(OlvColumn column)
        {
            var olv = column.ListView as ObjectListView;
            if (olv == null || olv.IsDisposed)
            {
                return;
            }
            olv.ResetColumnFiltering();
        }

        protected virtual void EnactFilter(ToolStripCheckedListBox checkedList, OlvColumn column)
        {
            var olv = column.ListView as ObjectListView;
            if (olv == null || olv.IsDisposed)
            {
                return;
            }
            /* Collect all the checked values */
            var chosenValues = new ArrayList();
            foreach (var cluster in checkedList.CheckedItems.OfType<ICluster>())
            {
                chosenValues.Add(cluster.ClusterKey);
            }
            column.ValuesChosenForFiltering = chosenValues;
            olv.UpdateColumnFiltering();
        }
    }
}