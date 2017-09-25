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
using System.Reflection;
using libolv.CellEditing.CellEditors;

namespace libolv.CellEditing
{
    public delegate Control EditorCreatorDelegate(Object model, OlvColumn column, Object value);

    public class EditorRegistry
    {

        private EditorCreatorDelegate _firstChanceCreator;
        private EditorCreatorDelegate _defaultCreator;
        private readonly Dictionary<Type, EditorCreatorDelegate> _creatorMap = new Dictionary<Type, EditorCreatorDelegate>();

        public EditorRegistry()
        {
            InitializeStandardTypes();
        }

        private void InitializeStandardTypes()
        {
            Register(typeof (Boolean), typeof (BooleanCellEditor));
            Register(typeof (Int16), typeof (IntUpDown));
            Register(typeof (Int32), typeof (IntUpDown));
            Register(typeof (Int64), typeof (IntUpDown));
            Register(typeof (UInt16), typeof (UintUpDown));
            Register(typeof (UInt32), typeof (UintUpDown));
            Register(typeof (UInt64), typeof (UintUpDown));
            Register(typeof (Single), typeof (FloatCellEditor));
            Register(typeof (Double), typeof (FloatCellEditor));
            Register(typeof (DateTime), delegate
                                            {
                                                var c = new DateTimePicker
                                                            {
                                                                Format = DateTimePickerFormat.Short
                                                            };
                                                return c;
                                            });
            Register(typeof (Boolean), delegate(Object model, OlvColumn column, Object value)
                                                {
                                                    CheckBox c = new BooleanCellEditor2();
                                                    c.ThreeState = column.TriStateCheckBoxes;
                                                    return c;
                                                });
        }

        /* Registering */
        public void Register(Type type, Type controlType)
        {
            Register(type, delegate
                               {
                                   return
                                       controlType.InvokeMember("", BindingFlags.CreateInstance, null, null, null)
                                       as Control;
                               });
        }

        public void Register(Type type, EditorCreatorDelegate creator)
        {
            _creatorMap[type] = creator;
        }

        public void RegisterDefault(EditorCreatorDelegate creator)
        {
            _defaultCreator = creator;
        }

        public void RegisterFirstChance(EditorCreatorDelegate creator)
        {
            _firstChanceCreator = creator;
        }

        /* Accessing */
        public Control GetEditor(Object model, OlvColumn column, Object value)
        {
            Control editor;
            /* Give the first chance delegate a chance to decide */
            if (_firstChanceCreator != null)
            {
                editor = _firstChanceCreator(model, column, value);
                if (editor != null)
                {
                    return editor;
                }
            }
            /* Try to find a creator based on the type of the value (or the column) */
            var type = value == null ? column.DataType : value.GetType();
            if (type != null && _creatorMap.ContainsKey(type))
            {
                editor = _creatorMap[type](model, column, value);
                if (editor != null)
                {
                    return editor;
                }
            }
            /* Enums without other processing get a special editor */
            if (value != null && value.GetType().IsEnum)
            {
                return CreateEnumEditor(value.GetType());
            }
            /* Give any default creator a final chance */
            return _defaultCreator != null ? _defaultCreator(model, column, value) : null;
        }

        protected Control CreateEnumEditor(Type type)
        {
            return new EnumCellEditor(type);
        }
    }
}
