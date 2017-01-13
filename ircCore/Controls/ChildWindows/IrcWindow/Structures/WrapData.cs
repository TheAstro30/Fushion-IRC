/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace ircCore.Controls.ChildWindows.IrcWindow.Structures
{
    /* Wrapped line data class for output text */
    [Serializable]
    public class WrapData
    {
        [Serializable]
        public class WrapLineData
        {
            public string Text { get; set; }

            public List<ControlByteData> ControlBytes { get; set; }

            public bool BrokenAtStart { get; set; }
            public bool BrokenAtEnd { get; set; }

            public bool IsBold { get; set; }
            public bool IsUnderline { get; set; }
            public bool IsItalic { get; set; }

            public WrapLineData()
            {
                ControlBytes = new List<ControlByteData>();
            }            

            public List<ControlByte> GetControlBytesAtPosition(int position, out int index)
            {
                /* This gets a list of control bytes at a given position and returns what the control byte is at that position */
                index = -1;
                var cbl = new List<ControlByte>();
                foreach (var cb in ControlBytes.TakeWhile(cb => cb.Position <= position))
                {
                    index++;
                    if (cb.Position == position)
                    {                        
                        cbl.Add(cb.ControlByte);
                    }
                }
                return cbl;                
            }
        }

        public List<WrapLineData> Lines { get; set; }        

        public WrapData()
        {
            Lines = new List<WrapLineData>();
        }
    }
}
