﻿/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FusionIRC.Forms.Settings.Controls.Base;
using FusionIRC.Forms.Settings.Controls.Client;
using FusionIRC.Forms.Settings.Controls.Connection;
using FusionIRC.Forms.Settings.Controls.Dcc;
using FusionIRC.Forms.Settings.Controls.Mouse;
using FusionIRC.Helpers;
using ircCore.Controls;
using ircCore.Settings;
using ircCore.Settings.SettingsBase.Structures;
using ircCore.Settings.SettingsBase.Structures.Client;
using ircCore.Settings.Theming;
using ircCore.Utils;

namespace FusionIRC.Forms.Settings
{
    public sealed class FrmSettings : FormEx
    {
        private readonly TreeView _tvMenu;
        private readonly Button _btnApply;
        private readonly Button _btnCancel;
        private readonly Button _btnOk;
               
        private readonly ConnectionIdentDaemon _connectionIdentDaemon;
        private readonly ConnectionLocalInfo _connectionLocalInfo;
        private readonly ConnectionOptions _connectionOptions;
        private readonly ConnectionServers _connectionServers;

        private readonly ClientOptions _clientOptions;
        private readonly ClientMessages _clientMessages;
        private readonly ClientCaching _clientCaching;
        private readonly ClientLogging _clientLogging;
        private readonly ClientSystemTray _clientSystemTray;
        private readonly ClientConfirm _clientConfirm;

        private readonly DccRequests _dccRequests;
        private readonly DccOptions _dccOptions;

        private readonly MouseDoubleClick _mouseDoubleClick;

        private readonly Timer _tmrSelect;

        private bool _initialize;

        private bool _loggingChanged;

        public FrmSettings()
        {
            _initialize = true;
            ClientSize = new Size(607, 407);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0)));
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"FusionIRC Settings";

            _tvMenu = new TreeView
                          {
                              HideSelection = false,
                              Location = new Point(12, 12),
                              Size = new Size(150, 344),
                              BorderStyle = BorderStyle.FixedSingle,
                              TabIndex = 0
                          };

            _btnApply = new Button
                            {
                                Enabled = false,
                                Location = new Point(358, 372),
                                Size = new Size(75, 23),
                                TabIndex = 1,
                                Text = @"Apply",
                                UseVisualStyleBackColor = true
                            };

            _btnOk = new Button
                         {
                             DialogResult = DialogResult.OK,
                             Location = new Point(439, 372),
                             Size = new Size(75, 23),
                             TabIndex = 2,
                             Text = @"OK",
                             UseVisualStyleBackColor = true
                         };

            _btnCancel = new Button
                             {
                                 DialogResult = DialogResult.Cancel,
                                 Location = new Point(520, 372),
                                 Size = new Size(75, 23),
                                 TabIndex = 3,
                                 Text = @"Cancel",
                                 UseVisualStyleBackColor = true
                             };
            /* Connection */
            _connectionServers = new ConnectionServers {Location = new Point(168, 12), Visible = false};
            _connectionOptions = new ConnectionOptions {Location = new Point(168, 12), Visible = false};
            _connectionIdentDaemon = new ConnectionIdentDaemon {Location = new Point(168, 12), Visible = false};
            _connectionLocalInfo = new ConnectionLocalInfo {Location = new Point(168, 12), Visible = false};
            /* Client */
            _clientOptions = new ClientOptions {Location = new Point(168, 12), Visible = false};
            _clientMessages = new ClientMessages { Location = new Point(168, 12), Visible = false };
            _clientCaching = new ClientCaching { Location = new Point(168, 12), Visible = false };
            _clientLogging = new ClientLogging {Location = new Point(168, 12), Visible = false};
            _clientSystemTray = new ClientSystemTray { Location = new Point(168, 12), Visible = false };
            _clientConfirm = new ClientConfirm { Location = new Point(168, 12), Visible = false };
            /* Mouse */
            _mouseDoubleClick = new MouseDoubleClick {Location = new Point(168, 12), Visible = false};
            /* DCC */
            _dccRequests = new DccRequests { Location = new Point(168, 12), Visible = false };
            _dccOptions = new DccOptions {Location = new Point(168, 12), Visible = false};

            Controls.AddRange(new Control[]
                                  {
                                      _tvMenu,
                                      _btnApply,
                                      _btnOk,
                                      _btnCancel,
                                      _connectionServers,
                                      _connectionOptions,
                                      _connectionIdentDaemon,
                                      _connectionLocalInfo,
                                      _clientOptions,
                                      _clientMessages,
                                      _clientCaching,
                                      _clientLogging,
                                      _clientSystemTray,
                                      _clientConfirm,
                                      _mouseDoubleClick,
                                      _dccRequests,
                                      _dccOptions
                                  });
            /* Connection */
            _connectionServers.OnSettingsChanged += OnSettingsChanged;
            _connectionOptions.OnSettingsChanged += OnSettingsChanged;
            _connectionIdentDaemon.OnSettingsChanged += OnSettingsChanged;
            _connectionLocalInfo.OnSettingsChanged += OnSettingsChanged;
            /* Client */
            _clientOptions.OnSettingsChanged += OnSettingsChanged;
            _clientMessages.OnSettingsChanged += OnSettingsChanged;
            _clientCaching.OnSettingsChanged += OnSettingsChanged;
            _clientLogging.OnSettingsChanged += OnSettingsChanged;
            _clientSystemTray.OnSettingsChanged += OnSettingsChanged;
            _clientConfirm.OnSettingsChanged += OnSettingsChanged;
            /* Mouse */
            _mouseDoubleClick.OnSettingsChanged += OnSettingsChanged;
            /* DCC */
            _dccRequests.OnSettingsChanged += OnSettingsChanged;
            _dccOptions.OnSettingsChanged += OnSettingsChanged;

            BuildTreeMenuNodes();

            AcceptButton = _btnOk;
            _btnApply.Click += ButtonClickHandler;

            _tmrSelect = new Timer
                             {
                                 Interval = 10,
                                 Enabled = true
                             };
            _tmrSelect.Tick += TimerSelect;
            _tvMenu.AfterSelect += MenuAfterSelect;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_btnApply.Enabled && DialogResult != DialogResult.Cancel)
            {
                /* Settings not saved yet */
                SaveSettings();
            }
            base.OnFormClosing(e);
        }

        /* Treeview handelrs */
        private void MenuAfterSelect(object sender, EventArgs e)
        {
            /* This is simpler than how I had it which was getting the selected node's name, switching it and
             * telling the control to become visible. After a lot of option panels, this method would have become
             * over 200 lines long! Far too long :) */
            if (_initialize)
            {
                return;
            }
            /* Hide all/any shown "panels" */
            foreach (var c in from object c in Controls where c is ISettings && ((Control) c).Visible select c)
            {
                ((Control) c).Visible = false;
            }
            SettingsManager.Settings.Client.Tabs.Settings = _tvMenu.SelectedNode.Name;
            var ctl = (BaseControlRenderer) _tvMenu.SelectedNode.Tag;
            if (ctl != null)
            {
                ctl.Visible = true;
            }
        }

        private void ButtonClickHandler(object sender, EventArgs e)
        {
            var btn = (Button) sender;
            if (btn == null)
            {
                return;
            }
            switch (btn.Text.ToUpper())
            {
                case "APPLY":                    
                    SaveSettings();
                    break;
            }
        }

        private void BuildTreeMenuNodes()
        {
            /* Build menu - this is actually simpler than how I had it. Connection options... */
            _tvMenu.Nodes.Add("CONNECTION", "Connection").Nodes.AddRange(new[]
                                                                            {
                                                                                new TreeNode("Servers")
                                                                                    {
                                                                                        Tag = _connectionServers,
                                                                                        Name = "CONNECTIONSERVERS"
                                                                                    },
                                                                                new TreeNode("Options")
                                                                                    {
                                                                                        Tag = _connectionOptions,
                                                                                        Name = "CONNECTIONOPTIONS"
                                                                                    },
                                                                                new TreeNode("Identd")
                                                                                    {
                                                                                        Tag = _connectionIdentDaemon,
                                                                                        Name = "CONNECTIONIDENTDAEMON"
                                                                                    },
                                                                                new TreeNode("Local Info")
                                                                                    {
                                                                                        Tag = _connectionLocalInfo,
                                                                                        Name = "CONNECTIONLOCALINFO"
                                                                                    }
                                                                            });
            _tvMenu.Nodes[0].Tag = _connectionServers;            
            /* Client options... */
            _tvMenu.Nodes.Add("CLIENT", "Client").Nodes.AddRange(new[]
                                                                     {
                                                                         new TreeNode("Options")
                                                                             {
                                                                                 Tag = _clientOptions,
                                                                                 Name = "CLIENTOPTIONS"
                                                                             },
                                                                         new TreeNode("Messages")
                                                                             {
                                                                                 Tag = _clientMessages,
                                                                                 Name = "CLIENTMESSAGES"
                                                                             },
                                                                         new TreeNode("Caching")
                                                                             {
                                                                                 Tag = _clientCaching,
                                                                                 Name = "CLIENTCACHING"
                                                                             },
                                                                         new TreeNode("Logging")
                                                                             {
                                                                                 Tag = _clientLogging,
                                                                                 Name = "CLIENTLOGGING"
                                                                             },
                                                                         new TreeNode("System Tray")
                                                                             {
                                                                                 Tag = _clientSystemTray,
                                                                                 Name = "CLIENTTRAY"
                                                                             },
                                                                         new TreeNode("Confirmation")
                                                                             {
                                                                                 Tag = _clientConfirm,
                                                                                 Name = "CLIENTCONFIRM"
                                                                             }
                                                                     });
            _tvMenu.Nodes[1].Tag = _clientOptions;
            /* Mouse options... */
            _tvMenu.Nodes.Add("MOUSE", "Mouse").Nodes.AddRange(new[]
                                                                   {
                                                                       new TreeNode("Double-Click")
                                                                           {
                                                                               Tag = _mouseDoubleClick,
                                                                               Name = "MOUSEDOUBLECLICK"
                                                                           }
                                                                   });
            _tvMenu.Nodes[2].Tag = _mouseDoubleClick;
            /* DCC... */
            _tvMenu.Nodes.Add("DCC", "DCC").Nodes.AddRange(new[]
                                                               {
                                                                   new TreeNode("Requests")
                                                                       {
                                                                           Tag = _dccRequests,
                                                                           Name = "DCCREQUESTS"
                                                                       },
                                                                   new TreeNode("Options")
                                                                       {
                                                                           Tag = _dccOptions,
                                                                           Name = "DCCOPTIONS"
                                                                       }
                                                               });
            _tvMenu.Nodes[3].Tag = _dccRequests;
            _tvMenu.ExpandAll();
        }

        private void OnSettingsChanged(ISettings control)
        {
            /* Really all this callback does :) */
            _btnApply.Enabled = true;
            if (control.GetType() == typeof(ClientLogging))
            {
                _loggingChanged = true;
            }
        }

        /* Save settings - also, any settings relevant to client appearance should also update main client
         * window/child controls */
        private void SaveSettings()
        {
            foreach (var panel in Controls.OfType<ISettings>().Where(panel => panel.SettingsChanged))
            {
                panel.SaveSettings();
            }
            _btnApply.Enabled = false;
            SettingsManager.Save();
            /* Update child windows */
            var type = SettingsManager.Settings.Client.Logging.KeepLogsType;
            var close = false;
            foreach (var w in WindowManager.Windows.SelectMany(c => c.Value))
            {                
                if (_loggingChanged)
                {
                    /* Update logging */
                    switch (w.WindowType)
                    {
                        case ChildWindowType.Channel:
                            if (type != LoggingType.Channels && type != LoggingType.Both)
                            {
                                close = true;
                            }
                            break;

                        case ChildWindowType.Private:
                        case ChildWindowType.DccChat:
                            if (type != LoggingType.Chats && type != LoggingType.Both)
                            {
                                close = true;
                            }
                            break;
                    }
                    if (close)
                    {
                        w.Logger.CloseLog();
                    }
                    else
                    {
                        if (type != LoggingType.None && w.WindowType != ChildWindowType.Console)
                        {
                            w.Logger.CloseLog(); /* Must call this first when changing any setting */
                            /* Forgot to add this!! */
                            w.Logger.FilePath =
                                w.Logger.FilePath =
                                string.Format("{0}.log",
                                              Functions.GetLogFileName(w.Client.Network, w.Tag.ToString(), true));
                            w.Logger.CreateLog();
                        }
                    }
                }
                /* Line spacing */
                w.Output.LinePaddingPixels = SettingsManager.Settings.Client.Messages.LinePadding;
                w.Output.LineSpacingStyle = SettingsManager.Settings.Client.Messages.LineSpacing;
                w.Output.AdjustWidth(false);
                /* Input box */
                w.Input.ConfirmPaste = SettingsManager.Settings.Client.Confirmation.ConfirmPaste;
                w.Input.ConfirmPasteLines = SettingsManager.Settings.Client.Confirmation.PasteLines;
            }
            /* Update tray icon */
            var owner = (TrayIcon) WindowManager.MainForm;
            if (owner != null)
            {
                owner.TrayHideOnMinimize = SettingsManager.Settings.Client.TrayIcon.HideMinimized;
                var ico = Functions.MainDir(SettingsManager.Settings.Client.TrayIcon.Icon);
                owner.TrayNotifyIcon.Icon = !string.IsNullOrEmpty(ico) && File.Exists(ico)
                                                ? Icon.ExtractAssociatedIcon(ico)
                                                : owner.Icon;
                owner.TrayAlwaysShowIcon = SettingsManager.Settings.Client.TrayIcon.AlwaysShow;
                owner.TrayNotifyIcon.Visible = owner.TrayAlwaysShowIcon;
                owner.TrayShowNotifications = SettingsManager.Settings.Client.TrayIcon.ShowNotifications;
                /* Control bars */
                ((FrmClientWindow) owner).MenuBar.Visible =
                    SettingsManager.Settings.Client.Appearance.ControlBars.Control[0].Visible;
                ((FrmClientWindow) owner).ToolBar.Visible =
                    SettingsManager.Settings.Client.Appearance.ControlBars.Control[1].Visible;
            }
        }

        /* Select last known node */
        private void TimerSelect(object sender, EventArgs e)
        {
            _tmrSelect.Enabled = false;
            var tab = SettingsManager.Settings.Client.Tabs.Settings;
            if (string.IsNullOrEmpty(tab))
            {
                tab = "CONNECTIONSERVERS";
            }
            switch (tab)
            {
                case "CONNECTION":
                    tab = "CONNECTIONSERVERS";
                    break;

                case "CLIENT":
                    tab = "CLIENTOPTIONS";
                    break;

                case "MOUSE":
                    tab = "MOUSEDOUBLECLICK";
                    break;

                case "DCC":
                    tab = "DCCREQUESTS";
                    break;
            }
            var node = _tvMenu.Nodes.Find(tab, true);
            if (node.Length == 0)
            {
                /* It should exist, unless someone modifies the settings XML file ;) */
                _initialize = false;
                return;
            }
            _initialize = false;
            _tvMenu.SelectedNode = node[0];
        }
    }
}