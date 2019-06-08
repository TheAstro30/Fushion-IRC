/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FusionIRC.Forms.Misc;
using ircCore.Autos;
using ircCore.Settings;
using ircScript.Controls;

namespace FusionIRC.Forms.Autos.Controls
{
    public sealed class AutomationPerform : UserControl
    {
        private readonly bool _initialize;
        private bool _switch;

        private readonly CheckBox _chkEnable;
        private readonly Label _lblNetwork;
        private readonly ComboBox _cmbNetwork;        
        private readonly Button _btnNew;
        private readonly Button _btnRemove;
        private readonly ScriptEditor _txtCommands;

        public AutomationPerform()
        {
            _initialize = true;
            BackColor = Color.Transparent;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Size = new Size(383, 336);

            _chkEnable = new CheckBox
                             {
                                 AutoSize = true,
                                 Location = new Point(3, 3),
                                 Size = new Size(199, 19),
                                 TabIndex = 0,
                                 Text = @"Enable auto-perform on connect",
                                 UseVisualStyleBackColor = true
                             };

            _lblNetwork = new Label
                              {
                                  AutoSize = true,
                                  Location = new Point(3, 31),
                                  Size = new Size(55, 15),
                                  Text = @"Network:"
                              };

            _cmbNetwork = new ComboBox
                              {
                                  DropDownStyle = ComboBoxStyle.DropDownList,
                                  FormattingEnabled = true,
                                  Location = new Point(64, 28),
                                  Size = new Size(152, 23),
                                  TabIndex = 1
                              };

            _btnNew = new Button
                          {
                              Location = new Point(222, 28),
                              Size = new Size(75, 23),
                              TabIndex = 3,
                              Text = @"New",
                              UseVisualStyleBackColor = true
                          };

            _btnRemove = new Button
                             {
                                 Location = new Point(303, 28),
                                 Size = new Size(75, 23),
                                 TabIndex = 4,
                                 Text = @"Remove",
                                 UseVisualStyleBackColor = true
                             };

            _txtCommands = new ScriptEditor
                               {
                                   Location = new Point(3, 57),
                                   BorderStyle = BorderStyle.FixedSingle,
                                   Size = new Size(375, 276),
                                   TabIndex = 5,
                                   EnableSyntaxHighlight = SettingsManager.Settings.Editor.SyntaxHighlight,
                                   ShowLineNumbers = SettingsManager.Settings.Editor.LineNumbering,
                                   Zoom = SettingsManager.Settings.Editor.Zoom,
                                   CommentColor = SettingsManager.Settings.Editor.Colors.CommentColor,
                                   CommandColor = SettingsManager.Settings.Editor.Colors.CommandColor,
                                   KeywordColor = SettingsManager.Settings.Editor.Colors.KeyWordColor,
                                   VariableColor = SettingsManager.Settings.Editor.Colors.VariableColor,
                                   IdentifierColor = SettingsManager.Settings.Editor.Colors.IdentifierColor,
                                   CustomIdentifierColor = SettingsManager.Settings.Editor.Colors.CustomIdentifierColor,
                                   MiscColor = SettingsManager.Settings.Editor.Colors.MiscColor
                               };

            Controls.AddRange(new Control[] {_chkEnable, _lblNetwork, _cmbNetwork, _btnNew, _btnRemove, _txtCommands});

            _chkEnable.Checked = AutomationsManager.Automations.Perform.Enable;
            /* Handlers */
            _chkEnable.CheckedChanged += CheckBoxCheckChanged;
            _cmbNetwork.SelectedIndexChanged += ComboSelectedIndexChanged;
            _btnNew.Click += ButtonClickHandler;
            _btnRemove.Click += ButtonClickHandler;
            _txtCommands.TextChangedDelayed += TextBoxTextChanged;
            /* Fill combo with stored networks and update listview */
            _cmbNetwork.Items.AddRange(AutomationsManager.GetAllNetworks(AutomationsManager.AutomationType.Perform));
            if (_cmbNetwork.Items.Count > 0)
            {
                _cmbNetwork.SelectedIndex = 0;
            }
            _initialize = false;
        }        

        /* Handlers */
        private void CheckBoxCheckChanged(object sender, EventArgs e)
        {
            AutomationsManager.Automations.Perform.Enable = _chkEnable.Checked;
        }

        private void ComboSelectedIndexChanged(object sender, EventArgs e)
        {
            BuildCommandList();
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
                case "NEW":
                    AddNetwork();
                    break;

                case "REMOVE":
                    RemoveNetwork();
                    break;
            }
        }

        private void TextBoxTextChanged(object sender, EventArgs e)
        {
            if (_initialize || _switch)
            {
                return;
            }
            var nd = AutomationsManager.GetAutomationByNetwork(AutomationsManager.AutomationType.Perform,
                                                               _cmbNetwork.Text);
            if (nd == null)
            {
                return;
            }
            nd.Commands = new List<string>(_txtCommands.Lines);
        }

        /* Private methods */
        private void BuildCommandList()
        {
            _switch = true;
            _txtCommands.Text = string.Empty;
            if (_cmbNetwork.Items.Count == 0)
            {
                _btnRemove.Enabled = false;
                _txtCommands.Enabled = false;
                _switch = false;
                return;
            } 
            var nd = AutomationsManager.GetAutomationByNetwork(AutomationsManager.AutomationType.Perform,
                                                               _cmbNetwork.Text);
            _txtCommands.Text = string.Join(Environment.NewLine, nd.Commands);
            _btnRemove.Enabled = true;
            _txtCommands.Enabled = true;
            _switch = false;
        }

        /* Add/remove network */
        private void AddNetwork()
        {
            using (var network = new FrmNetwork(true))
            {
                if (network.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                var nd = new AutoList.AutoNetworkData { Name = network.Network };
                if (AutomationsManager.GetAutomationByNetwork(AutomationsManager.AutomationType.Perform, nd.Name) == null)
                {
                    /* If the network doesn't already exist, we add a new one */
                    AutomationsManager.Automations.Perform.Network.Add(nd);
                    _cmbNetwork.Items.Add(nd.Name);
                    _cmbNetwork.SelectedIndex = _cmbNetwork.Items.Count - 1;
                }
                else
                {
                    /* No point adding it if it already exists, just select it in the combo box */
                    var index =
                        _cmbNetwork.Items.Cast<object>().TakeWhile(
                            n => !n.ToString().Equals(nd.Name, StringComparison.InvariantCultureIgnoreCase)).Count();
                    _cmbNetwork.SelectedIndex = index;
                }
            }
        }

        private void RemoveNetwork()
        {
            var nd = AutomationsManager.GetAutomationByNetwork(AutomationsManager.AutomationType.Perform,
                                                               _cmbNetwork.Text);
            if (nd == null)
            {
                /* It shouldn't be null... */
                return;
            }
            /* Remove network from stored list */
            AutomationsManager.Automations.Perform.Network.Remove(nd);
            /* Remove it from combobox */
            var index = _cmbNetwork.SelectedIndex - 1;
            if (index < 0)
            {
                index = 0;
            }
            _cmbNetwork.Items.RemoveAt(_cmbNetwork.SelectedIndex);
            if (_cmbNetwork.Items.Count > 0)
            {
                _cmbNetwork.SelectedIndex = index;
            }
            else
            {
                /* Combo list is empty... */
                BuildCommandList();
            }
        }
    }
}
