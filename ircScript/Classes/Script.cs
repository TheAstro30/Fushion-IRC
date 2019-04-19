/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Linq;
using ircScript.Classes.Structures;
using ircScript.Helpers;

namespace ircScript.Classes
{
    public class Script
    {
        /* Main script file */
        public string Name { get; set; }

        public List<string> LineData = new List<string>();

        public event Action<Script, ScriptArgs, string> LineParsed;
        public event Action<Script> ParseCompleted;

        public void Parse(ScriptArgs e, string[] args)
        {
            /* Parse each line */
            foreach (var line in LineData.Where(line => LineParsed != null))
            {                
                LineParsed(this, e, ScriptParser.ParseLine(e, line, args));
            }
            if (ParseCompleted != null)
            {
                ParseCompleted(this);
            }
        }
    }
}
