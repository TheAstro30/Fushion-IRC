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
using System.Windows.Forms;
using System.Drawing;
using libolv.Rendering.Styles;

namespace libolv.Implementation
{
    public delegate Object AspectGetterDelegate(Object rowObject);
    public delegate void AspectPutterDelegate(Object rowObject, Object newValue);
    public delegate string AspectToStringConverterDelegate(Object value);
    public delegate String CellToolTipGetterDelegate(OlvColumn column, Object modelObject);
    public delegate CheckState CheckStateGetterDelegate(Object rowObject);
    public delegate bool BooleanCheckStateGetterDelegate(Object rowObject);
    public delegate CheckState CheckStatePutterDelegate(Object rowObject, CheckState newValue);
    public delegate bool BooleanCheckStatePutterDelegate(Object rowObject, bool newValue);
    public delegate void ColumnRightClickEventHandler(object sender, ColumnClickEventArgs e);
    public delegate bool HeaderDrawingDelegate(Graphics g, Rectangle r, int columnIndex, OlvColumn column, bool isPressed, HeaderStateStyle stateStyle);
    public delegate void GroupFormatterDelegate(OlvGroup group, GroupingParameters parms);
    public delegate Object GroupKeyGetterDelegate(Object rowObject);
    public delegate string GroupKeyToTitleConverterDelegate(Object groupKey);
    public delegate String HeaderToolTipGetterDelegate(OlvColumn column);
    public delegate Object ImageGetterDelegate(Object rowObject);
    public delegate bool RenderDelegate(EventArgs e, Graphics g, Rectangle r, Object rowObject);
    public delegate Object RowGetterDelegate(int rowIndex);
    public delegate void RowFormatterDelegate(OlvListItem olvItem);
    public delegate void SortDelegate(OlvColumn column, SortOrder sortOrder);
}
