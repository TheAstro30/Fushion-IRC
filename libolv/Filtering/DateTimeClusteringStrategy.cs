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
using System.Globalization;

namespace libolv.Filtering
{
    [Flags]
    public enum DateTimePortion
    {
        Year = 0x01,
        Month = 0x02,
        Day = 0x04,
        Hour = 0x08,
        Minute = 0x10,
        Second = 0x20
    }

    public class DateTimeClusteringStrategy : ClusteringStrategy
    {
        private DateTimePortion _portions = DateTimePortion.Year | DateTimePortion.Month;

        public DateTimeClusteringStrategy() : this(DateTimePortion.Year | DateTimePortion.Month, "MMMM yyyy")
        {
            /* Empty */
        }

        public DateTimeClusteringStrategy(DateTimePortion portions, string format)
        {
            Portions = portions;
            Format = format;
        }

        /* Properties */
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets the parts of the DateTime that will be extracted when
        /// determining the clustering key for an object.
        /// </summary>
        public DateTimePortion Portions
        {
            get { return _portions; }
            set { _portions = value; }
        }

        /* IClusterStrategy implementation */
        public override object GetClusterKey(object model)
        {
            /* Get the data attribute we want from the given model
             * Make sure the returned value is a DateTime */
            var dateTime = Column.GetValue(model) as DateTime?;
            if (!dateTime.HasValue)
            {
                return null;
            }
            /* Extract the parts of the datetime that we are intereted in.
             * Even if we aren't interested in a particular portion, we still have to give it a reasonable default
             * otherwise we won't be able to build a DateTime object for it */
            var year = ((Portions & DateTimePortion.Year) == DateTimePortion.Year) ? dateTime.Value.Year : 1;
            var month = ((Portions & DateTimePortion.Month) == DateTimePortion.Month) ? dateTime.Value.Month : 1;
            var day = ((Portions & DateTimePortion.Day) == DateTimePortion.Day) ? dateTime.Value.Day : 1;
            var hour = ((Portions & DateTimePortion.Hour) == DateTimePortion.Hour) ? dateTime.Value.Hour : 0;
            var minute = ((Portions & DateTimePortion.Minute) == DateTimePortion.Minute)
                             ? dateTime.Value.Minute
                             : 0;
            var second = ((Portions & DateTimePortion.Second) == DateTimePortion.Second)
                             ? dateTime.Value.Second
                             : 0;

            return new DateTime(year, month, day, hour, minute, second);
        }

        public override string GetClusterDisplayLabel(ICluster cluster)
        {
            var dateTime = cluster.ClusterKey as DateTime?;
            return ApplyDisplayFormat(cluster, dateTime.HasValue ? DateToString(dateTime.Value) : NullLabel);
        }

        protected virtual string DateToString(DateTime dateTime)
        {
            if (string.IsNullOrEmpty(Format))
            {
                return dateTime.ToString(CultureInfo.CurrentUICulture);
            }
            try
            {
                return dateTime.ToString(Format);
            }
            catch (FormatException)
            {
                return string.Format("Bad format string '{0}' for value '{1}'", Format, dateTime);
            }
        }
    }
}