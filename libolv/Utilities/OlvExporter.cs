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
using System.Text;

namespace libolv.Utilities
{
    public class OlvExporter
    {
        public enum ExportFormat
        {
            TabSeparated = 1,
            Tsv = 1,
            Csv = 2,
            Html = 3
        }

        private delegate string StringToString(string str);

        private bool _includeColumnHeaders = true;
        private IList _modelObjects = new ArrayList();
        private Dictionary<ExportFormat, string> _results;

        public OlvExporter()
        {
            /* Empty */
        }

        public OlvExporter(ObjectListView olv) : this(olv, olv.Objects)
        {
            /* Empty */
        }

        public OlvExporter(ObjectListView olv, IEnumerable objectsToExport)
        {
            if (olv == null) { throw new ArgumentNullException("olv"); }
            if (objectsToExport == null) { throw new ArgumentNullException("objectsToExport"); }
            ListView = olv;
            ModelObjects = ObjectListView.EnumerableToArray(objectsToExport, true);
        }

        /* Properties */
        public bool IncludeHiddenColumns { get; set; }

        public bool IncludeColumnHeaders
        {
            get { return _includeColumnHeaders; }
            set { _includeColumnHeaders = value; }
        }

        public ObjectListView ListView { get; set; }

        public IList ModelObjects
        {
            get { return _modelObjects; }
            set { _modelObjects = value; }
        }

        /* Commands */
        public string ExportTo(ExportFormat format)
        {
            if (_results == null)
            {
                Convert();
            }
            return _results != null ? _results[format] : null;
        }

        public void Convert()
        {

            IList<OlvColumn> columns = IncludeHiddenColumns
                                           ? ListView.AllColumns
                                           : ListView.ColumnsInDisplayOrder;

            var sbText = new StringBuilder();
            var sbCsv = new StringBuilder();
            var sbHtml = new StringBuilder("<table>");
            /* Include column headers */
            if (IncludeColumnHeaders)
            {
                var strings = columns.Select(col => col.Text).ToList();
                WriteOneRow(sbText, strings, "", "\t", "", null);
                WriteOneRow(sbHtml, strings, "<tr><td>", "</td><td>", "</td></tr>", HtmlEncode);
                WriteOneRow(sbCsv, strings, "", ",", "", CsvEncode);
            }

            foreach (var strings in from object modelObject in ModelObjects select columns.Select(col => col.GetStringValue(modelObject)).ToList())
            {
                WriteOneRow(sbText, strings, "", "\t", "", null);
                WriteOneRow(sbHtml, strings, "<tr><td>", "</td><td>", "</td></tr>", HtmlEncode);
                WriteOneRow(sbCsv, strings, "", ",", "", CsvEncode);
            }
            sbHtml.AppendLine("</table>");

            _results = new Dictionary<ExportFormat, string>();
            _results[ExportFormat.TabSeparated] = sbText.ToString();
            _results[ExportFormat.Csv] = sbCsv.ToString();
            _results[ExportFormat.Html] = sbHtml.ToString();
        }        

        private static void WriteOneRow(StringBuilder sb, IEnumerable<string> strings, string startRow, string betweenCells, string endRow, StringToString encoder)
        {
            sb.Append(startRow);
            var first = true;
            foreach (var s in strings)
            {
                if (!first)
                {
                    sb.Append(betweenCells);
                }
                sb.Append(encoder == null ? s : encoder(s));
                first = false;
            }
            sb.AppendLine(endRow);
        }

        /* Encoding */
        private static string CsvEncode(string text)
        {
            if (text == null)
            {
                return null;
            }
            const string doublequote = @""""; /* one double quote */
            const string twodoubequotes = @""""""; /* two double quotes */

            var sb = new StringBuilder(doublequote);
            sb.Append(text.Replace(doublequote, twodoubequotes));
            sb.Append(doublequote);

            return sb.ToString();
        }
        
        private static string HtmlEncode(string text)
        {
            if (text == null)
            {
                return null;
            }
            var sb = new StringBuilder(text.Length);

            var len = text.Length;
            for (var i = 0; i < len; i++)
            {
                switch (text[i])
                {
                    case '<':
                        sb.Append("&lt;");
                        break;

                    case '>':
                        sb.Append("&gt;");
                        break;

                    case '"':
                        sb.Append("&quot;");
                        break;

                    case '&':
                        sb.Append("&amp;");
                        break;

                    default:
                        if (text[i] > 159)
                        {
                            /* decimal numeric entity */
                            sb.Append("&#");
                            sb.Append(((int)text[i]).ToString(CultureInfo.InvariantCulture));
                            sb.Append(";");
                        }
                        else
                        {
                            sb.Append(text[i]);
                        }
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
