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
using System.ComponentModel;
using System.Linq;

namespace libolv.Implementation.Adapters
{
    public class TreeDataSourceAdapter : DataSourceAdapter
    {
        private string _keyAspectName;
        private string _parentKeyAspectName;
        private object _rootKeyValue;
        private bool _showKeyColumns = true;
        private readonly DataTreeListView _treeListView;
        private Munger.Munger _keyMunger;
        private Munger.Munger _parentKeyMunger;

        public TreeDataSourceAdapter(DataTreeListView tlv) : base(tlv)
        {
            _treeListView = tlv;
            _treeListView.CanExpandGetter = CalculateHasChildren;
            _treeListView.ChildrenGetter = CalculateChildren;
        }

        /* Properties */
        public virtual string KeyAspectName
        {
            get { return _keyAspectName; }
            set
            {
                if (_keyAspectName == value)
                {
                    return;
                }
                _keyAspectName = value;
                _keyMunger = new Munger.Munger(KeyAspectName);
                InitializeDataSource();
            }
        }

        public virtual string ParentKeyAspectName
        {
            get { return _parentKeyAspectName; }
            set
            {
                if (_parentKeyAspectName == value)
                {
                    return;
                }
                _parentKeyAspectName = value;
                _parentKeyMunger = new Munger.Munger(ParentKeyAspectName);
                InitializeDataSource();
            }
        }

        public virtual object RootKeyValue
        {
            get { return _rootKeyValue; }
            set
            {
                if (Equals(_rootKeyValue, value))
                {
                    return;
                }
                _rootKeyValue = value;
                InitializeDataSource();
            }
        }

        public virtual bool ShowKeyColumns
        {
            get { return _showKeyColumns; }
            set { _showKeyColumns = value; }
        }

        /* Implementation properties */
        protected DataTreeListView TreeListView
        {
            get { return _treeListView; }
        }

        /* Implementation */

        protected override void InitializeDataSource()
        {
            base.InitializeDataSource();
            TreeListView.RebuildAll(true);
        }

        protected override void SetListContents()
        {
            TreeListView.Roots = CalculateRoots();
        }

        protected override bool ShouldCreateColumn(PropertyDescriptor property)
        {
            /* If the property is a key column, and we aren't supposed to show keys, don't show it */
            return (ShowKeyColumns ||
                    (property.Name != KeyAspectName && property.Name != ParentKeyAspectName)) &&
                   base.ShouldCreateColumn(property);
        }

        protected override void HandleListChangedItemChanged(ListChangedEventArgs e)
        {
            /* If the id or the parent id of a row changes, we just rebuild everything.
             * We can't do anything more specific. We don't know what the previous values, so we can't 
             * tell the previous parent to refresh itself. If the id itself has changed, things that used
             * to be children will no longer be children. Just rebuild everything.
             * It seems PropertyDescriptor is only filled in .NET 4 :( */
            if (e.PropertyDescriptor != null && (e.PropertyDescriptor.Name == KeyAspectName || e.PropertyDescriptor.Name == ParentKeyAspectName))
            {
                InitializeDataSource();
            }
            else
            {
                base.HandleListChangedItemChanged(e);
            }
        }

        protected override void ChangePosition(int index)
        {
            /* We can't use our base method directly, since the normal position management
             * doesn't know about our tree structure. They treat our dataset as a flat list
             * but we have a collapsable structure. This means that the 5'th row to them
             * may not even be visible to us
             * To display the n'th row, we have to make sure that all its ancestors
             * are expanded. Then we will be able to select it. */
            var model = CurrencyManager.List[index];
            var parent = CalculateParent(model);
            while (parent != null && !TreeListView.IsExpanded(parent))
            {
                TreeListView.Expand(parent);
                parent = CalculateParent(parent);
            }
            base.ChangePosition(index);
        }

        private IEnumerable CalculateRoots()
        {
            return from object x in CurrencyManager.List let parentKey = GetParentValue(x) where Equals(RootKeyValue, parentKey) select x;
        }

        private bool CalculateHasChildren(object model)
        {
            var keyValue = GetKeyValue(model);
            if (keyValue == null)
            {
                return false;
            }
            return (from object x in CurrencyManager.List select GetParentValue(x)).Any(parentKey => Equals(keyValue, parentKey));
        }

        private IEnumerable CalculateChildren(object model)
        {
            var keyValue = GetKeyValue(model);
            if (keyValue == null) { yield break; }
            foreach (var x in from object x in CurrencyManager.List let parentKey = GetParentValue(x) where Equals(keyValue, parentKey) select x)
            {
                yield return x;
            }
        }

        private object CalculateParent(object model)
        {
            var parentValue = GetParentValue(model);
            if (parentValue == null)
            {
                return null;
            }
            return (from object x in CurrencyManager.List let key = GetKeyValue(x) where Equals(parentValue, key) select x).FirstOrDefault();
        }

        private object GetKeyValue(object model)
        {
            return _keyMunger == null ? null : _keyMunger.GetValue(model);
        }

        private object GetParentValue(object model)
        {
            return _parentKeyMunger == null ? null : _parentKeyMunger.GetValue(model);
        }        
    }
}