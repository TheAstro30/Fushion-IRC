/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;

namespace ircScript.Helpers
{
    public class ScriptException : ArgumentException
    {
        /* Not sure if I'm going to handle script errors this way yet */
        public ScriptException()
        {
            /* No implementation */
        }

        public ScriptException(string message) : base(message)
        {
            /* No implementation */
        }

        public ScriptException(string message, Exception inner) : base(message, inner)
        {
            /* No implementation */
        }

        public ScriptException(string message, string param, Exception inner) : base(message, param, inner)
        {
            /* No implementation */
        }
    }
}
