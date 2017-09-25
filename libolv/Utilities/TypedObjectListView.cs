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
using libolv.Implementation;

namespace libolv.Utilities
{
    public class TypedObjectListView<T> where T : class
    {
        public delegate CheckState TypedCheckStateGetterDelegate(T rowObject);
        public delegate bool TypedBooleanCheckStateGetterDelegate(T rowObject);
        public delegate CheckState TypedCheckStatePutterDelegate(T rowObject, CheckState newValue);
        public delegate bool TypedBooleanCheckStatePutterDelegate(T rowObject, bool newValue);
        public delegate String TypedCellToolTipGetterDelegate(OlvColumn column, T modelObject);

        private ObjectListView _olv;
        private TypedCheckStateGetterDelegate _checkStateGetter;
        private TypedCheckStatePutterDelegate _checkStatePutter;

        public TypedObjectListView(ObjectListView olv)
        {
            _olv = olv;
        }

        public virtual T CheckedObject
        {
            get { return (T)_olv.CheckedObject; }
        }

        public virtual IList<T> CheckedObjects
        {
            get
            {
                var checkedObjects = _olv.CheckedObjects;
                var objects = new List<T>(checkedObjects.Count);
                objects.AddRange(checkedObjects.Cast<T>());
                return objects;
            }
            set { _olv.CheckedObjects = (IList)value; }
        }

        public virtual ObjectListView ListView
        {
            get { return _olv; }
            set { _olv = value; }
        }

        public virtual IList<T> Objects
        {
            get
            {
                var objects = new List<T>(_olv.GetItemCount());
                for (var i = 0; i < _olv.GetItemCount(); i++)
                {
                    objects.Add(GetModelObject(i));
                }
                return objects;
            }
            set { _olv.SetObjects(value); }
        }

        public virtual T SelectedObject
        {
            get { return (T)_olv.SelectedObject; }
            set { _olv.SelectedObject = value; }
        }

        public virtual IList<T> SelectedObjects
        {
            get
            {
                var objects = new List<T>(_olv.SelectedIndices.Count);
                objects.AddRange(from int index in _olv.SelectedIndices select (T)_olv.GetModelObject(index));
                return objects;
            }
            set { _olv.SelectedObjects = (IList)value; }
        }

        public virtual TypedColumn<T> GetColumn(int i)
        {
            return new TypedColumn<T>(_olv.GetColumn(i));
        }

        public virtual TypedColumn<T> GetColumn(string name)
        {
            return new TypedColumn<T>(_olv.GetColumn(name));
        }

        public virtual T GetModelObject(int index)
        {
            return (T)_olv.GetModelObject(index);
        }
       
        public virtual TypedCheckStateGetterDelegate CheckStateGetter
        {
            get { return _checkStateGetter; }
            set
            {
                _checkStateGetter = value;
                if (value == null)
                {
                    _olv.CheckStateGetter = null;
                }
                else
                {
                    _olv.CheckStateGetter = x => _checkStateGetter((T)x);
                }
            }
        }

        public virtual TypedBooleanCheckStateGetterDelegate BooleanCheckStateGetter
        {
            set
            {
                if (value == null)
                {
                    _olv.BooleanCheckStateGetter = null;
                }
                else
                {
                    _olv.BooleanCheckStateGetter = x => value((T)x);
                }
            }
        }

        public virtual TypedCheckStatePutterDelegate CheckStatePutter
        {
            get { return _checkStatePutter; }
            set
            {
                _checkStatePutter = value;
                if (value == null)
                {
                    _olv.CheckStatePutter = null;
                }
                else
                {
                    _olv.CheckStatePutter = (x, newValue) => _checkStatePutter((T)x, newValue);
                }
            }
        }

        public virtual TypedBooleanCheckStatePutterDelegate BooleanCheckStatePutter
        {
            set
            {
                if (value == null)
                {
                    _olv.BooleanCheckStatePutter = null;
                }
                else
                {
                    _olv.BooleanCheckStatePutter = (x, newValue) => value((T)x, newValue);
                }
            }
        }

        public virtual TypedCellToolTipGetterDelegate CellToolTipGetter
        {
            set
            {
                if (value == null)
                {
                    _olv.CellToolTipGetter = null;
                }
                else
                {
                    _olv.CellToolTipGetter = (col, x) => value(col, (T)x);
                }
            }
        }

        public virtual HeaderToolTipGetterDelegate HeaderToolTipGetter
        {
            get { return _olv.HeaderToolTipGetter; }
            set { _olv.HeaderToolTipGetter = value; }
        }

        /* Commands */
        public virtual void GenerateAspectGetters()
        {
            for (var i = 0; i < ListView.Columns.Count; i++)
            {
                GetColumn(i).GenerateAspectGetter();
            }
        }
    }
}