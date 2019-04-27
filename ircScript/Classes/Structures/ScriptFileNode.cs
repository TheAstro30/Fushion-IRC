/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Collections.Generic;

namespace ircScript.Classes.Structures
{
    public class ScriptFileNode
    {
        public ScriptType Type { get; set; }

        public string Name { get; set; }

        public List<ScriptData> Data = new List<ScriptData>();
    }
}
