/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ircCore.Utils;
using ircCore.Utils.Serialization;

namespace ircCore.Users
{
    public sealed class UserManager
    {
        private static UserList _users = new UserList();

        private static readonly string FileName = Functions.MainDir(@"\data\users.xml");

        /* Event raised by this class for notify nicks */
        public static event Action<string> NotifyChanged;
        
        /* Load/save functions */
        public static void Load()
        {
            if (!XmlSerialize<UserList>.Load(FileName, ref _users))
            {
                _users = new UserList();
            }
        }

        public static void Save()
        {
            if (_users.Notify.Users.Count == 0 && _users.Ignore.Users.Count == 0)
            {               
                /* No point saving or keeping an empty list */
                if (File.Exists(FileName))
                {
                    File.Delete(FileName);
                }
                return;
            }
            XmlSerialize<UserList>.Save(FileName, _users);
        }

        /* Notify functions */
        public static void AddNotify(string nick)
        {
            AddNotify(nick, null);           
        }

        public static bool AddNotify(string nick, string note)
        {
            var u = IsNotify(nick);
            if (u != null)
            {
                /* Just update notes field */
                u.Note = note;
                return false;
            }
            /* Create new entry */
            u = new User {Nick = nick, Note = note};
            AddNotify(u);
            return true;
        }

        public static void AddNotify(User data)
        {
            _users.Notify.Users.Add(data);
            if (NotifyChanged != null)
            {
                NotifyChanged(string.Format("+{0}", data.Nick));
            }
        }

        public static bool RemoveNotify(string nick)
        {
            var u = IsNotify(nick);
            if (u == null)
            {
                return false;
            }
            RemoveNotify(u);
            return true;
        }

        public static void RemoveNotify(User data)
        {
            _users.Notify.Users.Remove(data);
            if (NotifyChanged != null)
            {
                NotifyChanged(string.Format("-{0}", data.Nick));
            }
        }

        public static void EditNotify(string oldNick, string newNick)
        {
            /* Used from GUI to raise event to send to server */
            if (oldNick.Equals(newNick, StringComparison.InvariantCultureIgnoreCase))
            {
                /* Nick part for this user was never changed, we don't have to bother the server */
                return;
            }
            if (NotifyChanged != null)
            {
                NotifyChanged(string.Format("-{0} +{1}", oldNick, newNick));
            }
        }

        public static void ClearNotify()
        {
            var u = string.Format("-{0}",
                                  string.Join(" ", _users.Notify.Users.Select(o => o.Nick).ToArray()).ReplaceEx(" ",
                                                                                                                " -"));
            _users.Notify.Users.Clear();
            if (NotifyChanged != null)
            {
                NotifyChanged(u);
            }
        }

        public static List<User> GetNotifyList()
        {
            return _users.Notify.Users;
        }

        public static User IsNotify(string nick)
        {
            return _users.Notify.Users.FirstOrDefault(u => u.Nick.Equals(nick, StringComparison.InvariantCultureIgnoreCase));
        }

        /* Ignore functions */
        public static bool AddIgnore(string addressMask)
        {
            if (IsIgnored(addressMask))
            {
                return false;
            }
            var u = new User {Address = addressMask};
            AddIgnore(u);
            return true;
        }

        public static void AddIgnore(User data)
        {
            _users.Ignore.Users.Add(data);
        }

        public static bool RemoveIgnore(string addressMask)
        {
            foreach (var u in _users.Ignore.Users.Where(u => u.Address == addressMask))
            {
                RemoveIgnore(u);
                return true;
            }
            return false;
        }

        public static void RemoveIgnore(User data)
        {
            _users.Ignore.Users.Remove(data);
        }

        public static void ClearIgnore()
        {
            _users.Ignore.Users.Clear();
        }

        public static bool IsIgnored(string address)
        {
            return
                _users.Ignore.Users.Select(u => new WildcardMatch(u.Address, RegexOptions.IgnoreCase)).Any(
                    ignore => ignore.IsMatch(address));
        }

        public static List<User> GetIgnoreList()
        {
            return _users.Ignore.Users;
        }       
    }
}
