/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;

namespace ircCore.Settings.Theming.Structures
{
    [Serializable]
    public class ThemeMessageData
    {
        public int DefaultColor { get; set; }

        public string Message { get; set; }
    }

    public class IncomingMessageData
    {
        public ThemeMessage Message { get; set; }

        public DateTime TimeStamp { get; set; }

        public string Nick { get; set; }
        public string Prefix { get; set; }
        public string Address { get; set; }
        public string Target { get; set; }
        public string Text { get; set; }
        public string NewNick { get; set; }
        public string KickedNick { get; set; }

        /* Server related properties */
        public string Server { get; set; }
        public int Port { get; set; }

        /* DNS */
        public string DnsAddress { get; set; }
        public string DnsHost { get; set; }
    }
}
