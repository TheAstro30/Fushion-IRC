/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System.IO;
using ircCore.Utils;
using ircCore.Utils.Serialization;

namespace ircCore.Settings.Channels
{
    public static class ChannelManager
    {
        public static Channels Channels = new Channels();

        private static readonly string FileName = Functions.MainDir(@"\data\channels.xml", false);

        public static void Load()
        {
            if (!XmlSerialize<Channels>.Load(FileName, ref Channels))
            {
                Channels = new Channels();
            }            
        }

        public static void Save()
        {
            if (Channels.Favorites.Favorite.Count == 0 && Channels.Recent.Channel.Count == 0)
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
    }
}
