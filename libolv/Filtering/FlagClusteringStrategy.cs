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
using System.Globalization;
using System.Linq;
using libolv.Filtering.Filters;

namespace libolv.Filtering
{
    public class FlagClusteringStrategy : ClusteringStrategy
    {
        public FlagClusteringStrategy(Type enumType)
        {
            if (enumType == null) { throw new ArgumentNullException("enumType"); }
            if (!enumType.IsEnum) { throw new ArgumentException(@"Type must be enum", "enumType"); }
            if (enumType.GetCustomAttributes(typeof (FlagsAttribute), false) == null)
            {
                throw new ArgumentException(@"Type must have [Flags] attribute", "enumType");
            }
            SetValues((from object x in Enum.GetValues(enumType) select Convert.ToInt64(x)).ToArray(),
                      Enum.GetNames(enumType).ToArray());
        }

        public FlagClusteringStrategy(long[] values, string[] labels)
        {
            SetValues(values, labels);
        }

        /* Implementation */

        public long[] Values { get; private set; }
        public string[] Labels { get; private set; }

        private void SetValues(long[] flags, string[] flagLabels)
        {
            if (flags == null || flags.Length == 0) throw new ArgumentNullException("flags");
            if (flagLabels == null || flagLabels.Length == 0) throw new ArgumentNullException("flagLabels");
            if (flags.Length != flagLabels.Length)
            {
                throw new ArgumentException(@"values and labels must have the same number of entries", "flags");
            }
            Values = flags;
            Labels = flagLabels;
        }

        /* Implementation of IClusteringStrategy */
        public override object GetClusterKey(object model)
        {
            var flags = new List<long>();
            try
            {
                var modelValue = Convert.ToInt64(Column.GetValue(model));
                flags.AddRange(Values.Where(x => (x & modelValue) == x));
                return flags;
            }
            catch (InvalidCastException ex)
            {
                System.Diagnostics.Debug.Write(ex);
                return flags;
            }
            catch (FormatException ex)
            {
                System.Diagnostics.Debug.Write(ex);
                return flags;
            }
        }

        public override string GetClusterDisplayLabel(ICluster cluster)
        {
            var clusterKeyAsUlong = Convert.ToInt64(cluster.ClusterKey);
            for (var i = 0; i < Values.Length; i++)
            {
                if (clusterKeyAsUlong == Values[i])
                {
                    return ApplyDisplayFormat(cluster, Labels[i]);
                }
            }
            return ApplyDisplayFormat(cluster, clusterKeyAsUlong.ToString(CultureInfo.CurrentUICulture));
        }

        public override IModelFilter CreateFilter(IList valuesChosenForFiltering)
        {
            return new FlagBitSetFilter(GetClusterKey, valuesChosenForFiltering);
        }
    }
}