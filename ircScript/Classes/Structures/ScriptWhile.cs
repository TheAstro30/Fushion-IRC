/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
namespace ircScript.Classes.Structures
{
    internal class ScriptWhile
    {
        /* Basic "loop" class for scripting while (condition) { */
        public bool Execute { get; set; }

        public int StartLine { get; set; }

        public int EndBlock { get; set; }
    }
}
