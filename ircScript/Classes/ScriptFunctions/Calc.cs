/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections;
using System.Globalization;
using ircCore.Utils;

namespace ircScript.Classes.ScriptFunctions
{
    public class Calc
    {
        /* Expression calculator (BODMAS) (C# version)
           Original code ©2006 - Samuel Gomes 
           Modified class code ©2009/2011 - Jason James Newland
           KangaSoft Software, All Rights Reserved. */
        private const short StateNone = 0;
        private const short StateOperand = 1;
        private const short StateOperator = 2;
        private const short StateUnaryop = 3;
        private const string UnaryNeg = "(-)";

        public double Evaluate(string expression)
        {
            string buffer = null;
            /* Convert to postfix expression */
            var errPosition = InfixToPostfix(expression, ref buffer);
            /* Error in expression, return 0 */
            return errPosition != 0 ? double.NaN : Evalutate(buffer);
        }

        /* Private helper methods */
        private static int InfixToPostfix(string expression, ref string buffer)
        {
            /* Converts an infix expression to a postfix expression
               that contains exactly one space following each token. */
            var i = 0;
            var tokens = new Stack();
            var currentState = StateNone;
            var parenCount = 0;
            if (!string.IsNullOrEmpty(expression))
            {
                while (!(i > expression.Length - 1))
                {
                    var ch = expression.Substring(i, 1);
                    string temp;
                    switch (ch)
                    {
                        case "(":
                            /* Cannot follow operand */
                            if (currentState == StateOperand) { return i + 1; }
                            /* Allow additional unary operators after "(" */
                            if (currentState == StateUnaryop) { currentState = StateOperator; }
                            /* Push opening parenthesis onto stack */
                            tokens.Push(ch);
                            /* Keep count of parentheses on stack */
                            parenCount += 1;
                            break;

                        case ")":
                            /* Must follow operand */
                            if (currentState != StateOperand) { return i + 1; }
                            /* Must have matching open parenthesis */
                            if (parenCount == 0) { return i + 1; }
                            /* Pop all operators until matching "(" found */
                            temp = tokens.Pop().ToString();
                            while (temp != "(")
                            {
                                buffer += temp + " ";
                                temp = tokens.Pop().ToString();
                            }
                            /* Keep count of parentheses on stack */
                            parenCount -= 1;
                            break;

                        case "+":
                        case "-":
                        case "*":
                        case "/":
                        case @"\":
                        case "%":
                        case "^":
                            /* Need a bit of extra code to handle unary operators */
                            switch (currentState)
                            {
                                case StateOperand:
                                    while (tokens.Count > 0)
                                    {
                                        if (GetPrecedence(tokens.Peek().ToString()) < GetPrecedence(ch))
                                        {
                                            break;
                                        }
                                        buffer += tokens.Pop() + " ";
                                    }
                                    tokens.Push(ch);
                                    currentState = StateOperator;
                                    break;
                                case StateUnaryop:
                                    return i + 1;

                                default:
                                    switch (expression[i])
                                    {
                                        case '-':
                                            tokens.Push(UnaryNeg);
                                            currentState = StateUnaryop;
                                            break;
                                        case '+':
                                            currentState = StateUnaryop;
                                            break;
                                        default:
                                            return i + 1;
                                    }
                                    break;
                            }
                            break;

                        case "0":
                        case "1":
                        case "2":
                        case "3":
                        case "4":
                        case "5":
                        case "6":
                        case "7":
                        case "8":
                        case "9":
                        case ".":
                            /* Cannot follow other operand */
                            if (currentState == StateOperand) { return i + 1; }
                            temp = null;
                            var bDecPoint = false;
                            while (("0123456789.".Contains(ch)))
                            {
                                if (expression[i] == '.')
                                {
                                    if (bDecPoint) { return i + 1; }
                                    bDecPoint = true;
                                }
                                temp += ch;
                                i += 1;
                                if (i > expression.Length - 1) { break; }
                                ch = expression.Substring(i, 1);
                            }
                            i -= 1;
                            /* Error if number contains decimal point only */
                            if (temp == ".") { return i + 1; }
                            buffer += temp + " ";
                            currentState = StateOperand;
                            break;

                        case " ":
                            /* Ignore spaces */
                            break;

                        default:
                            /* Symbol name cannot follow other operand */
                            if (currentState == StateOperand) { return i + 1; }
                            if (IsSymbolCharFirst(ch))
                            {
                                temp = ch;
                                i += 1;
                                if (i <= expression.Length - 1)
                                {
                                    ch = expression.Substring(i, 1);
                                    while (IsSymbolChar(ch))
                                    {
                                        temp += ch;
                                        i += 1;
                                        if (i > expression.Length - 1) { break; }
                                        ch = expression.Substring(i, 1);
                                    }
                                }
                            }
                            else
                            {
                                /* Unexpected character */
                                return i + 1;
                            }
                            /* Reset error position to start of symbol */
                            i -= temp.Length - 1;
                            return i + 1;
                    }
                    i += 1;
                }
                /* Expression cannot end with operator */
                if (currentState == StateOperator | currentState == StateUnaryop)
                {
                    return i + 1;
                }
                /* Check for balanced parentheses */
                if (parenCount > 0)
                {
                    return i + 1;
                }
                /* Retrieve remaining operators from stack */
                while (tokens.Count != 0)
                {
                    buffer += tokens.Pop() + " ";
                }
                /* Indicate no error */
                return 0;
            }
            return 0;
        }
        
        private static double Evalutate(string expression)
        {
            /* Evaluates the given expression and returns the result.
               It is assumed that the expression has been converted to
               a postix expression and that a space follows each token. */
            var i = 0;
            var tokens = new Stack();
            /* Locate first token */
            if (!string.IsNullOrEmpty(expression))
            {
                var j = expression.IndexOf(' ');
                while (j != -1)
                {
                    /* Extract token from expression */
                    var temp = expression.Substring(i, j - i);
                    if (Functions.IsNumeric(temp))
                    {
                        /* If operand, push onto stack */
                        tokens.Push(Convert.ToDouble(temp));
                    }
                    else
                    {
                        /* If operator, perform calculations */
                        object op1;
                        object op2;
                        switch (temp)
                        {
                            case "+":
                                tokens.Push(Convert.ToDouble(tokens.Pop()) + Convert.ToDouble(tokens.Pop()));
                                break;

                            case "-":
                                op1 = tokens.Pop();
                                op2 = tokens.Pop();
                                tokens.Push(Convert.ToDouble(op2) - Convert.ToDouble(op1));
                                break;

                            case "*":
                                tokens.Push(Convert.ToDouble(tokens.Pop()) * Convert.ToDouble(tokens.Pop()));
                                break;

                            case "/":
                                op1 = tokens.Pop();
                                op2 = tokens.Pop();
                                tokens.Push(Convert.ToDouble(op2) / Convert.ToDouble(op1));
                                break;

                            case @"\":
                                op1 = tokens.Pop();
                                op2 = tokens.Pop();
                                tokens.Push(Convert.ToInt64(Convert.ToDecimal(op2) / Convert.ToDecimal(op1)));
                                break;

                            case "%":
                                op1 = tokens.Pop();
                                op2 = tokens.Pop();
                                tokens.Push(Convert.ToDecimal(op2) % Convert.ToDecimal(op1));
                                break;

                            case "^":
                                op1 = tokens.Pop();
                                op2 = tokens.Pop();
                                tokens.Push(Math.Pow(Convert.ToDouble(op2), Convert.ToDouble(op1)));
                                break;

                            case UnaryNeg:
                                tokens.Push(-Convert.ToDouble(tokens.Pop()));
                                break;

                            default:
                                /* This should never happen (bad tokens caught in InfixToPostfix) */
                                return 0;
                        }
                    }
                    /* Find next token */
                    i = j + 1;
                    j = expression.IndexOf(' ', i);
                }
                /* Remaining item on stack contains result */
                return tokens.Count > 0 ? Convert.ToDouble(tokens.Pop()) : 0;
            }
            return 0;
        }
        
        private static short GetPrecedence(string chr)
        {
            /* Returns a number that indicates the relative precedence of an operator. */
            switch (chr)
            {
                case "+":
                case "-":
                    return 1;

                case "*":
                case "/":
                case @"\":
                case "%":
                    return 2;

                case "^":
                    return 3;

                case UnaryNeg:
                    return 10;

                default:
                    return 0;
            }
        }

        private static bool IsSymbolCharFirst(string chr)
        {
            /*  Returns a boolean value that indicates if chr is a valid
                character to be used as the first character in symbols names */
            if (!string.IsNullOrEmpty(chr))
            {
                var c = chr.ToUpper()[0];
                if ((c >= 65 && c <= 90) || c.ToString(CultureInfo.InvariantCulture).Contains("_"))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsSymbolChar(string chr)
        {
            /*  Returns a boolean value that indicates if chr is a valid
                character to be used in symbols names */
            const string cP = "0123456789_";
            if (!string.IsNullOrEmpty(chr))
            {
                var c = chr.ToUpper()[0];
                if ((c >= 65 && c <= 90) || cP.Contains(c.ToString(CultureInfo.InvariantCulture))) { return true; }
            }
            return false;
        }
    }
}
