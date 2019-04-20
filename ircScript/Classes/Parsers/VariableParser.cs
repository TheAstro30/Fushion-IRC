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

namespace ircScript.Classes.Parsers
{
    internal class VariableParser
    {
        private readonly Regex _var = new Regex(@"%[a-zA-Z]+", RegexOptions.Compiled); /* Used to find instances of %[var] */
    }
}
