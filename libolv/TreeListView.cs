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
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using libolv.Filtering.Filters;
using libolv.Implementation;
using libolv.Implementation.Events;
using libolv.Implementation.TreeView;

namespace libolv
{
    public partial class TreeListView : VirtualObjectListView
    {
        private bool _revealAfterExpand = true;
        private TreeRenderer _treeRenderer;
        private bool _useWaitCursorWhenExpanding = true;

        public delegate bool CanExpandGetterDelegate(Object model);
        public delegate IEnumerable ChildrenGetterDelegate(Object model);

        [Category("ObjectListView"), Description("This event is triggered when a branch is about to expand.")]
        public event EventHandler<TreeBranchExpandingEventArgs> Expanding;

        [Category("ObjectListView"), Description("This event is triggered when a branch is about to collapsed.")]
        public event EventHandler<TreeBranchCollapsingEventArgs> Collapsing;

        [Category("ObjectListView"),
        Description("This event is triggered when a branch has been expanded.")]
        public event EventHandler<TreeBranchExpandedEventArgs> Expanded;

        [Category("ObjectListView"), Description("This event is triggered when a branch has been collapsed.")]
        public event EventHandler<TreeBranchCollapsedEventArgs> Collapsed;

        public TreeListView()
        {
            Init();
        }

        private void Init()
        {
            TreeModel = new Tree(this);
            OwnerDraw = true;
            View = View.Details;
            VirtualListDataSource = TreeModel;
            TreeColumnRenderer = new TreeRenderer();
            /* This improves hit detection even if we don't have any state image */
            StateImageList = new ImageList();
        }

        /* Properties */
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual CanExpandGetterDelegate CanExpandGetter
        {
            get { return TreeModel.CanExpandGetter; }
            set { TreeModel.CanExpandGetter = value; }
        }

        [Browsable(false)]
        public override bool CanShowGroups
        {
            get { return false; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual ChildrenGetterDelegate ChildrenGetter
        {
            get { return TreeModel.ChildrenGetter; }
            set { TreeModel.ChildrenGetter = value; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IEnumerable ExpandedObjects
        {
            get { return TreeModel.MapObjectToExpanded.Keys; }
            set
            {
                TreeModel.MapObjectToExpanded.Clear();
                if (value == null) { return; }
                foreach (var x in value)
                {
                    TreeModel.SetModelExpanded(x, true);
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override IListFilter ListFilter
        {
            get { return null; }
            set { System.Diagnostics.Debug.Assert(value == null, "TreeListView do not support ListFilters"); }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override IEnumerable Objects
        {
            get { return Roots; }
            set { Roots = value; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override IEnumerable ObjectsForClustering
        {
            get
            {
                for (var i = 0; i < TreeModel.GetObjectCount(); i++)
                {
                    yield return TreeModel.GetNthObject(i);
                }
            }
        }

        [Category("ObjectListView"), Description("Should the parent of an expand subtree be scrolled to the top revealing the children?"), DefaultValue(true)]
        public bool RevealAfterExpand
        {
            get { return _revealAfterExpand; }
            set { _revealAfterExpand = value; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual IEnumerable Roots
        {
            get { return TreeModel.RootObjects; }
            set
            {
                TreeColumnRenderer = TreeColumnRenderer;
                TreeModel.RootObjects = value ?? new ArrayList();
                UpdateVirtualListSize();
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual TreeRenderer TreeColumnRenderer
        {
            get { return _treeRenderer ?? (_treeRenderer = new TreeRenderer()); }
            set
            {
                _treeRenderer = value ?? new TreeRenderer();
                EnsureTreeRendererPresent(_treeRenderer);
            }
        }

        [Category("ObjectListView"), Description("Should a wait cursor be shown when a branch is being expaned?"), DefaultValue(true)]
        public virtual bool UseWaitCursorWhenExpanding
        {
            get { return _useWaitCursorWhenExpanding; }
            set { _useWaitCursorWhenExpanding = value; }
        }

        protected Tree TreeModel { get; set; }

        /* Accessing */
        public virtual bool IsExpanded(Object model)
        {
            Branch br = TreeModel.GetBranch(model);
            return (br != null && br.IsExpanded);
        }

        protected virtual void EnsureTreeRendererPresent(TreeRenderer renderer)
        {
            if (Columns.Count == 0)
            {
                return;
            }
            foreach (var col in Columns.Cast<OlvColumn>().Where(col => col.Renderer is TreeRenderer))
            {
                col.Renderer = renderer;
                return;
            }
            /* No column held a tree renderer, so give column 0 one */
            var columnZero = GetColumn(0);
            columnZero.Renderer = renderer;
            columnZero.WordWrap = columnZero.WordWrap;
        }

        /* Commands */
        public virtual void Collapse(Object model)
        {
            if (GetItemCount() == 0)
            {
                return;
            }
            var selection = SelectedObjects;
            var index = TreeModel.Collapse(model);
            if (index < 0) { return; }
            UpdateVirtualListSize();
            SelectedObjects = selection;
            RedrawItems(index, GetItemCount() - 1, false);
        }

        public virtual void CollapseAll()
        {
            if (GetItemCount() == 0)
            {
                return;
            }
            var selection = SelectedObjects;
            var index = TreeModel.CollapseAll();
            if (index < 0) { return; }
            UpdateVirtualListSize();
            SelectedObjects = selection;
            RedrawItems(index, GetItemCount() - 1, false);
        }

        public override void ClearObjects()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(ClearObjects));
            }
            else
            {
                DiscardAllState();
            }
        }

        public virtual void DiscardAllState()
        {
            CheckStateMap.Clear();
            RebuildAll(false);
        }

        public virtual void RebuildAll(bool preserveState)
        {
            var previousTopItemIndex = preserveState ? TopItemIndex : -1;
            RebuildAll(
                preserveState ? SelectedObjects : null,
                preserveState ? ExpandedObjects : null,
                preserveState ? CheckedObjects : null);
            if (preserveState)
            {
                TopItemIndex = previousTopItemIndex;
            }
        }
        
        protected virtual void RebuildAll(IList selected, IEnumerable expanded, IList checkedObjects)
        {
            /* Remember the bits of info we don't want to forget (anyone ever see Memento?) */
            var roots = Roots;
            var canExpand = CanExpandGetter;
            var childrenGetter = ChildrenGetter;
            try
            {
                BeginUpdate();
                /* Give ourselves a new data structure */
                TreeModel = new Tree(this);
                VirtualListDataSource = TreeModel;
                /* Put back the bits we didn't want to forget */
                CanExpandGetter = canExpand;
                ChildrenGetter = childrenGetter;
                if (expanded != null)
                {
                    ExpandedObjects = expanded;
                }
                Roots = roots;
                if (selected != null)
                {
                    SelectedObjects = selected;
                }
                if (checkedObjects != null)
                {
                    CheckedObjects = checkedObjects;
                }
            }
            finally
            {
                EndUpdate();
            }
        }

        public virtual void Expand(Object model)
        {
            if (GetItemCount() == 0)
            {
                return;
            }
            /* Remember the selection so we can put it back later */
            var selection = SelectedObjects;
            /* Expand the model first */
            var index = TreeModel.Expand(model);
            if (index < 0)
            {
                return;
            }
            /* Update the size of the list and restore the selection */
            UpdateVirtualListSize();
            using (SuspendSelectionEventsDuring())
            {
                SelectedObjects = selection;
            }
            /* Redraw the items that were changed by the expand operation */
            RedrawItems(index, GetItemCount() - 1, false);
            if (!RevealAfterExpand || index <= 0) { return; }
            /* TODO: This should be a separate method */
            BeginUpdate();
            try
            {
                var countPerPage = NativeMethods.GetCountPerPage(this);
                var descedentCount = TreeModel.GetVisibleDescendentCount(model);
                /* If all of the descendents can be shown in the window, make sure that last one is visible.
                 * If all the descendents can't fit into the window, move the model to the top of the window
                 * (which will show as many of the descendents as possible) */
                if (descedentCount < countPerPage)
                {
                    EnsureVisible(index + descedentCount);
                }
                else
                {
                    TopItemIndex = index;
                }
            }
            finally
            {
                EndUpdate();
            }
        }

        public virtual void ExpandAll()
        {
            if (GetItemCount() == 0)
            {
                return;
            }
            var selection = SelectedObjects;
            var index = TreeModel.ExpandAll();
            if (index < 0)
            {
                return;
            }
            UpdateVirtualListSize();
            using (SuspendSelectionEventsDuring())
            {
                SelectedObjects = selection;
            }
            RedrawItems(index, GetItemCount() - 1, false);
        }

        public override void RefreshObjects(IList modelObjects)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)(() => RefreshObjects(modelObjects)));
                return;
            }
            /* There is no point in refreshing anything if the list is empty */
            if (GetItemCount() == 0)
            {
                return;
            }
            /* Remember the selection so we can put it back later */
            var selection = SelectedObjects;
            /* Refresh each object, remembering where the first update occured */
            var firstChange = (from object model in modelObjects
                               where model != null
                               select TreeModel.RebuildChildren(model)
                               into index where index >= 0 select index).Concat(new[] {Int32.MaxValue}).Min();
            /* If we didn't refresh any objects, don't do anything else */
            if (firstChange >= GetItemCount())
            {
                return;
            }
            ClearCachedInfo();
            UpdateVirtualListSize();
            SelectedObjects = selection;
            /* Redraw everything from the first update to the end of the list */
            RedrawItems(firstChange, GetItemCount() - 1, false);
        }

        public virtual void ToggleExpansion(Object model)
        {
            var item = ModelToItem(model);
            if (IsExpanded(model))
            {
                var args = new TreeBranchCollapsingEventArgs(model, item);
                OnCollapsing(args);
                if (!args.Canceled)
                {
                    Collapse(model);
                    OnCollapsed(new TreeBranchCollapsedEventArgs(model, item));
                }
            }
            else
            {
                var args = new TreeBranchExpandingEventArgs(model, item);
                OnExpanding(args);
                if (!args.Canceled)
                {
                    Expand(model);
                    OnExpanded(new TreeBranchExpandedEventArgs(model, item));
                }
            }
        }

        /* Commands - Tree traversal */
        public virtual bool CanExpand(Object model)
        {
            var br = TreeModel.GetBranch(model);
            return (br != null && br.CanExpand);
        }

        public virtual Object GetParent(Object model)
        {
            var br = TreeModel.GetBranch(model);
            return br == null || br.ParentBranch == null ? null : br.ParentBranch.Model;
        }

        public virtual IEnumerable GetChildren(Object model)
        {
            var br = TreeModel.GetBranch(model);
            if (br == null || !br.CanExpand)
            {
                return new ArrayList();
            }
            if (!br.IsExpanded)
            {
                br.Expand();
            }
            return br.Children;
        }

        /* Implementation */
        protected override bool ProcessLButtonDown(OlvListViewHitTestInfo hti)
        {
            /* Did they click in the expander? */
            if (hti.HitTestLocation == HitTestLocation.ExpandButton)
            {
                PossibleFinishCellEditing();
                ToggleExpansion(hti.RowObject);
                return true;
            }
            return base.ProcessLButtonDown(hti);
        }

        public override OlvListItem MakeListViewItem(int itemIndex)
        {
            var olvItem = base.MakeListViewItem(itemIndex);
            var br = TreeModel.GetBranch(olvItem.RowObject);
            if (br != null)
            {
                olvItem.IndentCount = br.Level - 1;
            }
            return olvItem;
        }

                /* Event handlers */
        protected virtual void OnExpanding(TreeBranchExpandingEventArgs e)
        {
            if (Expanding != null)
            {
                Expanding(this, e);
            }
        }

        protected virtual void OnCollapsing(TreeBranchCollapsingEventArgs e)
        {
            if (Collapsing != null)
            {
                Collapsing(this, e);
            }
        }

        protected virtual void OnExpanded(TreeBranchExpandedEventArgs e)
        {
            if (Expanded != null)
            {
                Expanded(this, e);
            }
        }

        protected virtual void OnCollapsed(TreeBranchCollapsedEventArgs e)
        {
            if (Collapsed != null)
            {
                Collapsed(this, e);
            }
        }

        protected override void HandleApplicationIdle(object sender, EventArgs e)
        {
            base.HandleApplicationIdle(sender, e);
            /* There is an annoying redraw bug on ListViews that use indentation and
             * that have full row select enabled. When the selection reduces to a subset
             * of previously selected rows, or when the selection is extended using
             * shift-pageup/down, then the space occupied by the identation is not
             * invalidated, and hence remains highlighted.
             * Ideally we'd want to know exactly which rows were selected or deselected
             * and then invalidate just the indentation region of those rows,
             * but that's too much work. So just redraw the control.
             * Actually... the selection issues show just slightly for non-full row select
             * controls as well. So, always redraw the control after the selection
             * changes. */
            Invalidate();
        }

        protected override bool IsInputKey(Keys keyData)
        {
            /* We want to handle Left and Right keys within the control */
            var key = keyData & Keys.KeyCode;
            return key == Keys.Left || key == Keys.Right || base.IsInputKey(keyData);
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            var focused = FocusedItem as OlvListItem;
            if (focused == null)
            {
                base.OnKeyDown(e);
                return;
            }
            var modelObject = focused.RowObject;
            var br = TreeModel.GetBranch(modelObject);
            switch (e.KeyCode)
            {
                case Keys.Left:
                    /* If the branch is expanded, collapse it. If it's collapsed,
                     * select the parent of the branch. */
                    if (br.IsExpanded)
                    {
                        Collapse(modelObject);
                    }
                    else
                    {
                        if (br.ParentBranch != null && br.ParentBranch.Model != null)
                        {
                            SelectObject(br.ParentBranch.Model, true);
                        }
                    }
                    e.Handled = true;
                    break;

                case Keys.Right:
                    /* If the branch is expanded, select the first child.
                     * If it isn't expanded and can be, expand it. */
                    if (br.IsExpanded)
                    {
                        var filtered = br.FilteredChildBranches;
                        if (filtered.Count > 0)
                        {
                            SelectObject(filtered[0].Model, true);
                        }
                    }
                    else
                    {
                        if (br.CanExpand)
                        {
                            Expand(modelObject);
                        }
                    }
                    e.Handled = true;
                    break;
            }
            base.OnKeyDown(e);
        }
    }    
}
