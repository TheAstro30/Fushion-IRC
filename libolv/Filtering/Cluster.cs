/*
 * Cluster - Implements a simple cluster
 *
 * Author: Phillip Piper
 * Date: 3-March-2011 10:53 pm
 *
 * Change log:
 * 2011-03-03  JPP  - First version
 * 
 * Copyright (C) 2011-2012 Phillip Piper
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

namespace libolv.Filtering
{
    public class Cluster : ICluster
    {
        public Cluster(object key)
        {
            Count = 1;
            ClusterKey = key;
        }

        public override string ToString()
        {
            return DisplayLabel ?? "[empty]";
        }

        /* Implementation of ICluster */
        public int Count { get; set; }
        public string DisplayLabel { get; set; }
        public object ClusterKey { get; set; }

        /* Implementation of IComparable */
        public int CompareTo(object other)
        {
            if (other == null || other == DBNull.Value)
            {
                return 1;
            }
            var otherCluster = other as ICluster;
            if (otherCluster == null)
            {
                return 1;
            }
            var keyAsString = ClusterKey as string;
            if (keyAsString != null)
            {
                return String.Compare(keyAsString, otherCluster.ClusterKey as string,
                                      StringComparison.CurrentCultureIgnoreCase);
            }
            var keyAsComparable = ClusterKey as IComparable;
            if (keyAsComparable != null)
            {
                return keyAsComparable.CompareTo(otherCluster.ClusterKey);
            }
            return -1;
        }
    }
}
