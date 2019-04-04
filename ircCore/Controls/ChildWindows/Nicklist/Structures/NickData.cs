/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Collections.Generic;
using System.Linq;
using ircCore.Controls.ChildWindows.Nicklist.Helpers;

namespace ircCore.Controls.ChildWindows.Nicklist.Structures
{
    /* Main nick class - holds current known user modes as well as nick */
    internal class NickData
    {
        private class UserModeComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (NickComparerHelper.GetCharOrder(x[0]) > NickComparerHelper.GetCharOrder(y[0])) { return 1; }
                return NickComparerHelper.GetCharOrder(x[0]) < NickComparerHelper.GetCharOrder(y[0]) ? -1 : x.CompareTo(y);
            }
        }

        private readonly UserModeComparer _comparer = new UserModeComparer();
        private readonly List<string> _userModes = new List<string>();

        public string Nick { get; set; }
        public string Address { get; set; }

        public bool AddUserMode(string modeChar)
        {
            /* Add a @ or + etc. as "o" or "v" */            
            if (_userModes.FirstOrDefault(o => o == modeChar) != null)
            {                
                return false;
            }
            _userModes.Add(modeChar);
            _userModes.Sort(_comparer);            
            return true;
        }

        public bool RemoveUserMode(string modeChar)
        {
            /* Remove a @ or + etc. as "o" or "v" */
            if (_userModes.FirstOrDefault(o => o == modeChar) != null)
            {
                _userModes.Remove(modeChar);
                return true;
            }
            return false;
        }

        public string GetUserMode()
        {
            return _userModes.Count > 0 ? _userModes[0] : string.Empty;
        }

        public override string ToString()
        {
            return string.Format("{0}{1}", _userModes.Count > 0 ? _userModes[0] : "", Nick);
        }
    }
}
