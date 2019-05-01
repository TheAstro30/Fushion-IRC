/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Diagnostics;
using System.IO;
using ircCore.Settings.SettingsBase.Structures;

namespace ircCore.Utils
{
    public enum LogType
    {
        Channel = 0,
        Chat = 1
    }

    public class Logging
    {
        /* Log provider for chat windows - the aim of this class is to provide a modular way to handle various
         * options in logging (dating by day, making new folders per network, truncating the file, etc.) */
        private readonly SettingsLog _settings;

        private FileStream _stream;
        private StreamWriter _writer;

        public LogType Type { get; set; }

        public string Name { get; set; } /* The name (channel name, etc) of the window being logged */

        public Logging(SettingsLog settings)
        {
            _settings = settings;
        }

        ~Logging()
        {
            if (_stream != null && _writer != null)
            {
                CloseLog();
            }
        }

        public void CreateLog()
        {
            var doNotLog = false;
            switch (Type)
            {
                case LogType.Channel:
                    doNotLog = !_settings.LogChannels;
                    break;

                case LogType.Chat:
                    doNotLog = !_settings.LogChats;
                    break;
            }
            if (doNotLog)
            {
                if (_writer != null)
                {
                    /* If it was logging and in settings we turned it off... close it */
                    CloseLog();
                }
                return;
            }
            try
            {
                /* Remember to dispose of this later */
                _stream = new FileStream(CreateLogFileName(), FileMode.Append, FileAccess.Write);
                /* And this... */
                _writer = new StreamWriter(_stream);
                WriteLog(string.Format(@"*** {0} {1} ***", "Log file created:", DateTime.Now));
            }
            catch (Exception)
            {
                Debug.Assert(true);
            }            
        }

        public void CloseLog()
        {
            if (_writer != null)
            {
                WriteLog(string.Format(@"*** {0} {1} ***{2}", "Log file closed:", DateTime.Now, Environment.NewLine));
                _writer.Flush();
                _writer.Close();
                _writer.Dispose();
            }
            if (_stream != null)
            {
                _stream.Close();
                _stream.Dispose();
            }
            _stream = null;
            _writer = null;
        }

        public void WriteLog(string line)
        {
            try
            {
                if (_stream != null && _writer != null)
                {
                    _writer.WriteLine(line);
                }
            }
            catch
            {
                Debug.Assert(true);
            }
        }

        /* Private helpers */
        private string CreateLogFileName()
        {
            /* Based on settings, we format our log file name */
            var logPath = Functions.MainDir(_settings.LogPath, false);
            if (!Directory.Exists(logPath))
            {
                /* Create it */
                Directory.CreateDirectory(logPath);
            }
            /* We also have to be aware of illegal characters in file names... */
            //for now, return basic file name
            return string.Format(@"{0}\{1}.txt", logPath, Name);
        }
    }
}
