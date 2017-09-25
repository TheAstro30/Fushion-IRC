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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using libolv.Implementation;
using libolv.Implementation.Munger;

namespace libolv.Utilities
{
    public interface IGenerator
    {
        void GenerateAndReplaceColumns(ObjectListView olv, Type type, bool allProperties);
        IList<OlvColumn> GenerateColumns(Type type, bool allProperties);
    }

    public class Generator : IGenerator
    {
        private static IGenerator _instance;

        /* Static convenience methods */
        public static IGenerator Instance
        {
            get { return _instance ?? (_instance = new Generator()); }
            set { _instance = value; }
        }

        public static void GenerateColumns(ObjectListView olv, IEnumerable enumerable)
        {
            GenerateColumns(olv, enumerable, false);
        }
        
        public static void GenerateColumns(ObjectListView olv, IEnumerable enumerable, bool allProperties)
        {
            /* Generate columns based on the type of the first model in the collection and then quit */
            if (enumerable != null)
            {
                foreach (var model in enumerable)
                {
                    Instance.GenerateAndReplaceColumns(olv, model.GetType(), allProperties);
                    return;
                }
            }
            /* If we reach here, the collection was empty, so we clear the list */
            Instance.GenerateAndReplaceColumns(olv, null, allProperties);
        }

        public static void GenerateColumns(ObjectListView olv, Type type)
        {
            Instance.GenerateAndReplaceColumns(olv, type, false);
        }
        
        public static void GenerateColumns(ObjectListView olv, Type type, bool allProperties)
        {
            Instance.GenerateAndReplaceColumns(olv, type, allProperties);
        }
        
        public static IList<OlvColumn> GenerateColumns(Type type)
        {
            return Instance.GenerateColumns(type, false);
        }

        /* Public interface */
        public virtual void GenerateAndReplaceColumns(ObjectListView olv, Type type, bool allProperties)
        {
            var columns = GenerateColumns(type, allProperties);
            var tlv = olv as TreeListView;
            if (tlv != null)
            {
                TryGenerateChildrenDelegates(tlv, type);
            }
            ReplaceColumns(olv, columns);
        }

        public virtual IList<OlvColumn> GenerateColumns(Type type, bool allProperties)
        {
            var columns = new List<OlvColumn>();            
            if (type == null)
            {
                return columns;
            }
            /* Iterate all public properties in the class and build columns from those that have
             * an OLVColumn attribute and that are not ignored. */
            foreach (var pinfo in type.GetProperties())
            {
                if (Attribute.GetCustomAttribute(pinfo, typeof (OlvIgnoreAttribute)) != null)
                {
                    continue;
                }
                var attr = Attribute.GetCustomAttribute(pinfo, typeof (OlvColumnAttribute)) as OlvColumnAttribute;
                if (attr == null)
                {
                    if (allProperties)
                    {
                        columns.Add(MakeColumnFromPropertyInfo(pinfo));
                    }
                }
                else
                {
                    columns.Add(MakeColumnFromAttribute(pinfo, attr));
                }
            }
            /* How many columns have DisplayIndex specifically set? */
            var countPositiveDisplayIndex = columns.Count(col => col.DisplayIndex >= 0);
            /* Give columns that don't have a DisplayIndex an incremental index */
            var columnIndex = countPositiveDisplayIndex;
            foreach (OlvColumn col in columns.Where(col => col.DisplayIndex < 0))
            {
                col.DisplayIndex = (columnIndex++);
            }
            columns.Sort((x, y) => x.DisplayIndex.CompareTo(y.DisplayIndex));
            return columns;
        }

        /* Implementation */
        protected virtual void ReplaceColumns(ObjectListView olv, IList<OlvColumn> columns)
        {
            olv.Reset();
            /* Are there new columns to add? */
            if (columns == null || columns.Count == 0)
            {
                return;
            }
            /* Setup the columns */
            olv.AllColumns.AddRange(columns);
            PostCreateColumns(olv);
        }

        public virtual void PostCreateColumns(ObjectListView olv)
        {
            if (olv.AllColumns.Exists(x => x.CheckBoxes))
            {
                olv.UseSubItemCheckBoxes = true;
            }
            if (olv.AllColumns.Exists(x => x.Index > 0 && (x.ImageGetter != null || !string.IsNullOrEmpty(x.ImageAspectName))))
            {
                olv.ShowImagesOnSubItems = true;
            }
            olv.RebuildColumns();
            olv.AutoSizeColumns();
        }

        protected virtual OlvColumn MakeColumnFromAttribute(PropertyInfo pinfo, OlvColumnAttribute attr)
        {
            return MakeColumn(pinfo.Name, DisplayNameToColumnTitle(pinfo.Name), pinfo.CanWrite, pinfo.PropertyType, attr);
        }

        protected virtual OlvColumn MakeColumnFromPropertyInfo(PropertyInfo pinfo)
        {
            return MakeColumn(pinfo.Name, DisplayNameToColumnTitle(pinfo.Name), pinfo.CanWrite, pinfo.PropertyType, null);
        }

        public virtual OlvColumn MakeColumnFromPropertyDescriptor(PropertyDescriptor pd)
        {
            var attr = pd.Attributes[typeof (OlvColumnAttribute)] as OlvColumnAttribute;
            return MakeColumn(pd.Name, DisplayNameToColumnTitle(pd.DisplayName), !pd.IsReadOnly, pd.PropertyType, attr);
        }

        protected virtual OlvColumn MakeColumn(string aspectName, string title, bool editable, Type propertyType, OlvColumnAttribute attr)
        {
            var column = MakeColumn(aspectName, title, attr);
            column.Name = (attr == null || string.IsNullOrEmpty(attr.Name)) ? aspectName : attr.Name;
            ConfigurePossibleBooleanColumn(column, propertyType);
            if (attr == null)
            {
                column.IsEditable = editable;
                return column;
            }
            column.AspectToStringFormat = attr.AspectToStringFormat;
            if (attr.IsCheckBoxesSet)
            {
                column.CheckBoxes = attr.CheckBoxes;
            }
            column.DisplayIndex = attr.DisplayIndex;
            column.FillsFreeSpace = attr.FillsFreeSpace;
            if (attr.IsFreeSpaceProportionSet)
            {
                column.FreeSpaceProportion = attr.FreeSpaceProportion;
            }
            column.GroupWithItemCountFormat = attr.GroupWithItemCountFormat;
            column.GroupWithItemCountSingularFormat = attr.GroupWithItemCountSingularFormat;
            column.Hyperlink = attr.Hyperlink;
            column.ImageAspectName = attr.ImageAspectName;
            column.IsEditable = attr.IsEditableSet ? attr.IsEditable : editable;
            column.IsTileViewColumn = attr.IsTileViewColumn;
            column.IsVisible = attr.IsVisible;
            column.MaximumWidth = attr.MaximumWidth;
            column.MinimumWidth = attr.MinimumWidth;
            column.Tag = attr.Tag;
            if (attr.IsTextAlignSet)
            {
                column.TextAlign = attr.TextAlign;
            }
            column.ToolTipText = attr.ToolTipText;
            if (attr.IsTriStateCheckBoxesSet)
            {
                column.TriStateCheckBoxes = attr.TriStateCheckBoxes;
            }
            column.UseInitialLetterForGroup = attr.UseInitialLetterForGroup;
            column.Width = attr.Width;
            if (attr.GroupCutoffs != null && attr.GroupDescriptions != null)
            {
                column.MakeGroupies(attr.GroupCutoffs, attr.GroupDescriptions);
            }
            return column;
        }

        protected virtual OlvColumn MakeColumn(string aspectName, string title, OlvColumnAttribute attr)
        {
            var columnTitle = (attr == null || string.IsNullOrEmpty(attr.Title)) ? title : attr.Title;
            return new OlvColumn(columnTitle, aspectName);
        }

        protected virtual string DisplayNameToColumnTitle(string displayName)
        {
            var title = displayName.Replace("_", " ");
            /* Put a space between a lower-case letter that is followed immediately by an upper case letter */
            title = Regex.Replace(title, @"(\p{Ll})(\p{Lu})", @"$1 $2");
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title);
        }

        protected virtual void ConfigurePossibleBooleanColumn(OlvColumn column, Type propertyType)
        {
            if (propertyType != typeof (bool) && propertyType != typeof (bool?) && propertyType != typeof (CheckState))
            {
                return;
            }
            column.CheckBoxes = true;
            column.TextAlign = HorizontalAlignment.Center;
            column.Width = 32;
            column.TriStateCheckBoxes = (propertyType == typeof (bool?) || propertyType == typeof (CheckState));
        }

        protected virtual void TryGenerateChildrenDelegates(TreeListView tlv, Type type)
        {
            foreach (var pinfo in from pinfo in type.GetProperties() let attr = Attribute.GetCustomAttribute(pinfo, typeof (OlvChildrenAttribute)) as OlvChildrenAttribute where attr != null select pinfo)
            {
                GenerateChildrenDelegates(tlv, pinfo);
                return;
            }
        }

        protected virtual void GenerateChildrenDelegates(TreeListView tlv, PropertyInfo pinfo)
        {
            var childrenGetter = new Munger(pinfo.Name);
            tlv.CanExpandGetter = delegate(object x)
                                      {
                                          try
                                          {
                                              var result = childrenGetter.GetValueEx(x) as IEnumerable;
                                              return !ObjectListView.IsEnumerableEmpty(result);
                                          }
                                          catch (MungerException ex)
                                          {
                                              System.Diagnostics.Debug.WriteLine(ex);
                                              return false;
                                          }
                                      };
            tlv.ChildrenGetter = delegate(object x)
                                     {
                                         try
                                         {
                                             return childrenGetter.GetValueEx(x) as IEnumerable;
                                         }
                                         catch (MungerException ex)
                                         {
                                             System.Diagnostics.Debug.WriteLine(ex);
                                             return null;
                                         }
                                     };
        }
    }
}