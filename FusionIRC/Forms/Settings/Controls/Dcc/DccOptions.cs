/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FusionIRC.Forms.Settings.Controls.Base;
using FusionIRC.Forms.Settings.Editing;
using ircCore.Settings;
using ircCore.Settings.SettingsBase.Structures;
using ircCore.Settings.SettingsBase.Structures.Dcc;
using ircCore.Utils;
using libolv;

namespace FusionIRC.Forms.Settings.Controls.Dcc
{
    public partial class DccOptions : BaseControlRenderer, ISettings
    {
        private readonly OlvColumn _colExt;

        private readonly List<SettingsDcc.SettingsDccOptions.SettingsDccFilter.SettingsDccExtension> _ext;

        public event Action<ISettings> OnSettingsChanged;

        public bool SettingsChanged { get; set; }
        
        public DccOptions()
        {
            InitializeComponent();

            Header = "DCC Options";

            _colExt = new OlvColumn("Extensions:", "Name")
                          {
                              CellPadding = null,
                              IsEditable = false,
                              Sortable = false,
                              Width = 120,
                              FillsFreeSpace = true
                          };
            lvExt.AllColumns.Add(_colExt);
            lvExt.Columns.Add(_colExt);

            txtMin.Text = SettingsManager.Settings.Dcc.Options.General.MinimumPort.ToString();
            txtMax.Text = SettingsManager.Settings.Dcc.Options.General.MaximumPort.ToString();
            chkRandomize.Checked = SettingsManager.Settings.Dcc.Options.General.Randomize;
            chkBind.Checked = SettingsManager.Settings.Dcc.Options.General.BindToIp;
            txtBindAddress.Text = SettingsManager.Settings.Dcc.Options.General.BindIpAddress;
            txtBindAddress.Enabled = chkBind.Checked;

            txtRequests.Text = SettingsManager.Settings.Dcc.Options.Timeouts.GetSendRequest.ToString();
            txtTransfers.Text = SettingsManager.Settings.Dcc.Options.Timeouts.GetSendTransfer.ToString();
            txtConnections.Text = SettingsManager.Settings.Dcc.Options.Timeouts.ChatConnection.ToString();

            cmbPacketSize.Items.AddRange(new object[] {512, 1024, 2048, 4096, 8192});
            cmbPacketSize.SelectedItem = SettingsManager.Settings.Dcc.Options.General.PacketSize;

            cmbMethod.Items.AddRange(Functions.EnumUtils.GetDescriptions(typeof(DccFilterMethod)));
            cmbMethod.SelectedIndex = (int) SettingsManager.Settings.Dcc.Options.Filter.FilterMethod;

            _ext =
                new List<SettingsDcc.SettingsDccOptions.SettingsDccFilter.SettingsDccExtension>(
                    SettingsManager.Settings.Dcc.Options.Filter.Extension);
            lvExt.SetObjects(_ext);

            chkDialog.Checked = SettingsManager.Settings.Dcc.Options.Filter.ShowRejectionDialog;

            txtMin.TextChanged += ControlsChanged;
            txtMax.TextChanged += ControlsChanged;
            chkRandomize.CheckedChanged += ControlsChanged;
            chkBind.CheckedChanged += ControlsChanged;
            txtBindAddress.TextChanged += ControlsChanged;
            cmbPacketSize.SelectedIndexChanged += ControlsChanged;

            txtRequests.TextChanged += ControlsChanged;
            txtTransfers.TextChanged += ControlsChanged;
            txtConnections.TextChanged += ControlsChanged;

            cmbMethod.SelectedIndexChanged += ControlsChanged;
            chkDialog.CheckedChanged += ControlsChanged;
            lvExt.SelectedIndexChanged += ListSelectedIndexChanged;
            
            btnAdd.Click += ButtonClickHandler;
            btnDelete.Click += ButtonClickHandler;
        }
        
        public void SaveSettings()
        {
            int i;
            if (!int.TryParse(txtMin.Text, out i))
            {
                i = 1024;
            }
            SettingsManager.Settings.Dcc.Options.General.MinimumPort = i;
            if (!int.TryParse(txtMax.Text, out i))
            {
                i = 5000;
            }
            SettingsManager.Settings.Dcc.Options.General.MaximumPort = i;
            SettingsManager.Settings.Dcc.Options.General.Randomize = chkRandomize.Checked;
            SettingsManager.Settings.Dcc.Options.General.BindToIp = chkBind.Checked;
            SettingsManager.Settings.Dcc.Options.General.BindIpAddress = Functions.GetFirstWord(txtBindAddress.Text);
            SettingsManager.Settings.Dcc.Options.General.PacketSize = (int) cmbPacketSize.SelectedItem;
            
            if (!int.TryParse(txtRequests.Text, out i))
            {
                i = 120;
            }
            SettingsManager.Settings.Dcc.Options.Timeouts.GetSendRequest = i;
            if (!int.TryParse(txtTransfers.Text, out i))
            {
                i = 120;
            }
            SettingsManager.Settings.Dcc.Options.Timeouts.GetSendTransfer = i;
            if (!int.TryParse(txtConnections.Text, out i))
            {
                i = 120;
            }
            SettingsManager.Settings.Dcc.Options.Timeouts.ChatConnection = i;

            SettingsManager.Settings.Dcc.Options.Filter.FilterMethod = (DccFilterMethod) cmbMethod.SelectedIndex;
            SettingsManager.Settings.Dcc.Options.Filter.Extension =
                new List<SettingsDcc.SettingsDccOptions.SettingsDccFilter.SettingsDccExtension>(_ext);
            SettingsManager.Settings.Dcc.Options.Filter.ShowRejectionDialog = chkDialog.Checked;
            SettingsChanged = false;
        }

        /* Callbacks */
        private void ControlsChanged(object sender, EventArgs e)
        {
            txtBindAddress.Enabled = chkBind.Checked;            
            SettingsChanged = true;
            if (OnSettingsChanged != null)
            {
                OnSettingsChanged(this);
            }
        }

        private void ListSelectedIndexChanged(object sender, EventArgs e)
        {
            btnDelete.Enabled = lvExt.SelectedObjects.Count > 0;
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
                case "ADD":
                    using (var d = new FrmAddExtension())
                    {
                        if (d.ShowDialog(this) == DialogResult.Cancel || string.IsNullOrEmpty(d.Extension))
                        {
                            return;
                        }
                        var name = Functions.GetFirstWord(d.Extension);
                        var ext = new SettingsDcc.SettingsDccOptions.SettingsDccFilter.SettingsDccExtension
                                      {
                                          Name =
                                              !name.StartsWith("*.") && name[0] != '.'
                                                  ? string.Format("*.{0}", name)
                                                  : name[0] == '.' ? string.Format("*{0}", name) : name
                                      };
                        _ext.Add(ext);
                        _ext.Sort();
                        lvExt.SetObjects(_ext);
                    }
                    break;

                case "DELETE":
                    if (lvExt.SelectedObjects.Count == 0)
                    {
                        return;
                    }
                    foreach (var o in lvExt.SelectedObjects)
                    {
                        _ext.Remove((SettingsDcc.SettingsDccOptions.SettingsDccFilter.SettingsDccExtension) o);
                    }
                    lvExt.SetObjects(_ext);
                    btnDelete.Enabled = false;
                    break;
            }
            SettingsChanged = true;
            if (OnSettingsChanged != null)
            {
                OnSettingsChanged(this);
            }
        }
    }
}
