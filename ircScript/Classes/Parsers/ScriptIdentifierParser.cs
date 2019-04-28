/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */

using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ircScript.Classes.Structures;

namespace ircScript.Classes.Parsers
{
    internal class ScriptIdentifierParser
    {
        internal class IdentifierParams
        {
            public string Id { get; set; }

            public string Args { get; set; }
        }

        private readonly Regex _tokenIdentifiers = new Regex(@"\$\w+", RegexOptions.Compiled); /* Replaces $chan, etc */
        private readonly Regex _tokenParenthisisParts = new Regex(@"\$([a-zA-Z_][a-zA-Z0-9_]*)\(((?<BR>\()|(?<-BR>\))|[^()]*)+\)", RegexOptions.Compiled);
        private readonly Regex _parenthisisPart = new Regex(@"^\$(?<id>\w+?)\((?<args>.+)\)$", RegexOptions.Compiled);

        private Stack<IdentifierParams> _nestedIdStack;

        public string Parse(ScriptArgs e, string lineData)
        {
            /* Parse parenthisised $id(args) first */            
            lineData = ParseParenthisis(e, lineData);
            var sb = new StringBuilder(lineData);
            var m = _tokenIdentifiers.Matches(lineData);
            if (m.Count > 0)
            {
                for (var i = m.Count - 1; i >= 0; i--)
                {
                    if (e == null)
                    {
                        /* Remove the identifers found */
                        sb.Replace(m[i].Value, "", m[i].Index, m[i].Length);
                    }
                    else
                    {
                        /* Replace identifiers from the values in ScriptArgs */
                        switch (m[i].Value.ToUpper())
                        {
                            case "$ME":
                                sb.Replace(m[i].Value, e.ClientConnection.UserInfo.Nick, m[i].Index, m[i].Value.Length);
                                break;

                            case "$CHAN":
                                sb.Replace(m[i].Value, e.Channel, m[i].Index, m[i].Value.Length);
                                break;

                            case "$NICK":
                                sb.Replace(m[i].Value, e.Nick, m[i].Index, m[i].Value.Length);
                                break;

                            default:
                                /* Check if it's an alias */
                                var script = ScriptManager.GetScript(ScriptManager.Aliases, m[i].Value.Replace("$", ""));
                                var rec = string.Empty;
                                if (script != null)
                                {
                                   rec = script.Parse(e, null); /* Args aren't required here */
                                }
                                sb.Replace(m[i].Value, rec);
                                break;
                        }
                    }
                }
            }
            return sb.ToString(); 
        }

        private string ParseParenthisis(ScriptArgs e, string line)
        {
            var part = _tokenParenthisisParts.Matches(line);
            var sb = new StringBuilder(line);
            if (part.Count > 0)
            {
                foreach (Match pt in part)
                {
                    sb.Replace(pt.Value, ParseIdentifierArgs(e, pt.Value));
                }
            }
            return sb.ToString();
        }

        private string ParseIdentifierArgs(ScriptArgs e, string id)
        {
            _nestedIdStack = new Stack<IdentifierParams>();
            var m = _parenthisisPart.Match(id);
            IdentifierParams p;
            while (m.Success)
            {
                p = new IdentifierParams
                        {
                            Id = m.Groups[1].Value,
                            Args = m.Groups[2].Value
                        };
                _nestedIdStack.Push(p);
                m = _parenthisisPart.Match(p.Args);
            }
            var rec = string.Empty;
            while (_nestedIdStack.Count > 0)
            {
                p = _nestedIdStack.Pop();
                if (string.IsNullOrEmpty(rec))
                {
                    rec = p.Args;
                }
                /* Check if it's an alias */
                var script = ScriptManager.GetScript(ScriptManager.Aliases, p.Id);
                rec = script != null ? script.Parse(e, rec.Split(new[] {','})) : ParseInternalIdentifier(p.Id, rec);
            }
            /* Return final result */
            return rec;
        }

        private string ParseInternalIdentifier(string id, string args)
        {
            return "N/A";
        }
    }
}
