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
using System.Linq;
using System.Windows.Forms;
using libolv.Filtering.Filters;
using libolv.Implementation.Adapters;
using libolv.Implementation.Comparers;

namespace libolv.Implementation.TreeView
{
    public class Tree : IVirtualListDataSource, IFilterableDataSource
    {
        private OlvColumn _lastSortColumn;
        private SortOrder _lastSortOrder;
        private readonly Dictionary<Object, Branch> _mapObjectToBranch = new Dictionary<object, Branch>();
        internal Dictionary<Object, bool> MapObjectToExpanded = new Dictionary<object, bool>();
        private readonly Dictionary<Object, int> _mapObjectToIndex = new Dictionary<object, int>();
        private ArrayList _objectList = new ArrayList();
        private readonly Branch _trunk;

        protected IModelFilter ModelFilter;
        protected IListFilter ListFilter;

        public Tree(TreeListView treeView)
        {
            TreeView = treeView;
            _trunk = new Branch(null, this, null)
                        {
                            IsExpanded = true
                        };
        }

        /* Properties */
        public TreeListView.CanExpandGetterDelegate CanExpandGetter { get; set; }
        public TreeListView.ChildrenGetterDelegate ChildrenGetter { get; set; }

        public IEnumerable RootObjects
        {
            get { return _trunk.Children; }
            set
            {
                _trunk.Children = value;
                foreach (Branch br in _trunk.ChildBranches)
                {
                    br.RefreshChildren();
                }
                RebuildList();
            }
        }

        public TreeListView TreeView { get; private set; }

        /* Commands */
        public virtual int Collapse(Object model)
        {
            var br = GetBranch(model);
            if (br == null || !br.IsExpanded)
            {
                return -1;
            }
            /* Remember that the branch is collapsed, even if it's currently not visible */
            if (!br.Visible)
            {
                br.Collapse();
                return -1;
            }
            var count = br.NumberVisibleDescendents;
            br.Collapse();
            /* Remove the visible descendents from after the branch itself */
            var index = GetObjectIndex(model);
            _objectList.RemoveRange(index + 1, count);
            RebuildObjectMap(index + 1);
            return index;
        }

        public virtual int CollapseAll()
        {
            foreach (var br in _trunk.ChildBranches.Where(br => br.IsExpanded))
            {
                br.Collapse();
            }
            RebuildList();
            return 0;
        }

        public virtual int Expand(Object model)
        {
            var br = GetBranch(model);
            if (br == null || !br.CanExpand || br.IsExpanded)
            {
                return -1;
            }
            /* Remember that the branch is expanded, even if it's currently not visible */
            if (!br.Visible)
            {
                br.Expand();
                return -1;
            }
            var index = GetObjectIndex(model);
            InsertChildren(br, index + 1);
            return index;
        }
        public virtual int ExpandAll()
        {
            _trunk.ExpandAll();
            Sort(_lastSortColumn, _lastSortOrder);
            return 0;
        }

        public virtual Branch GetBranch(object model)
        {
            if (model == null)
            {
                return null;
            }
            Branch br;
            _mapObjectToBranch.TryGetValue(model, out br);
            return br;
        }

        public virtual int GetVisibleDescendentCount(object model)
        {
            var br = GetBranch(model);
            return br == null || !br.IsExpanded ? 0 : br.NumberVisibleDescendents;
        }

        public virtual int RebuildChildren(Object model)
        {
            var br = GetBranch(model);
            if (br == null || !br.Visible || !br.CanExpand)
            {
                return -1;
            }
            var count = br.NumberVisibleDescendents;
            br.ClearCachedInfo();
            /* Remove the visible descendents from after the branch itself */
            var index = GetObjectIndex(model);
            if (count > 0)
            {
                _objectList.RemoveRange(index + 1, count);
            }
            if (br.CanExpand && br.IsExpanded)
            {
                br.FetchChildren();
                InsertChildren(br, index + 1);
            }
            return index;
        }

        /* Implementation */
        internal bool IsModelExpanded(object model)
        {
            /* Special case: model == null is the container for the roots. This is always expanded */
            if (model == null)
            {
                return true;
            }
            bool isExpanded;
            MapObjectToExpanded.TryGetValue(model, out isExpanded);
            return isExpanded;
        }

        internal void SetModelExpanded(object model, bool isExpanded)
        {
            if (model == null) { return; }
            if (isExpanded)
            {
                MapObjectToExpanded[model] = true;
            }
            else
            {
                MapObjectToExpanded.Remove(model);
            }
        }

        protected virtual void InsertChildren(Branch br, int index)
        {
            /* Expand the branch */
            br.Expand();
            br.Sort(GetBranchComparer());
            /* Insert the branch's visible descendents after the branch itself */
            _objectList.InsertRange(index, br.Flatten());
            RebuildObjectMap(index);
        }

        protected virtual void RebuildList()
        {
            _objectList = ArrayList.Adapter(_trunk.Flatten());
            var filtered = _trunk.FilteredChildBranches;
            if (filtered.Count > 0)
            {
                filtered[0].IsFirstBranch = true;
                filtered[0].IsOnlyBranch = (filtered.Count == 1);
            }
            RebuildObjectMap(0);
        }

        protected virtual void RebuildObjectMap(int startIndex)
        {
            if (startIndex == 0)
            {
                _mapObjectToIndex.Clear();
            }
            for (var i = startIndex; i < _objectList.Count; i++)
            {
                _mapObjectToIndex[_objectList[i]] = i;
            }
        }

        internal Branch MakeBranch(Branch parent, object model)
        {
            var br = new Branch(parent, this, model);
            /* Remember that the given branch is part of this tree. */
            _mapObjectToBranch[model] = br;
            return br;
        }

        /* IVirtualListDataSource Members */
        public virtual object GetNthObject(int n)
        {
            return _objectList[n];
        }

        public virtual int GetObjectCount()
        {
            return _trunk.NumberVisibleDescendents;
        }

        public virtual int GetObjectIndex(object model)
        {
            int index;
            return model != null && _mapObjectToIndex.TryGetValue(model, out index) ? index : -1;
        }

        public virtual void PrepareCache(int first, int last)
        {
            /* Empty */
        }

        public virtual int SearchText(string value, int first, int last, OlvColumn column)
        {
            return AbstractVirtualListDataSource.DefaultSearchText(value, first, last, column, this);
        }

        public virtual void Sort(OlvColumn column, SortOrder order)
        {
            _lastSortColumn = column;
            _lastSortOrder = order;
            /* TODO: Need to raise an AboutToSortEvent here
             * Sorting is going to change the order of the branches so clear
             * the "first branch" flag */
            foreach (var b in _trunk.ChildBranches)
            {
                b.IsFirstBranch = false;
            }
            _trunk.Sort(GetBranchComparer());
            RebuildList();
        }

        protected virtual BranchComparer GetBranchComparer()
        {
            return _lastSortColumn == null
                       ? null
                       : new BranchComparer(new ModelObjectComparer(_lastSortColumn, _lastSortOrder,
                                                                    TreeView.GetColumn(0), _lastSortOrder));
        }

        public virtual void AddObjects(ICollection modelObjects)
        {
            var newRoots = new ArrayList();
            foreach (var x in TreeView.Roots)
            {
                newRoots.Add(x);
            }
            foreach (var x in modelObjects)
            {
                newRoots.Add(x);
            }
            SetObjects(newRoots);
        }

        public virtual void RemoveObjects(ICollection modelObjects)
        {
            var newRoots = new ArrayList();
            foreach (var x in TreeView.Roots)
            {
                newRoots.Add(x);
            }
            foreach (var x in modelObjects)
            {
                newRoots.Remove(x);
                _mapObjectToIndex.Remove(x);
            }
            SetObjects(newRoots);
        }

        public virtual void SetObjects(IEnumerable collection)
        {
            /* We interpret a SetObjects() call as setting the roots of the tree */
            TreeView.Roots = collection;
        }

        /* IFilterableDataSource Members */
        public void ApplyFilters(IModelFilter modelFilter, IListFilter listFilter)
        {
            ModelFilter = modelFilter;
            ListFilter = listFilter;
            RebuildList();
        }

        internal bool IsFiltering
        {
            get { return TreeView.UseFiltering && (ModelFilter != null || ListFilter != null); }
        }

        internal bool IncludeModel(object model)
        {
            if (!TreeView.UseFiltering)
            {
                return true;
            }
            return ModelFilter == null || ModelFilter.Filter(model);
        }
    }
}
