/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ircCore.Controls.ChildWindows.Helpers;
using ircCore.Forms;
using ircCore.Settings;

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
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetCaretPos(ref Point lpPoint);

        private static string _mainFolder;        

        private const string CodePattern = "\u0003[0-9]{1,2}(,[0-9]{1,2})|\u0003[0-9]{1,2}|\u0003";

        private static readonly Regex RFileObj = new Regex("[\\\\\\/:\\*\\?\"'<>|]", RegexOptions.Compiled);

        private static readonly Regex RegExAllCodes =
            new Regex(
                string.Format(@"{0}|{1}|{2}|{3}|{4}|{5}|{6}", CodePattern, (char) ControlByte.Bold, (char) ControlByte.Underline,
                              (char) ControlByte.Italic, (char) ControlByte.Reverse, (char) ControlByte.Normal,
                              (char) ControlByte.Reverse), RegexOptions.Compiled);

        private static FrmColorIndex _frmColor;

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
            if (!Directory.Exists(_mainFolder + @"\scripts"))
            {
                Directory.CreateDirectory(_mainFolder + @"\scripts");
            }
            if (!Directory.Exists(_mainFolder + @"\downloads"))
            {
                Directory.CreateDirectory(_mainFolder + @"\downloads");
            }
        }

        public static string MainDir(string path)
        {
            return MainDir(path, false, false);
        }

        public static string MainDir(string path, bool forceAppDir)
        {
            return MainDir(path, forceAppDir, false);
        }

        public static string MainDir(string path, bool forceAppDir, bool allowEmptyPath)
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
            return allowEmptyPath ? sFolder : string.Empty;
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

        public static string GetLogFileName(string network, string name)
        {
            return GetLogFileName(network, name, false);
        }

        public static string GetLogFileName(string network, string name, bool allowDate)
        {
            var p = SettingsManager.Settings.Client.Logging.LogPath;
            if (string.IsNullOrEmpty(p))
            {
                p = @"\logs";
                SettingsManager.Settings.Client.Logging.LogPath = p;
            }
            var path = Functions.MainDir(p);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var b = new StringBuilder(path);
            /* Build log file name based on logging settings */
            if (SettingsManager.Settings.Client.Logging.CreateFolder)
            {
                b.Append(string.Format(@"\{0}", network));
                p = string.Format(@"{0}\{1}", path, network);
                if (!Directory.Exists(p))
                {
                    Directory.CreateDirectory(p);
                }
            }
            b.Append(string.Format(@"\{0}", RFileObj.Replace(name, "_")));
            if (SettingsManager.Settings.Client.Logging.DateByDay && allowDate)
            {
                b.Append(string.Format("-{0}", DateTime.Now.ToString("ddMMyyyy")));
            }
            return b.ToString();
        }

        public static string StripControlCodes(string text, bool strip)
        {
            return strip ? string.IsNullOrEmpty(text) ? string.Empty : RegExAllCodes.Replace(text, "") : text;
        }

        public static string TruncateString(string text, int length)
        {
            return text.Length > length ? string.Format("{0}...", text.Substring(0, length)) : text;
        }

        public static FrmColorIndex ShowColorIndexBox(Control box, int selectionStart)
        {
            if (_frmColor == null)
            {
                var pt = Point.Empty;
                /* Init new color form */
                _frmColor = new FrmColorIndex {Box = box};
                /* Get cursor position */
                GetCaretPos(ref pt);
                var x = box.PointToScreen(pt).X - (_frmColor.Size.Width / 2);
                var y = box.PointToScreen(pt).Y - ((_frmColor.Height - _frmColor.ClientRectangle.Height) - SystemInformation.CaptionHeight);
                /* Check form isn't off the screen */
                var i = Screen.PrimaryScreen.Bounds.Width - _frmColor.Size.Width;
                if (x < 0) { x = 0; }
                if (x > i) { x = i; }
                /* Set bounds */
                _frmColor.SetBounds(x, y - _frmColor.Size.Height, _frmColor.Size.Width, _frmColor.Size.Height);
                /* Show and set focus back to this textbox */                
                _frmColor.Show(box);
                box.Focus();
                return _frmColor;
            }
            return null;
        }

        public static void DestroyColorIndexBox()
        {
            if (_frmColor == null)
            {
                return;
            }
            _frmColor.Close();
            _frmColor.Dispose();
            _frmColor = null;
        }

        public static string FormatBytes(string bytes)
        {
            string[] sizes = { " Bytes", " KB", " MB", " GB", " TB", " PB" };
            double b;
            if (!double.TryParse(bytes, out b))
            {
                return "0 Bytes";
            }
            for (var i = sizes.Length - 1; i >= 0; i--)
            {
                if (b >= Math.Pow(1024, i))
                {
                    return ThreeNonZeroDigits(b / Math.Pow(1024, i)) + sizes[i];
                }
            }
            return "0 Bytes";
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

        private static string ThreeNonZeroDigits(double value)
        {
            return value >= 100 ? Math.Round(value).ToString() : value.ToString(value >= 10 ? "0.0" : "0.00");
        }
    }
}
