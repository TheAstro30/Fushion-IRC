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
        private int _endBlock;

        public bool Parse(string lineData, ref int lineNumber)
        {
            ScriptWhile sw;
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
                        _endBlock++;
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
                        _endBlock++;
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

                    case "WHILE":
                        if (!_processCode)
                        {
                            return false;
                        }
                        _endBlock++;
                        var create = true;
                        if (_while.Count > 0)
                        {
                            sw = _while.Peek();
                            create = sw.Execute;
                            if (sw.StartLine == lineNumber && sw.Execute)
                            {
                                if (!ParseConditional(sp[1]))
                                {
                                    sw.Execute = false;
                                    _processCode = false;
                                }
                                return false;
                            }
                        }
                        if (!create)
                        {
                            return false;
                        }
                        /* Create a new entry */
                        sw = new ScriptWhile
                                 {
                                     StartLine = lineNumber,
                                     EndBlock = _endBlock,
                                     Execute = ParseConditional(sp[1])
                                 };
                        _while.Push(sw);
                        return false;
                }
            }
            sp = lineData.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (sp.Length == 2)
            {
                switch (sp[0].Trim().ToUpper())
                {
                    case "ELSE":
                       _endBlock++;
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
            switch (lineData.Trim().ToUpper())
            {
                case "BREAK":                                    
                    if (_processCode && _while.Count > 0)
                    {
                        /* Pop out of current executing while */
                        _while.Pop();
                        return false;
                    }
                    break; 
               
                case "CONTINUE":
                    if (_processCode && _while.Count > 0)
                    {
                        /* Jump back to start of loop */
                        sw = _while.Peek();
                        if (sw.Execute)
                        {
                            lineNumber = sw.StartLine - 1;
                            _endBlock = sw.EndBlock - 1;
                        }
                        return false;
                    }
                    break;

                case "}":
                    /* End of code block */
                    if (_while.Count > 0)
                    {
                        /* Get last entry */
                        sw = _while.Peek();
                        if (_endBlock == sw.EndBlock)
                        {
                            if (sw.Execute)
                            {
                                /* Now we jump back to our start line and begin processing again */
                                lineNumber = sw.StartLine - 1;                                
                                _endBlock--;
                                return false;
                            }
                            /* Remove it */
                            _processCode = true;
                            _endBlock--;
                            _while.Pop();
                            return false;
                        }
                    }
                    if (_if.Count > 0)
                    {
                        _ifLastExecute = _if.Pop();
                    }
                    if (_if.Count == _ifResumeProcess)
                    {
                        _processCode = true;
                    }
                    _endBlock--;
                    return false;

                default:
                    if (_processCode && _while.Count > 0)
                    {
                        sw = _while.Peek();
                        if (!sw.Execute)
                        {
                            return false;
                        }
                    }
                    break;
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
                //return conditionTrue ? string.IsNullOrEmpty(args[0]) : !string.IsNullOrEmpty(args[0]);
                return false;
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
                var conditionTrue = ParseConditional(sTmpArgs[i]);          
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
