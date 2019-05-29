/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using ircCore.Settings.SettingsBase.Structures;
using ircScript.Classes.Parsers;
using ircScript.Classes.Structures;

namespace ircScript.Classes
{
    public class Script
    {
        /* Main script file - yes, it has flaws and a few things could be written a little better, but it works */
        private ScriptVariables _localVariables = new  ScriptVariables();

        /* Public properties */
        public PopupType PopupType { get; set; }

        public ScriptEventParams EventParams = new ScriptEventParams();

        public string Name { get; set; }

        public List<string> LineData = new List<string>();

        /* Events raised by this class */
        public event Action<Script, ScriptArgs, string> LineParsed;
        public event Action<Script> ParseCompleted;

        /* Main entry point - this method can be used 2 ways
         * 1) Directly to return a value, or
         * 2) Event based.
         * The reason for this design was so the CommandProcessor of the main client didn't look huge 
         * with a shit load of code for "is this an alias, OK, get return value, pass it back to
         * CommandProcessor...", it just looks more tidy if this method raised events.
         * However, calling this as a $alias, for instance, events won't work (well, they will
         * but the code looks messy as SHIT) - so, having a direct return value option is a better
         * solution in that instance.
         */
        public string Parse()
        {
            return Parse(null, null);
        }

        public string Parse(ScriptArgs e)
        {
            return Parse(e, null);
        }

        public string Parse(ScriptArgs e, string[] args)
        {
            /* Make sure local variables are empty on each call of this script */
            _localVariables = new ScriptVariables();
            var parser = new ScriptParser(_localVariables);
            var conditional = new ScriptConditionalParser();
            var br = false;
            var finalResult = string.Empty;            
            /* Parse each line - conditions then finally check for return/break */
            for (var l = 0; l < LineData.Count; l++)
            {
                var line = LineData[l];
                var parsed = parser.Parse(e, line, args);
                if (!conditional.Parse(parsed, ref l))
                {
                    continue;
                }
                /* Now parse everything else (if, etc) */
                string com;
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

                    case "HALT":
                        /* ALL execution stops here and LineParsed is NOT raised */
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

        public override string ToString()
        {
            return Name;
        }
    }
}
