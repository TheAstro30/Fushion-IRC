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

namespace libolv.Rendering.Styles
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class CellStyle : IItemStyle
    {
        public Font Font { get; set; }

        [DefaultValue(FontStyle.Regular)]
        public FontStyle FontStyle { get; set; }

        [DefaultValue(typeof(Color), "")]
        public Color ForeColor { get; set; }

        [DefaultValue(typeof(Color), "")]
        public Color BackColor { get; set; }
    }
}
