/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ircScript.Classes.ScriptFunctions
{
    public static class Address
    {
        private static readonly Regex IrcAddress = new Regex(@"(?<nick>[^ ]+?)\!(?<user>[^ ]+?)@(?<host>[^ ]+?)$", RegexOptions.Compiled);

        public static string GetIrcAddressMask(string address, int mask)
        {
            /* By full NICK!~IDENT@XXX.XXX.XXX.XXX numbers 0 to 10 (11 to 20)
               Split address up in to its three elements, nick, ident and IP/Host */
            var m = IrcAddress.Match(address);
            if (!m.Success)
            {
                return address;
            }
            var nick = m.Groups[1].Value;
            var ident = m.Groups[2].Value;
            var addr = m.Groups[3].Value;
            /* Resolve each mask */
            if (!string.IsNullOrEmpty(addr))
            {
                switch (mask % 10)
                {
                    case 0:
                        return string.Format("*!{0}@{1}", ident, addr);

                    case 1:
                        return string.Format("*!{0}@{1}", ident.Replace("~", "*"), addr);

                    case 2:
                        return string.Format("*!*@{0}", addr);

                    case 3:
                        return string.Format("*!{0}@{1}", ident.Replace("~", "*"), GetAddressWildCard(addr, 1));

                    case 4:
                        return string.Format("*!*@{0}", GetAddressWildCard(addr, 1));

                    case 5:
                        return string.Format("{0}!{1}@{2}", nick, ident, addr);

                    case 6:
                        return string.Format("{0}!{1}@{2}", nick, ident.Replace("~", "*"), addr);

                    case 7:
                        return string.Format("{0}!*@{1}", nick, addr);

                    case 8:
                        return string.Format("{0}!{1}@{2}", nick, ident.Replace("~", "*"), GetAddressWildCard(addr, 1));

                    case 9:
                        return string.Format("{0}!*@{1}", nick, GetAddressWildCard(addr, 1));
                }
            }
            return address;           
        }

        public static string CheckIrcAddress(string address)
        {
            /* Check IRC address - validate the input is of *!*@* */
            if (string.IsNullOrEmpty(address))
            {
                return "*!*@*";
            }
            /* Validate the passed address mask matches the IRC protocol */
            var m = IrcAddress.Match(address);
            if (m.Success)
            {
                /* Valid address format */
                return !address.Contains(".") || address.EndsWith(".") ? string.Format("{0}*", address) : address;
            }
            var index = address.IndexOf('!');
            switch (index)
            {
                case -1:
                    return string.Format("{0}!*@*", address);

                default:
                    if (index == address.Length - 1)
                    {
                        return string.Format("{0}*@*", address);
                    }
                    break;
            }
            index = address.IndexOf('@');
            if (index == -1)
            {
                return string.Format("{0}*@*", address);
            }
            /* Make sure @ or . isn't the last character */
            return index == address.Length - 1
                       ? string.Format("{0}*", address)
                       : address;
        }

        private static string GetAddressWildCard(string address, int level)
        {
            if (address.Contains("."))
            {
                var sp = address.Split('.').ToList();
                if (sp.Count > 1)
                {
                    sp.RemoveAt(sp.Count - (level == 0 ? 1 : level));
                }
                return string.Format("{0}.*", string.Join(".", sp));
            }
            return address;
        }
    }
}
