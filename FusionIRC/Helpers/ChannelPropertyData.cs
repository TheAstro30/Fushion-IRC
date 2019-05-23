/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
namespace FusionIRC.Helpers
{
    public enum ChannelPropertyType
    {
        Bans = 0,
        Excepts = 1,
        Invites = 2
    }

    internal enum ModeType
    {
        Topic = 0,
        NoExternalMessages = 1,
        Invite = 2,
        Moderated = 3,
        Limit = 4,
        Key = 5,
        Private = 6,
        Secret = 7
    }

    internal class ChannelCurrentModes
    {        
        public ModeType Mode { get; set; }
    }

    public class ChannelPropertyData
    {
        public string Address { get; set; }

        public string SetByNick { get; set; }

        public string Date { get; set; }
    }
}
