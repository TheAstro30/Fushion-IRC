/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FusionIRC.Forms.Child;
using FusionIRC.Forms.Settings.Controls.Base;
using FusionIRC.Forms.Settings.Controls.Connection;
using FusionIRC.Helpers;
using ircCore.Controls;
using ircCore.Settings;
using ircCore.Settings.SettingsBase.Structures;
using ircCore.Settings.Theming;

namespace FusionIRC.Forms.Settings
{
    public sealed class FrmSettings : FormEx
    {
        private readonly TreeView _tvMenu;
        private readonly Button _btnApply;
        private readonly Button _btnCancel;
        private readonly Button _btnOk;
       
        private readonly ClientLogging _clientLogging;
        private readonly ConnectionIdentDaemon _connectionIdentDaemon;
        private readonly ConnectionLocalInfo _connectionLocalInfo;
        private readonly ConnectionOptions _connectionOptions;
        private readonly ConnectionServers _connectionServers;

        private readonly Timer _tmrSelect;

        private bool _initialize;

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

            _connectionServers = new ConnectionServers {Location = new Point(168, 12), Visible = false};
            _connectionOptions = new ConnectionOptions {Location = new Point(168, 12), Visible = false};
            _connectionIdentDaemon = new ConnectionIdentDaemon {Location = new Point(168, 12), Visible = false};
            _connectionLocalInfo = new ConnectionLocalInfo {Location = new Point(168, 12), Visible = false};

            _clientLogging = new ClientLogging {Location = new Point(168, 12), Visible = false};

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
                                      _clientLogging
                                  });

            _connectionServers.OnSettingsChanged += OnSettingsChanged;
            _connectionOptions.OnSettingsChanged += OnSettingsChanged;
            _connectionIdentDaemon.OnSettingsChanged += OnSettingsChanged;
            _connectionLocalInfo.OnSettingsChanged += OnSettingsChanged;

            _clientLogging.OnSettingsChanged += OnSettingsChanged;

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
            if (_btnApply.Enabled)
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
            /* Client options... */
            _tvMenu.Nodes.Add("CLIENT", "Client").Nodes.AddRange(new[]
                                                                    {
                                                                        new TreeNode("Logging")
                                                                            {
                                                                                Tag = _clientLogging,
                                                                                Name = "CLIENTLOGGING"
                                                                            }
                                                                    });
        }

        private void OnSettingsChanged()
        {
            /* Really all this callback does :) */
            _btnApply.Enabled = true;
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
            /* Update logging */
            var type = SettingsManager.Settings.Client.Logging.KeepLogsType;
            var close = false;
            foreach (var w in WindowManager.Windows.SelectMany(c => c.Value))
            {
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
                    if (type != LoggingType.None)
                    {
                        w.Logger.CreateLog();
                    }
                }
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