/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ircCore.Autos
{
    public class AutoList
    {
        public class AutoData
        {
            [XmlAttribute("item")]
            public string Item { get; set; }

            [XmlAttribute("value")]
            public string Value { get; set; }
        }

        public class AutoNetworkData
        {
            [XmlAttribute("name")]
            public string Name { get; set; }

            [XmlElement("data")]
            public List<AutoData> Data = new List<AutoData>();

            public override string ToString()
            {
                return Name;
            }
        }

        [XmlAttribute("enable")]
        public bool Enable { get; set; }

        [XmlElement("network")]
        public List<AutoNetworkData> Network = new List<AutoNetworkData>();
    }

    [Serializable, XmlRoot("automations")]
    public class Automations
    {
        [XmlElement("join")]
        public AutoList Join = new AutoList();

        [XmlElement("identify")]
        public AutoList Identify = new AutoList();
    }
}
