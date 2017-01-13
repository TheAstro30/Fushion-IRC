/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Threading;
using System.Windows.Forms;

namespace ircCore.Controls.ChildWindows.IrcWindow.Helpers
{
    internal static class InternalClipboard
    {
        /* Clipboard helper
           By: Jason James Newland & Adam Oldham
           ©2011-2012 - KangaSoft Software
           All Rights Reserved
         */
        private static string _data;
        private static string _newData;

        public static void AddText(string text)
        {
            if (string.IsNullOrEmpty(_data))
            {
                if (string.IsNullOrEmpty(text))
                {
                    return;
                }
                _data = text;
                var clipboardThread = new Thread(ClipboardThreadCallBack);
                clipboardThread.SetApartmentState(ApartmentState.STA);
                clipboardThread.IsBackground = true;
                clipboardThread.Start();
                return;
            }
            /* Thread currently running */
            _newData = text;
        }

        public static string GetText()
        {
            /* If an external exception with the clipboard is currently happening, return internal data */
            if (!string.IsNullOrEmpty(_newData))
            {
                return _newData;
            }
            if (!string.IsNullOrEmpty(_data))
            {
                return _data;
            }
            /* Return from system clipboard */
            var s = Clipboard.GetText();
            return !string.IsNullOrEmpty(s) ? s : null;
        }

        /* Thread callback */
        private static void ClipboardThreadCallBack()
        {
            var bSuccess = false;
            while (!bSuccess)
            {
                /* Load clipboard data until it actually loads */
                try
                {
                    Clipboard.SetText(!String.IsNullOrEmpty(_newData) ? _newData : _data);
                    bSuccess = true;
                    _data = null;
                    _newData = null;
                    return;
                }
                catch
                {
                    Thread.Sleep(100);
                }
            }
        }
    }
}