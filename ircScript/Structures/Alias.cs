/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Xml.Serialization;
using ircScript.Classes;

namespace ircScript.Structures
{
    [Serializable]
    public class Alias : ScriptParser, IScript
    {
        /* Currently, this supports only single-line aliases - plans to expand this in the future */
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("displayName")]
        public string DisplayName { get; set; }

        [XmlAttribute("data")]
        public string LineData { get; set; }

        public string Parse(string[] args)
        {
            return Parse(LineData, args);
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", DisplayName, LineData);
        }
    }
}
