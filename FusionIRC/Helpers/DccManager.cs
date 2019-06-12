/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using FusionIRC.Forms.DirectClientConnection;
using ircClient;
using ircCore.Dcc;
using ircCore.Settings;
using ircCore.Settings.SettingsBase.Structures;
using ircCore.Settings.Theming;
using ircCore.Utils;
using ircCore.Utils.Serialization;

namespace FusionIRC.Helpers
{
    internal static class DccManager
    {
        /* This class is mainly used to store DCC file transfers and NOT really intended for
         * creating DCC get/send/chats */
        public static DccTransferData DccTransfers = new DccTransferData();

        /* Server callbacks */
        public static void OnDccChat(ClientConnection client, string nick, string address, string ip, string port)
        {
            using (var d = new FrmDccConfirm(DccType.DccChat)
                            {
                                NickName = string.Format("{0} ({1})", nick, address),
                            })
            {
                if (d.ShowDialog(WindowManager.MainForm) == DialogResult.Cancel)
                {
                    return;
                }
            }
            /* Port */
            int p;
            if (!int.TryParse(port, out p))
            {
                return;
            }
            var add = IpConvert(ip, true);
            /* Create a new window */
            var w = WindowManager.AddWindow(client, ChildWindowType.DccChat, WindowManager.MainForm,
                                            string.Format("={0} ({1})", nick, address), string.Format("={0}", nick),
                                            true);
            if (w == null)
            {
                return;
            }
            /* Which it shouldn't... */
            w.Dcc.Port = p;
            w.Dcc.Address = add;
            w.Dcc.DccChatType = DccChatType.Receive;
            w.Dcc.BeginConnect();
            AddDccHistory(nick);
        }

        public static void OnDccSend(ClientConnection client, string nick, string address, string file, string ip, string port, string length)
        {
            System.Diagnostics.Debug.Print("DCC SEND " + nick + " " + address + " " + file + " " + ip + " " + port + " " +
                                           length);
            /* Check current DCC filter settings to see if this file is acceptable to download */
            var f = CheckFilter(file);
            var ignore = false;
            System.Diagnostics.Debug.Print(f + " hmm");
            switch (SettingsManager.Settings.Dcc.Options.Filter.FilterMethod)
            {
                case DccFilterMethod.Disabled:
                    return;

                case DccFilterMethod.AcceptOnly:
                    ignore = !f;
                    if (f)
                    {
                        /* This file is allowed - show request dialog */
                        return;
                    }
                    break;

                case DccFilterMethod.IgnoreOnly:
                    ignore = f;
                    break;
            }
            /* This file is NOT allowed, show reject dialog */
            if (!ignore || !SettingsManager.Settings.Dcc.Options.Filter.ShowRejectionDialog)
            {
                return;
            }
            using (var d = new FrmDccReject
                               {
                                   NickName = string.Format("{0} ({1})", nick, address),
                                   FileName = file,
                                   FileSize = Functions.FormatBytes(length)
                               })
            {
                d.ShowDialog(WindowManager.MainForm);
            }
        }

        /* Load/save functions */
        public static void Load()
        {
            /* Load settings from disk */
            if (!XmlSerialize<DccTransferData>.Load(Functions.MainDir(@"\data\transfers.xml"), ref DccTransfers))
            {
                DccTransfers = new DccTransferData();
            }
        }

        public static void Save()
        {
            /* Save settings to disk */
            XmlSerialize<DccTransferData>.Save(Functions.MainDir(@"\data\transfers.xml"), DccTransfers);
        }

        public static void AddFile(DccFile file)
        {
            /* Check the total number of transfers, remove first entry if greater than maximum */
            if (DccTransfers.FileData.Count > 100)
            {
                DccTransfers.FileData.RemoveAt(0);
            }
            DccTransfers.FileData.Add(file);
            /* Sort list by uploads/downloads (type - downloads first, then by filename) */
            DccTransfers.FileData.Sort((x, y) =>
                                           {
                                               var result = x.FileType.CompareTo(y.FileType);
                                               return result != 0 ? result : x.FileName.CompareTo(y.FileName);
                                           });
        }

        public static void ClearFiles()
        {
            DccTransfers.FileData.Clear();
        }

        public static void AddDccHistory(string nick)
        {
            var list = SettingsManager.Settings.Dcc.History.Data;
            foreach (var n in list.Where(n => n.Nick.Equals(nick, StringComparison.InvariantCultureIgnoreCase)))
            {
                /* Insert at top of list */                    
                list.Insert(0, new SettingsDcc.SettingsDccHistory.SettingsDccHistoryData { Nick = n.Nick });
                list.Remove(n);
                return;
            }
            /* Insert at bottom of list */
            list.Add(new SettingsDcc.SettingsDccHistory.SettingsDccHistoryData {Nick = nick});
        }

        public static int GetFreeDccPort()
        {
            return 0;
        }

        public static string IpConvert(string address, bool reverseToIp)
        {
            if (!reverseToIp)
            {
                /* Convert to "long" */
                if (string.IsNullOrEmpty(address))
                {
                    return "0";
                }
                double num = 0;
                var arrDec = address.Split('.');
                var i = arrDec.Length - 1;
                while (i >= 0)
                {
                    num += ((Int32.Parse(arrDec[i]) % 256) * Math.Pow(256, (3 - i)));
                    i--;
                }
                return num.ToString();
            }
            /* Convert back to decimal/string */
            return IPAddress.Parse(address).ToString();
        }

        /* Private helper methods */
        private static bool CheckFilter(string file)
        {
            return
                SettingsManager.Settings.Dcc.Options.Filter.Extension.Select(
                    s => new WildcardMatch(s.Name, RegexOptions.IgnoreCase)).Any(w => w.IsMatch(file));
        }
    }
}
