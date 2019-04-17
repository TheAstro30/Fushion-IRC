/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using FusionIRC.Forms.Favorites.Editing;
using FusionIRC.Properties;
using ircClient;
using ircCore.Controls;
using ircCore.Settings;
using ircCore.Settings.Channels;
using ircCore.Utils;
using libolv;

namespace FusionIRC.Forms.Favorites
{
    public sealed class FrmFavorites : FormEx
    {
        private readonly ObjectListView _lvFave;
        private readonly OlvColumn _colChan;
        private readonly OlvColumn _colDesc;
        private readonly Button _btnAdd;        
        private readonly Button _btnEdit;
        private readonly Button _btnDelete;
        private readonly Button _btnClear;
        private readonly Button _btnJoin;
        private readonly CheckBox _chkShow;
        private readonly Button _btnClose;               

        private readonly ImageList _imageList;

        private readonly ClientConnection _client;

        public FrmFavorites(ClientConnection client)
        {
            _client = client;

            ClientSize = new Size(382, 402);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0)));
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = @"Channel Favorites";

            _lvFave = new ObjectListView
                          {
                              FullRowSelect = true,
                              HeaderStyle = ColumnHeaderStyle.Nonclickable,
                              Location = new Point(12, 12),
                              MultiSelect = false,
                              Size = new Size(277, 341),
                              TabIndex = 0,
                              UseCompatibleStateImageBehavior = false,
                              View = View.Details
                          };

            _colChan = new OlvColumn("Channel:", "Name")
                           {
                               CellPadding = null,
                               IsEditable = false,
                               Sortable = false,
                               Width = 120,
                               ImageGetter = delegate { return 0; }
                           };

            _colDesc = new OlvColumn("Description:", "Description")
                           {
                               CellPadding = null,
                               IsEditable = false,
                               Sortable = false,
                               Width = 300
                           };

            _lvFave.AllColumns.AddRange(new[] {_colChan, _colDesc});
            _lvFave.Columns.AddRange(new[] {_colChan, _colDesc});

            _btnAdd = new Button
                          {
                              Location = new Point(295, 12),
                              Size = new Size(75, 23),
                              TabIndex = 1,
                              Tag = "ADD",
                              Text = @"Add",
                              UseVisualStyleBackColor = true
                          };

            _btnEdit = new Button
                           {
                               Enabled = false,
                               Location = new Point(295, 54),
                               Size = new Size(75, 23),
                               TabIndex = 2,
                               Tag = "EDIT",
                               Text = @"Edit",
                               UseVisualStyleBackColor = true
                           };

            _btnDelete = new Button
                             {
                                 Enabled = false,
                                 Location = new Point(295, 83),
                                 Size = new Size(75, 23),
                                 TabIndex = 3,
                                 Tag = "DELETE",
                                 Text = @"Delete",
                                 UseVisualStyleBackColor = true
                             };

            _btnClear = new Button
                            {
                                Enabled = false,
                                Location = new Point(295, 112),
                                Size = new Size(75, 23),
                                TabIndex = 4,
                                Tag = "CLEAR",
                                Text = @"Clear",
                                UseVisualStyleBackColor = true
                            };

            _btnJoin = new Button
                           {
                               Enabled = false,
                               Location = new Point(295, 330),
                               Size = new Size(75, 23),
                               TabIndex = 5,
                               Tag = "JOIN",
                               Text = @"Join",
                               UseVisualStyleBackColor = true
                           };

            _chkShow = new CheckBox
                           {
                               AutoSize = true,
                               Location = new Point(12, 371),
                               Name = "chkShow",
                               Size = new Size(154, 19),
                               TabIndex = 6,
                               Text = @"Show dialog on connect",
                               UseVisualStyleBackColor = true
                           };

            _btnClose = new Button
                            {
                                DialogResult = DialogResult.OK,
                                Location = new Point(295, 367),
                                Size = new Size(75, 23),
                                TabIndex = 7,
                                Text = @"Close",
                                UseVisualStyleBackColor = true
                            };

            Controls.AddRange(new Control[] {_lvFave, _btnAdd, _btnEdit, _btnDelete, _btnClear, _btnJoin, _chkShow, _btnClose});

            _imageList = new ImageList {ColorDepth = ColorDepth.Depth32Bit, ImageSize = new Size(16, 16)};
            _imageList.Images.Add(Resources.channel);

            _lvFave.SmallImageList = _imageList;

            /* Add favorite data to list */
            _lvFave.AddObjects(ChannelManager.Channels.Favorites.Favorite);

            _btnClear.Enabled = _lvFave.GetItemCount() > 0;
            _chkShow.Checked = SettingsManager.Settings.Client.Channels.ShowFavoritesDialogOnConnect;

            /* Handlers */
            _lvFave.SelectionChanged += ListSelectionChanged;
            _lvFave.DoubleClick += ListDoubleClicked;

            _btnAdd.Click += ButtonClickHandler;
            _btnEdit.Click += ButtonClickHandler;
            _btnDelete.Click += ButtonClickHandler;
            _btnClear.Click += ButtonClickHandler;
            _btnJoin.Click += ButtonClickHandler;

            _chkShow.CheckedChanged += CheckChanged;
        }

        /* Callbacks */
        private void ListSelectionChanged(object sender, EventArgs e)
        {
            var enable = _lvFave.SelectedObject != null;
            _btnEdit.Enabled = enable;
            _btnDelete.Enabled = enable;
            _btnJoin.Enabled = enable;
        }

        private void ListDoubleClicked(object sender, EventArgs e)
        {
            JoinChannel();
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
                case "ADD":
                    AddFavorite();
                    break;

                case "EDIT":
                    EditFavorite();
                    break;

                case "DELETE":
                    DeleteFavorite();
                    break;

                case "CLEAR":
                    ClearFavorites();
                    break;

                case "JOIN":
                    JoinChannel();
                    break;
            }
        }

        private void CheckChanged(object sender, EventArgs e)
        {
            SettingsManager.Settings.Client.Channels.ShowFavoritesDialogOnConnect = _chkShow.Checked;
        }

        /* Private methods */
        private void AddFavorite()
        {
            ChannelFavorites.ChannelFavoriteData data;
            using (var edit = new FrmChannelEdit(DialogEditType.Add))
            {
                if (edit.ShowDialog(this) == DialogResult.Cancel || string.IsNullOrEmpty(edit.Channel))
                {
                    return;
                }
                data = new ChannelFavorites.ChannelFavoriteData
                           {
                               Name = Functions.GetFirstWord(edit.Channel),
                               Password = Functions.GetFirstWord(edit.Password),
                               Description = edit.Description
                           };
            }
            /* Add channel to list */
            ChannelManager.Channels.Favorites.Favorite.Add(data);
            ChannelManager.Channels.Favorites.Favorite.Sort();
            _lvFave.SetObjects(ChannelManager.Channels.Favorites.Favorite);
            _btnEdit.Enabled = false;
            _btnDelete.Enabled = false;
            _btnJoin.Enabled = false;
            _btnClear.Enabled = true;
        }

        private void EditFavorite()
        {
            var data = _lvFave.SelectedObject;
            if (data == null)
            {
                return;
            }
            var d = (ChannelFavorites.ChannelFavoriteData)data;
            using (var edit = new FrmChannelEdit(DialogEditType.Edit))
            {                
                edit.Channel = d.Name;
                edit.Password = d.Password;
                edit.Description = d.Description;
                if (edit.ShowDialog(this) == DialogResult.Cancel || string.IsNullOrEmpty(edit.Channel))
                {
                    return;
                }        
                /* Update data */
                d.Name = Functions.GetFirstWord(edit.Channel);
                d.Password = Functions.GetFirstWord(edit.Password);
                d.Description = edit.Description;
            }
            ChannelManager.Channels.Favorites.Favorite.Sort();
            _lvFave.SetObjects(ChannelManager.Channels.Favorites.Favorite);            
            _btnEdit.Enabled = false;
            _btnDelete.Enabled = false;
            _btnJoin.Enabled = false;
        }

        private void DeleteFavorite()
        {
            var data = _lvFave.SelectedObject;
            if (data == null)
            {
                return;
            }
            var d = (ChannelFavorites.ChannelFavoriteData)data;
            ChannelManager.Channels.Favorites.Favorite.Remove(d);
            _lvFave.RemoveObject(d);
            _btnEdit.Enabled = false;
            _btnDelete.Enabled = false;
            _btnJoin.Enabled = false;
            _btnClear.Enabled = ChannelManager.Channels.Favorites.Favorite.Count > 0;
        }

        private void ClearFavorites()
        {
            ChannelManager.Channels.Favorites.Favorite.Clear();
            _lvFave.ClearObjects();
            _btnEdit.Enabled = false;
            _btnDelete.Enabled = false;
            _btnJoin.Enabled = false;
            _btnClear.Enabled = false;
        }

        private void JoinChannel()
        {
            var chan = _lvFave.SelectedObject;
            if (chan == null || !_client.IsConnected)
            {
                return;
            }
            var c = (ChannelFavorites.ChannelFavoriteData) chan;
            _client.Send(string.Format("JOIN {0} {1}", c.Name, c.Password));
        }
    }
}