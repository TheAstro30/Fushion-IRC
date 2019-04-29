/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Linq;
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

        public override string ToString()
        {
            return !string.IsNullOrEmpty(Name)
                       ? string.Format("{0}={1}", Name, Value)
                       : string.Empty;
        }
    }

    [Serializable, XmlRoot("variables")]
    public class ScriptVariables
    {
        [XmlElement("variable")]
        public List<ScriptVariable> Variable = new List<ScriptVariable>();

        public ScriptVariable GetVariable(string name)
        {
            return Variable.FirstOrDefault(v => v.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }        

        public void Increment(ScriptVariable v, int value)
        {
            if (v == null)
            {
                return;
            }
            /* Use "GetVariable" first - this assumes it's numerical */
            int i;
            if (!int.TryParse(v.Value, out i))
            {
                return;
            }
            i += value;
            v.Value = i.ToString();
        }

        public void Decrement(ScriptVariable v, int value)
        {
            if (v == null)
            {
                return;
            }
            /* Use "GetVariable" first - this assumes it's numerical */
            int i;
            if (!int.TryParse(v.Value, out i))
            {
                return;
            }
            i -= value;
            v.Value = i.ToString();
        }
    }
}
