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
        public event Action<ClientConnection, string, string, string> OnJoinUser;
        public event Action<ClientConnection, string> OnJoinSelf;
        public event Action<ClientConnection, string, string, string> OnPartUser;

        public event Action<ClientConnection, string, string, string, string> OnTextChannel;
        public event Action<ClientConnection, string, string, string> OnTextSelf;
        public event Action<ClientConnection, string, string, string, string> OnActionChannel;
        public event Action<ClientConnection, string, string, string> OnActionSelf;

        public event Action<ClientConnection, string, string> OnNick;

        public event Action<ClientConnection, string, string> OnNames;

        public string JoinChannelsOnConnect { get; set; }

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

                case "001":
                    /* Welcome message */
                    _client.IsConnected = true;
                    _client.IsConnecting = false;
                    break;

                case "376":
                    /* End of MOTD */                    
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

        private void ParseNames(string data)
        {
            var tmp = RemoveColon(data);
            var i = tmp.IndexOf(' ');
            if (i <= -1)
            {
                return;
            }
            var channel = tmp.Substring(i).Trim();
            i = channel.IndexOf(' ');
            if (i <= -1)
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
