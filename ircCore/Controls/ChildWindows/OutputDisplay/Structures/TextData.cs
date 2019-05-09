/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ircCore.Controls.ChildWindows.OutputDisplay.Structures
{
    public class LineIndexData
    {
        public int ActualLineNumber { get; set; }
        public int WrappedLineNumber { get; set; }
    }

    [Serializable]
    public class TextData
    {
        [Serializable]
        public class Text
        {
            public Color DefaultColor { get; set; }            

            public string Line { get; set; } /* The original unmodified line */

            public bool IsLineMarker { get; set; }
        }

        public bool LoadBuffer { get; set; } /* Used only when loading a buffer to stop a re-wrap if unnecessary */

        public int WindowWidth { get; set; } /* Used only when loading a buffer to stop a re-wrap if unnecessary */

        public List<Text> Lines { get; set; }

        public List<WrapData> Wrapped { get; set; } /* These are the lines drawn out using ControlByteData for positions */
        
        public int WrappedLinesCount
        {
            get { return Wrapped.Sum(o => o.Lines.Count); /* The sum of all wrapped lines */ }
        }

        public TextData()
        {
            Lines = new List<Text>();
            Wrapped = new List<WrapData>();
        }

        public LineIndexData GetWrappedIndexByLineNumber(int line)
        {           
            var currentLine = 0;
            for (var x = 0; x <= Wrapped.Count - 1; x++)
            {
                for (var y = 0; y <= Wrapped[x].Lines.Count - 1; y++)
                {                    
                    if (currentLine == line)
                    {                        
                        return new LineIndexData
                                   {
                                       ActualLineNumber = x,
                                       WrappedLineNumber = y
                                   };
                    }
                    currentLine++;
                }
            }
            return null;
        }
    }
}