/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.IO;
using System.Linq;
using ircCore.Utils;
using ircCore.Utils.Serialization;

namespace ircCore.Settings.Channels
{
    public static class ChannelManager
    {
        public static Channels Channels = new Channels();

        private static readonly string FileName = Functions.MainDir(@"\data\channels.xml");

        public static void Load()
        {
            if (!XmlSerialize<Channels>.Load(FileName, ref Channels))
            {
                Channels = new Channels();
            }            
        }

        public static void Save()
        {
            if (Channels.Favorites.Favorite.Count == 0 && Channels.Recent.Channels.Count == 0)
            {
                /* No point saving or keeping an empty list */
                if (File.Exists(FileName))
                {
                    File.Delete(FileName);
                }
                return;
            }
            XmlSerialize<Channels>.Save(FileName, Channels);
        }

        public static ChannelRecent.ChannelNetworkData GetNetwork(string network)
        {
            foreach (var n in Channels.Recent.Channels)
            {
                if (n.Network.Equals(network, StringComparison.InvariantCultureIgnoreCase))
                {
                    return n;
                }
            }
            return null;
        }

        public static ChannelRecent.ChannelData GetChannel(ChannelRecent.ChannelNetworkData network, string channel)
        {
            foreach (var c in network.Channel)
            {
                if (c.Name.Equals(channel, StringComparison.InvariantCultureIgnoreCase))
                {
                    return c;
                }
            }
            return null;
        }
    }
}
