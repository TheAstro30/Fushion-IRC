/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ircCore.Utils
{
    public enum DialogEditType
    {
        Add = 0,
        Edit = 1
    }

    public enum UserListType
    {
        Notify = 0,
        Ignore = 1
    }

    public static class Functions
    {
        private static string _mainFolder;

        private static readonly Regex AddressValidate = new Regex(@"(?<nick>[^ ]+?)\!(?<user>[^ ]+?)@(?<host>[^ ]+?)$", RegexOptions.Compiled);

        public sealed class EnumUtils
        {
            public static string GetDescriptionFromEnumValue(Enum value)
            {
                /* GetDescriptionFromEnumValue((EnumName)<intValue>) */
                var attribute = value.GetType()
                                    .GetField(value.ToString())
                                    .GetCustomAttributes(typeof(DescriptionAttribute), false)
                                    .SingleOrDefault() as DescriptionAttribute;
                return attribute == null ? value.ToString() : attribute.Description;
            }

            public static object[] GetDescriptions(Type type)
            {
                /* Used for populating combos/list boxes using AddRange */
                var names = Enum.GetNames(type);
                return (from name in names select type.GetField(name) into field from DescriptionAttribute fd in field.GetCustomAttributes(typeof (DescriptionAttribute), true) select fd.Description).Cast<object>().ToArray();
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
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), String.Empty));
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
            if (!Directory.Exists(_mainFolder + @"\scripts"))
            {
                Directory.CreateDirectory(_mainFolder + @"\scripts");
            }
        }

        public static string MainDir(string path, bool forceAppDir)
        {
            var sFolder = !forceAppDir ? _mainFolder : AppDomain.CurrentDomain.BaseDirectory;
            if (!String.IsNullOrEmpty(path))
            {
                if (path.ToLower().Contains(sFolder.ToLower()))
                {
                    /* Remove app path */
                    return @"\" + path.Replace(sFolder, null);
                }
                return path.Substring(0, 1) == @"\" ? (sFolder + path).Replace(@"\\", @"\") : path.Replace(@"\\", @"\");
            }
            /* Failed */
            return String.Empty;
        }

        /* Check IRC addrss - validate the input is of *!*@* */
        public static string CheckAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return "*!*@*";
            }
            /* Validate the passed address mask matches the IRC protocol */
            var m = AddressValidate.Match(address);
            if (m.Success)
            {
                /* Valid address format */
                return !address.Contains(".") || address.EndsWith(".") ? string.Format("{0}*", address) : address;
            }
            var index = address.IndexOf('!');
            switch (index)
            {
                case -1:
                    return string.Format("{0}!*@*", address);
                default:
                    if (index == address.Length - 1)
                    {
                        return string.Format("{0}*@*", address);
                    }
                    break;
            }
            index = address.IndexOf('@');
            if (index == -1)
            {
                return string.Format("{0}*@*", address);
            }
            /* Make sure @ or . isn't the last character */
            return index == address.Length - 1
                       ? string.Format("{0}*", address)
                       : address;
        }

        public static bool IsNumeric(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                if (!s.Contains("."))
                {
                    int i;
                    return int.TryParse(s, out i);
                }
                decimal d;
                return decimal.TryParse(s, out d);
            }
            return false;
        }

        /* Private methods */
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
