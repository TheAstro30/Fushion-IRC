/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Text;
using System.Text.RegularExpressions;
using ircScript.Classes.ScriptFunctions;
using ircScript.Classes.Structures;

namespace ircScript.Classes.Parsers
{
    /* Variable parser - this has to be used in conjunction with ScriptIdentifierParser in-line
     * or it WILL NOT function correctly. This is about the 5th refactoring of code, until I ended
     * up with a more succint, logical flowing code structure. */
    internal class ScriptVariableParser
    {
        private readonly Regex _var = new Regex(@"%\w+", RegexOptions.Compiled); /* Used to find instances of %[var] */
        private readonly Regex _commandVariable = new Regex(@"(?<com>\w+) %(?<var>\w+)", RegexOptions.Compiled);
        private readonly Regex _variableMath = new Regex(@"\+|\-|\*|\/", RegexOptions.Compiled);

        private readonly ScriptVariables _local;

        private readonly ScriptIdentifierParser _id = new ScriptIdentifierParser(); /* $id(<n>) parsing */
        private readonly Calc _calc = new Calc(); /* Used for math operations on variable assignments */

        public ScriptVariableParser(ScriptVariables localVariables)
        {
            _local = localVariables;
        }

        public string Parse(ScriptArgs e, string line)
        {
            /* Variable main parsing method - we have to parse $id's in-line with this method, may seem a bit
             * archaic, but we're dealing with strings... */
            var com = string.Empty;
            var name = string.Empty;
            var value = string.Empty;
            if (GetVariableAssignment(line, ref com, ref name, ref value))
            {                
                /* Parse just the variable value for instances of %[N] and replace them */
                value = _id.Parse(e, ReplaceVariables(value));
                /* We need to check if we start with var, set or unset or %var = */
                if (ParseVariableAssignment(com, name, value))
                {
                    return string.Empty; /* No further processing of line */
                }
            }
            /* Now we parse the line and replace %<var> with the actual values including $id(<n>) identifiers */
            return _id.Parse(e, ReplaceVariables(line)).Replace((char) 7, (char) 44);
        }

        private bool GetVariableAssignment(string line, ref string command, ref string name, ref string value)
        {
            var m = _commandVariable.Match(line);
            if (m.Success)
            {
                command = m.Groups[1].Value.ToUpper();
                name = string.Format("%{0}", m.Groups[2].Value);
                var tmp = line.Substring(m.Groups[0].Length);
                var i = tmp.IndexOf('=');
                value = i != -1
                            ? tmp.Substring(i + 1).Trim()
                            : tmp.Trim();
                return true;
            }
            if (line[0] == '%')
            {
                /* Our variable assignment is %var=%var [+/-///*] [N]/%var=%var2, etc */                
                var i = line.IndexOf('=');
                if (i != -1)
                {
                    name = line.Substring(0, i).Trim();
                    value = line.Substring(i + 1).Trim();
                }
                return true;
            }
            return false;
        }

        private bool ParseVariableAssignment(string com, string name, string value)
        {
            /* Check for VAR/SET/UNSET first, %var = second then do math operations */
            ScriptVariable var = null;
            /* Identifiers get replaced/evaluated next */
            var m = _variableMath.Match(value);
            var process = false;
            if (!string.IsNullOrEmpty(com))
            {
                switch (com)
                {
                    case "VAR":
                    case "SET":
                    case "UNSET":
                        /* Global or local variable - if it's "UNSET", global value is used */
                        var s = com == "VAR" ? _local : ScriptManager.Variables;
                        var = s.GetVariable(name);                        
                        if (var == null)
                        {
                            /* New declaration */
                            var = new ScriptVariable
                                      {
                                          Name = name,
                                          Value = value.Replace((char) 7, (char) 44)
                                      };
                            s.Variable.Add(var);                       
                        }
                        else
                        {
                            if (com == "VAR" || com == "SET")
                            {
                                var.Value = value.Replace((char) 7, (char) 44);
                            }
                            else
                            {
                                ScriptManager.Variables.Variable.Remove(var);
                            }
                        }
                        process = true;
                        break;

                    case "INC":
                    case "DEC":
                        var = _local.GetVariable(name) ?? ScriptManager.Variables.GetVariable(name);
                        int i;
                        if (!int.TryParse(value, out i))
                        {
                            i = 1;
                        }
                        if (var != null)
                        {
                            if (com == "INC")
                            {
                                ScriptManager.Variables.Increment(var, i);
                            }
                            else
                            {
                                ScriptManager.Variables.Decrement(var, i);
                            }
                        }
                        process = true;
                        break;
                }
            }
            else
            {
                /* Local or global declare */
                var = _local.GetVariable(name) ?? ScriptManager.Variables.GetVariable(name);
                if (var == null)
                {
                    /* Push as a new variable to globals */
                    var = new ScriptVariable { Name = name };
                    ScriptManager.Variables.Variable.Add(var);
                }                
                var.Value = value.Replace((char) 7, (char) 44); /* Forgot to set the value for %var = value, lol */
                process = true;
            }
            if (m.Success && var != null)
            {
                var d = _calc.Evaluate(value);
                if (!double.IsNaN(d))
                {
                    var.Value = d.ToString();
                }
            }
            return process;
        }

        private string ReplaceVariables(string line)
        {
            /* Replace all instances of %[X] with their data value */
            var vars = _var.Matches(line);
            if (vars.Count > 0)
            {
                var sb = new StringBuilder(line);
                foreach (Match m in vars)
                {
                    var var = _local.GetVariable(m.Value) ?? ScriptManager.Variables.GetVariable(m.Value);
                    sb.Replace(m.Value, var != null ? var.Value.Replace((char)44,(char)7) : string.Empty);
                }
                return sb.ToString();
            }
            return line;
        }
    }
    /* Was 600+ lines, yesterday, now only 185 :P */
}