/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using FusionIRC.Forms.Child;
using FusionIRC.Helpers.Commands;
using ircClient;
using ircClient.Parsing.Helpers;
using ircScript.Classes.Helpers;

namespace FusionIRC.Helpers
{
    public static class CommandProcessor
    {
        /* Main parsing entry point */
        public static void Parse(ClientConnection client, FrmChildWindow child, string data)
        {
            var i = data.IndexOf(' ');
            string com;
            var args = string.Empty;
            if (i != -1)
            {
                com = data.Substring(0, i).Trim().ToUpper().Replace("/", "");
                args = data.Substring(i).Trim();
            }
            else
            {
                com = data.ToUpper().Replace("/", "");
            }
            ParseCommand(client, child, com, args);
        }

        public static void ParseCommand(ClientConnection client, FrmChildWindow child, string command, string args)
        {
            /* First check it's not an alias */
            if (CommandAlias.ParseAlias(client, child, command, args))
            {
                return; /* Process no further */
            }
            switch (command)
            {
                case "CLR":
                case "CLEAR":
                    /* Clear child output window of text */
                    child.Output.Clear();
                    break;

                case "SERVER":
                    CommandServer.ParseServerConnection(client, args);
                    break;

                case "DISCONNECT":
                    CommandServer.ParseServerDisconnection(client);
                    break;

                case "ME":
                case "ACTION":
                case "DESCRIBE":
                    /* Action */
                    CommandText.ParseAction(client, child, args);
                    break;

                case "AME":
                    CommandText.ParseAme(client, args);
                    break;

                case "AMSG":
                    CommandText.ParseAmsg(client, args);
                    break;

                case "MSG":
                    CommandText.ParseMsg(client, child, args);
                    break;

                case "SAY":
                    CommandText.ParseSay(client, child, args);
                    break;

                case "NOTICE":
                    CommandText.ParseNotice(client, child, args);
                    break;

                case "PART":
                    CommandChannel.ParsePart(client, child, args);
                    break;

                case "HOP":
                    CommandChannel.ParseHop(client, child);
                    break;

                case "NICK":
                    CommandNick.ParseNick(client, args);
                    break;

                case "NAMES":
                    CommandChannel.ParseNames(client, args);
                    break;

                case "TOPIC":
                    CommandChannel.ParseTopic(client, args);
                    break;

                case "WHOIS":
                    if (!client.IsConnected || string.IsNullOrEmpty(args))
                    {
                        return;
                    }
                    client.Parser.Whois = new WhoisInfo();
                    var n = args.Split(' ');
                    if (n.Length == 0)
                    {
                        return;
                    }
                    client.Send(string.Format("WHOIS {0}", n[0]));
                    break;

                case "CTCP":
                    CommandCtcp.ParseCtcp(client, args);
                    break;

                case "DNS":
                    CommandDns.ParseDns(client, args);
                    break;

                case "PASS":
                    client.Send(string.Format("PASS :{0}", args));
                    break;

                case "ECHO":
                    CommandText.ParseEcho(client, args);
                    break;

                case "SPLAY":
                    CommandSoundPlay.Parse(args);
                    break;

                case "WRITE":
                    CommandWrite.Write(args);
                    break;

                case "WRITEINI":
                    CommandFiles.WriteIni(args);
                    break;

                case "HMAKE":
                    CommandHash.HashMake(args);
                    break;

                case "HFREE":
                    CommandHash.HashFree(args);
                    break;

                case "HDEL":
                    CommandHash.HashDelete(args);
                    break;

                case "HLOAD":
                    CommandHash.HashLoad(args);
                    break;

                case "HSAVE":
                    CommandHash.HashSave(args);
                    break;

                case "HADD":
                    CommandHash.HashAdd(args);
                    break;

                default:
                    /* Send command to server */
                    if (!client.IsConnected)
                    {
                        return;
                    }
                    client.Send(string.Format("{0} {1}", command, args));
                    break;
            }
        }
    }
}
