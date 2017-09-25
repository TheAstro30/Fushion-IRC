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
using System.Windows.Forms;
using libolv.Filtering.Filters;

namespace libolv.Implementation.Adapters
{
    public interface IVirtualListDataSource
    {
        Object GetNthObject(int n);
        int GetObjectCount();
        int GetObjectIndex(Object model);
        void PrepareCache(int first, int last);
        int SearchText(string value, int first, int last, OlvColumn column);
        void Sort(OlvColumn column, SortOrder order);
        void AddObjects(ICollection modelObjects);
        void RemoveObjects(ICollection modelObjects);
        void SetObjects(IEnumerable collection);
    }

    public interface IFilterableDataSource
    {
        void ApplyFilters(IModelFilter modelFilter, IListFilter listFilter);
    }

    public class AbstractVirtualListDataSource : IVirtualListDataSource, IFilterableDataSource
    {
        protected VirtualObjectListView ListView;

        public AbstractVirtualListDataSource(VirtualObjectListView listView)
        {
            ListView = listView;
        }

        public virtual object GetNthObject(int n)
        {
            return null;
        }

        public virtual int GetObjectCount()
        {
            return -1;
        }

        public virtual int GetObjectIndex(object model)
        {
            return -1;
        }

        public virtual void PrepareCache(int from, int to)
        {
            /* Empty */
        }

        public virtual int SearchText(string value, int first, int last, OlvColumn column)
        {
            return -1;
        }

        public virtual void Sort(OlvColumn column, SortOrder order)
        {
            /* Empty */
        }

        public virtual void AddObjects(ICollection modelObjects)
        {
            /* Empty */
        }

        public virtual void RemoveObjects(ICollection modelObjects)
        {
            /* Empty */
        }

        public virtual void SetObjects(IEnumerable collection)
        {
            /* Empty */
        }

        public static int DefaultSearchText(string value, int first, int last, OlvColumn column, IVirtualListDataSource source)
        {
            if (first <= last)
            {
                for (var i = first; i <= last; i++)
                {
                    var data = column.GetStringValue(source.GetNthObject(i));
                    if (data.StartsWith(value, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return i;
                    }
                }
            }
            else
            {
                for (var i = first; i >= last; i--)
                {
                    var data = column.GetStringValue(source.GetNthObject(i));
                    if (data.StartsWith(value, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        /* IFilterableDataSource Members */
        public virtual void ApplyFilters(IModelFilter modelFilter, IListFilter listFilter)
        {
            /* Empty */
        }
    }
}
