/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Xml.Serialization;

namespace ircCore.Utils
{
    [Serializable]
    public class DnsResult
    {
        [XmlIgnore]
        public string Lookup { get; set; }

        [XmlAttribute("address")]
        public string Address { get; set; }

        [XmlAttribute("hostName")]
        public string HostName { get; set; }        
    }
}
