/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Xml.Serialization;

namespace ircCore.Autos
{
    [Serializable, XmlRoot("automations")]
    public class Automations
    {
        [XmlElement("connect")]
        public AutoList Connect = new AutoList();

        [XmlElement("join")]
        public AutoList Join = new AutoList();

        [XmlElement("identify")]
        public AutoList Identify = new AutoList();

        [XmlElement("perform")]
        public AutoList Perform = new AutoList();
    }
}
