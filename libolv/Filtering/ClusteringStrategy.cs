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
using libolv.Filtering.Filters;

namespace libolv.Filtering
{
    public class ClusteringStrategy : IClusteringStrategy
    {
        public static string NullLabel = "[null]";
        public static string EmptyLabel = "[empty]";

        private static string _defaultDisplayLabelFormatSingular = "{0} ({1} item)";
        private static string _defaultDisplayLabelFormatPural = "{0} ({1} items)";

        public ClusteringStrategy()
        {
            DisplayLabelFormatSingular = DefaultDisplayLabelFormatSingular;
            DisplayLabelFormatPlural = DefaultDisplayLabelFormatPlural;
        }

        /* Static properties */
        public static string DefaultDisplayLabelFormatSingular
        {
            get { return _defaultDisplayLabelFormatSingular; }
            set { _defaultDisplayLabelFormatSingular = value; }
        }

        public static string DefaultDisplayLabelFormatPlural
        {
            get { return _defaultDisplayLabelFormatPural; }
            set { _defaultDisplayLabelFormatPural = value; }
        }

        /* Public properties */
        public OlvColumn Column { get; set; }
        public string DisplayLabelFormatSingular { get; set; }
        public string DisplayLabelFormatPlural { get; set; }

        /* ICluster implementation */
        public virtual object GetClusterKey(object model)
        {
            return Column.GetValue(model);
        }

        public virtual ICluster CreateCluster(object clusterKey)
        {
            return new Cluster(clusterKey);
        }

        public virtual string GetClusterDisplayLabel(ICluster cluster)
        {
            var s = Column.ValueToString(cluster.ClusterKey) ?? NullLabel;
            if (string.IsNullOrEmpty(s))
            {
                s = EmptyLabel;
            }
            return ApplyDisplayFormat(cluster, s);
        }

        public virtual IModelFilter CreateFilter(IList valuesChosenForFiltering)
        {
            return new OneOfFilter(GetClusterKey, valuesChosenForFiltering);
        }

        protected virtual string ApplyDisplayFormat(ICluster cluster, string s)
        {
            string format = (cluster.Count == 1) ? DisplayLabelFormatSingular : DisplayLabelFormatPlural;
            return string.IsNullOrEmpty(format) ? s : string.Format(format, s, cluster.Count);
        }
    }
}
