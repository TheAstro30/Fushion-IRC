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
using System.Windows.Forms;

namespace ircScript.Controls.SyntaxHighlight.Forms.Hotkeys
{
    internal class HotkeyWrapper
    {
        private bool _ctrl;
        private bool _shift;
        private bool _alt;

        public HotkeyWrapper(Keys keyData, FctbAction action)
        {
            var a = new KeyEventArgs(keyData);
            _ctrl = a.Control;
            _shift = a.Shift;
            _alt = a.Alt;

            Key = a.KeyCode;
            Action = action;
        }

        public Keys ToKeyData()
        {
            var res = Key;
            if (_ctrl) res |= Keys.Control;
            if (_alt) res |= Keys.Alt;
            if (_shift) res |= Keys.Shift;
            return res;
        }

        public string Modifiers
        {
            get
            {
                var res = "";
                if (_ctrl)
                {
                    res += "Ctrl + ";
                }
                if (_shift)
                {
                    res += "Shift + ";
                }
                if (_alt)
                {
                    res += "Alt + ";
                }
                return res.Trim(' ', '+');
            }
            set
            {
                if (value == null)
                {
                    _ctrl = _alt = _shift = false;
                }
                else
                {
                    _ctrl = value.Contains("Ctrl");
                    _shift = value.Contains("Shift");
                    _alt = value.Contains("Alt");
                }
            }
        }

        public Keys Key { get; set; }
        public FctbAction Action { get; set; }
    }
}
