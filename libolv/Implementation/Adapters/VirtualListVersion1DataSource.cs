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

namespace libolv.Implementation.Adapters
{
    public class VirtualListVersion1DataSource : AbstractVirtualListDataSource
    {
        public VirtualListVersion1DataSource(VirtualObjectListView listView) : base(listView)
        {
            /* Empty */
        }

        /* Public properties */
        public RowGetterDelegate RowGetter { get; set; }

        /* IVirtualListDataSource implementation */
        public override object GetNthObject(int n)
        {
            return RowGetter == null ? null : RowGetter(n);
        }

        public override int SearchText(string value, int first, int last, OlvColumn column)
        {
            return DefaultSearchText(value, first, last, column, this);
        }
    }
}
