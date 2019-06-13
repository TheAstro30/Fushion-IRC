/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.IO;
using System.Windows.Forms;
using FusionIRC.Forms.DirectClientConnection.Helper;
using ircClient;
using ircClient.Tcp;
using ircCore.Settings;
using ircCore.Settings.Theming;

namespace FusionIRC.Helpers.Commands
{
    internal static class CommandDcc
    {
        public static void Parse(ClientConnection client, string args)
        {
            if (!client.IsConnected)
            {
                return;
            }
            var sp = args.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            if (sp.Length < 2)
            {
                return;
            }
            /* Get long address */
            var add = SettingsManager.Settings.Connection.LocalInfo.HostInfo.Address;
            var ip = DccManager.IpConvert(add, false);
            if (string.IsNullOrEmpty(ip))
            {
                /* Failed to get IP address */
                return;
            }
            var port = DccManager.GetFreeDccPort();
            switch (sp[0].ToUpper())
            {
                case "SEND":
                    /* Send new file */
                    using (var ofd = new OpenFileDialog { Multiselect = false, Title = string.Format("Send {0} a file", sp[1]) })
                    {
                        if (ofd.ShowDialog(WindowManager.MainForm) == DialogResult.Cancel)
                        {
                            return;
                        }
                        /* Create a new DCC file object */
                        var fs = new FileInfo(ofd.FileName);
                        var dcc = new Dcc(DccManager.DccManagerWindow)
                                      {
                                          UserName = sp[1],
                                          FileName = Path.GetFileName(ofd.FileName),
                                          DccFolder = Path.GetDirectoryName(ofd.FileName),
                                          DccType = DccType.DccFileTransfer,
                                          DccFileType = DccFileType.Upload,
                                          FileSize = (uint)fs.Length,
                                          Port = port
                                      };
                        dcc.OnDccTransferProgress += DccManager.OnDccTransferProgress;
                        DccManager.AddPort(port);                        
                        DccManager.AddTransfer(dcc);
                        dcc.BeginConnect();
                        /* Send notification to user */
                        var fl = dcc.FileName.Replace(" ", "_");
                        client.Send(
                            string.Format(
                                "NOTICE {0} :DCC Send {1} ({2}){3}PRIVMSG {0} :\u0001DCC SEND {1} {4} {5} {6}\u0001",
                                sp[1], fl, add, Environment.NewLine, ip, port, fs.Length));
                        DccManager.AddDccHistory(sp[1]);
                    }
                    break;

                case "CHAT":
                    /* Create a new DCC chat based on arguments */
                    var n = string.Format("={0}", sp[1]);
                    var w = WindowManager.GetWindow(client, n) ??
                            WindowManager.AddWindow(client, ChildWindowType.DccChat, WindowManager.MainForm,
                                                    string.Format("Chat {0}", sp[1]), n, true);
                    DccManager.AddPort(port);
                    w.Dcc.Port = port;
                    w.Dcc.UserName = sp[1];
                    w.Dcc.DccChatType = DccChatType.Send;
                    w.Dcc.BeginConnect();
                    /* Send notification to user */
                    client.Send(
                        string.Format("NOTICE {0} :DCC Chat ({1}){2}PRIVMSG {0} :\u0001DCC CHAT chat {3} {4}\u0001",
                                      sp[1], add, Environment.NewLine, ip, port));                    
                    DccManager.AddDccHistory(sp[1]);
                    break;
            }
        }
    }
}
