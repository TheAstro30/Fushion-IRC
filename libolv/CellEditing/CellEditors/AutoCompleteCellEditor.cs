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
using System.ComponentModel;
using System.Windows.Forms;

namespace libolv.CellEditing.CellEditors
{
    [ToolboxItem(false)]
    public class AutoCompleteCellEditor : ComboBox
    {
        public AutoCompleteCellEditor(ObjectListView lv, OlvColumn column)
        {
            DropDownStyle = ComboBoxStyle.DropDown;
            var alreadySeen = new Dictionary<string, bool>();
            for (var i = 0; i < Math.Min(lv.GetItemCount(), 1000); i++)
            {
                var str = column.GetStringValue(lv.GetModelObject(i));
                if (alreadySeen.ContainsKey(str)) { continue; }
                Items.Add(str);
                alreadySeen[str] = true;
            }
            Sorted = true;
            AutoCompleteSource = AutoCompleteSource.ListItems;
            AutoCompleteMode = AutoCompleteMode.Append;
        }
    }
}
