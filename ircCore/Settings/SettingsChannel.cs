/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Xml.Serialization;

namespace ircCore.Settings
{
    [Serializable]
    public class SettingsChannel
    {
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
    }
}
