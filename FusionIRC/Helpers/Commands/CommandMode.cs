/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Text;
using ircClient;

namespace FusionIRC.Helpers.Commands
{
    public static class CommandMode
    {
        public static void ParseModes(ClientConnection client, string args)
        {
            var i = args.IndexOf(' ');
            var extra = string.Empty;
            if (i == -1)
            {
                return;
            }
            var channel = args.Substring(0, i);
            var mode = args.Substring(i + 1);
            i = mode.IndexOf(' ');
            if (i != -1)
            {
                extra = mode.Substring(i + 1);
                mode = mode.Substring(0, i); 
            }
            ParseOddModes(client, channel, mode, extra);
        }

        public static void ParseOddModes(ClientConnection client, string channel, string modeString, string modeExtra)
        {
            /*  Parses odd modes and splits them down to the allowed mode length
                ie +bb-b+ov-q+bb and if max modes is 6 will become
                +bb-b+ov-q
                +bb
                respectively */
            if (string.IsNullOrEmpty(modeString))
            {
                return;
            }
            var j = 0;
            var t = 0;
            var mode = new StringBuilder();
            var extra = modeExtra.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var bPlusMode = false;
            var modes = client.Parser.ModeLength;
            for (var i = 0; i <= modeString.Length - 1; i++)
            {
                switch (modeString[i])
                {
                    case '+':
                        mode.Append("+");
                        bPlusMode = true;
                        continue;

                    case '-':
                        mode.Append("-");
                        bPlusMode = false;
                        continue;

                    default:
                        mode.Append(modeString[i]);
                        t++;
                        break;
                }
                if (t != modes)
                {
                    continue;
                }
                t = 0;
                if (mode.Length > 0)
                {
                    if (mode[0] != '+' && mode[0] != '-')
                    {
                        mode.Insert(0, bPlusMode ? '+' : '-');                        
                    }
                    client.Send(string.Format("MODE {0} {1} {2}", channel, mode, string.Join(" ", extra, j, modes)));                    
                }
                mode.Clear();
                j += modes;
            }
            /* Whatever is left */
            if (mode.Length == 0)
            {
                return;
            }
            if (mode[0] != '+' && mode[0] != '-')
            {
                mode.Insert(0, bPlusMode ? '+' : '-');               
            }
            if (j + modes > extra.Length)
            {
                modes = extra.Length - j;
            }
            client.Send(string.Format("MODE {0} {1} {2}", channel, mode, string.Join(" ", extra, j, modes)));
        }

        //public static void MassMode(ClientConnection client, string channel, List<string> nicks, string mode)
        //{
        //    if (!client.IsConnected || nicks == null || nicks.Count == 0)
        //    {
        //        return;
        //    }
        //    var length = client.Parser.ModeLength;
        //    if (mode == "+b")
        //    {
        //        /* Get each user IAL */
        //        var ial = nicks.Select(n => client.Ial.Get(n)).ToArray();
        //        DoMassMode(client, length, channel, mode, ial);
        //    }
        //    else
        //    {
        //        DoMassMode(client, length, channel, mode, nicks.ToArray());
        //    }
        //}

        //private static void DoMassMode(ClientConnection client, int length, string channel, string mode, string[] data)
        //{
        //    for (var i = 0; i <= data.Length - 1; i++)
        //    {
        //        if (i + length > data.Length)
        //        {
        //            length = data.Length - i;
        //        }
        //        client.Send(string.Format("MODE {0} {1}{2} {3}", channel, mode[0], new string(mode[1], length),
        //                                  string.Join(" ", data, i, length)));
        //        i += length - 1;
        //    }
        //}
    }
}
