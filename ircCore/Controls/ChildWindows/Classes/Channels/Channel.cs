/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;

namespace ircCore.Controls.ChildWindows.Classes.Channels
{
    public class Channel : ChannelBase
    {
        /* The idea of this class is to provide a way of parsing +ntrR channel modes and sorting them including when
         * the modes are changed or limit and key is set/unset and provide a topic string for each channel
         * title bar. #<channame> [+nt]: <topic> */
        private readonly ModeComparer _comparer = new ModeComparer();

        public event Action<Channel> OnSettingsChanged;

        public void SetModes(string modes)
        {
            /* This is only called when setting bulk modes (+ntris) from the server, call add/remove method for mode
             * changes on a channel */
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
                            if (!Modes.Contains(modes[i]))
                            {
                                Modes.Add(modes[i]);
                            }
                        }
                        break;

                    case 'l':
                        if (add)
                        {
                            hasLimit = true;
                            if (!Modes.Contains(modes[i]))
                            {
                                Modes.Add(modes[i]);
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
                            if (!Modes.Contains(modes[i]))
                            {
                                Modes.Add(modes[i]);
                            }
                        }
                        else
                        {
                            if (Modes.Contains(modes[i]))
                            {
                                Modes.Remove(modes[i]);
                            }
                        }
                        break;
                }
            }
            Key = string.Empty;
            Modes.Sort(_comparer);
            ProcessKeyLimit(hasLimit, hasKey, modes.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            /* Raise event */
            if (OnSettingsChanged != null)
            {
                OnSettingsChanged(this);
            }
        }

        public void SetTopic(string topic)
        {
            Topic = topic;
            if (OnSettingsChanged != null)
            {
                OnSettingsChanged(this);
            }
        }

        public void AddMode(char m, string text)
        {
            ProcessKeyLimit(text);
            if (Modes.Contains(m))
            {
                return;
            }
            Modes.Add(m);
            Modes.Sort(_comparer);
            if (OnSettingsChanged != null)
            {
                OnSettingsChanged(this);
            }
        }

        public void RemoveMode(char m)
        {
            if (!Modes.Contains(m))
            {
                return;
            }
            Modes.Remove(m);
            if (OnSettingsChanged != null)
            {
                OnSettingsChanged(this);
            }
        }

        public void ClearModes()
        {
            Modes.Clear();
            if (OnSettingsChanged != null)
            {
                OnSettingsChanged(this);
            }
        }

        /* Private helper methods */
        private void ProcessKeyLimit(bool hasLimit, bool hasKey, IList<string> p)
        {
            if (hasKey && hasLimit)
            {
                /* Needs to be three parts to 'p' */
                if (p.Count == 3)
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
