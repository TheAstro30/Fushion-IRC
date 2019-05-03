/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ircCore.Controls.ChildWindows.Classes.Channels
{
    public class ChannelBase
    {
        public string Key { get; set; }

        public int Limit { get; set; }

        internal List<char> Modes = new List<char>();
                
        internal string Topic { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (Modes.Count > 0)
            {
                var modes = string.Join(string.Empty, Modes.ToList());
                if (Limit > 0 && !string.IsNullOrEmpty(Key))
                {
                    sb.Append(string.Format("[+{0} {1} {2}]", modes, Limit, Key));
                }
                else if (Limit > 0)
                {
                    sb.Append(string.Format("[+{0} {1}]", modes, Limit));
                }
                else
                {
                    sb.Append(!string.IsNullOrEmpty(Key) ? string.Format("[+{0} {1}]", modes, Key) : string.Format("[+{0}]", modes));
                }
            }
            if (!string.IsNullOrEmpty(Topic))
            {
                sb.Append(sb.Length != 0 ? string.Format(": {0}", Topic) : string.Format("{0}", Topic));
            }
            return sb.ToString();
        }        
    }
}
