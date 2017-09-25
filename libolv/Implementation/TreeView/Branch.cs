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

namespace libolv.Implementation.TreeView
{
    [Flags]
    public enum BranchFlags
    {
        FirstBranch = 1,
        LastChild = 2,
        OnlyBranch = 4
    }

    public class Branch
    {
        private List<Branch> _childBranches = new List<Branch>();
        private bool _alreadyHasChildren;
        private BranchFlags _flags;

        public Branch(Branch parent, Tree tree, Object model)
        {
            ParentBranch = parent;
            Tree = tree;
            Model = model;
        }
        
        /* Public properties */
        public virtual IList<Branch> Ancestors
        {
            get
            {
                var ancestors = new List<Branch>();
                if (ParentBranch != null)
                {
                    ParentBranch.PushAncestors(ancestors);
                }
                return ancestors;
            }
        }

        private void PushAncestors(IList<Branch> list)
        {
            /* This is designed to ignore the trunk (which has no parent) */
            if (ParentBranch == null) { return; }
            ParentBranch.PushAncestors(list);
            list.Add(this);
        }

        public virtual bool CanExpand
        {
            get { return Tree.CanExpandGetter != null && Model != null && Tree.CanExpandGetter(Model); }
        }

        public List<Branch> ChildBranches
        {
            get { return _childBranches; }
            set { _childBranches = value; }
        }

        public virtual IEnumerable Children
        {
            get
            {
                var children = new ArrayList();
                foreach (var x in ChildBranches)
                {
                    children.Add(x.Model);
                }
                return children;
            }
            set
            {
                ChildBranches.Clear();
                foreach (var x in value)
                {
                    AddChild(x);
                }
            }
        }

        private void AddChild(object model)
        {
            var br = Tree.GetBranch(model);
            if (br == null)
            {
                br = Tree.MakeBranch(this, model);
            }
            else
            {
                br.ParentBranch = this;
            }
            ChildBranches.Add(br);
        }

        public List<Branch> FilteredChildBranches
        {
            get
            {
                if (!IsExpanded)
                {
                    return new List<Branch>();
                }
                if (!Tree.IsFiltering)
                {
                    return ChildBranches;
                }
                var filtered = new List<Branch>();
                foreach (var b in ChildBranches)
                {
                    if (Tree.IncludeModel(b.Model))
                    {
                        filtered.Add(b);
                    }
                    else
                    {
                        /* Also include this branch if it has any filtered branches (yes, its recursive) */
                        if (b.FilteredChildBranches.Count > 0)
                        {
                            filtered.Add(b);
                        }
                    }
                }
                return filtered;
            }
        }

        public bool IsExpanded
        {
            get { return Tree.IsModelExpanded(Model); }
            set { Tree.SetModelExpanded(Model, value); }
        }

        public virtual bool IsFirstBranch
        {
            get { return ((_flags & BranchFlags.FirstBranch) != 0); }
            set
            {
                if (value)
                {
                    _flags |= BranchFlags.FirstBranch;
                }
                else
                {
                    _flags &= ~BranchFlags.FirstBranch;
                }
            }
        }

        public virtual bool IsLastChild
        {
            get { return ((_flags & BranchFlags.LastChild) != 0); }
            set
            {
                if (value)
                {
                    _flags |= BranchFlags.LastChild;
                }
                else
                {
                    _flags &= ~BranchFlags.LastChild;
                }
            }
        }

        public virtual bool IsOnlyBranch
        {
            get { return ((_flags & BranchFlags.OnlyBranch) != 0); }
            set
            {
                if (value)
                {
                    _flags |= BranchFlags.OnlyBranch;
                }
                else
                {
                    _flags &= ~BranchFlags.OnlyBranch;
                }
            }
        }

        public int Level
        {
            get { return ParentBranch == null ? 0 : ParentBranch.Level + 1; }
        }

        public object Model { get; set; }

        public virtual int NumberVisibleDescendents
        {
            get
            {
                if (!IsExpanded)
                {
                    return 0;
                }
                var filtered = FilteredChildBranches;
                return filtered.Count + filtered.Sum(br => br.NumberVisibleDescendents);
            }
        }

        public Branch ParentBranch { get; set; }
        public Tree Tree { get; set; }

        public virtual bool Visible
        {
            get { return ParentBranch == null || ParentBranch.IsExpanded && ParentBranch.Visible; }
        }

        /* Commands */
        public virtual void ClearCachedInfo()
        {
            Children = new ArrayList();
            _alreadyHasChildren = false;
        }

        public virtual void Collapse()
        {
            IsExpanded = false;
        }

        public virtual void Expand()
        {
            if (!CanExpand) { return; }
            IsExpanded = true;
            FetchChildren();
        }

        public virtual void ExpandAll()
        {
            Expand();
            foreach (var br in ChildBranches.Where(br => br.CanExpand))
            {
                br.ExpandAll();
            }
        }

        public virtual void FetchChildren()
        {
            if (_alreadyHasChildren)
            {
                return;
            }
            _alreadyHasChildren = true;
            if (Tree.ChildrenGetter == null)
            {
                return;
            }
            if (Tree.TreeView.UseWaitCursorWhenExpanding)
            {
                var previous = Cursor.Current;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    Children = Tree.ChildrenGetter(Model);
                }
                finally
                {
                    Cursor.Current = previous;
                }
            }
            else
            {
                Children = Tree.ChildrenGetter(Model);
            }
        }

        public virtual IList Flatten()
        {
            var flatList = new ArrayList();
            if (IsExpanded)
            {
                FlattenOnto(flatList);
            }
            return flatList;
        }

        public virtual void FlattenOnto(IList flatList)
        {
            Branch lastBranch = null;
            foreach (var br in FilteredChildBranches)
            {
                lastBranch = br;
                br.IsLastChild = false;
                flatList.Add(br.Model);
                if (br.IsExpanded)
                {
                    br.FlattenOnto(flatList);
                }
            }
            if (lastBranch != null)
            {
                lastBranch.IsLastChild = true;
            }
        }

        public virtual void RefreshChildren()
        {
            if (!IsExpanded || !CanExpand) { return; }
            FetchChildren();
            foreach (var br in ChildBranches)
            {
                br.RefreshChildren();
            }
        }

        public virtual void Sort(BranchComparer comparer)
        {
            if (ChildBranches.Count == 0)
            {
                return;
            }
            if (comparer != null)
            {
                ChildBranches.Sort(comparer);
            }
            foreach (var br in ChildBranches)
            {
                br.Sort(comparer);
            }
        }
    }
}
