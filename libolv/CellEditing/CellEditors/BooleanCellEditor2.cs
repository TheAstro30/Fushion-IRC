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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace libolv.CellEditing.CellEditors
{
    [ToolboxItem(false)]
    public class BooleanCellEditor2 : CheckBox
    {
        public bool? Value
        {
            get
            {
                switch (CheckState)
                {
                    case CheckState.Checked:
                        return true;

                    case CheckState.Indeterminate:
                        return null;

                    default:
                        return false;
                }
            }
            set
            {
                CheckState = value.HasValue
                                 ? (value.Value ? CheckState.Checked : CheckState.Unchecked)
                                 : CheckState.Indeterminate;
            }
        }

        public new HorizontalAlignment TextAlign
        {
            get
            {
                switch (CheckAlign)
                {
                    case ContentAlignment.MiddleRight:
                        return HorizontalAlignment.Right;

                    case ContentAlignment.MiddleCenter:
                        return HorizontalAlignment.Center;

                    default:
                        return HorizontalAlignment.Left;
                }
            }
            set
            {
                switch (value)
                {
                    case HorizontalAlignment.Left:
                        CheckAlign = ContentAlignment.MiddleLeft;
                        break;

                    case HorizontalAlignment.Center:
                        CheckAlign = ContentAlignment.MiddleCenter;
                        break;

                    case HorizontalAlignment.Right:
                        CheckAlign = ContentAlignment.MiddleRight;
                        break;
                }
            }
        }
    }
}
