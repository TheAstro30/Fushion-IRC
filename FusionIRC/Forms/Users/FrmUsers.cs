/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Windows.Forms;
using System.Drawing;
using FusionIRC.Forms.Users.Controls;
using ircCore.Controls;
using ircCore.Settings;
using ircCore.Utils;

namespace FusionIRC.Forms.Users
{
    public sealed class FrmUsers : FormEx
    {
        private readonly UserListView _lvNotify;
        private readonly UserListView _lvIgnore;
        private readonly TabControl _tabUsers;
        private readonly TabPage _tabNotify;
        private readonly TabPage _tabIgnore;
        private readonly Button _btnClose;
      
        public FrmUsers()
        {
            ClientSize = new Size(412, 396);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;            
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"User List";

            _lvNotify = new UserListView(UserListType.Notify)
                            {
                                BackColor = Color.Transparent,
                                Dock = DockStyle.Fill,
                                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0),
                                Location = new Point(3, 3),                                
                                Size = new Size(374, 300),
                                TabIndex = 0                                
                            };

            _lvIgnore = new UserListView(UserListType.Ignore)
                            {
                                BackColor = Color.Transparent,
                                Dock = DockStyle.Fill,
                                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0),
                                Location = new Point(3, 3),                                
                                Size = new Size(374, 300),
                                TabIndex = 0                                
                            };

            _tabUsers = new TabControl
                            {
                                Location = new Point(12, 12),                                
                                SelectedIndex = 0,
                                Size = new Size(388, 334),
                                TabIndex = 0
                            };

            _tabNotify = new TabPage
                             {
                                 Location = new Point(4, 24),                                 
                                 Padding = new Padding(3),
                                 Size = new Size(380, 306),
                                 TabIndex = 0,
                                 Text = @"Notify",
                                 UseVisualStyleBackColor = true
                             };

            _tabNotify.Controls.Add(_lvNotify);

            _tabIgnore = new TabPage
                             {
                                 Location = new Point(4, 24),                                 
                                 Padding = new Padding(3),
                                 Size = new Size(380, 306),
                                 TabIndex = 1,
                                 Text = @"Ignore",
                                 UseVisualStyleBackColor = true
                             };

            _tabIgnore.Controls.Add(_lvIgnore);
            _tabUsers.Controls.AddRange(new Control[] { _tabNotify, _tabIgnore});

            _tabUsers.SelectedIndex = SettingsManager.Settings.Client.Tabs.UserList;

            _btnClose = new Button
                            {
                                DialogResult = DialogResult.OK,
                                Location = new Point(325, 361),                                
                                Size = new Size(75, 23),
                                TabIndex = 1,
                                Text = @"Close",
                                UseVisualStyleBackColor = true
                            };

            Controls.AddRange(new Control[] {_tabUsers, _btnClose});

            _tabUsers.SelectedIndexChanged += TabSelectedIndexChanged;
        }

        private void TabSelectedIndexChanged(object sender, EventArgs e)
        {
            SettingsManager.Settings.Client.Tabs.UserList = _tabUsers.SelectedIndex;
        }
    }
}
