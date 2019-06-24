/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ircClient.Parsing.Helpers
{
    public class NotifyIsonData
    {
        public string Nick { get; set; }

        public string Address { get; set; }

        public bool AddressRequested { get; set; }

        public bool IsOnline { get; set; }
    }

    public class NotifyIson
    {
        /* This class is designed to replace WATCH on servers that don't support it
         * - address will have to be gotten from the server later */
        private readonly Timer _timerIson;

        private readonly List<NotifyIsonData> _users = new List<NotifyIsonData>();

        public event Action<NotifyIsonData> OnUserOnline;
        public event Action<NotifyIsonData> OnUserOffline;

        public event Action OnUserAddressRequest;

        public event Action<string> OnIsonRequest;

        public NotifyIson()
        {
            _timerIson = new Timer
                             {
                                 Interval = 5000,
                                 Enabled = true
                             };
            _timerIson.Tick += TimerIson;
        }

        public void AddNotify(string nick)
        {
            var n = GetNotify(nick);
            if (n != null)
            {
                return;
            }
            n = new NotifyIsonData {Nick = nick};
            _users.Add(n);
        }

        public void RemoveNotify(string nick)
        {
            var n = GetNotify(nick);
            if (n == null)
            {
                return;
            }
            _users.Remove(n);
            /* We should probably notify the event callback that the nick was removed so it can be removed
             * from the treeview display of the client window */
            if (OnUserOffline != null)
            {
                OnUserOffline(n);
            }
        }

        public void UpdateNotifyAddress(string nick, string address)
        {
            var n = GetNotify(nick);
            if (n == null)
            {
                /* It shouldn't ... */
                return;
            }
            n.Address = address;
        }

        public void ParseIson(string nicks)
        {
            /* Now, the bastard part is keeping track of nicks that have already raised the "online" event */
            var n = nicks.Split(' ');
            foreach (var nick in n)
            {
                var data = GetNotify(nick);
                if (data == null || data.IsOnline)
                {
                    /* Return if it's null (shouldn't be) or the "IsOnline" flag has already been set */
                    return;
                }
                /* This part is fine, but we have no way of knowing if they've gone offline... */
                data.IsOnline = true;
                /* Begin requesting address from server by using /USERINFO */
                data.AddressRequested = true;
                if (OnUserAddressRequest != null)
                {
                    OnUserAddressRequest();
                }
                /* Raise main "online" event */
                if (OnUserOnline != null)
                {
                    OnUserOnline(data);
                }
            }
        }

        public NotifyIsonData GetNotify(string nick)
        {
            return _users.FirstOrDefault(n => n.Nick.Equals(nick, StringComparison.InvariantCultureIgnoreCase));
        }

        /* Timer callback */
        private void TimerIson(object sender, EventArgs e)
        {
            if (OnIsonRequest != null)
            {
                OnIsonRequest(string.Join(" ", _users.Select(o => o.Nick)));
            }
        }
    }
}
