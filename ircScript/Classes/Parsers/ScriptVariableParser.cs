/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ircScript.Classes.Structures;

namespace ircScript.Classes.Parsers
{
    internal class ScriptVariableParser
    {
        private readonly Regex _var = new Regex(@"%\w+", RegexOptions.Compiled); /* Used to find instances of %[var] */

        public string Parse(ScriptVariables local, string line)
        {
            /* We need to check if we start with var, set or unset or %var = */
            var i = line.IndexOf(' ');
            string com;
            string name;
            var value = string.Empty;
            if (i != -1)
            {
                com = line.Substring(0, i);
                var arg = line.Substring(i + 1);
                var p = arg.IndexOf('=');
                if (p != -1)
                {
                    name = arg.Substring(0, p);
                    value = arg.Substring(p + 1);
                }
                else
                {
                    name = arg;
                }
            }
            else
            {
                com = line;
                var p = line.IndexOf('=');
                if (p != -1)
                {
                    name = line.Substring(0, p);
                    value = line.Substring(p + 1);
                }
                else
                {
                    name = line;
                }
            }
            if (!string.IsNullOrEmpty(name))
            {
                System.Diagnostics.Debug.Print("VARNAME " + name);
                ScriptVariable var;
                switch (com.ToUpper())
                {
                    case "VAR":
                        /* Local variable declaration */
                        var = local.GetVariable(name);
                        if (var == null)
                        {
                            /* New declaration */
                            local.Variable.Add(new ScriptVariable {Name = name, Value = value});
                        }
                        else
                        {
                            var.Value = value;
                        }
                        return string.Empty;

                    case "SET":
                        /* Global variable */
                        var = ScriptManager.Variables.GetVariable(name);
                        if (var == null)
                        {
                            /* New declaration */
                            ScriptManager.Variables.Variable.Add(new ScriptVariable {Name = name, Value = value});
                        }
                        else
                        {
                            var.Value = value;
                        }
                        return string.Empty;

                    case "UNSET":
                        /* Global variable */
                        var = ScriptManager.Variables.GetVariable(name);
                        if (var != null)
                        {
                            ScriptManager.Variables.Variable.Remove(var);
                        }
                        return string.Empty;

                    case "INC":
                        return string.Empty;

                    case "DEC":
                        return string.Empty;

                    default:
                        if (com[0] == '%')
                        {
                            /* Local or global declare */
                            var = local.GetVariable(com) ?? ScriptManager.Variables.GetVariable(com);
                            if (var != null)
                            {
                                /* We do either mathematical operations on the variable, or we're assigning one to another */
                            }
                            else
                            {
                                /* Push this to the global variables */
                            }
                        }
                        break;
                }
            }
            return line;
        }
    }
}
