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
using System.Collections;
using System.Linq;
using libolv.Implementation;

namespace libolv.Filtering.Filters
{
    public class OneOfFilter : IModelFilter
    {
        private AspectGetterDelegate _valueGetter;
        private IList _possibleValues;

        public OneOfFilter(AspectGetterDelegate valueGetter) : this(valueGetter, new ArrayList())
        {
            /* Empty */
        }

        public OneOfFilter(AspectGetterDelegate valueGetter, ICollection possibleValues)
        {
            _valueGetter = valueGetter;
            _possibleValues = new ArrayList(possibleValues);
        }

        public virtual AspectGetterDelegate ValueGetter
        {
            get { return _valueGetter; }
            set { _valueGetter = value; }
        }

        public virtual IList PossibleValues
        {
            get { return _possibleValues; }
            set { _possibleValues = value; }
        }

        public virtual bool Filter(object modelObject)
        {
            if (ValueGetter == null || PossibleValues == null || PossibleValues.Count == 0)
            {
                return false;
            }
            var result = ValueGetter(modelObject);
            var enumerable = result as IEnumerable;
            if (result is string || enumerable == null)
            {
                return DoesValueMatch(result);
            }
            return enumerable.Cast<object>().Any(DoesValueMatch);
        }

        protected virtual bool DoesValueMatch(object result)
        {
            return PossibleValues.Contains(result);
        }
    }
}
