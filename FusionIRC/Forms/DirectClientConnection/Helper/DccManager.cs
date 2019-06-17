/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.IO;
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
using ircCore.Users;
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
            /* Check the nick isn't ignored first */
            if (UserManager.IsIgnored(string.Format("{0}!{1}", nick, address)))
            {
                return;
            }
            /* Auto-accept is the default action and is not checked here */
            switch (SettingsManager.Settings.Dcc.Requests.ChatRequest)
            {
                case DccRequestAction.Ask:
                    using (var d = new FrmDccConfirm(DccType.DccChat) {NickName = String.Format("{0} ({1})", nick, address)})
                    {
                        if (d.ShowDialog(WindowManager.MainForm) == DialogResult.Cancel)
                        {
                            return;
                        }
                    }
                    break;

                case DccRequestAction.Ignore:
                    return;
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
            /* Setup structure */
            AddPort(p);
            w.Dcc.Port = p;
            w.Dcc.Address = add;
            w.Dcc.DccChatType = DccChatType.Receive;
            w.Dcc.BeginConnect();
            AddDccHistory(nick);
        }

        public static void OnDccSend(ClientConnection client, string nick, string address, string file, string ip, string port, string length)
        {
            /* Check the nick isn't ignored first */
            if (UserManager.IsIgnored(string.Format("{0}!{1}", nick, address)))
            {
                return;
            }
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
                        /* This file is allowed */
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
                        var resume = false;
                        var path = Functions.MainDir(@"\downloads");
                        var fullPath = string.Format(@"{0}\{1}", path, file);
                        var fs = new FileInfo(fullPath);
                        /* Auto-accept is the default action and is not checked here */
                        switch (SettingsManager.Settings.Dcc.Requests.GetRequest)
                        {
                            case DccRequestAction.Ask:
                                using (var req = new FrmDccConfirm(DccType.DccFileTransfer) { FileName = file, NickName = string.Format("{0} ({1})", nick, address) })
                                {
                                    if (req.ShowDialog(WindowManager.MainForm) == DialogResult.Cancel)
                                    {
                                        return;
                                    }
                                    /* Check if the file already exists */
                                    if (File.Exists(fullPath))
                                    {
                                        /* Overwrite option is the default action and not checked here */
                                        switch (SettingsManager.Settings.Dcc.Requests.GetFileExists)
                                        {
                                            case DccFileExistsAction.Ask:
                                                var type = fs.Length < i ? DccWriteMode.Resume : DccWriteMode.SaveAs;
                                                using (var de = new FrmDccExists(type, fullPath))
                                                {
                                                    if (de.ShowDialog(DccManagerWindow) == DialogResult.Cancel)
                                                    {
                                                        return;
                                                    }
                                                    file = Path.GetFileName(de.FileName);
                                                    path = Path.GetDirectoryName(de.FileName);
                                                    resume = de.WriteMode == DccWriteMode.Resume;
                                                }
                                                break;

                                            case DccFileExistsAction.Cancel:
                                                return;
                                        }
                                    }
                                }
                                break;

                            case DccRequestAction.Ignore:
                                return;
                        }                        
                        var dcc = GetTransfer(nick, file);
                        if (dcc == null)
                        {
                            dcc = new Dcc(DccManagerWindow)
                                      {
                                          Client = client,
                                          DccType = DccType.DccFileTransfer,
                                          DccFileType = DccFileType.Download,
                                          UserName = nick,
                                          Address = IpConvert(ip, true),
                                          Port = p,
                                          DccFolder = path,
                                          FileName = file,
                                          FileSize = i
                                      };
                            dcc.OnDccTransferProgress += OnDccTransferProgress;
                            AddDccHistory(nick);
                            AddPort(p);
                            AddTransfer(dcc);
                        }
                        dcc.IsResume = resume;
                        if (resume)
                        {
                            /* We request the sender to resume */
                            client.Send(string.Format("PRIVMSG {0} :\u0001DCC RESUME \"{1}\" {2} {3}\u0001",
                                                      nick, file, port, fs.Length));
                        }
                        /* Make sure the current port hasn't changed, sometimes mIRC changes the port when resending */
                        if (dcc.Port != p)
                        {
                            RemovePort(dcc.Port);
                            dcc.Port = p;
                            AddPort(p);
                        }
                        dcc.BeginConnect();
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

        public static void OnDccAcceptResume(ClientConnection client, bool get, string nick, string file, string port, string position)
        {
            /* Get resume */
            uint i;
            int p;
            var dcc = GetTransfer(nick, get ? file : file.Replace("_", " "));
            if (dcc == null || !uint.TryParse(position, out i) || !int.TryParse(port, out p))
            {
                return;
            }
            dcc.FilePos = i;
            if (get)
            {                
                dcc.BeginGetResume();
            }
            else
            {
                /* Send DCC ACCEPT handshake */
                dcc.BeginSendResume();
                client.Send(string.Format("PRIVMSG {0} :\x0001DCC ACCEPT \"{1}\" {2} {3}\x0001", nick, file, port, position));
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

        public static Dcc GetTransfer(string nick, string fileName)
        {
            /* Get current DCC object by both nick and filename */
            return
                DccTransfers.FirstOrDefault(
                    d =>
                    d.UserName.Equals(nick, StringComparison.InvariantCultureIgnoreCase) &&
                    d.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase));
        }

        public static void RemoveTransfer(Dcc file)
        {
            file.OnDccTransferProgress -= OnDccTransferProgress;
            DccTransfers.Remove(file);
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
                return num > 0 ? num.ToString() : string.Empty;
            }
            /* Convert back to decimal/string */
            return IPAddress.Parse(address).ToString();
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
            list.Add(new SettingsDcc.SettingsDccHistory.SettingsDccHistoryData { Nick = nick });
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
