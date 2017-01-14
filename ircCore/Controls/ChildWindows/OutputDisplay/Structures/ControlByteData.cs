/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using ircCore.Controls.ChildWindows.Helpers;

namespace ircCore.Controls.ChildWindows.OutputDisplay.Structures
{   
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
