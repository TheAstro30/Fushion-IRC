//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE.
//
//  License: GNU Lesser General Public License (LGPLv3)
//
//  Email: pavel_torgashov@ukr.net
//
//  Copyright (C) Pavel Torgashov, 2011-2016.
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace ircScript.Controls.SyntaxHighlight.Helpers
{
    public class MacrosManager
    {
        private readonly List<object> _macro = new List<object>();
        private bool _isRecording;

        public bool AllowMacroRecordingByUser { get; set; }

        public bool IsRecording
        {
            get { return _isRecording; }
            set { _isRecording = value; UnderlayingControl.Invalidate(); }
        }

        public FastColoredTextBox UnderlayingControl { get; private set; }

        public bool MacroIsEmpty { get { return _macro.Count == 0; } }

        public string Macros
        {
            get
            {
                var cult = Thread.CurrentThread.CurrentUICulture;
                Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
                var kc = new KeysConverter();
                var sb = new StringBuilder();
                sb.AppendLine("<macros>");
                foreach (var item in _macro)
                {
                    if (item is Keys)
                    {
                        sb.AppendFormat("<item key='{0}' />\r\n", kc.ConvertToString((Keys)item));
                    }
                    else if (item is KeyValuePair<char, Keys>)
                    {
                        var p = (KeyValuePair<char, Keys>)item;
                        sb.AppendFormat("<item char='{0}' key='{1}' />\r\n", (int)p.Key, kc.ConvertToString(p.Value));
                    }
                }
                sb.AppendLine("</macros>");
                Thread.CurrentThread.CurrentUICulture = cult;
                return sb.ToString();
            }
            set
            {
                _isRecording = false;
                ClearMacros();
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }
                var doc = new XmlDocument();
                doc.LoadXml(value);
                var list = doc.SelectNodes("./macros/item");
                var cult = Thread.CurrentThread.CurrentUICulture;
                Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
                var kc = new KeysConverter();
                if (list != null)
                {
                    foreach (XmlElement node in list)
                    {
                        var ca = node.GetAttributeNode("char");
                        var ka = node.GetAttributeNode("key");
                        object convertFromString;
                        if (ca != null)
                        {
                            if (ka != null)
                            {
                                convertFromString = kc.ConvertFromString(ka.Value);
                                if (convertFromString != null)
                                {
                                    AddCharToMacros((char)int.Parse(ca.Value), (Keys)convertFromString);
                                }
                            }
                            else
                            {
                                AddCharToMacros((char)int.Parse(ca.Value), Keys.None);
                            }
                        }
                        else
                        {
                            if (ka != null)
                            {
                                convertFromString = kc.ConvertFromString(ka.Value);
                                if (convertFromString != null)
                                {
                                    AddKeyToMacros((Keys)convertFromString);
                                }
                            }
                        }
                    }
                }
                Thread.CurrentThread.CurrentUICulture = cult;
            }
        }

        internal MacrosManager(FastColoredTextBox ctrl)
        {
            UnderlayingControl = ctrl;
            AllowMacroRecordingByUser = true;
        }

        public void ExecuteMacros()
        {
            IsRecording = false;
            UnderlayingControl.BeginUpdate();
            UnderlayingControl.Selection.BeginUpdate();
            UnderlayingControl.BeginAutoUndo();
            foreach (var item in _macro)
            {
                if (item is Keys)
                {
                    UnderlayingControl.ProcessKey((Keys)item);
                }
                if (!(item is KeyValuePair<char, Keys>))
                {
                    continue;
                }
                var p = (KeyValuePair<char, Keys>)item;
                UnderlayingControl.ProcessKey(p.Key, p.Value);
            }
            UnderlayingControl.EndAutoUndo();
            UnderlayingControl.Selection.EndUpdate();
            UnderlayingControl.EndUpdate();
        }

        public void AddCharToMacros(char c, Keys modifiers)
        {
            _macro.Add(new KeyValuePair<char, Keys>(c, modifiers));
        }

        public void AddKeyToMacros(Keys keyData)
        {
            _macro.Add(keyData);
        }

        public void ClearMacros()
        {
            _macro.Clear();
        }

        internal void ProcessKey(Keys keyData)
        {
            if (IsRecording)
            {
                AddKeyToMacros(keyData);
            }
        }

        internal void ProcessKey(char c, Keys modifiers)
        {
            if (IsRecording)
            {
                AddCharToMacros(c, modifiers);
            }
        }
    }
}