/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Linq;
using ircScript.Classes.Parsers;
using ircScript.Classes.Structures;

namespace ircScript.Classes
{
    public class Script
    {
        /* Main script file - yes, it has flaws and a few things could be written a little better, but it works */
        private ScriptVariables _localVariables = new  ScriptVariables();

        /* Public properties */
        public ScriptEventParams EventParams = new ScriptEventParams();

        public string Name { get; set; }

        public List<string> LineData = new List<string>();

        /* Events raised by this class */
        public event Action<Script, ScriptArgs, string> LineParsed;
        public event Action<Script> ParseCompleted;

        /* Main entry point */
        public string Parse(ScriptArgs e, string[] args)
        {
            /* Make sure local variables are empty on each call of this script */
            _localVariables = new ScriptVariables();
            var parser = new ScriptParser(_localVariables);
            var conditional = new ScriptConditionalParser();
            var br = false;
            var finalResult = string.Empty;
            /* Parse each line - conditions then finally check for return/break */
            //foreach (var line in from line in LineData let parsed = parser.Parse(e, line, args) where conditional.Parse(parsed) select line)
            //{
            foreach (var line in LineData)
            {
                var parsed = parser.Parse(e, line, args);
                /* Now parse everything else (if, etc) */
                if (conditional.Parse(parsed))
                {
                    if (LineParsed != null)
                    {
                        LineParsed(this, e, parsed);
                    }
                }
                string com;
                //var l = parser.Parse(e, line, args);
                var arg = string.Empty;
                var i = parsed.IndexOf(' ');
                if (i != -1)
                {
                    com = parsed.Substring(0, i);
                    arg = parsed.Substring(i + 1);                        
                }
                else
                {
                    com = parsed;
                }                
                switch (com.ToUpper())
                {
                    case "RETURN":                    
                        /* Execution of this script stops here */   
                        br = true;
                        break;

                    case "BREAK":
                        /* Break is treated differently, ALL execution stops here and LineParsed is NOT raised */
                        if (ParseCompleted != null)
                        {
                            ParseCompleted(this);
                        }
                        return string.Empty;

                    default:
                        arg = parsed; /* Set line back to original parsed line to continue processing */
                        break;
                }
                /* Raise event with resultant output */
                finalResult = arg;
                if (LineParsed != null && !string.IsNullOrEmpty(arg))
                {
                    LineParsed(this, e, arg);
                }
                if (br)
                {
                    /* Return or break detected, exit execution of this loop */
                    break;
                }
            }
            if (ParseCompleted != null)
            {
                ParseCompleted(this);
            }
            /* Short, but sweet :P */
            return finalResult;
        }
    }
}
