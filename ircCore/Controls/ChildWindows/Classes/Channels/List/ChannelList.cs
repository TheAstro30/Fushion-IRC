/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using ircCore.Settings.Channels;
using libolv;

namespace ircCore.Controls.ChildWindows.Classes.Channels.List
{
    public class ChannelList : ObjectListView
    {
        private readonly Timer _timer;

        internal class ChannelListCompare : IComparer<ChannelListData>
        {
            public int Compare(ChannelListData a, ChannelListData b)
            {
                var i = new CaseInsensitiveComparer().Compare(b.Users, a.Users); /* Decending sort */
                if (i == 0)
                {
                    i = new CaseInsensitiveComparer().Compare(a.Name, b.Name);
                }
                return i;
            }
        }

        public List<ChannelListData> Channels = new List<ChannelListData>();

        public ChannelList()
        {
            _timer = new Timer {Interval = 1000};
            _timer.Tick += TimerTick;
        }
        
        public void AddChannel(ChannelListData data)
        {
            Channels.Add(data);
            Channels.Sort(new ChannelListCompare());
            if (_timer.Enabled)
            {
                return;
            }
            _timer.Enabled = true;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            /* Update the list in "chunks" */
            _timer.Enabled = false;
            SetObjects(Channels);
        }
    }
}
