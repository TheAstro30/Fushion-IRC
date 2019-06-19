/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Xml.Serialization;

namespace ircCore.Settings.SettingsBase.Structures.Mouse
{
    [Serializable]
    public class SettingsMouse
    {
        [XmlAttribute("console")]
        public string Console { get; set; }

        [XmlAttribute("channel")]
        public string Channel { get; set; }

        [XmlAttribute("query")]
        public string Query { get; set; }

        [XmlAttribute("nickList")]
        public string Nicklist { get; set; }
    }
}
