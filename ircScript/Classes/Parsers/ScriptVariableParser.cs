/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Text;
using System.Text.RegularExpressions;
using ircScript.Classes.Structures;

namespace ircScript.Classes.Parsers
{
    internal class ScriptVariableParser
    {
        private readonly Regex _var = new Regex(@"%\w+", RegexOptions.Compiled); /* Used to find instances of %[var] */
        private readonly Regex _commandVariable = new Regex(@"(?<com>\w+) %(?<var>\w+)", RegexOptions.Compiled);
        private readonly Regex _variableMath = new Regex(@"\+|\-|\*|\/", RegexOptions.Compiled);

        private readonly ScriptIdentifierParser _id = new ScriptIdentifierParser();

        public string Parse(ScriptArgs e, ScriptVariables local, string line)
        {
            /* Variable main parsing method - we have to parse $id's in-line with this method, may seem a bit
             * archaic, but we're dealing with strings... */
            var com = string.Empty;
            var name = string.Empty;
            var value = string.Empty;
            if (GetVariableAssignment(local, line, ref com, ref name, ref value))
            {
                
                value = ReplaceVariables(local,value); //must be here
                //System.Diagnostics.Debug.Print("Cunt " + name + " " + value);
                if (!string.IsNullOrEmpty(com))
                {
                    /* We need to check if we start with var, set or unset or %var = */
                    if (ParseVariableAssignment(e, local, com, name, value))
                    {
                        return string.Empty; /* No further processing of line */
                    }
                }
                else
                {
                    /* Attempt to parse as %var1 =, etc */
                    if (ParseVariableMath(e, local, name, value))
                    { 
                        return string.Empty; /* No further processing of line */
                    }
                }
            }
            /* Now we parse the line and replace %<var> with the actual values... */
            //var vars = _var.Matches(line);            
            //if (vars.Count > 0)
            //{
            //    var sb = new StringBuilder(line);
            //    foreach (Match m in vars)
            //    {
            //        var var = local.GetVariable(m.Value) ?? ScriptManager.Variables.GetVariable(m.Value); ;
            //        sb.Replace(m.Value, var != null ? var.Value.Replace((char) 44, (char) 7) : string.Empty);
            //    }
            //    System.Diagnostics.Debug.Print("Fuck my ass " + sb.ToString());
            //    return sb.ToString();
            //}
            //return line;
       
            
            return ReplaceVariables(local, _id.Parse(e, line));
        }

        private string ReplaceVariables(ScriptVariables local, string line)
        {
            var vars = _var.Matches(line);
            if (vars.Count > 0)
            {
                var sb = new StringBuilder(line);
                foreach (Match m in vars)
                {
                    var var = local.GetVariable(m.Value) ?? ScriptManager.Variables.GetVariable(m.Value); ;
                    sb.Replace(m.Value, var != null ? var.Value : string.Empty);
                }
                return sb.ToString();
            }
            return line;
        }

        private bool GetVariableAssignment(ScriptVariables local, string line, ref string command, ref string name, ref string value)
        {
            var m = _commandVariable.Match(line);
            if (m.Success)
            {
                command = m.Groups[1].Value.ToUpper();
                name = string.Format("%{0}", m.Groups[2].Value);
                var tmp = line.Substring(m.Groups[0].Length);
                var i = tmp.IndexOf('=');
                value = i != -1 ? tmp.Substring(i + 1).Trim() : tmp.Trim();                
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

        private bool ParseVariableAssignment(ScriptArgs e, ScriptVariables local, string com, string name, string value)
        {
            ScriptVariable var;
            string v;
            switch (com)
            {
                case "VAR":
                    /* Local variable declaration */
                    var = local.GetVariable(name);
                    v = ReplaceVariables(local, _id.Parse(e, value));
                    if (var == null)
                    {
                        /* New declaration */
                        local.Variable.Add(new ScriptVariable { Name = name, Value = v });
                    }
                    else
                    {
                        var.Value = v;
                    }
                    return true;

                case "SET":
                case "UNSET":
                    /* Global variable */
                    var = ScriptManager.Variables.GetVariable(name);
                    v = ReplaceVariables(local, _id.Parse(e, value));
                   // v = value;
                    System.Diagnostics.Debug.Print("cunt " + v + " " +value);
                    if (var == null)
                    {
                        /* New declaration */
                        ScriptManager.Variables.Variable.Add(new ScriptVariable
                                                                 {
                                                                     Name = name,
                                                                     Value = v
                                                                 });
                        if (com != "UNSET")
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (com == "SET")
                        {
                            var.Value = v;
                        }
                        else
                        {
                            ScriptManager.Variables.Variable.Remove(var);
                        }
                    }
                    return true;

                case "INC":
                case "DEC":
                    var = local.GetVariable(name) ?? ScriptManager.Variables.GetVariable(name);
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
                    return true;
            }
            return false;
        }

        private bool ParseVariableMath(ScriptArgs e, ScriptVariables local, string name, string value)
        {
            /* Local or global declare */
            var var = local.GetVariable(name) ?? ScriptManager.Variables.GetVariable(name);
            if (var == null)
            {
                /* Push as a new variable to globals */
                var = new ScriptVariable { Name = name };
                ScriptManager.Variables.Variable.Add(var);
            }
            /* We do either mathematical operations on the variable, or we're assigning one to another */
            var m = _variableMath.Match(value);
            if (m.Success)
            {
                var operand = m.Value;
                var left = value.Substring(0, m.Index).Trim();
                var right = value.Length >= m.Index + 1 ? value.Substring(m.Index + 1).Trim() : string.Empty;
                /* Check left is a variable */
                ScriptVariable var2;
                int leftValue;
                if (left[0] == '%')
                {
                    var2 = local.GetVariable(left) ?? ScriptManager.Variables.GetVariable(left);
                    if (var2 == null)
                    {
                        /* Push to global */
                        var2 = new ScriptVariable { Name = left };
                        ScriptManager.Variables.Variable.Add(var2);
                    }
                    /* Are they the same variable? */
                    if (var2.Name.Equals(var.Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var2.Value = var.Value;
                    }
                    else
                    {
                        /* Original variable now has to equal the contents of this one */
                        var.Value = var2.Value;
                    }
                    /* Try to parse the value as numeric */
                    int.TryParse(var.Value, out leftValue);
                }
                else
                {
                    /* Numeric? */
                    int.TryParse(left, out leftValue);
                }                
                int rightValue;
                if (!string.IsNullOrEmpty(right))
                {
                    /* Is right a variable? */
                    var2 = local.GetVariable(right) ?? ScriptManager.Variables.GetVariable(right);
                    if (var2 == null)
                    {
                        int.TryParse(right, out rightValue);
                    }
                    else
                    {
                        /* Check right is numeric */
                        int.TryParse(var2.Value, out rightValue);
                    }
                }
                else
                {
                    rightValue = 0;
                }
                //still screwed here
                /* Begin actually doing the math on the values */
                //if (ParseVariableMathOperand(var, operand[0], leftValue, rightValue))
                //{
                //    return true;
                //}
            }
            else
            {
                /* Assume we're trying to concat strings */
                return ParseVariableConcat(e, var, local, value);
            }
            return false;
        }

        private static bool ParseVariableMathOperand(ScriptVariable var, char operand, int leftValue, int rightValue)
        {
            if (leftValue == -1 || rightValue == -1)
            {
                return false;
            }
            try
            {
                switch (operand)
                {
                    case '+':
                        var.Value = (leftValue + rightValue).ToString();
                        break;

                    case '-':
                        var.Value = (leftValue - rightValue).ToString();
                        break;

                    case '*':
                        var.Value = (leftValue*rightValue).ToString();
                        break;

                    case '/':
                        var.Value = (leftValue/rightValue).ToString();
                        break;
                }
            }
            catch
            {
                var.Value = "0";
            }
            return true;
        }

        private bool ParseVariableConcat(ScriptArgs e, ScriptVariable var, ScriptVariables local, string right)
        {
            var m = _var.Matches(right);
            if (m.Count > 0)
            {
                var sb = new StringBuilder(right);
                foreach (Match match in m)
                {
                    var var2 = local.GetVariable(match.Value) ?? ScriptManager.Variables.GetVariable(match.Value);
                    sb.Replace(match.Value, var2 != null ? var2.Value : string.Empty);
                }
                System.Diagnostics.Debug.Print("CONCAT 1");
                var.Value = ReplaceVariables(local, _id.Parse(e, sb.ToString()));
                return true;
            }
            System.Diagnostics.Debug.Print("CONCAT 2");
            var.Value = ReplaceVariables(local, _id.Parse(e, right));
            //return false;
            return false;
        }
    }
}
