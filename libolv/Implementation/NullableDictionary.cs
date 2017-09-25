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
using System.Collections.Generic;
using System.Collections;

namespace libolv.Implementation
{
    internal class NullableDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        private bool _hasNullKey;
        private TValue _nullValue;

        public new TValue this[TKey key]
        {
            get
            {
                if (key.Equals(null))
                {
                    return base[key];
                }
                if (_hasNullKey)
                {
                    return _nullValue;
                }
                throw new KeyNotFoundException();
            }
            set
            {
                if (key.Equals(null))
                {
                    _hasNullKey = true;
                    _nullValue = value;
                }
                else
                {
                    base[key] = value;
                }
            }
        }

        public new bool ContainsKey(TKey key)
        {
            return key.Equals(null) ? _hasNullKey : base.ContainsKey(key);
        }

        public new IList Keys
        {
            get
            {
                var list = new ArrayList(base.Keys);
                if (_hasNullKey)
                {
// ReSharper disable AssignNullToNotNullAttribute
                    list.Add(null);
// ReSharper restore AssignNullToNotNullAttribute
                }
                return list;
            }
        }

        public new IList<TValue> Values
        {
            get
            {
                var list = new List<TValue>(base.Values);
                if (_hasNullKey)
                {
                    list.Add(_nullValue);
                }
                return list;
            }
        }
    }
}
