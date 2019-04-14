/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using ircCore.Utils.Serialization;
using ircScript.Structures;

namespace ircScript
{
    public enum ScriptType
    {
        Alias = 0   
    }

    [Serializable, XmlRoot("aliases")]
    public class Aliases
    {
        [XmlElement("alias")]
        public List<Alias> AliasData = new List<Alias>();
    }

    public static class ScriptManager
    {
        public static Aliases Aliases = new Aliases();

        public static void AddScript(ScriptType type, string data)
        {
            /* Find first space which will become our "Name" */
            var i = data.Trim().IndexOf(' ');
            string name;
            string displayName;
            var lineData = string.Empty;
            if (i <= 0)
            {
                name = data;
                displayName = name;
                name = name.Replace("/", ""); /* Remove '/' */
            }
            else
            {
                name = data.Substring(0, i);
                lineData = data.Substring(i).Trim();
                displayName = name;
                name = name.Replace("/", ""); /* Remove '/' */
            }
            /* Check script doesn't already exist, if it does, we overwrite it */
            var script = GetScript(type, name);
            if (script == null)
            {
                /* Create script data */
                switch (type)
                {
                    default:
                        script = new Alias();
                        CreateUpdateScriptData(script, name, displayName, lineData);
                        Aliases.AliasData.Add((Alias)script);
                        break;
                }
            }
            else
            {
                /* Update script data */
                CreateUpdateScriptData(script, name, displayName, lineData);
            }
        }

        public static IScript GetScript(ScriptType type, string name)
        {
            switch (type)
            {
                case ScriptType.Alias:
                    return GetScript(Aliases.AliasData, name);
            }
            return null;
        }

        public static IScript GetScript(IEnumerable<IScript> scripts, string name)
        {
            /* Get script by name */
            return scripts.FirstOrDefault(s => s.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        /* Load/Save methods */
        public static void LoadScript(ScriptType type, string fileName)
        {
            switch (type)
            {
                case ScriptType.Alias:
                    if (!XmlSerialize<Aliases>.Load(fileName, ref Aliases))
                    {
                        Aliases = new Aliases();
                    }
                    break;
            }
        }

        public static void SaveScript(ScriptType type, string fileName)
        {
            switch (type)
            {
                case ScriptType.Alias:
                    XmlSerialize<Aliases>.Save(fileName, Aliases);
                    break;
            }
        }

        ///* Parsing */
        //public static string Parse(IScript script)
        //{
        //    return Parse(script, null);
        //}

        //public static string Parse(IScript script, string[] args)
        //{
        //    return script == null ? string.Empty : script.Parse(args);
        //}

        /* Private methods */
        private static void CreateUpdateScriptData(IScript script, string name, string displayName, string lineData)
        {
            /* Update script data */
            script.Name = name;
            script.DisplayName = displayName;
            script.LineData = lineData; 
        }
    }
}
