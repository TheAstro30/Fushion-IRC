/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using ircCore.Utils;

namespace ircScript.Classes.Structures
{
    [Serializable, XmlRoot("script")]
    public class ScriptData : ICloneList<ScriptData>
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("data")]
        public List<string> RawScriptData = new List<string>();

        [XmlIgnore]
        public bool ContentsChanged { get; set; }

        public ScriptData Clone()
        {
            return (ScriptData) MemberwiseClone();
        }

        public override string ToString()
        {
            return string.Format("{0}.xml", Name);
        }
    }
}
