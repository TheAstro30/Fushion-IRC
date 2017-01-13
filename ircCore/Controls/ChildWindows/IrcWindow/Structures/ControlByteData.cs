/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;

namespace ircCore.Controls.ChildWindows.IrcWindow.Structures
{
    public enum ControlByte
    {
        None = 0,
        Bold = 2,
        Color = 3,
        Normal = 15,
        Reverse = 22,
        Italic = 29,
        Underline = 31
    }
    
    [Serializable]
    public class ControlByteData
    {
        public ControlByte ControlByte { get; set; }

        public string OriginalColor { get; set; }

        public int Position { get; set; } /* Position in string (text starts after and is removed from wrapped lines) would make it quicker at drawing */

        public Color ForeColor { get; set; } /* text fore color */
        public Color BackColor { get; set; } /* text back color */
        
        public int CurrentMeasurement { get; set; }
    }
}
