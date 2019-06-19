/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Text;
using System.Text.RegularExpressions;
using ircCore.Utils;
using ircScript.Classes.Structures;

namespace ircScript.Classes.Parsers
{
    /* Script parser class */
    internal class ScriptParser
    {
        private readonly Regex _singleArgs = new Regex(@"\$\d+", RegexOptions.Compiled); /* Replaces $[N] */
        private readonly Regex _multiArgs = new Regex(@"\$\d+-", RegexOptions.Compiled); /* Replaces $[N]- */

        private readonly ScriptVariableParser _variables;
        private readonly ScriptVariables _localVariables;

        internal ScriptParser(ScriptVariables localVariables)
        {
            _localVariables = localVariables;
            _variables = new ScriptVariableParser(_localVariables);
        }
        
        internal string Parse(ScriptArgs e, string lineData, string[] args)
        {
            /* Main script "parser" - mainly for parsing arguments - replace $[N]- tokens first, single $[N]
             * tokens second then variables and internal identifiers last */
            return _variables.Parse(e, ReplaceTokens(_singleArgs,
                                                     ReplaceTokens(_multiArgs, lineData, args,
                                                     true), args, false));
        }

        /* Private parsing methods */
        private static string ReplaceTokens(Regex match, string lineData, string[] args, bool multi)
        {
            var m = match.Matches(lineData);
            if (m.Count > 0)
            {
                var sb = new StringBuilder(lineData);
                for (var i = m.Count - 1; i >= 0; i--)
                {
                    int index;
                    if (multi)
                    {
                        /* Attempt to get the numeric value of $[N]- */                        
                        if (!int.TryParse(m[i].Value.ReplaceEx("$", string.Empty).ReplaceEx("-", string.Empty), out index))
                        {
                            index = 0;
                        }
                        sb.Replace(m[i].Value,
                                   args != null && args.Length > 0 && args.Length - (index - 1) > 0
                                       ? string.Join(" ", args, index - 1, args.Length - (index - 1)).Replace(
                                           (char) 44, (char) 7)
                                       : string.Empty);
                    }
                    else
                    {
                        /* Single tokens $[N] */
                        if (!int.TryParse(m[i].Value.ReplaceEx("$", string.Empty), out index))
                        {
                            index = 0;
                        }
                        sb.Replace(m[i].Value,
                                   args != null && index - 1 < args.Length
                                       ? args[index - 1].Replace((char) 44, (char) 7)
                                       : string.Empty);
                    }
                }     
                return sb.ToString();
            }
            /* Short, but, again, sweet :P */
            return lineData;
        }
    }
}
