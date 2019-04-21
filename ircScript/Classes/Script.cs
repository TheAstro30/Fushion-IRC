/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using ircScript.Classes.Parsers;
using ircScript.Classes.Structures;

namespace ircScript.Classes
{
    public class Script
    {
        /* Main script file - yes, it has flaws and a few things could be written a little better, but it works */
        private ScriptVariables _localVariables = new  ScriptVariables();

        /* Public properties */
        public string Name { get; set; }

        public List<string> LineData = new List<string>();

        /* Events raised by this class */
        public event Action<Script, ScriptArgs, string> LineParsed;
        public event Action<Script> ParseCompleted;

        /* Main entry point */
        public void Parse(ScriptArgs e, string[] args)
        {
            /* Make sure local variables are empty on each call of this script */
            _localVariables = new ScriptVariables();
            var parser = new ScriptParser(_localVariables);
            var conditional = new ScriptConditionalParser();
            /* Parse each line */
            foreach (var line in LineData)
            {
                var parsed = parser.Parse(e, line, args);
                /* Now parse everything else (if, etc) */
                if (conditional.Parse(parsed))
                {                    
                    if (LineParsed != null)
                    {
                        LineParsed(this, e, parser.Parse(e, line, args));
                    }
                }                                
            }
            if (ParseCompleted != null)
            {
                ParseCompleted(this);
            }
            /* Short, but sweet :P */
        }
    }
}
