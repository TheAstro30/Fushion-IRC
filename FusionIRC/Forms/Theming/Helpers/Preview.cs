/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using ircCore.Controls.ChildWindows.OutputDisplay;
using ircCore.Settings.Theming;
using ircCore.Settings.Theming.Structures;

namespace FusionIRC.Forms.Theming.Helpers
{
    public static class Preview
    {
        public static void Show(OutputWindow window, Theme theme, ThemeMessage message)
        {
            /* Human readble data */
            window.Clear();
            window.BackColor = theme.Colors[theme.ThemeColors[ThemeColor.OutputWindowBackColor]];
            var tmd = new IncomingMessageData
                          {
                              Message = message,
                              TimeStamp = DateTime.Now,
                              Nick = "sampleNick",
                              NewNick = "newNick",
                              KickedNick = "kickedNick",
                              Address = "address.com",
                              Target = "#Preview",
                              Server = "irc.preview.com",
                              Port = 6667,
                              Text = "Sample text",
                              DnsAddress = "192.168.0.1",
                              DnsHost = "irc.preview.com",
                              Prefix = "@"
                          };
            var pmd = ThemeManager.ParseMessage(tmd, theme);
            window.AddLine(pmd.DefaultColor, pmd.Message);
        }
    }
}
