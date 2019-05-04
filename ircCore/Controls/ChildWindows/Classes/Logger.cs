/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Diagnostics;
using System.IO;

namespace ircCore.Controls.ChildWindows.Classes
{
    public class Logger
    {
        /* Log provider for chat windows - the aim of this class is to provide a modular way to handle various
         * options in logging (dating by day, making new folders per network, truncating the file, etc.) */
        private FileStream _stream;
        private StreamWriter _writer;

        public string FilePath { get; set; } /* The name (channel name, etc) of the window being logged */

        /* Destructor */
        ~Logger()
        {
            if (_stream != null && _writer != null)
            {
                CloseLog();
            }
        }

        public void CreateLog()
        {
            try
            {
                if (string.IsNullOrEmpty(FilePath))
                {
                    return;
                }
                /* Remember to dispose of this later */
                _stream = new FileStream(FilePath, FileMode.Append, FileAccess.Write);
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
                if (_stream != null && _writer != null && line != ((char)0).ToString())
                {
                    _writer.WriteLine(line);
                    _writer.Flush();
                    _stream.Flush();
                }
            }
            catch
            {
                Debug.Assert(true);
            }
        }
    }
}
