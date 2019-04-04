/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Collections.Generic;

namespace ircClient.Classes
{
    public class WhoisInfo
    {
        public string Nick { get; set; }
        public string Address { get; set; }
        public string Realname { get; set; }
        public string Server { get; set; }
        public string Channels { get; set; }
        public string AwayMessage { get; set; }
        public List<string> OtherInfo { get; set; }

        public WhoisInfo()
        {
            OtherInfo = new List<string>();
        }

        public WhoisInfo(WhoisInfo info)
        {
            Nick = info.Nick;
            Address = info.Address;
            Realname = info.Realname;
            Server = info.Server;
            Channels = info.Channels;
            AwayMessage = info.AwayMessage;
            OtherInfo = new List<string>(info.OtherInfo);
        }
    }
}
