/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Reflection;
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

        public event Action<ClientConnection, string, bool> OnMotd;
        public event Action<ClientConnection, string> OnLUsers;
        public event Action<ClientConnection, string> OnWelcome;
        public event Action<ClientConnection, string, string, string> OnInvite;

        public event Action<ClientConnection, string, string, string> OnWallops;

        public event Action<ClientConnection, string, string> OnTopicIs;
        public event Action<ClientConnection, string, string> OnTopicSetBy;
        public event Action<ClientConnection, string, string, string> OnTopicChanged;

        public event Action<ClientConnection, string, string, string> OnCtcp;
        public event Action<ClientConnection, string, string, string, string> OnCtcpReply;
        public event Action<ClientConnection> OnWhois;
        public event Action<ClientConnection, string> OnRaw;

        public event Action<ClientConnection, string> OnOther; //probably deleting this
       
        /* Public properties */
        public string JoinChannelsOnConnect { get; set; }

        public string UserModeCharacters { get; set; }
        public string UserModes { get; set; }

        public WhoisInfo Whois = new WhoisInfo();

        /* Constructor */
        public Parser(ClientConnection client)
        {
            _client = client;
        }

        /* Main parsing entry point */
        public void Parse(string first, string second, string third, string fourth)
        {            
            if (!string.IsNullOrEmpty(first))
            {
                switch (first.ToUpper())
                {
                    case "NOTICE":
                        /* Undernet crap */
                        if (second.Equals("auth", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (OnNotice != null)
                            {
                                OnNotice(_client, _client.Server.Address, "", RemoveColon(string.Format("{0} {1}", third, fourth)));
                            }
                        }
                        break;
                }
            }
            int i;
            string[] n;
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

                case "INVITE":
                    ParseInvite(first, fourth);
                    break;

                case "001":
                    /* Welcome message */
                    _client.IsConnected = true;
                    _client.IsConnecting = false;
                    if (!third.Equals(_client.UserInfo.Nick, StringComparison.InvariantCultureIgnoreCase))
                    {
                        /* Nickname from server doesn't match what was sent - possibly truncated */
                        _client.UserInfo.Nick = third;
                    }
                    if (OnWelcome != null)
                    {
                        OnWelcome(_client, RemoveColon(fourth));
                    }
                    break;

                case "002":
                case "003":
                case "004":
                    if (!third.Equals(_client.UserInfo.Nick, StringComparison.InvariantCultureIgnoreCase))
                    {
                        /* Nickname from server doesn't match what was sent - possibly truncated */
                        _client.UserInfo.Nick = third;
                    }
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

                case "250":
                case "251":
                case "252":
                case "253":
                case "254":
                case "255":
                case "265":
                case "266":
                    /* LUsers */
                    if (OnLUsers != null)
                    {
                        OnLUsers(_client, fourth.Replace(":", ""));
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
                        OnMotd(_client, RemoveColon(fourth), false);
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
                        OnMotd(_client, RemoveColon(fourth), true);
                    }
                    break;

                case "301":
                    /* Whois replies */
                    i = fourth.IndexOf(' ');
                    if (i > -1)
                    {
                        Whois.AwayMessage = RemoveColon(fourth.Substring(i).Trim());
                    }
                    break;

                case "275":
                case "307":
                case "308":
                case "309":
                case "310":
                case "313":
                case "316":
                case "330":
                case "378":
                case "379":
                    /* Whois replies */
                    Whois.OtherInfo.Add(fourth.Replace(":", ""));
                    break;

                case "317":
                    /* Idle/signon */
                    n = fourth.Split(' ');
                    if (n.Length > 2)
                    {
                        int idle;
                        int.TryParse(n[1], out idle);
                        Whois.OtherInfo.Add(string.Format("{0} has been idle {1}, signed on: {2}", n[0],
                                                          TimeFunctions.GetDuration(idle, false),
                                                          TimeFunctions.FormatAsciiTime(n[2], null)));
                    }
                    break;

                case "311":
                    /* Whois user <nick> <user> <host> * :<real_name> */
                    var colon = fourth.IndexOf(':');                    
                    if (colon > -1)
                    {
                        var nick = fourth.Substring(0, colon).Trim();
                        var name = fourth.Substring(colon).Trim();
                        n = nick.Split(' ');
                        if (n.Length > 2)
                        {
                            Whois.Nick = n[0];
                            Whois.Address = string.Format("{0}@{1}", n[1], n[2]);
                        }
                        Whois.Realname = RemoveColon(name);
                    }                    
                    break;

                case "312":
                    /* Whois Server */
                    i = fourth.IndexOf(' ');
                    if (i > -1)
                    {
                        Whois.Server = string.Format("{0})", fourth.Substring(i).Trim().Replace(":", "("));
                    }
                    break;

                case "318":
                    /* End of WHOIS */
                    if (OnWhois != null)
                    {
                        OnWhois(_client);
                    }
                    break;

                case "319":
                    /* Whois channels */
                    i = fourth.IndexOf(' ');
                    if (i > -1)
                    {
                        Whois.Channels = RemoveColon(fourth.Substring(i).Trim()).Replace(" ", ", ");
                    }
                    break;

                case "404":
                case "421":
                case "432":
                case "471":
                case "473":
                case "474":
                case "475":
                    /* These raws we take the first token and put it at the end */
                    i = fourth.IndexOf(' ');
                    if (i != -1)
                    {
                        var s = fourth.Substring(0, i).Trim();
                        fourth = fourth.Substring(i).Trim();
                        if (OnRaw != null)
                        {
                            OnRaw(_client, string.Format("{0}: {1}", RemoveColon(fourth), s));
                        }
                    }
                    break;

                case "331":
                case "381":
                case "396":
                case "401":                
                case "433":
                case "437":
                case "464":
                case "468":
                case "472":
                case "481":
                case "491":
                case "501":
                    /* Other raws */
                    if (OnRaw != null)
                    {
                        OnRaw(_client, string.Format("{0}", fourth.Replace(":", "")));
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

        private void ParseInvite(string nick, string channel)
        {
            var n = RemoveColon(nick).Split('!');
            if (n.Length == 0)
            {
                return; /*This should never happen */
            }
            if (OnInvite != null)
            {
                OnInvite(_client, n[0], n.Length > 2 ? n[1] : "", RemoveColon(channel));
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
                    /* Either a CTCP or ACTION */
                    s = RemoveColon(s.Substring(1, s.Length - 2));
                    switch (s)
                    {
                        case "VERSION":
                            var version = Assembly.GetExecutingAssembly().GetName().Version;
                            _client.Send(string.Format("NOTICE {0} :{1}VERSION FusionIRC v{2}.{3} by Jason James Newland{4}", n[0], (char) 1, version.Major, version.Minor, (char) 1));
                            break;

                        case "TIME":
                            _client.Send(string.Format("NOTICE {0} :{1}TIME {2}{3}", n[0], (char) 1, string.Format("{0:ddd dd MMM yyyy, H:mm:ss tt}", DateTime.Now), (char) 1));
                            break;
                    }
                    i = s.IndexOf(' ');
                    if (i == -1)
                    {
                        if (OnCtcp != null)
                        {
                            OnCtcp(_client, n[0], n.Length > 1 ? n[1] : string.Empty, s);
                        }
                        /* Ignore it */
                        return;
                    }
                    var ctcp = s.Substring(0, i).Trim();
                    var t = s.Substring(i, s.Length - i).Trim();
                    switch (ctcp)
                    {
                        case "PING":
                            _client.Send(string.Format("NOTICE {0} :{1}PING {2}{3}", n[0], (char) 1, t, (char) 1));
                            if (OnCtcp != null)
                            {
                                OnCtcp(_client, n[0], n.Length > 1 ? n[1] : string.Empty, ctcp);
                            }
                            break;

                        case "DCC":
                            /* Place holder */
                            System.Diagnostics.Debug.Print("DCC " + t);
                            break;

                        case "ACTION":
                            if (OnActionSelf != null)
                            {
                                OnActionSelf(_client, n[0], n.Length > 1 ? n[1] : "", t);
                            }
                            break;
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
            /* Determine if it's a notice or a CTCP reply */
            var s = RemoveColon(msg);
            int i;
            if (s[0] == '\x01')
            {
                /* Most likely a CTCP */
                s = s.Substring(1, s.Length - 2);                
                i = s.IndexOf(' ');
                if (i > -1)
                {
                    if (OnCtcpReply != null)
                    {                        
                        OnCtcpReply(_client, n[0], n.Length > 1 ? n[1] : "", s.Substring(0, i).Trim(), s.Substring(i).Trim());
                    }
                }
                return;
            }
            i = s.IndexOf(' ');
            if (i > -1)
            {
                /* :Saphira.US.DragonIRC.com NOTICE AUTH :*** Looking up your hostname... */
                var tmp = msg.Substring(0, i).Trim();
                if (!string.IsNullOrEmpty(tmp) && tmp.Equals("auth", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (OnNotice != null)
                    {
                        OnNotice(_client, n[0], n.Length > 1 ? n[1] : "", s.Substring(i).Trim());
                    }
                    return;
                }
            }
            /* Either a server notice or user notice - its kind of difficult to differentiate between the two */
            if (OnNotice != null)
            {
                OnNotice(_client, n[0], n.Length > 1 ? n[1] : "", s);
            }
        }

        private void ParseWallops(string nick, string data)
        {
            var n = RemoveColon(nick).Split('!');
            if (n.Length == 0)
            {
                return; /*This should never happen */
            }            
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
            if (n.Length == 0 || n[0].Equals(_client.UserInfo.Nick, StringComparison.InvariantCultureIgnoreCase))
            {
                return; /*This should never happen - however, some servers send QUIT on self-quit */
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
