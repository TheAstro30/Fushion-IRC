/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using FusionIRC.Helpers;
using ircClient;
using ircClient.Tcp;
using ircCore.Settings;
using ircCore.Settings.SettingsBase.Structures;
using ircCore.Settings.Theming;
using ircCore.Utils;

namespace FusionIRC.Forms.DirectClientConnection.Helper
{
    internal static class DccManager
    {
        private static readonly List<int> Ports = new List<int>();

        private static readonly List<Dcc> DccTransfers = new List<Dcc>();

        /* DCC file transfer manager window */
        public static FrmDccManager DccManagerWindow;

        static DccManager()
        {
            /* Create DCC file manager window */
            DccManagerWindow = new FrmDccManager(DccTransfers);
        }

        /* Server callbacks */
        public static void OnDccChat(ClientConnection client, string nick, string address, string ip, string port)
        {
            using (var d = new FrmDccConfirm(DccType.DccChat)
                            {
                                NickName = String.Format("{0} ({1})", nick, address),
                            })
            {
                if (d.ShowDialog(WindowManager.MainForm) == DialogResult.Cancel)
                {
                    return;
                }
            }
            /* Port */
            int p;
            if (!Int32.TryParse(port, out p))
            {
                return;
            }
            var add = IpConvert(ip, true);
            /* Create a new window */
            var n = string.Format("={0}", nick);
            var w = WindowManager.GetWindow(client, n) ??
                    WindowManager.AddWindow(client, ChildWindowType.DccChat, WindowManager.MainForm,
                                            string.Format("Chat {0}", nick), n, true);
            /* Which it shouldn't... */
            AddPort(p);
            w.Dcc.Port = p;
            w.Dcc.Address = add;
            w.Dcc.DccChatType = DccChatType.Receive;
            w.Dcc.BeginConnect();
            AddDccHistory(nick);
        }

        public static void OnDccSend(ClientConnection client, string nick, string address, string file, string ip, string port, string length)
        {
            Debug.Print("DCC SEND " + nick + " " + address + " " + file + " " + ip + " " + port + " " +
                                           length);
            /* Check current DCC filter settings to see if this file is acceptable to download */
            var f = CheckFilter(file);
            var ignore = false;
            switch (SettingsManager.Settings.Dcc.Options.Filter.FilterMethod)
            {
                case DccFilterMethod.Disabled:
                    return;

                case DccFilterMethod.AcceptOnly:
                    ignore = !f;
                    if (f)
                    {
                        /* This file is allowed - show request dialog */
                        using (var req = new FrmDccConfirm(DccType.DccFileTransfer) { FileName = file, NickName = string.Format("{0} ({1})", nick, address) })
                        {
                            if (req.ShowDialog(WindowManager.MainForm) == DialogResult.OK)
                            {
                                /* Create a new DCC get, add it to list of files */
                                uint i;
                                if (!uint.TryParse(length, out i))
                                {
                                    return;
                                }
                                int p;
                                if (!int.TryParse(port, out p))
                                {
                                    return;
                                }
                                var dcc = new Dcc(DccManagerWindow)
                                              {
                                                  DccType = DccType.DccFileTransfer,
                                                  DccFileType = DccFileType.Download,
                                                  UserName = nick,
                                                  Address = IpConvert(ip, true),
                                                  Port = p,
                                                  DccFolder = Functions.MainDir(@"\downloads"),
                                                  FileName = file,
                                                  FileSize = i
                                              };
                                dcc.OnDccTransferProgress += OnDccTransferProgress;
                                AddDccHistory(nick);
                                AddPort(p);
                                AddTransfer(dcc);
                                dcc.BeginConnect();
                            }
                        }
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
                                   NickName = String.Format("{0} ({1})", nick, address),
                                   FileName = file,
                                   FileSize = Functions.FormatBytes(length)
                               })
            {
                d.ShowDialog(WindowManager.MainForm);
            }
        }

        public static void OnDccTransferProgress(Dcc dcc)
        {
            DccManagerWindow.UpdateTransferData();
        }

        /* Public methods */
        public static void AddTransfer(Dcc file)
        {
            /* Check the total number of transfers, remove first entry if greater than maximum */
            DccTransfers.Add(file);
            /* Sort list by uploads/downloads (type - downloads first, then by filename) */
            DccTransfers.Sort((x, y) =>
                                  {
                                      var result = x.DccFileType.CompareTo(y.DccFileType);
                                      return result != 0 ? result : x.FileName.CompareTo(y.FileName);
                                  });
            DccManagerWindow.AddTransfer(file);
        }

        public static void RemoveTransfer(Dcc file)
        {
            file.OnDccTransferProgress -= OnDccTransferProgress;
            DccTransfers.Remove(file);
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

        public static void AddPort(int port)
        {
            if (Ports.Contains(port))
            {
                return;
            }
            Ports.Add(port);
            Ports.Sort();
        }

        public static void RemovePort(int port)
        {
            Ports.Remove(port);
        }

        public static int GetFreeDccPort()
        {
            int p;
            if (!SettingsManager.Settings.Dcc.Options.General.Randomize)
            {
                p = Ports.Count > 0 ? Ports.Max() + 1 : SettingsManager.Settings.Dcc.Options.General.MinimumPort;
                if (p > SettingsManager.Settings.Dcc.Options.General.MaximumPort)
                {
                    p = SettingsManager.Settings.Dcc.Options.General.MinimumPort;
                }
            }
            else
            {
                var r = new Random();                
                p = r.Next(SettingsManager.Settings.Dcc.Options.General.MinimumPort,
                           SettingsManager.Settings.Dcc.Options.General.MaximumPort);
            }
            /* Need to ensure port doesn't already exist */
            foreach (var port in Ports)
            {
                if (port == p)
                {
                    p++;
                    continue;
                }
                break;
            }
            AddPort(p);
            return p;
        }

        public static string IpConvert(string address, bool reverseToIp)
        {
            if (!reverseToIp)
            {
                /* Convert to "long" */
                if (String.IsNullOrEmpty(address))
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
                return num > 0 ? num.ToString() : string.Empty;
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
