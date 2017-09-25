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
using System.Windows.Forms;
using libolv.Utilities;

namespace libolv.DragDrop
{
    public class OlvDataObject : DataObject
    {
        private readonly bool _includeHiddenColumns;
        private readonly bool _includeColumnHeaders;
        private readonly ObjectListView _objectListView;
        private readonly IList _modelObjects;

        public OlvDataObject(ObjectListView olv) : this(olv, olv.SelectedObjects)
        {
            /* Empty */
        }

        public OlvDataObject(ObjectListView olv, IList modelObjects)
        {
            _objectListView = olv;
            _modelObjects = modelObjects;
            _includeHiddenColumns = olv.IncludeHiddenColumnsInDataTransfer;
            _includeColumnHeaders = olv.IncludeColumnHeadersInCopy;
            CreateTextFormats();
        }

        /* Properties */
        public bool IncludeHiddenColumns
        {
            get { return _includeHiddenColumns; }
        }

        public bool IncludeColumnHeaders
        {
            get { return _includeColumnHeaders; }
        }

        public ObjectListView ListView
        {
            get { return _objectListView; }
        }

        public IList ModelObjects
        {
            get { return _modelObjects; }
        }

        public void CreateTextFormats()
        {
            var exporter = CreateExporter();
            /* Put both the text and html versions onto the clipboard.
             * For some reason, SetText() with UnicodeText doesn't set the basic CF_TEXT format,
             * but using SetData() does. */
            SetData(exporter.ExportTo(OlvExporter.ExportFormat.TabSeparated));
            SetText(exporter.ExportTo(OlvExporter.ExportFormat.Csv), TextDataFormat.CommaSeparatedValue);
            SetText(ConvertToHtmlFragment(exporter.ExportTo(OlvExporter.ExportFormat.Html)), TextDataFormat.Html);
        }

        protected OlvExporter CreateExporter()
        {
            var exporter = new OlvExporter(ListView)
                               {
                                   IncludeColumnHeaders = IncludeColumnHeaders,
                                   IncludeHiddenColumns = IncludeHiddenColumns,
                                   ModelObjects = ModelObjects
                               };
            return exporter;
        }

        [Obsolete("Use OlvExporter directly instead", false)]
        public string CreateHtml()
        {
            var exporter = CreateExporter();
            return exporter.ExportTo(OlvExporter.ExportFormat.Html);
        }

        private static string ConvertToHtmlFragment(string fragment)
        {
            /* Minimal implementation of HTML clipboard format */
            const string source = "http://www.codeproject.com/Articles/16009/A-Much-Easier-to-Use-ListView";
            const String markerBlock =
                "Version:1.0\r\n" +
                "StartHTML:{0,8}\r\n" +
                "EndHTML:{1,8}\r\n" +
                "StartFragment:{2,8}\r\n" +
                "EndFragment:{3,8}\r\n" +
                "StartSelection:{2,8}\r\n" +
                "EndSelection:{3,8}\r\n" +
                "SourceURL:{4}\r\n" +
                "{5}";

            var prefixLength = string.Format(markerBlock, 0, 0, 0, 0, source, "").Length;
            const String defaultHtmlBody =
                "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">" +
                "<HTML><HEAD></HEAD><BODY><!--StartFragment-->{0}<!--EndFragment--></BODY></HTML>";

            var html = string.Format(defaultHtmlBody, fragment);
            var startFragment = prefixLength + html.IndexOf(fragment, StringComparison.Ordinal);
            var endFragment = startFragment + fragment.Length;
            return string.Format(markerBlock, prefixLength, prefixLength + html.Length, startFragment, endFragment,
                                 source, html);
        }
    }
}