/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ircCore.Settings.SettingsBase.Structures.Client
{
    [Serializable]
    public class SettingsChannel
    {
        public class SettingsChannelList
        {
            [XmlAttribute("minimum")]
            public int Minimum { get; set; }

            [XmlAttribute("maximum")]
            public int Maximum { get; set; }

            [XmlElement("match")]
            public List<string> Match = new List<string>();            
        }

        [XmlAttribute("keepChannelsOpen")]
        public bool KeepChannelsOpen { get; set; }

        [XmlAttribute("joinOpenChannelsOnConnect")]
        public bool JoinOpenChannelsOnConnect { get; set; }

        [XmlAttribute("showFavoritesDialogOnConnect")]
        public bool ShowFavoritesDialogOnConnect { get; set; }

        [XmlAttribute("rejoinChannelsOnKick")]
        public bool ReJoinChannelsOnKick { get; set; }

        [XmlAttribute("joinChannelsOnInvite")]
        public bool JoinChannelsOnInvite { get; set; }

        [XmlElement("channelsList")]
        public SettingsChannelList ChannelList = new SettingsChannelList();
    }
}
