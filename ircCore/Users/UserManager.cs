/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */

using System;
using System.IO;
using System.Linq;
using ircCore.Utils;
using ircCore.Utils.Serialization;

namespace ircCore.Users
{
    public sealed class UserManager
    {
        private static UserList _userList = new UserList();

        private static readonly string FileName = Functions.MainDir(@"\data\users.xml", false);

        /* Load/save functions */
        public static void Load()
        {
            if (!XmlSerialize<UserList>.Load(FileName, ref _userList))
            {
                _userList = new UserList();
            }
        }

        public static void Save()
        {
            if (_userList.Notify.Users.Count == 0 && _userList.Ignore.Users.Count == 0)
            {               
                /* No point saving or keeping an empty list */
                if (File.Exists(FileName))
                {
                    File.Delete(FileName);
                }
                return;
            }
            XmlSerialize<UserList>.Save(FileName, _userList);
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
            _userList.Notify.Users.Add(u);
        }

        public static void RemoveNotify(string nick)
        {
            var u = IsNotify(nick);
            if (u == null)
            {
                return;
            }
            _userList.Notify.Users.Remove(u);
        }

        public static User IsNotify(string nick)
        {
            return _userList.Notify.Users.FirstOrDefault(u => u.Nick.Equals(nick, StringComparison.InvariantCultureIgnoreCase));
        }

        /* Ignore functions */
        public static void AddIgnore(string addressMask)
        {
            if (IsIgnored(addressMask))
            {
                return;
            }
            _userList.Ignore.Users.Add(new User {Address = addressMask});
        }

        public static void RemoveIgnore(string addressMask)
        {
            foreach (var u in _userList.Ignore.Users.Where(u => u.Address == addressMask))
            {
                _userList.Ignore.Users.Remove(u);
                break;
            }
        }

        public static bool IsIgnored(string address)
        {
            return _userList.Ignore.Users.Select(u => new WildcardMatch(u.Address)).Any(ignore => ignore.IsMatch(address));
        }
    }
}
