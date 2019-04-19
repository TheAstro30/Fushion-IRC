/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ircScript.Classes.Structures
{
    [Serializable, XmlRoot("script")]
    public class ScriptData
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("data")]
        public List<string> RawScriptData = new List<string>();
    }
}
