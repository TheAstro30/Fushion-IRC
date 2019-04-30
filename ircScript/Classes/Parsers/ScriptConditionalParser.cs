/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ircCore.Utils;
using ircScript.Classes.Structures;

namespace ircScript.Classes.Parsers
{
    internal class ScriptConditionalParser
    {
        private readonly Stack<bool> _if = new Stack<bool>();
        private readonly Stack<ScriptWhile> _while = new Stack<ScriptWhile>();

        private int _ifResumeProcess;
        private bool _ifLastExecute;
        private bool _processCode = true;

        public bool Parse(string lineData)
        {
            var sp = lineData.Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            if (sp.Length == 3)
            {
                /* Conditional if (N) { */
                switch (sp[0].Trim().ToUpper())
                {
                    case "IF":
                        if (_if.Any() && !_if.Peek())
                        {
                            _processCode = false;
                            _if.Push(false);
                            return false;
                        }
                        if (ParseConditional(sp[1]))
                        {
                            _if.Push(true);
                            return false;
                        }
                        if (_processCode)
                        {
                            _processCode = false;
                            _ifResumeProcess = _if.Count;
                        }
                        _if.Push(false);
                        break;

                    case "ELSEIF":
                        if (_if.Any() && !_if.Peek())
                        {
                            _processCode = false;
                            _if.Push(false);
                            return false;
                        }
                        if (_ifLastExecute)
                        {
                            if (_processCode)
                            {
                                _processCode = false;
                                _ifResumeProcess = _if.Count;
                            }
                            _if.Push(true);
                            return false;
                        }
                        if (ParseConditional(sp[1]))
                        {
                            _if.Push(true);
                            return false;
                        }
                        if (_processCode)
                        {
                            _processCode = false;
                            _ifResumeProcess = _if.Count;
                        }
                        _if.Push(false);
                        break;
                }
            }
            sp = lineData.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (sp.Length == 2)
            {
                switch (sp[0].Trim().ToUpper())
                {
                    case "ELSE":
                        if (_ifLastExecute)
                        {
                            if (_processCode)
                            {
                                _processCode = false;
                                _ifResumeProcess = _if.Count;
                            }
                            _if.Push(true);
                            return false;
                        }
                        _if.Push(true);
                        return false;
                }
            }
            if (lineData.Trim() == "}")
            {
                /* End of code block */
                if (_if.Count > 0)
                {
                    _ifLastExecute = _if.Pop();
                }
                if (_if.Count == _ifResumeProcess)
                {
                    _processCode = true;
                }
                return false;
            }
            return _processCode;
        }
        
        public bool ParseConditional(string s)
        {
            /* Written with the help of sexist (Ryan Alexander) */
            var args = new string[2];
            var conditionTrue = false;
            /* What we'd do first is check if && or || is present, split by those and Parse
               of each side */
            if (s.Contains(" && "))
            {
                return ParseAndOr(" && ", s);
            }
            /* Or is different, either side can be true or false */
            if (s.Contains(" || "))
            {
                return ParseAndOr(" || ", s);
            }
            /* Search for the operand in the string (this allows us to use spaces in both sides of the
               arguments */
            var operAnd = GetOperand(s, ref args);
            if (string.IsNullOrEmpty(args[0])) { args[0] = s; }
            /* If second argument is null ... */
            if (string.IsNullOrEmpty(args[1]))
            {              
                /* Check ! operand and if its a variable */
                if (!string.IsNullOrEmpty(args[0]))
                {
                    if (args[0].StartsWith("!"))
                    {
                        conditionTrue = true;
                    }
                }                
                /* Parse it as numeric? */
                if (Functions.IsNumeric(args[0]))
                {
                    /* If its a number */
                    if (Convert.ToInt32(args[0]) > 0)
                    {
                        return !conditionTrue;
                    }
                    return conditionTrue;
                }
                /* It's a string, check if its an operand !isnum or isnum, etc */
                if (!string.IsNullOrEmpty(args[0]))
                {
                    var tmpArgs = args[0].Split(' ');
                    if (tmpArgs.Length > 1)
                    {
                        switch (tmpArgs[1].ToUpper())
                        {
                            case "ISNULL":
                                return string.IsNullOrEmpty(tmpArgs[0]);

                            case "!ISNULL":
                                return !string.IsNullOrEmpty(tmpArgs[0]);

                            case "ISNUM":
                                return Functions.IsNumeric(tmpArgs[0]);

                            case "!ISNUM":
                                return !Functions.IsNumeric(tmpArgs[0]);
                        }
                    }
                }
                /* Still here? */
                return conditionTrue ? string.IsNullOrEmpty(args[0]) : !string.IsNullOrEmpty(args[0]);
            }
            /* Second param is not null ... check firstarg is a variable... */                        
            if (args[0] == null || args[0].Length == 0)
            {
                args[0] = ((char)0).ToString(CultureInfo.InvariantCulture);
            }
            if (args[1] == null || args[1].Length == 0)
            {
                args[1] = ((char)0).ToString(CultureInfo.InvariantCulture);
            }
            /* Process operands */
            return ParseOperand(operAnd, args[0], args[1]);
        }

        /* Private parsing methods */
        private bool ParseAndOr(string andOr, string scriptLine)
        {
            var orConditionTrue = false;
            var sTmpArgs = scriptLine.Split(new[] { andOr }, StringSplitOptions.RemoveEmptyEntries);
            /* We process each "side" and if the first is false, we do a shortcircuited AND (&&)
               on it (if condition == false) and return false without processing any further */
            for (var i = 0; i <= sTmpArgs.Length - 1; i++)
            {
                /* Remove leading or trailing () "if ((var == true) && (var2 == true)):{" */
                while (sTmpArgs[i].StartsWith("("))
                {
                    sTmpArgs[i] = sTmpArgs[i].Substring(1);
                }
                while (sTmpArgs[i].EndsWith(")"))
                {
                    sTmpArgs[i] = sTmpArgs[i].Substring(0, sTmpArgs[i].Length - 1);
                }
                var conditionTrue = Parse(sTmpArgs[i]);                
                if (andOr != " || " && !conditionTrue)
                {
                    return false;
                }
                /* Set this flag once (obviously if the expression isn't false) */
                if (!orConditionTrue)
                {
                    orConditionTrue = conditionTrue;
                }
            }
            /* We got this far, so must be true (or its an OR) */
            return orConditionTrue;
        }

        private static bool ParseOperand(int operAnd, object firstArg, object secondArg)
        {
            /* Null may occur after parsing identifiers */
            if (firstArg == null)
            {
                firstArg = (char)0;
            }
            if (secondArg == null)
            {
                secondArg = (char)0;
            }            
            switch (operAnd)
            {
                case 0:
                    /* == */
                    return ((string) firstArg).Equals((string) secondArg, StringComparison.InvariantCultureIgnoreCase);

                case 1:
                    /* != */                    
                    return !((string) firstArg).Equals((string) secondArg, StringComparison.InvariantCultureIgnoreCase);

                case 2:
                    /* > */
                    if (Functions.IsNumeric(firstArg.ToString()) && Functions.IsNumeric(secondArg.ToString()))
                    {
                        return Convert.ToInt32(firstArg) > Convert.ToInt32(secondArg);
                    }
                    break;

                case 3:
                case 4:
                    /* >= => */
                    if (Functions.IsNumeric(firstArg.ToString()) && Functions.IsNumeric(secondArg.ToString()))
                    {
                        return Convert.ToInt32(firstArg) > Convert.ToInt32(secondArg) ||
                               Convert.ToInt32(firstArg) == Convert.ToInt32(secondArg);
                    }
                    break;

                case 5:
                    /* < */
                    if (Functions.IsNumeric(firstArg.ToString()) && Functions.IsNumeric(secondArg.ToString()))
                    {
                        return Convert.ToInt32(firstArg) < Convert.ToInt32(secondArg);
                    }
                    break;

                case 6:
                case 7:
                    /* <= =< */
                    if (Functions.IsNumeric(firstArg.ToString()) && Functions.IsNumeric(secondArg.ToString()))
                    {
                        return Convert.ToInt32(firstArg) < Convert.ToInt32(secondArg) ||
                               Convert.ToInt32(firstArg) == Convert.ToInt32(secondArg);
                    }
                    break;

                case 8:
                    /* isin */
                    return secondArg.ToString().ToLower().Contains(firstArg.ToString().ToLower());

                case 9:
                    /* !isin */
                    return !secondArg.ToString().ToLower().Contains(firstArg.ToString().ToLower());
            }
            return false;
        }

        private static int GetOperand(string scriptLine, ref string[] args)
        {
            var sOper = new[]
                            {
                                " == ",
                                " != ",
                                " > ",
                                " >= ",
                                " => ",
                                " < ",
                                " <= ",
                                " =< ",
                                " isin ",
                                " !isin "
                            };
            for (var i = 0; i <= sOper.Length - 1; i++)
            {
                var t = scriptLine.ToLower().IndexOf(sOper[i], StringComparison.Ordinal);
                if (t == -1)
                {
                    continue;
                }
                args[0] = scriptLine.Substring(0, t);
                args[1] = scriptLine.Substring(t + sOper[i].Length);
                return i;
            }
            return 0;
        }
    }
}
