/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ircCore.Users
{
    [Serializable]
    public class User
    {
        [XmlAttribute("nick")]
        public string Nick { get; set; }

        [XmlAttribute("address")]
        public string Address { get; set; }

        [XmlAttribute("note")]
        public string Note { get; set; }
    }

    [Serializable, XmlRoot("userList")]
    public class UserList
    {
        [Serializable]
        public class UserData
        {
            [XmlElement("user")]
            public List<User> Users = new List<User>();
        }

        [XmlElement("notify")]
        public UserData Notify = new UserData();

        [XmlElement("ignore")]
        public UserData Ignore = new UserData();
    }
}
