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
using System.Windows.Forms;

namespace libolv.Implementation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OlvColumnAttribute : Attribute
    {
        private bool _checkBoxes;
        private int _displayIndex = -1;
        private int _freeSpaceProportion;
        private bool _isVisible = true;
        private bool _isEditable = true;
        private int _maximumWidth = -1;
        private int _minimumWidth = -1;
        private HorizontalAlignment _textAlign = HorizontalAlignment.Left;
        private bool _triStateCheckBoxes;
        private int _width = 150;

        internal bool IsTriStateCheckBoxesSet;
        internal bool IsTextAlignSet;
        internal bool IsEditableSet;
        internal bool IsFreeSpaceProportionSet;
        internal bool IsCheckBoxesSet;

        public OlvColumnAttribute()
        {
            /* Empty */
        }

        public OlvColumnAttribute(string title)
        {
            Title = title;
        }

        /* Public properties */
        public string AspectToStringFormat { get; set; }

        public bool CheckBoxes
        {
            get { return _checkBoxes; }
            set
            {
                _checkBoxes = value;
                IsCheckBoxesSet = true;
            }
        }

        public int DisplayIndex
        {
            get { return _displayIndex; }
            set { _displayIndex = value; }
        }        

        public bool FillsFreeSpace { get; set; }

        public int FreeSpaceProportion
        {
            get { return _freeSpaceProportion; }
            set
            {
                _freeSpaceProportion = value;
                IsFreeSpaceProportionSet = true;
            }
        }

        public object[] GroupCutoffs { get; set; }
        public string[] GroupDescriptions { get; set; }
        public string GroupWithItemCountFormat { get; set; }
        public string GroupWithItemCountSingularFormat { get; set; }
        public bool Hyperlink { get; set; }
        public string ImageAspectName { get; set; }

        public bool IsEditable
        {
            get { return _isEditable; }
            set
            {
                _isEditable = value;
                IsEditableSet = true;
            }
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            set { _isVisible = value; }
        }

        public bool IsTileViewColumn { get; set; }

        public int MaximumWidth
        {
            get { return _maximumWidth; }
            set { _maximumWidth = value; }
        }
       
        public int MinimumWidth
        {
            get { return _minimumWidth; }
            set { _minimumWidth = value; }
        }

        public string Name { get; set; }

        public HorizontalAlignment TextAlign
        {
            get { return _textAlign; }
            set
            {
                _textAlign = value;
                IsTextAlignSet = true;
            }
        }

        public string Tag { get; set; }
        public string Title { get; set; }
        public string ToolTipText { get; set; }

        public bool TriStateCheckBoxes
        {
            get { return _triStateCheckBoxes; }
            set
            {
                _triStateCheckBoxes = value;
                IsTriStateCheckBoxesSet = true;
            }
        }

        public bool UseInitialLetterForGroup { get; set; }

        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }        
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class OlvChildrenAttribute : Attribute
    {
        /* Empty */
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class OlvIgnoreAttribute : Attribute
    {
        /* Empty */
    }
}
