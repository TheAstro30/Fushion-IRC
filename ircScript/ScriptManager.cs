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
        Alias = 0,
        Popup = 1
    }

    [Serializable, XmlRoot("aliases")]
    public class Aliases
    {
        [XmlElement("alias")]
        public List<Alias> AliasData = new List<Alias>();
    }

    [Serializable, XmlRoot("popups")]
    public class Popups
    {
        [XmlElement("popups")]
        public List<Popup> PopupData = new List<Popup>();
    }

    public static class ScriptManager
    {
        public static Aliases Aliases = new Aliases();
        public static Popups Popups = new Popups();

        public static void AddScript(ScriptType type, string data)
        {
            /* Find first space which will become our "Name" */
            var i = data.Trim().IndexOf(type == ScriptType.Alias ? ' ' : ':');
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
            IScript script;
            switch (type)
            {
                case ScriptType.Popup:
                    /* Create popup */
                    script = new Popup();
                    CreateUpdateScriptData(script, name, displayName, lineData);
                    Popups.PopupData.Add((Popup) script);
                    break;

                default:
                    /* Create alias */
                    script = new Alias();
                    CreateUpdateScriptData(script, name, displayName, lineData);
                    Aliases.AliasData.Add((Alias) script);
                    break;
            }
        }

        public static IScript GetScript(ScriptType type, string name)
        {
            switch (type)
            {
                case ScriptType.Alias:
                    return GetScript(Aliases.AliasData, name);

                case ScriptType.Popup:
                    return GetScript(Popups.PopupData, name);
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

                case ScriptType.Popup:
                    if (!XmlSerialize<Popups>.Load(fileName, ref Popups))
                    {
                        Popups = new Popups();
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

                case ScriptType.Popup:
                    XmlSerialize<Popups>.Save(fileName, Popups);
                    break;
            }
        }

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
