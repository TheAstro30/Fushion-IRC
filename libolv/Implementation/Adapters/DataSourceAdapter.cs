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
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using System.Diagnostics;
using libolv.Implementation.Events;
using libolv.Rendering.Renderers;
using libolv.Utilities;

namespace libolv.Implementation.Adapters
{
    public class DataSourceAdapter : IDisposable
    {
        private bool _autoGenerateColumns = true;
        private Object _dataSource;
        private string _dataMember = "";
        private bool _isChangingIndex;
        private bool _alreadyFreezing;

        public DataSourceAdapter(ObjectListView olv)
        {
            CurrencyManager = null;
            Init(olv);
        }

        ~DataSourceAdapter()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool fromUser)
        {
            UnbindListView(ListView);
            UnbindDataSource();
        }

        /* Public Properties */

        public bool AutoGenerateColumns
        {
            get { return _autoGenerateColumns; }
            set { _autoGenerateColumns = value; }
        }

        public virtual Object DataSource
        {
            get { return _dataSource; }
            set
            {
                _dataSource = value;
                RebindDataSource(true);
            }
        }

        public virtual string DataMember
        {
            get { return _dataMember; }
            set
            {
                if (_dataMember != value)
                {
                    _dataMember = value;
                    RebindDataSource();
                }
            }
        }

        public ObjectListView ListView { get; internal set; }

        /* Implementation properties */
        protected CurrencyManager CurrencyManager { get; set; }

        /* Binding and unbinding */

        protected virtual void BindListView(ObjectListView olv)
        {
            if (olv == null)
            {
                return;
            }
            olv.Freezing += HandleListViewFreezing;
            olv.SelectedIndexChanged += HandleListViewSelectedIndexChanged;
            olv.BindingContextChanged += HandleListViewBindingContextChanged;
        }

        protected virtual void UnbindListView(ObjectListView olv)
        {
            if (olv == null)
            {
                return;
            }
            olv.Freezing -= HandleListViewFreezing;
            olv.SelectedIndexChanged -= HandleListViewSelectedIndexChanged;
            olv.BindingContextChanged -= HandleListViewBindingContextChanged;
        }

        protected virtual void BindDataSource()
        {
            if (CurrencyManager == null)
            {
                return;
            }
            CurrencyManager.MetaDataChanged += HandleCurrencyManagerMetaDataChanged;
            CurrencyManager.PositionChanged += HandleCurrencyManagerPositionChanged;
            CurrencyManager.ListChanged += CurrencyManagerListChanged;
        }

        protected virtual void UnbindDataSource()
        {
            if (CurrencyManager == null)
            {
                return;
            }
            CurrencyManager.MetaDataChanged -= HandleCurrencyManagerMetaDataChanged;
            CurrencyManager.PositionChanged -= HandleCurrencyManagerPositionChanged;
            CurrencyManager.ListChanged -= CurrencyManagerListChanged;
        }

        /* Initialization */
        private void Init(ObjectListView olv)
        {
            if (olv == null)
            {
                throw new ArgumentNullException("olv");
            }
            ListView = olv;
            BindListView(ListView);
        }

        protected virtual void RebindDataSource()
        {
            RebindDataSource(false);
        }

        protected virtual void RebindDataSource(bool forceDataInitialization)
        {
            CurrencyManager tempCurrencyManager = null;
            if (ListView != null && ListView.BindingContext != null && DataSource != null)
            {
                tempCurrencyManager = ListView.BindingContext[DataSource, DataMember] as CurrencyManager;
            }
            /* Has our currency manager changed? */
            if (CurrencyManager != tempCurrencyManager)
            {
                UnbindDataSource();
                CurrencyManager = tempCurrencyManager;
                BindDataSource();
                /* Our currency manager has changed so we have to initialize a new data source */
                forceDataInitialization = true;
            }
            if (forceDataInitialization)
            {
                InitializeDataSource();
            }
        }

        protected virtual void InitializeDataSource()
        {
            if (ListView.Frozen || CurrencyManager == null)
            {
                return;
            }
            CreateColumnsFromSource();
            CreateMissingAspectGettersAndPutters();
            SetListContents();
            ListView.AutoSizeColumns();
        }

        protected virtual void SetListContents()
        {
            ListView.Objects = CurrencyManager.List;
        }

        protected virtual void CreateColumnsFromSource()
        {
            if (CurrencyManager == null)
            {
                return;
            }
            /* Don't generate any columns in design mode. If we do, the user will see them,
             * but the Designer won't know about them and won't persist them, which is very confusing */
            if (ListView.IsDesignMode)
            {
                return;
            }
            /* Don't create columns if we've been told not to */
            if (!AutoGenerateColumns)
            {
                return;
            }
            /* Use a Generator to create columns */
            var generator = Generator.Instance as Generator ?? new Generator();
            var properties = CurrencyManager.GetItemProperties();
            if (properties.Count == 0)
            {
                return;
            }
            foreach (PropertyDescriptor property in properties)
            {
                if (!ShouldCreateColumn(property))
                {
                    continue;
                }
                /* Create a column */
                var column = generator.MakeColumnFromPropertyDescriptor(property);
                ConfigureColumn(column, property);
                /* Add it to our list */
                ListView.AllColumns.Add(column);
            }
            generator.PostCreateColumns(ListView);
        }

        protected virtual bool ShouldCreateColumn(PropertyDescriptor property)
        {
            /* Is there a column that already shows this property? If so, we don't show it again */
            if (ListView.AllColumns.Exists(x => x.AspectName == property.Name))
            {
                return false;
            }
            /* Relationships to other tables turn up as IBindibleLists. Don't make columns to show them.
             * CHECK: Is this always true? What other things could be here? Constraints? Triggers? */
            if (property.PropertyType == typeof (IBindingList))
            {
                return false;
            }
            /* Ignore anything marked with [OlvIgnore] */
            return property.Attributes[typeof (OlvIgnoreAttribute)] == null;
        }

        protected virtual void ConfigureColumn(OlvColumn column, PropertyDescriptor property)
        {
            column.LastDisplayIndex = ListView.AllColumns.Count;
            /* If our column is a BLOB, it could be an image, so assign a renderer to draw it.
             * CONSIDER: Is this a common enough case to warrant this code? */
            if (property.PropertyType == typeof (Byte[]))
            {
                column.Renderer = new ImageRenderer();
            }
        }

        protected virtual void CreateMissingAspectGettersAndPutters()
        {
            foreach (var x in ListView.AllColumns)
            {
                var column = x; /* stack based variable accessible from closures */
                if (column.AspectGetter == null && !string.IsNullOrEmpty(column.AspectName))
                {
                    column.AspectGetter = delegate(object row)
                                              {
                                                  /* In most cases, rows will be DataRowView objects */
                                                  var drv = row as DataRowView;
                                                  if (drv == null)
                                                  {
                                                      return column.GetAspectByName(row);
                                                  }
                                                  return (drv.Row.RowState == DataRowState.Detached)
                                                             ? null
                                                             : drv[column.AspectName];
                                              };
                }
                if (column.IsEditable && column.AspectPutter == null && !string.IsNullOrEmpty(column.AspectName))
                {
                    column.AspectPutter = delegate(object row, object newValue)
                                              {
                                                  /* In most cases, rows will be DataRowView objects */
                                                  var drv = row as DataRowView;
                                                  if (drv == null)
                                                  {
                                                      column.PutAspectByName(row, newValue);
                                                  }
                                                  else
                                                  {
                                                      if (drv.Row.RowState != DataRowState.Detached)
                                                      {
                                                          drv[column.AspectName] = newValue;
                                                      }
                                                  }
                                              };
                }
            }
        }

        /* Event Handlers */
        protected virtual void CurrencyManagerListChanged(object sender, ListChangedEventArgs e)
        {
            Debug.Assert(sender == CurrencyManager);
            /* Ignore changes make while frozen, since we will do a complete rebuild when we unfreeze */
            if (ListView.Frozen)
            {
                return;
            }
            switch (e.ListChangedType)
            {
                case ListChangedType.Reset:
                    HandleListChangedReset(e);
                    break;

                case ListChangedType.ItemChanged:
                    HandleListChangedItemChanged(e);
                    break;

                case ListChangedType.ItemAdded:
                    HandleListChangedItemAdded(e);
                    break;
                    
                case ListChangedType.ItemDeleted:
                    /* An item has gone away. */
                    HandleListChangedItemDeleted(e);
                    break;
                    
                case ListChangedType.ItemMoved:
                    /* An item has changed its index. */
                    HandleListChangedItemMoved(e);
                    break;
                    
                case ListChangedType.PropertyDescriptorAdded:
                case ListChangedType.PropertyDescriptorChanged:
                case ListChangedType.PropertyDescriptorDeleted:
                    /* Something has changed in the metadata.
                     * CHECK: When are these events actually fired? */
                    HandleListChangedMetadataChanged(e);
                    break;
            }            
        }

        protected virtual void HandleListChangedMetadataChanged(ListChangedEventArgs e)
        {
            InitializeDataSource();
        }

        protected virtual void HandleListChangedItemMoved(ListChangedEventArgs e)
        {
            // When is this actually triggered?
            InitializeDataSource();
        }

        protected virtual void HandleListChangedItemDeleted(ListChangedEventArgs e)
        {
            InitializeDataSource();
        }

        protected virtual void HandleListChangedItemAdded(ListChangedEventArgs e)
        {
            /* We get this event twice if certain grid controls are used to add a new row to a
             * datatable: once when the editing of a new row begins, and once again when that
             * editing commits. (If the user cancels the creation of the new row, we never see
             * the second creation.) We detect this by seeing if this is a view on a row in a
             * DataTable, and if it is, testing to see if it's a new row under creation. */
            var newRow = CurrencyManager.List[e.NewIndex];
            var drv = newRow as DataRowView;
            if (drv == null || !drv.IsNew)
            {
                /* Either we're not dealing with a view on a data table, or this is the commit
                 * notification. Either way, this is the final notification, so we want to
                 * handle the new row now! */
                InitializeDataSource();
            }
        }

        protected virtual void HandleListChangedReset(ListChangedEventArgs e)
        {
            /* The whole list has changed utterly, so reload it. */
            InitializeDataSource();
        }

        protected virtual void HandleListChangedItemChanged(ListChangedEventArgs e)
        {
            /* A single item has changed, so just refresh that. */
            var changedRow = CurrencyManager.List[e.NewIndex];
            ListView.RefreshObject(changedRow);
        }

        protected virtual void HandleCurrencyManagerMetaDataChanged(object sender, EventArgs e)
        {
            InitializeDataSource();
        }

        protected virtual void HandleCurrencyManagerPositionChanged(object sender, EventArgs e)
        {
            var index = CurrencyManager.Position;
            /* Make sure the index is sane (-1 pops up from time to time) */
            if (index < 0 || index >= ListView.GetItemCount())
            {
                return;
            }
            /* Avoid recursion. If we are currently changing the index, don't
             * start the process again. */
            if (_isChangingIndex)
            {
                return;
            }
            try
            {
                _isChangingIndex = true;
                ChangePosition(index);
            }
            finally
            {
                _isChangingIndex = false;
            }
        }

        protected virtual void ChangePosition(int index)
        {
            /* We can't use the index directly, since our listview may be sorted */
            ListView.SelectedObject = CurrencyManager.List[index];
            /* THINK: Do we always want to bring it into view? */
            if (ListView.SelectedIndices.Count > 0)
            {
                ListView.EnsureVisible(ListView.SelectedIndices[0]);
            }
        }

        /* ObjectListView event handlers */
        protected virtual void HandleListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            /* Prevent recursion */
            if (_isChangingIndex)
            {
                return;
            }
            /* If we are bound to a datasource, and only one item is selected,
             * tell the currency manager which item is selected. */
            if (ListView.SelectedIndices.Count != 1 || CurrencyManager == null) { return; }
            try
            {
                _isChangingIndex = true;
                /* We can't use the selectedIndex directly, since our listview may be sorted.
                 * So we have to find the index of the selected object within the original list. */
                CurrencyManager.Position = CurrencyManager.List.IndexOf(ListView.SelectedObject);
            }
            finally
            {
                _isChangingIndex = false;
            }
        }

        protected virtual void HandleListViewFreezing(object sender, FreezeEventArgs e)
        {
            if (_alreadyFreezing || e.FreezeLevel != 0) { return; }
            try
            {
                _alreadyFreezing = true;
                RebindDataSource(true);
            }
            finally
            {
                _alreadyFreezing = false;
            }
        }

        protected virtual void HandleListViewBindingContextChanged(object sender, EventArgs e)
        {
            RebindDataSource(false);
        }
    }
}