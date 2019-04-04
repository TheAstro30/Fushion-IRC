/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Collections.Generic;
using ircCore.Controls.ChildWindows.Nicklist.Structures;

namespace ircCore.Controls.ChildWindows.Nicklist.Helpers
{
    /* Sort algorithm for aligning nicklist in order of ops, voice, regular, etc */
    internal class NickComparer : IComparer<NickData>
    {
        public int Compare(NickData x, NickData y)
        {
            var left = x.ToString()[0];
            var right = y.ToString()[0];
            if (NickComparerHelper.GetCharOrder(left) > NickComparerHelper.GetCharOrder(right)) { return 1; }
            return NickComparerHelper.GetCharOrder(left) < NickComparerHelper.GetCharOrder(right) ? -1 : x.ToString().CompareTo(y.ToString());
        }
    }
}
