/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */

using System;
using System.Drawing;
using System.Windows.Forms;
using FusionIRC.Forms.Autos.Controls;
using ircCore.Autos;
using ircCore.Controls;
using ircCore.Settings;

namespace FusionIRC.Forms.Autos
{
    public sealed class FrmAutos : FormEx
    {        
        private readonly TabControl _tabAutos;
        private readonly TabPage _tabJoin;
        private readonly TabPage _tabIdentify;
        private readonly AutomationsListView _autoJoin;
        private readonly AutomationsListView _autoIdentify;
        private readonly Button _btnClose;

        public FrmAutos()
        {
            /* Form code */
            ClientSize = new Size(421, 421);
            Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((0)));
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"Automations";

            _tabAutos = new TabControl
                            {
                                Location = new Point(12, 12),
                                SelectedIndex = 0,
                                Size = new Size(397, 368),
                                TabIndex = 0
                            };

            /* Auto join tab */
            _tabJoin = new TabPage();            
            _tabJoin.Location = new Point(4, 22);
            _tabJoin.Padding = new Padding(3);
            _tabJoin.Size = new Size(389, 342);
            _tabJoin.TabIndex = 0;
            _tabJoin.Text = @"Join";
            _tabJoin.UseVisualStyleBackColor = true;

            _autoJoin = new AutomationsListView(AutomationsManager.AutomationType.Join)
                            {
                                Dock = DockStyle.Fill,
                                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0))),
                                Location = new Point(3, 3),
                                Size = new Size(383, 336)
                            };
            _tabJoin.Controls.Add(_autoJoin);

            /* Auto identify tab */
            _tabIdentify = new TabPage
                               {
                                   Location = new Point(4, 22),
                                   Padding = new Padding(3),
                                   Size = new Size(389, 342),
                                   TabIndex = 1,
                                   Text = @"Identify",
                                   UseVisualStyleBackColor = true
                               };

            _autoIdentify = new AutomationsListView(AutomationsManager.AutomationType.Identify)
                                {
                                    Dock = DockStyle.Fill,
                                    Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0))),
                                    Location = new Point(3, 3),
                                    Size = new Size(383, 336)
                                };
            _tabIdentify.Controls.Add(_autoIdentify);

            /* Add tab pages to tab control */
            _tabAutos.Controls.AddRange(new Control[] {_tabJoin, _tabIdentify});
            _tabAutos.SelectedIndex = SettingsManager.Settings.Client.Tabs.AutoList;

            _btnClose = new Button
                            {
                                DialogResult = DialogResult.OK,
                                Location = new Point(334, 386),
                                Size = new Size(75, 23),
                                TabIndex = 0,
                                Text = @"Close",
                                UseVisualStyleBackColor = true
                            };

            Controls.AddRange(new Control[] {_tabAutos, _btnClose});
            _tabAutos.SelectedIndexChanged += TabSelectedIndexChanged;
        }

        private void TabSelectedIndexChanged(object sender, EventArgs e)
        {
            SettingsManager.Settings.Client.Tabs.AutoList = _tabAutos.SelectedIndex;
        }
    }
}