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
using libolv.Implementation;

namespace libolv.Filtering.Filters
{
    public class FlagBitSetFilter : OneOfFilter
    {
        private List<UInt64> _possibleValuesAsUlongs = new List<UInt64>();

        public FlagBitSetFilter(AspectGetterDelegate valueGetter, ICollection possibleValues) : base(valueGetter, possibleValues)
        {
            ConvertPossibleValues();
        }

        public override IList PossibleValues
        {
            get { return base.PossibleValues; }
            set
            {
                base.PossibleValues = value;
                ConvertPossibleValues();
            }
        }

        private void ConvertPossibleValues()
        {
            _possibleValuesAsUlongs = new List<UInt64>();
            foreach (var x in PossibleValues)
            {
                _possibleValuesAsUlongs.Add(Convert.ToUInt64(x));
            }
        }

        protected override bool DoesValueMatch(object result)
        {
            try
            {
                var value = Convert.ToUInt64(result);
                return _possibleValuesAsUlongs.Any(flag => (value & flag) == flag);
            }
            catch (InvalidCastException)
            {
                return false;
            }
            catch (FormatException)
            {
                return false;
            }
        }        
    }
}
