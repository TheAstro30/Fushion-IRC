/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ircCore.Settings.Networks;
using ircCore.Users;
using ircCore.Utils;
using ircCore.Utils.Serialization;

namespace ircCore.Autos
{
    public static class AutomationsManager
    {
        /* Mangaement class for automations such as auto join, identify, etc. */
        public static Automations Automations = new Automations();

        public static void Load()
        {
            if (!XmlSerialize<Automations>.Load(Functions.MainDir(@"\data\autos.xml", false), ref Automations))
            {
                Automations = new Automations();
            }
        }

        public static void Save()
        {            
            XmlSerialize<Automations>.Save(Functions.MainDir(@"\data\autos.xml", false), Automations);
        }
    }
}
