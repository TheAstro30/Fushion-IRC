/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ircCore.Utils;
using ircCore.Utils.Serialization;

namespace ircCore.Users
{
    public sealed class UserManager
    {
        public static UserList UserList = new UserList();

        private static readonly string FileName = Functions.MainDir(@"\data\users.xml", false);

        /* Load/save functions */
        public static void Load()
        {
            if (!XmlSerialize<UserList>.Load(FileName, ref UserList))
            {
                UserList = new UserList();
            }
        }

        public static void Save()
        {
            if (UserList.Notify.Users.Count == 0 && UserList.Ignore.Users.Count == 0)
            {               
                /* No point saving or keeping an empty list */
                if (File.Exists(FileName))
                {
                    File.Delete(FileName);
                }
                return;
            }
            XmlSerialize<UserList>.Save(FileName, UserList);
        }

        /* Notify functions */
        public static void AddNotify(string nick)
        {
            AddNotify(nick, null);           
        }

        public static void AddNotify(string nick, string note)
        {
            var u = IsNotify(nick);
            if (u != null)
            {
                /* Just update notes field */
                u.Note = note;
                return;
            }
            /* Create new entry */
            u = new User {Nick = nick, Note = note};
            UserList.Notify.Users.Add(u);
        }

        public static void RemoveNotify(string nick)
        {
            var u = IsNotify(nick);
            if (u == null)
            {
                return;
            }
            UserList.Notify.Users.Remove(u);
        }

        public static User IsNotify(string nick)
        {
            return UserList.Notify.Users.FirstOrDefault(u => u.Nick.Equals(nick, StringComparison.InvariantCultureIgnoreCase));
        }

        /* Ignore functions */
        public static void AddIgnore(string addressMask)
        {
            if (IsIgnored(addressMask))
            {
                return;
            }
            UserList.Ignore.Users.Add(new User {Address = addressMask});
        }

        public static void RemoveIgnore(string addressMask)
        {
            foreach (var u in UserList.Ignore.Users.Where(u => u.Address == addressMask))
            {
                UserList.Ignore.Users.Remove(u);
                break;
            }
        }

        public static bool IsIgnored(string address)
        {
            return
                UserList.Ignore.Users.Select(u => new WildcardMatch(u.Address, RegexOptions.IgnoreCase)).Any(
                    ignore => ignore.IsMatch(address));
        }
    }
}
