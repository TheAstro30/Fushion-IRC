/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ircCore.Settings.SettingsBase.Structures;
using ircCore.Utils;
using ircCore.Utils.Serialization;
using ircScript.Classes;
using ircScript.Classes.Structures;

namespace ircScript
{
    public enum ScriptType
    {
        Aliases = 0,
        Events = 1,
        Variables = 2
    }

    public static class ScriptManager
    {
        /* Raw script files */
        public static List<ScriptData> AliasData = new List<ScriptData>();
        public static List<ScriptData> EventData = new List<ScriptData>();

        /* Stored scripts */
        public static List<Script> Aliases = new List<Script>();
        public static List<Script> Events = new List<Script>();

        /* Global variables */
        public static ScriptVariables Variables = new ScriptVariables();

        /* Load/Save methods */

        public static ScriptData LoadScript(string fileName)
        {
            /* NOTE: This method returns NULL if failed - this is by design, always check for NULL at other end */
            ScriptData script = null;
            return !XmlSerialize<ScriptData>.Load(fileName, ref script) ? null : script;
        }

        public static void SaveScript(ScriptData script, string fileName)
        {
            XmlSerialize<ScriptData>.Save(fileName, script);
        }

        public static void LoadVariables(string fileName)
        {
            if (!XmlSerialize<ScriptVariables>.Load(fileName, ref Variables))
            {
                Variables = new ScriptVariables();
            }
        }

        public static void SaveVariables(string fileName)
        {
            XmlSerialize<ScriptVariables>.Save(fileName, Variables);
        }

        public static void LoadMultipleScripts(List<SettingsScripts.SettingsScriptPath> scripts, List<ScriptData> data)
        {
            var del = new Stack<SettingsScripts.SettingsScriptPath>();
            foreach (var s in scripts)
            {
                /* Check file even exists.. */
                var f = Functions.MainDir(s.Path);
                if (!File.Exists(f))
                {
                    del.Push(s);
                    continue;
                }
                var sf = LoadScript(Functions.MainDir(s.Path));
                if (sf == null)
                {
                    continue;
                }
                data.Add(sf);
            }
            /* Delete any found entries from "scripts" */
            while (del.Count > 0)
            {
                var d = del.Pop();
                scripts.Remove(d);
            }
        }

        public static Script GetScript(IEnumerable<Script> scripts, string name)
        {
            /* Get script by name */
            return scripts.FirstOrDefault(s => s.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }        

        public static List<Script> GetEvent(string name)
        {
            return Events.Where(script => script.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        public static SettingsScripts.SettingsScriptPath GetScriptFilePath(List<SettingsScripts.SettingsScriptPath> scriptList, ScriptData script)
        {
            return (from s in scriptList
                    let p = s.Path.Split(new[] {'\\'})
                    where
                        p[p.Length - 1].Equals(string.Format("{0}.xml", script.Name),
                                               StringComparison.InvariantCultureIgnoreCase)
                    select s).FirstOrDefault();
        }

        public static void RemoveScriptFilePath(List<SettingsScripts.SettingsScriptPath> scriptList, ScriptData script)
        {
            var p = GetScriptFilePath(scriptList, script);
            if (p == null)
            {
                return;
            }
            scriptList.Remove(p);
        }

        /* Must be called after editing or loading the raw data */
        public static void BuildScripts(ScriptType type, List<ScriptData> rawScriptData, List<Script> scriptList)
        {
            /* Empty current scripts */
            scriptList.Clear();
            foreach (var d in rawScriptData)
            {
                BuildScripts(type, d, scriptList);
            }
        }

        /* Private methods */
        private static void BuildScripts(ScriptType type, ScriptData data, ICollection<Script> scriptList)
        {
            var braceCount = 0;
            Script script = null;
            foreach (var line in data.RawScriptData.Select(d => d.TrimStart()).Where(line => !line.StartsWith("//")))
            {
                var lineData = line.TrimStart();
                var sp = new ScriptEventParams();
                if (braceCount == 0)
                {
                    var i = lineData.IndexOf(' ');
                    if (i == -1)
                    {
                        continue; /* Ignore as it's invalid */
                    }
                    var name = lineData.Substring(0, i).ToUpper();
                    lineData = lineData.Substring(i + 1).TrimStart();
                    switch (type)
                    {
                        case ScriptType.Aliases:
                            if (name == "ALIAS")
                            {                                
                                /* Look for next space */
                                i = lineData.IndexOf(' ');
                                if (i == -1)
                                {                 
                                    continue; /* Ignore this line as it's invalid */
                                }
                                name = lineData.Substring(0, i).TrimEnd().Replace("/", "");
                                lineData = lineData.Substring(i + 1);
                            }
                            break;

                        case ScriptType.Events:
                            if (name == "ON")
                            {
                                if (!ParseEventLine(ref sp, ref name, ref lineData))
                                {
                                    continue;
                                }
                            }
                            break;
                    }
                    script = new Script {EventParams = sp, Name = name};
                    /* Also need to consider checking the end of the line for // or /* */
                    if (lineData.Length == 0)
                    {
                        continue;
                    } 
                    if (lineData.Trim() == "{")
                    {
                        /* Multi line, count further closing } */
                        braceCount = 1;
                        continue;
                    }
                    /* Check if wasn't formatted "alias <name> { code }" */
                    if (lineData[0] == '{' && lineData.TrimEnd()[lineData.Length - 1] == '}')
                    {
                        lineData = lineData.Substring(1, lineData.Length - 2).Trim();
                    }
                    /* Single line */
                    script.LineData.Add(lineData);
                    scriptList.Add(script);
                }
                else
                {
                    /* Process multiple lines and add them to the SAME script data */
                    lineData = line.TrimEnd();
                    if (lineData.EndsWith("{"))
                    {
                        braceCount++;
                    }
                    else if (lineData.Length > 0 && lineData.Trim()[0] == '}')
                    {
                        braceCount--;
                        if (braceCount == 0)
                        {
                            /* Finished processing multi line - don't add last } */
                            scriptList.Add(script);
                            continue;
                        }
                    }
                    if (script != null)
                    {
                        script.LineData.Add(lineData);
                    }
                }
                   
            }
        }

        private static bool ParseEventLine(ref ScriptEventParams sp, ref string name, ref string lineData)
        {
            /* Events are on <eventName>:[<matchText>:]<target>]:[<commands>|{] */
            var p = lineData.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (p.Length == 0)
            {
                return false; /* Invalid */
            }
            /* First token is event name */
            name = p[0];
            /* Count number of "params" */
            if (p.Length > 3)
            {
                /* on event:match:target:[<commands>|{] */
                sp = new ScriptEventParams { Match = p[1], Target = p[2] };
                lineData = p[3];
            }
            else switch (p.Length)
            {
                case 3:
                    sp = new ScriptEventParams { Target = p[1] };
                    lineData = p[2];
                    break;

                case 2:
                    lineData = p[1];
                    break;

                default:
                    return false; /* Invalid length */
            }
            return true;
        }
    }
}
