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
    public static class ScriptManager
    {
        /* Raw script files */
        public static List<ScriptData> AliasData = new List<ScriptData>();
        public static List<ScriptData> PopupData = new List<ScriptData>();

        /* Stored scritps */
        public static List<Script> Aliases = new List<Script>();
        public static List<Script> Popups = new List<Script>();

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
                var f = Functions.MainDir(s.Path, false);
                if (!File.Exists(f))
                {
                    del.Push(s);
                    continue;
                }
                var sf = LoadScript(Functions.MainDir(s.Path, false));
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

        /* Must be called after editing or loading the raw data */
        public static void BuildScripts(List<ScriptData> rawScriptData, List<Script> scriptList)
        {
            /* Empty current scripts */
            scriptList.Clear();
            foreach (var d in rawScriptData)
            {
                BuildScripts(d, scriptList);
            }
        }

        public static void BuildFileList(List<SettingsScripts.SettingsScriptPath> scriptList, List<ScriptData> data)
        {
            scriptList.Clear();
            scriptList.AddRange(
                data.Select(
                    s => new SettingsScripts.SettingsScriptPath {Path = string.Format(@"\scripts\{0}.xml", s.Name)}));
        }

        /* Private methods */
        private static void BuildScripts(ScriptData data, ICollection<Script> scriptList)
        {
            /* This will be modified to multi-line scripts, but for now it's single */
            var braceCount = 0;
            Script script = null;
            foreach (var line in data.RawScriptData.Select(d => d.TrimStart()).Where(line => !line.StartsWith("//") && !line.StartsWith("/*")))
            {
                var lineData = line.TrimStart();
                if (braceCount == 0)
                {
                    var i = lineData.IndexOf(' ');
                    if (i == -1)
                    {
                        continue; /* Ignore as it's invalid */
                    }
                    /* Validate it's not "ALIAS" */
                    if (lineData.Substring(0, i).ToUpper() == "ALIAS")
                    {
                        lineData = lineData.Substring(i + 1).TrimStart();
                        /* Look for next space */
                        i = lineData.IndexOf(' ');
                        if (i == -1)
                        {
                            continue; /* Ignore this line as it's invalid */
                        }
                    }
                    script = new Script {Name = lineData.Substring(0, i).TrimEnd().Replace("/", "")};
                    /* Also need to consider checking the end of the line for // or /* */
                    lineData = lineData.Substring(i + 1);
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
                }
                else
                {
                    /* Process multiple lines and add them to the SAME script data */
                    lineData = line.TrimEnd();
                    if (lineData.EndsWith("{"))
                    {
                        braceCount++;
                    }
                    else if (lineData.StartsWith("}"))
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
                scriptList.Add(script);
            }
        }
    }
}
