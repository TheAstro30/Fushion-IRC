/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace ircCore.Controls.ChildWindows.Classes
{
    public class CurrentModes
    {
        internal List<string> Modes = new List<string>();

        public string Key { get; set; }

        public int Limit { get; set; }
        
        public override string ToString()
        {
            if (Modes.Count > 0)
            {
                var modes = string.Join(string.Empty, Modes.ToList());
                if (Limit > 0 && !string.IsNullOrEmpty(Key))
                {
                    return string.Format("[+{0} {1} {2}]", modes, Limit, Key);
                }
                if (Limit > 0)
                {
                    return string.Format("[+{0} {1}]", modes, Limit);
                }
                return !string.IsNullOrEmpty(Key) ? string.Format("[+{0} {1}]", modes, Key) : string.Format("[+{0}]", modes);
            }
            return string.Empty;
        }
    }

    public class ChannelModes : CurrentModes
    {
        public event Action<ChannelModes> OnModesChanged;

        public void SetModes(string modes)
        {
            SetModes(modes, string.Empty);
        }

        public void SetModes(string modes, string acceptableModes)
        {
            char[] c = null;
            if (!string.IsNullOrEmpty(acceptableModes))
            {
                c = acceptableModes.ToCharArray();
            }
            var add = false;
            var hasKey = false;
            var hasLimit = false;
            var b = false;
            for (var i = 0; i <= modes.Length -1;i++)
            {
                if (b)
                {
                    break;
                }
                var m = modes[i].ToString();
                switch (modes[i])
                {
                    case '+':
                        add = true;
                        break;

                    case '-':
                        add = false;
                        break;

                    case 'k':                        
                        if (add)
                        {
                            hasKey = true;
                            if (!Modes.Contains(m))
                            {
                                Modes.Add(m);
                            }
                        }
                        break;

                    case 'l':
                        if (add)
                        {
                            hasLimit = true;
                            if (!Modes.Contains(m))
                            {
                                Modes.Add(m);
                            }
                        }
                        else
                        {
                            Limit = 0;
                        }
                        break;

                    case ' ':
                        b = true;
                        break;

                    default:                                               
                        if (add)
                        {
                            if (c != null)
                            {
                                /* Verify the mode is a legal channel mode and not a user mode (+v/b, etc) */
                                if (c.Contains(modes[i]) && !Modes.Contains(m))
                                {
                                    Modes.Add(m);
                                }
                            }
                            else
                            {
                                /* Just add it */
                                if (!Modes.Contains(m))
                                {
                                    Modes.Add(m);
                                }
                            }
                        }
                        else
                        {
                            Modes.Remove(m);
                        }
                        break;
                }
            }
            Key = string.Empty;
            Modes.Sort();
            var p = modes.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            if (hasKey && hasLimit)
            {
                /* Needs to be three parts to 'p' */
                if (p.Length == 3)
                {
                    ProcessKeyLimit(p[1]);
                    ProcessKeyLimit(p[2]);
                }
            }
            if (hasKey || hasLimit)
            {
                /* Just two parts */
                ProcessKeyLimit(p[1]);
            }
            /* Raise event */
            if (OnModesChanged != null)
            {
                OnModesChanged(this);
            }
        }

        private void ProcessKeyLimit(string part)
        {
            if (string.IsNullOrEmpty(part))
            {
                return;
            }
            /* Is this a numeric? */
            int i;
            if (!int.TryParse(part, out i))
            {
                Key = part;
            }
            else
            {
                Limit = i;
            }
        }
    }
}
