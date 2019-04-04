/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;

namespace ircCore.Controls.ChildWindows.Nicklist.Helpers
{
    /* Helper class used between the nick and sort comparer classes */
    internal static class NickComparerHelper
    {
        public static int GetCharOrder(char c)
        {
            if (c == '!') { return 1; }
            if (c == '~') { return 2; }
            if (c == '.') { return 3; }
            if (c == '&') { return 4; }
            if (c == '@') { return 5; }
            if (c == '%') { return 6; }
            return c == '+' ? 7 : Char.ToLower(c);
        }
    }
}
