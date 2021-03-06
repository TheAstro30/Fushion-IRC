﻿/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Linq;
using ircCore.Utils;
using ircCore.Utils.Serialization;

namespace ircCore.Autos
{
    public static class AutomationsManager
    {
        /* Mangaement class for automations such as auto join, identify, etc. */
        public enum AutomationType
        {
            Connect = 0,
            Join = 1,
            Identify = 2,
            Perform = 3
        }

        public static Automations Automations = new Automations();

        public static void Load()
        {
            if (!XmlSerialize<Automations>.Load(Functions.MainDir(@"\data\autos.xml"), ref Automations))
            {
                Automations = new Automations();
            }
        }

        public static void Save()
        {            
            XmlSerialize<Automations>.Save(Functions.MainDir(@"\data\autos.xml"), Automations);
        }

        public static object[] GetAllNetworks(AutomationType type)
        {
            switch (type)
            {
                case AutomationType.Connect:
                    return Automations.Connect.Network.ToArray();

                case AutomationType.Join:
                    return Automations.Join.Network.ToArray();
                    
                case AutomationType.Identify:
                    return Automations.Identify.Network.ToArray();

                case AutomationType.Perform:
                    return Automations.Perform.Network.ToArray();
            }
            return null;
        }

        public static AutoList.AutoNetworkData GetAutomationByNetwork(AutomationType type, string network)
        {
            switch (type)
            {
                case AutomationType.Connect:
                    return Automations.Connect.Network.Count > 0 ? Automations.Connect.Network[0] : null;

                case AutomationType.Join:
                    return
                        Automations.Join.Network.FirstOrDefault(
                            n => n.Name.Equals(network, StringComparison.InvariantCultureIgnoreCase));

                case AutomationType.Identify:
                    return
                        Automations.Identify.Network.FirstOrDefault(
                            n => n.Name.Equals(network, StringComparison.InvariantCultureIgnoreCase));

                case AutomationType.Perform:
                    return
                        Automations.Perform.Network.FirstOrDefault(
                            n => n.Name.Equals(network, StringComparison.InvariantCultureIgnoreCase));
            }
            return null;
        }

        public static AutoList.AutoData GetAutomationValueByItem(AutoList.AutoNetworkData network, string item)
        {
            return network.Data.FirstOrDefault(d => d.Item.Equals(item, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
