/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ircClient.Tcp;

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

        public event Action<ClientConnection, string, string> OnNick;
        public event Action<ClientConnection, string, string, string> OnQuit;

        public event Action<ClientConnection, string, string, string, string> OnKickUser;
        public event Action<ClientConnection, string, string, string> OnKickSelf;
        public event Action<ClientConnection, string, string> OnModeSelf;
        public event Action<ClientConnection, string, string, string, string> OnModeChannel;

        public event Action<ClientConnection, string, string> OnNames;
        public event Action<ClientConnection, string, string, string> OnWho;

        public string JoinChannelsOnConnect { get; set; }

        public string UserModeCharacters { get; set; }
        public string UserModes { get; set; }

        public Parser(ClientConnection client)
        {
            _client = client;
        }

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

                case "001":
                    /* Welcome message */
                    _client.IsConnected = true;
                    _client.IsConnecting = false;
                    break;

                case "005":
                    ParseProtocols(fourth);
                    break;

                case "352":
                    /* Who list */
                    ParseWho(third, fourth);
                    break;

                case "376":
                case "422":
                    /* End of MOTD/MOTD file missing */                    
                    if (!string.IsNullOrEmpty(JoinChannelsOnConnect))
                    {
                        _client.Send(string.Format("JOIN {0}", JoinChannelsOnConnect));
                        JoinChannelsOnConnect = string.Empty;
                    }
                    break;

                case "353":
                    /* Channel names :server yournick := channel :names */
                    ParseNames(fourth);
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
            if (n[0].ToLower() == _client.UserInfo.Nick.ToLower())
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
            if (n[0].ToLower() == _client.UserInfo.Nick.ToLower())
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

        private void ParsePrivateMesasage(string nick, string target, string text)
        {
            var n = RemoveColon(nick).Split('!');
            if (n.Length == 0)
            {
                return; /*This should never happen */
            }
            var s = RemoveColon(text);
            int i;
            if (target.ToLower() == _client.UserInfo.Nick.ToLower())
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
            if (knick.ToLower() == _client.UserInfo.Nick.ToLower() && OnKickSelf != null)
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
            if (target.ToLower() == _client.UserInfo.Nick.ToLower())
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

        private void ParseWho(string nick, string data)
        {            
            var s = data.Split(' ');
            if (s.Length < 4)
            {
                return;
            }
            if (OnWho != null)
            {
                OnWho(_client, nick, s[0], string.Format("{0}@{1}", s[1], s[2]));
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
