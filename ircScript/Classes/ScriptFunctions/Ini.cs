/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using ircCore.Utils;

namespace ircScript.Classes.ScriptFunctions
{
    /* INI file manager
       By: Jason James Newland
       ©2012 - KangaSoft Software
       All Rights Reserved
     */
    internal class IniData
    {
        public string Key;
        public string Value;
    }

    internal class IniSection : Dictionary<string, IniData>
    {
        public string Name { get; set; }
    }

    internal class IniFile : Dictionary<string, IniSection>
    {
        public string Path { get; set; }
    }

    public sealed class Ini
    {
        private static readonly Dictionary<string, IniFile> IniFiles;

        static Ini()
        {
            IniFiles = new Dictionary<string, IniFile>();
        }

        public static string ReadIni(string fileName, string section, string key)
        {
            /* Check if ini file exists in the ini collection */
            var fileKey = fileName.ToLower();
            if (!IniFiles.ContainsKey(fileKey))
            {
                if (!ImportIni(fileKey))
                {
                    return string.Empty;
                }
            }
            /* Find section */
            if (IniFiles[fileKey].Count == 0)
            {
                return string.Empty;
            }
            if (IniFiles[fileKey].ContainsKey(section.ToLower()))
            {
                return IniFiles[fileKey][section.ToLower()].ContainsKey(key.ToLower())
                           ? IniFiles[fileKey][section.ToLower()][key.ToLower()].Value
                           : null;
            }
            return string.Empty;
        }

        public static void WriteIni(string fileName, string section, string key, string value)
        {
            /* Check if ini file exists in the ini collection */
            var fileKey = fileName.ToLower();
            if (!IniFiles.ContainsKey(fileKey))
            {
                if (!ImportIni(fileKey))
                {
                    /* Add a new blank file */
                    var ini = new IniFile
                                  {
                                      Path = fileName
                                  };
                    IniFiles.Add(fileKey, ini);
                }
            }
            /* Find section */
            if (IniFiles[fileKey].ContainsKey(section.ToLower()))
            {
                /* Find key, if exists replace it */
                if (IniFiles[fileKey][section.ToLower()].ContainsKey(key.ToLower()))
                {
                    IniFiles[fileKey][section.ToLower()][key.ToLower()].Value = value;
                    return;
                }
                var data = new IniData { Key = key, Value = value };
                IniFiles[fileKey][section.ToLower()].Add(key.ToLower(), data);
            }
            else
            {
                /* Create new ini section */
                var sec = new IniSection { Name = section };
                var data = new IniData { Key = key, Value = value };
                sec.Add(key.ToLower(), data);
                IniFiles[fileKey].Add(section.ToLower(), sec);
            }
        }

        public static void DeleteIniSection(string fileName, string section)
        {
            var fileKey = fileName.ToLower();
            if (!IniFiles.ContainsKey(fileKey))
            {
                if (!ImportIni(fileKey)) { return; }
            }
            if (IniFiles[fileKey].ContainsKey(section.ToLower()))
            {
                IniFiles[fileKey].Remove(section.ToLower());
            }
        }

        public static void FlushIni(string fileName)
        {
            var key = fileName.ToLower();
            if (!IniFiles.ContainsKey(key)) { return; }
            if (IniFiles[key].Count == 0 && File.Exists(fileName))
            {
                try
                {
                    File.Delete(fileName);
                }
                catch
                {
                    return;
                }
                return;
            }
            var file = IniFiles[key].Path;
            try
            {
                using (var stream = new FileStream(file, FileMode.Create))
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        var start = true;
                        /* Loop only if the number of selections for this file is > 0 */
                        foreach (var section in IniFiles[key].TakeWhile(section => section.Value.Count != 0))
                        {
                            /* Insert a blank line between each header if it's not the first one */
                            if (!start)
                            {
                                writer.WriteLine(Environment.NewLine + "[" + section.Value.Name + "]");
                            }
                            else
                            {
                                start = false;
                                writer.WriteLine("[" + section.Value.Name + "]");
                            }
                            foreach (var data in section.Value)
                            {
                                writer.WriteLine(data.Value.Key + "=" + data.Value.Value);
                            }
                        }
                        writer.Flush();
                        writer.Close();
                    }
                    stream.Close();
                }
            }
            catch
            {
                Debug.Assert(true);
            }
        }

        /* Private methods */
        private static bool ImportIni(string fileName)
        {
            if (!File.Exists(fileName)) { return false; }
            string[] data;
            try
            {
                using (var stream = new FileStream(fileName, FileMode.Open))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        data = reader.ReadToEnd().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        reader.Close();
                    }
                    stream.Close();
                }
            }
            catch
            {
                return false;
            }
            if (data.Length == 0)
            {
                return false;
            }
            var file = new IniFile
                           {
                               Path = fileName
                           };
            var section = new IniSection();
            foreach (var s in data)
            {
                if (s.StartsWith("[") && s.EndsWith("]"))
                {
                    /* Section header */
                    if (section.Count > 0)
                    {
                        /* Add current section */
                        file.Add(section.Name.ToLower(), section);
                    }
                    section = new IniSection
                                  {
                                      Name = s.ReplaceEx("[", string.Empty).ReplaceEx("]", string.Empty)
                                  };
                    continue;
                }
                /* Using current section, parse ini keys/values */
                var iniData = ParseIni(s);
                section.Add(iniData.Key.ToLower(), iniData);
            }
            if (section.Count > 0)
            {
                /* Add current section */
                file.Add(section.Name.ToLower(), section);
            }
            IniFiles.Add(fileName, file);
            return true;
        }

        private static IniData ParseIni(string s)
        {
            var parts = s.Split('=');
            return new IniData
                       {
                           Key = parts[0].Trim(), 
                           Value = parts.Length > 1 ? parts[1].Trim() : string.Empty
                       };
        }
    }
}
