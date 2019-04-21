/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Collections.Generic;
using ircScript.Classes.Structures;

namespace ircScript.Classes
{
    public enum ScriptFileNodeType
    {
        Aliases = 0,
        Popups = 1,
        Variables = 2
    }

    public class ScriptFileNode
    {
        public ScriptFileNodeType Type { get; set; }

        public string Name { get; set; }

        public List<ScriptData> Data = new List<ScriptData>();
    }
}
