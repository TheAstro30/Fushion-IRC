/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using ircCore.Utils;

namespace ircClient.Classes
{
    public class Parser
    {
        private readonly ClientConnection _client;

        /* Public events */
        public event Action<ClientConnection> OnServerPingPong;

        public event Action<ClientConnection, string, string, string> OnJoinUser;
        public event Action<ClientConnection, string> OnJoinSelf;
        public event Action<ClientConnection, string, string, string> OnPartUser;
        public event Action<ClientConnection, string> OnPartSelf;

        public event Action<ClientConnection, string, string, string, string> OnTextChannel;
        public event Action<ClientConnection, string, string, string> OnTextSelf;
        public event Action<ClientConnection, string, string, string, string> OnActionChannel;
        public event Action<ClientConnection, string, string, string> OnActionSelf;        
        public event Action<ClientConnection, string, string, string> OnNotice;

        public event Action<ClientConnection, string, string> OnNick;
        public event Action<ClientConnection, string, string, string> OnQuit;

        public event Action<ClientConnection, string, string, string, string> OnKickUser;
        public event Action<ClientConnection, string, string, string> OnKickSelf;
        public event Action<ClientConnection, string, string> OnModeSelf;
        public event Action<ClientConnection, string, string, string, string> OnModeChannel;

        public event Action<ClientConnection, string, string> OnNames;
        public event Action<ClientConnection, string, string, string> OnWho;

        public event Action<ClientConnection, string> OnMotd;
        public event Action<ClientConnection, string> OnWelcome;

        public event Action<ClientConnection, string, string, string> OnWallops;

        public event Action<ClientConnection, string, string> OnTopicIs;
        public event Action<ClientConnection, string, string> OnTopicSetBy;
        public event Action<ClientConnection, string, string, string> OnTopicChanged;

        public event Action<ClientConnection, string> OnRaw;

        public event Action<ClientConnection, string> OnOther;

        /* Public properties */
        public string JoinChannelsOnConnect { get; set; }

        public string UserModeCharacters { get; set; }
        public string UserModes { get; set; }

        /* Constructor */
        public Parser(ClientConnection client)
        {
            _client = client;
        }

        /* Main parsing entry point */
        public void Parse(string first, string second, string third, string fourth)
        {
            switch (second.ToUpper())
            {
                case "PING":
                    _client.Send(string.Format("PONG {0}", first));
                    if (OnServerPingPong != null)
                    {
                        OnServerPingPong(_client);
                    }
                    break;

                case "JOIN":
                    /* First token is the nick and address - third is target */
                    ParseJoin(first, third);                    
                    break;

                case "PART":
                    ParsePart(first, third);
                    break;

                case "PRIVMSG":
                    ParsePrivateMesasage(first, third, fourth);
                    break;

                case "NOTICE":
                    ParseNotice(first, fourth);
                    break;

                case "TOPIC":
                    ParseTopicChanged(first, third, fourth);
                    break;

                case "NICK":
                    ParseNick(first, third);
                    break;

                case "QUIT":
                    ParseQuit(first, string.Format("{0} {1}", third, fourth));
                    break;

                case "KICK":
                    ParseKick(first, third, fourth);
                    break;

                case "MODE":
                    ParseMode(first, third, fourth);
                    break;

                case "WALLOPS":                    
                    ParseWallops(first, string.Format("{0} {1}", third, fourth));
                    break;

                case "001":
                    /* Welcome message */
                    _client.IsConnected = true;
                    _client.IsConnecting = false;
                    if (OnWelcome != null)
                    {
                        OnWelcome(_client, RemoveColon(fourth));
                    }
                    break;

                case "002":
                case "003":
                case "004":
                    if (OnWelcome != null)
                    {
                        OnWelcome(_client, RemoveColon(fourth));
                    }
                    break;

                case "005":
                    ParseProtocols(fourth);
                    if (OnWelcome != null)
                    {
                        OnWelcome(_client, RemoveColon(fourth.Replace(":are", "are")));
                    }
                    break;

                case "332":
                    /* Topic is */
                    ParseTopicIs(fourth);
                    break;

                case "333":
                    /* Topic set by */
                    ParseTopicSetBy(fourth);
                    break;

                case "352":
                    /* Who list - #temp ~TheAstro3 Manager.DragonIRC.com Saphira.US.DragonIRC.com DragonsBlood H* :0 TheAstro30 */
                    ParseWho(fourth);
                    break;

                case "372":
                case "375":
                    /* Motd Text */
                    if (OnMotd != null)
                    {
                        OnMotd(_client, RemoveColon(fourth));
                    }
                    break;

                case "376":
                case "422":
                    /* End of MOTD/MOTD file missing */                    
                    if (!string.IsNullOrEmpty(JoinChannelsOnConnect))
                    {
                        _client.Send(string.Format("JOIN {0}", JoinChannelsOnConnect));
                        JoinChannelsOnConnect = string.Empty;
                    }
                    if (OnMotd != null)
                    {
                        OnMotd(_client, RemoveColon(fourth));
                    }
                    break;

                case "331":
                case "381":
                case "401":
                case "421":
                case "433":
                case "464":
                case "481":
                case "491":
                    /* Other raws */
                    if (OnRaw != null)
                    {
                        OnRaw(_client, string.Format("* {0}", fourth.Replace(":", "")));
                    }
                    break;

                case "353":
                    /* Channel names :server yournick := #channel :names */
                    ParseNames(fourth);
                    break;

                default:
                    if (OnOther != null)
                    {
                        OnOther(_client, string.Format("Unknown - {0} {1} {2} {3}", first, second, third, fourth));
                    }
                    break;
            }
        }

        /* Private parse methods */
        private void ParseJoin(string nick, string channel)
        {
            var n = RemoveColon(nick).Split('!');
            if (n.Length == 0)
            {
                return; /*This should never happen */
            }
            if (n[0].Equals(_client.UserInfo.Nick, StringComparison.InvariantCultureIgnoreCase))
            {
                /* Self join */
                if (OnJoinSelf != null)
                {
                    OnJoinSelf(_client, RemoveColon(channel));
                }
            }
            else
            {
                if (OnJoinUser != null)
                {
                    OnJoinUser(_client, n[0], n.Length > 1 ? n[1] : "", RemoveColon(channel));
                }
            }
        }

        private void ParsePart(string nick, string channel)
        {
            var n = RemoveColon(nick).Split('!');
            if (n.Length == 0)
            {
                return; /*This should never happen */
            }            
            if (n[0].Equals(_client.UserInfo.Nick, StringComparison.InvariantCultureIgnoreCase))
            {             
                /* Yourself */
                if (OnPartSelf != null)
                {             
                    OnPartSelf(_client, channel);
                }
                return;
            }                                        
            if (OnPartUser != null)
            {
                OnPartUser(_client, n[0], n.Length > 1 ? n[1] : "", RemoveColon(channel));
            }
        }

        private void ParseTopicIs(string data)
        {
            var i = data.IndexOf(' ');
            if (i == -1)
            {
                return;
            }
            /* First token is channel */
            var channel = data.Substring(0, i).Trim();
            data = RemoveColon(data.Substring(i).Trim());
            if (OnTopicIs != null)
            {
                OnTopicIs(_client, channel, data);
            }
        }

        private void ParseTopicSetBy(string data)
        {
            /* We treat this a little differently, we need to change the last token from a long format to a date format */
            var i = data.Split(' ');
            if (i.Length == 0)
            {
                return;
            }
            /* First token is channel */
            var channel = i[0];
            i[0] = string.Empty; /* Null it */
            i[i.Length - 1] = TimeFunctions.FormatAsciiTime(i[i.Length - 1], null);
            if (OnTopicSetBy != null)
            {
                OnTopicSetBy(_client, channel, string.Join(" ", i).Trim());
            }
        }

        private void ParseTopicChanged(string nick, string channel, string data)
        {
            var n = RemoveColon(nick).Split('!');
            if (n.Length == 0)
            {
                return; /*This should never happen */
            }
            if (OnTopicChanged != null)
            {
                OnTopicChanged(_client, n[0], channel, RemoveColon(data));
            }
        }

        private void ParsePrivateMesasage(string nick, string target, string text)
        {
            var n = RemoveColon(nick).Split('!');
            if (n.Length == 0)
            {
                return; /*This should never happen */
            }
            var s = RemoveColon(text);
            int i;
            if (target.Equals(_client.UserInfo.Nick, StringComparison.InvariantCultureIgnoreCase))
            {
                /* You were messaged */
                if (s[0] == '\x01')
                {
                    /* Action - \x01ACTION text\x01 */
                    i = s.IndexOf(' ');
                    if (i == -1)
                    {
                        return;
                    }
                    if (OnActionSelf != null)
                    {
                        OnActionSelf(_client, n[0], n.Length > 1 ? n[1] : "", s.Substring(i, s.Length - i - 1).Trim());
                    }
                    return;
                }
                if (OnTextSelf != null)
                {
                    OnTextSelf(_client, n[0], n.Length > 1 ? n[1] : "", s);
                }
            }
            else
            {
                /* Message is to a channel */
                if (s[0] == '\x01')
                {
                    /* Channel action - \x01ACTION text\x01 */
                    i = s.IndexOf(' ');
                    if (i == -1)
                    {
                        return;
                    }
                    if (OnActionChannel != null)
                    {
                        OnActionChannel(_client, n[0], n.Length > 1 ? n[1] : "", target, s.Substring(i, s.Length - i - 1).Trim());
                    }
                    return;
                }
                if (OnTextChannel != null)
                {
                    OnTextChannel(_client, n[0], n.Length > 1 ? n[1] : "", target, s);
                }
            }
        }

        private void ParseNotice(string nick, string msg)
        {
            /* Both server and normal notices raises similar events, so we just have one */
            var n = RemoveColon(nick).Split('!');
            if (n.Length == 0)
            {
                return; /*This should never happen */
            }
            var i = msg.IndexOf(' ');
            if (i > -1)
            {
                /* :Saphira.US.DragonIRC.com NOTICE AUTH :*** Looking up your hostname... */
                var tmp = msg.Substring(0, i).Trim();
                if (!string.IsNullOrEmpty(tmp) && tmp.Equals("auth", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (OnNotice != null)
                    {
                        OnNotice(_client, n[0], n.Length > 1 ? n[1] : "", RemoveColon(msg.Substring(i).Trim()));
                    }
                    return;
                }
            }
            /* Either a server notice or user notice - its kind of difficult to differentiate between the two */
            if (OnNotice != null)
            {
                OnNotice(_client, n[0], n.Length > 1 ? n[1] : "", RemoveColon(msg));
            }
        }

        private void ParseWallops(string nick, string data)
        {
            var n = RemoveColon(nick).Split('!');
            if (n.Length == 0)
            {
                return; /*This should never happen */
            }
            System.Diagnostics.Debug.Print(data);
            if (OnWallops != null)
            {
                OnWallops(_client, n[0], n.Length > 1 ? n[1] : "", RemoveColon(data));
            }
        }

        private void ParseNick(string nick, string newNick)
        {
            var n = RemoveColon(nick).Split('!');
            if (n.Length == 0)
            {
                return; /*This should never happen */
            }
            if (OnNick != null)
            {
                OnNick(_client, n[0], RemoveColon(newNick));
            }
        }

        private void ParseQuit(string nick, string msg)
        {
            var n = RemoveColon(nick).Split('!');
            if (n.Length == 0)
            {
                return; /*This should never happen */
            }
            if (OnQuit != null)
            {
                OnQuit(_client, n[0], n.Length > 1 ? n[1] : "", RemoveColon(msg.Trim()));
            }
        }

        private void ParseKick(string nick, string channel, string msg)
        {
            var n = RemoveColon(nick).Split('!');
            if (n.Length == 0)
            {
                return; /*This should never happen */
            }
            var i = msg.IndexOf(' ');
            if (i == -1)
            {
                return;
            }
            var knick = msg.Substring(0, i).Trim();
            msg = RemoveColon(msg.Substring(i).Trim());
            if (knick.Equals(_client.UserInfo.Nick, StringComparison.InvariantCultureIgnoreCase) && OnKickSelf != null)
            {
                OnKickSelf(_client, n[0], channel, msg);
            }
            else if (OnKickUser != null)
            {
                OnKickUser(_client, n[0], knick, channel, msg);
            }
        }

        private void ParseMode(string nick, string target, string data)
        {
            var n = RemoveColon(nick).Split('!');
            if (n.Length == 0)
            {
                return; /*This should never happen */
            }
            if (target.Equals(_client.UserInfo.Nick, StringComparison.InvariantCultureIgnoreCase))
            {           
                if (OnModeSelf != null)
                {
                    /* Self mode */
                    OnModeSelf(_client, n[0], RemoveColon(data));
                }
                return;
            }
            /* data = +vo nick nick ... etc */
            var i = data.IndexOf(' ');
            if (i == -1)
            {
                return;
            }
            var modes = data.Substring(0, i).Trim();
            data = RemoveColon(data.Substring(i).Trim());
            if (OnModeChannel != null)
            {
                OnModeChannel(_client, n[0], target, modes, data);
            }
        }

        private void ParseProtocols(string data)
        {
            var prot = RemoveColon(data).Split(' ');
            if (prot.Length == 0)
            {
                return;
            }
            foreach (var p in prot)
            {
                if (p == "IRCX")
                {
                    _client.Send("IRCX");
                    UserModeCharacters = ".@+";
                    UserModes = "qov";
                }
                var sections = p.Split('=');
                if (sections.Length < 2)
                {
                    continue;                    
                }
                switch (sections[0])
                {
                    case "PREFIX":                        
                        var prefix = sections[1].Split(')');
                        if (prefix.Length == 2)
                        {                            
                            UserModes = prefix[0].Replace("(", "");
                            UserModeCharacters = prefix[1];
                        }
                        break;
                }
            }
        }

        private void ParseNames(string data)
        {
            var tmp = RemoveColon(data);
            var i = tmp.IndexOf(' ');
            if (i == -1)
            {
                return;
            }
            var channel = tmp.Substring(i).Trim();
            i = channel.IndexOf(' ');
            if (i == -1)
            {
                return;
            }
            var names = RemoveColon(channel.Substring(i + 1).Trim());
            channel = channel.Substring(0, i).Trim();
            if (OnNames != null)
            {
                OnNames(_client, channel, names);
            }
        }

        private void ParseWho(string data)
        {            
            var s = data.Split(' ');
            if (s.Length < 4)
            {
                return;
            }
            if (OnWho != null)
            {
                OnWho(_client, s[4], s[0], string.Format("{0}@{1}", s[1], s[2]));
            }
        }

        /* Private methods */
        private static string RemoveColon(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return string.Empty;
            }
            return data[0] == ':' ? data.Substring(1) : data;
        }
    }
}
