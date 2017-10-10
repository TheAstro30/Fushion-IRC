/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FusionIRC.Forms.Settings.Controls.Base;
using FusionIRC.Forms.Settings.Controls.Connection;
using ircCore.Controls;
using ircCore.Settings;

namespace FusionIRC.Forms.Settings
{
    public partial class FrmSettings : FormEx
    {
        private readonly ConnectionServers _connectionServers;
        private readonly ConnectionOptions _connectionOptions;
        private readonly ConnectionIdentDaemon _connectionIdentDaemon;
        private readonly ConnectionLocalInfo _connectionLocalInfo;

        private readonly Timer _tmrSelect;

        public FrmSettings()
        {
            InitializeComponent();
            BuildTreeMenuNodes();

            _connectionServers = new ConnectionServers { Location = new Point(168, 12), Visible = false };
            _connectionOptions = new ConnectionOptions { Location = new Point(168, 12), Visible = false };
            _connectionIdentDaemon = new ConnectionIdentDaemon { Location = new Point(168, 12), Visible = false };
            _connectionLocalInfo = new ConnectionLocalInfo { Location = new Point(168, 12), Visible = false };

            Controls.AddRange(new Control[]
                                  {
                                      _connectionServers, _connectionOptions, _connectionIdentDaemon, _connectionLocalInfo
                                  });
                        
            _connectionServers.OnSettingsChanged += OnSettingsChanged;
            _connectionOptions.OnSettingsChanged += OnSettingsChanged;
            _connectionIdentDaemon.OnSettingsChanged += OnSettingsChanged;
            _connectionLocalInfo.OnSettingsChanged += OnSettingsChanged;

            btnApply.Click += ButtonClickHandler;

            _tmrSelect = new Timer
                             {
                                 Interval = 10,
                                 Enabled = true
                             };
            _tmrSelect.Tick += TimerSelect;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (btnApply.Enabled)
            {
                /* Settings not saved yet */
                SaveSettings();
            }
            base.OnFormClosing(e);
        }

        /* Treeview handelrs */
        private void MenuAfterSelect(object sender, EventArgs e)
        {
            /* Hide all/any shown "panels" */
            foreach (var c in from object c in Controls where c is ISettings && ((Control)c).Visible select c)
            {
                ((Control) c).Visible = false;
            }
            SettingsManager.Settings.Client.Tabs.Settings = tvMenu.SelectedNode.Name;
            switch (tvMenu.SelectedNode.Name)
            {
                case "CONNECTION":
                case "CONNECTIONSERVERS":
                    _connectionServers.Visible = true;
                    break;

                case "CONNECTIONOPTIONS":
                    _connectionOptions.Visible = true;
                    break;

                case "CONNECTIONIDENTDAEMON":
                    _connectionIdentDaemon.Visible = true;
                    break;

                case "CONNECTIONLOCALINFO":
                    _connectionLocalInfo.Visible = true;
                    break;
            }
        }

        private void ButtonClickHandler(object sender, EventArgs e)
        {
            var btn = (Button) sender;
            if (btn == null)
            {
                return;
            }
            switch (btn.Tag.ToString())
            {
                case "APPLY":
                    SaveSettings();
                    break;
            }
        }

        private void BuildTreeMenuNodes()
        {
            /* Connection */
            var node = tvMenu.Nodes.Add("CONNECTION", "Connection");
            node.Nodes.Add("CONNECTIONSERVERS", "Servers");
            node.Nodes.Add("CONNECTIONOPTIONS", "Options");
            node.Nodes.Add("CONNECTIONIDENTDAEMON", "Identd");
            node.Nodes.Add("CONNECTIONLOCALINFO", "Local Info"); 
        }

        private void OnSettingsChanged()
        {
            /* Really all this callback does :) */
            btnApply.Enabled = true;
        }

        /* Save settings - also, any settings relevant to client appearance should also update main client window/child controls */
        private void SaveSettings()
        {
            foreach (var panel in Controls.OfType<ISettings>().Where(panel => panel.SettingsChanged))
            {
                panel.SaveSettings();
            }
            btnApply.Enabled = false;
            SettingsManager.Save();
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
            var node = tvMenu.Nodes.Find(tab, true);
            if (node.Length == 0)
            {
                /* It should exist, unless someone modifies the settings XML file ;) */
                return;
            }
            tvMenu.AfterSelect += MenuAfterSelect;
            tvMenu.SelectedNode = node[0];            
        }
    }
}
