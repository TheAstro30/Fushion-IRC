/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Collections.Generic;

namespace ircCore.Controls.ChildWindows.Classes.Channels
{
    internal class ModeComparer : IComparer<char>
    {
        /* I want channel modes to be displayed +nt<rest> */
        public int Compare(char x, char y)
        {
            return CharOrder(x) > CharOrder(y) ? 1 : (CharOrder(x) < CharOrder(y) ? -1 : x.CompareTo(y));
        }

        private static int CharOrder(char c)
        {
            return c == 'n' ? 1 : (c == 't' ? 2 : 3);
        }
    }
}
