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
using System;
using System.Drawing;

namespace ircScript.Controls.SyntaxHighlight.Controls.AutoComplete
{
    public class AutoCompleteItem
    {
        public int ImageIndex = -1;
        public object Tag;
        public string Text;
        private string _menuText;
        private string _toolTipText;
        private string _toolTipTitle;

        public AutoCompleteItem()
        {
            /* Empty constructor */
        }

        public AutoCompleteItem(string text)
        {
            Text = text;
        }

        public AutoCompleteItem(string text, int imageIndex)
            : this(text)
        {
            ImageIndex = imageIndex;
        }

        public AutoCompleteItem(string text, int imageIndex, string menuText)
            : this(text, imageIndex)
        {
            _menuText = menuText;
        }

        public AutoCompleteItem(string text, int imageIndex, string menuText, string toolTipTitle, string toolTipText)
            : this(text, imageIndex, menuText)
        {
            _toolTipTitle = toolTipTitle;
            _toolTipText = toolTipText;
        }

        public AutoCompleteMenu Parent { get; internal set; }

        public virtual string ToolTipTitle
        {
            get { return _toolTipTitle; }
            set { _toolTipTitle = value; }
        }

        public virtual string ToolTipText
        {
            get { return _toolTipText; }
            set { _toolTipText = value; }
        }

        public virtual string MenuText
        {
            get { return _menuText; }
            set { _menuText = value; }
        }

        public virtual Color ForeColor
        {
            get { return Color.Transparent; }
            set { throw new NotImplementedException("Override this property to change color"); }
        }

        public virtual Color BackColor
        {
            get { return Color.Transparent; }
            set { throw new NotImplementedException("Override this property to change color"); }
        }

        public virtual string GetTextForReplace()
        {
            return Text;
        }

        public virtual CompareResult Compare(string fragmentText)
        {
            if (Text.StartsWith(fragmentText, StringComparison.InvariantCultureIgnoreCase) && Text != fragmentText)
            {
                return CompareResult.VisibleAndSelected;
            }
            return CompareResult.Hidden;
        }

        public override string ToString()
        {
            return _menuText ?? Text;
        }

        public virtual void OnSelected(AutoCompleteMenu popupMenu, SelectedEventArgs e)
        {
            /* This method is called after item inserted into text */
        }
    }
}