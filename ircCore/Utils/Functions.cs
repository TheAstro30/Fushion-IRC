/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ircCore.Utils
{
    public static class Functions
    {
        private static string _mainFolder;

        public sealed class EnumUtils
        {
            public static string GetDescriptionFromEnumValue(Enum value)
            {
                var attribute = value.GetType()
                                    .GetField(value.ToString())
                                    .GetCustomAttributes(typeof(DescriptionAttribute), false)
                                    .SingleOrDefault() as DescriptionAttribute;
                return attribute == null ? value.ToString() : attribute.Description;
            }
        }

        public class EnumComboData
        {
            public string Text { get; set; }
            public uint Data { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }

        /* Static functions */
        public static bool OpenProcess(string processName)
        {
            try
            {
                Process.Start(processName);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static string CleanFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        public static string GetFirstWord(string input)
        {
            var s = input.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return s.Length > 0 ? s[0] : input;
        }

        public static void SetMainDir()
        {
            var folder = AppDomain.CurrentDomain.BaseDirectory;
            var progFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var compare = folder.Replace(progFolder, null);
            if (folder.Substring(0, folder.Length - compare.Length) == progFolder)
            {
                /* If this is in progfiles, according to MicroSoft we can't access this folder,
                   so we have to use AppData instead */
                _mainFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\FusionIRC";
                /* Copy any files from application directory that need to be user read/writable */
                CopyDirectory(folder + @"\data", _mainFolder + @"\data");
                CopyDirectory(folder + @"\themes", _mainFolder + @"\themes");
            }
            else
            {
                _mainFolder = folder;
            }
        }

        public static void CheckFolders()
        {
            /* Check folders exist */
            if (!Directory.Exists(_mainFolder))
            {
                Directory.CreateDirectory(_mainFolder);
            }
            if (!Directory.Exists(_mainFolder + @"\data"))
            {
                Directory.CreateDirectory(_mainFolder + @"\data");
            }
            if (!Directory.Exists(_mainFolder + @"\themes"))
            {
                Directory.CreateDirectory(_mainFolder + @"\themes");
            }
        }

        public static string MainDir(string path, bool forceAppDir)
        {
            var sFolder = !forceAppDir ? _mainFolder : AppDomain.CurrentDomain.BaseDirectory;
            if (!string.IsNullOrEmpty(path))
            {
                if (path.ToLower().Contains(sFolder.ToLower()))
                {
                    /* Remove app path */
                    return @"\" + path.Replace(sFolder, null);
                }
                return path.Substring(0, 1) == @"\" ? (sFolder + path).Replace(@"\\", @"\") : path.Replace(@"\\", @"\");
            }
            /* Failed */
            return string.Empty;
        }

        private static void CopyDirectory(string srcFolder, string destFolder)
        {
            if (!Directory.Exists(srcFolder)) { return; }
            var sFiles = new List<string>();
            sFiles.AddRange(Directory.GetFiles(srcFolder));
            if (!Directory.Exists(destFolder)) { Directory.CreateDirectory(destFolder); }
            foreach (var s in sFiles)
            {
                var strTemp = Path.GetFileName(s);
                if (!File.Exists(destFolder + @"\" + strTemp))
                {
                    File.Copy(s, destFolder + @"\" + strTemp);
                }
            }
        }
    }
}
