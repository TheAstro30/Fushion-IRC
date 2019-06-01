/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Windows.Forms;
using ircClient;

namespace ircScript.Classes.Structures
{
    public class ScriptArgs
    {
        /* This is passed to the parser from the calling function to provide extra arguments for
         * $nick/$chan, etc, so it doesn't have to passed via the string[] args param of the Parse
         * function */
        public Form ChildWindow { get; set; } /* Used when called from the CommandProcessor */

        public ClientConnection ClientConnection { get; set; } /* Used when called from the CommandProcessor */

        public string Nick { get; set; }

        public string Address { get; set; }

        public string Channel { get; set; }
    }
}
