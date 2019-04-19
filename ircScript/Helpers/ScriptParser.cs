/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Text.RegularExpressions;
using ircScript.Classes.Structures;

namespace ircScript.Helpers
{
    /* Script parser class */
    public static class ScriptParser
    {
        private static readonly Regex ParamStrip = new Regex(@"\$\d+", RegexOptions.Compiled);

        /* Inherited on all subsequent "script" classes - this will grow over time and be improved upon */
        public static string ParseLine(ScriptArgs e, string lineData, string[] args)
        {
            /* Main script "parser" - mainly for parsing arguments - first check ScriptArgs */
            if (e != null)
            {
                /* Probably ugly code... */
                lineData = lineData.Replace("$chan", e.Channel).Replace("$nick", e.Nick);
            }
            if (args == null || args.Length == 0)
            {
                /* Strip out $1, $2 - etc */
                return ParamStrip.Replace(lineData, "");
            }
            /* Process command-line arguments - replacing $1, $2 etc */
            var index = 1;
            foreach (var s in args)
            {
                lineData = lineData.Replace(string.Format("${0}", index), s);
                index++;
            }
            /* Return the parsed line (remove any unused params) */
            return ParamStrip.Replace(lineData, "");
        }
    }
}
