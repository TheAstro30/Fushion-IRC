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
    [Serializable]
    public class ScriptVariable
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }
    }

    [Serializable, XmlRoot("variables")]
    public class ScriptVariables
    {
        [XmlElement("variable")]
        public List<ScriptVariable> Variable = new List<ScriptVariable>();
    }
}
