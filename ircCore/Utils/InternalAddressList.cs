/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace ircCore.Utils
{
    /* Simple classes to hold all nick/address information (used with $address(<nick>,<level>) in scripting) */
    public class IalData
    {
        public string Nick { get; set; }

        public string Address { get; set; }

        public List<string> Channels = new List<string>();

        public override string ToString()
        {
            return string.Format("{0}!{1}", Nick, Address);
        }
    }

    public class InternalAddressList : List<KeyValuePair<string, IalData>>
    {
        public void Add(string nick, string address, string channel)
        {
            var n = GetIal(nick);
            if (n != null)
            {
                n.Address = address;
                if (string.IsNullOrEmpty(GetChannel(n, channel)))
                {
                    n.Channels.Add(channel);
                }
                return;
            }
            var d = new IalData {Nick = nick, Address = address};
            Add(new KeyValuePair<string, IalData>(nick, d));
        }

        public void Update(string nick, string newNick)
        {
            /* Used for nick changes */
            foreach (var k in this.Where(k => k.Key.Equals(nick, StringComparison.InvariantCultureIgnoreCase)))
            {
                var d = new IalData
                            {
                                Nick = newNick,
                                Address = k.Value.Address,
                                Channels = new List<string>(k.Value.Channels)
                            };
                Add(new KeyValuePair<string, IalData>(newNick, d));
                Remove(k);
                return;
            }
        }

        public string Get(string nick)
        {
            var n = GetIal(nick);
            return n != null ? n.ToString() : string.Empty;
        }

        public List<string> IalMatch(string address)
        {
            return (from k in this where k.Value.Address.Equals(address, StringComparison.InvariantCultureIgnoreCase) select k.Value.Nick).ToList();
        }

        public void Remove(string nick)
        {
            foreach (var k in this.Where(k => k.Key.Equals(nick, StringComparison.InvariantCultureIgnoreCase)))
            {
                Remove(k);
                return;
            }
        }

        public void Remove(string nick, string channel)
        {
            foreach (var k in this.Where(k => k.Key.Equals(nick, StringComparison.InvariantCultureIgnoreCase)))
            {
                foreach (var c in k.Value.Channels.Where(c => c.Equals(channel, StringComparison.InvariantCultureIgnoreCase)))
                {
                    k.Value.Channels.Remove(c);
                    break;
                }
                if (k.Value.Channels.Count == 0)
                {
                    Remove(k);
                }
                return;
            }
        }

        public void RemoveChannel(string channel)
        {
            /* Removes any nick associated with channel argument - used for when YOU part a channel, keep the list down ;) */
            for (var i = Count - 1; i >= 0; i--)
            {
                var n = this[i];
                foreach (var c in n.Value.Channels.Where(c => c.Equals(channel, StringComparison.InvariantCultureIgnoreCase)))
                {
                    n.Value.Channels.Remove(c);
                    break;
                }
                if (n.Value.Channels.Count == 0)
                {
                    Remove(n);
                }
            }
        }

        /* Private helper functions */
        private IalData GetIal(string nick)
        {
            return (from k in this where k.Key.Equals(nick, StringComparison.InvariantCultureIgnoreCase) select k.Value).FirstOrDefault();
        }

        private static string GetChannel(IalData ial, string channel)
        {
            return ial.Channels.Any(c => c.Equals(channel, StringComparison.InvariantCultureIgnoreCase)) ? string.Empty : channel;
        }
    }
}