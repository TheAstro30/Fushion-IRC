/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using FusionIRC.Forms.Users.Editing;
using ircCore.Users;
using libolv;

namespace FusionIRC.Forms.Users.Controls
{
    public sealed class UserListView : UserControl
    {
        private readonly UserListType _userListType;

        private readonly ObjectListView _list;
        private readonly OlvColumn _colNick;
        private readonly OlvColumn _colNote;
        private readonly ImageList _imageList;
        private readonly Button _btnAdd;
        private readonly Button _btnEdit;
        private readonly Button _btnDelete;
        private readonly Button _btnClear;

        public UserListView(UserListType userListType)
        {
            _userListType = userListType;

            BackColor = Color.Transparent;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Size = new Size(371, 297);

            _list = new ObjectListView
                        {
                            FullRowSelect = true,
                            HeaderStyle = ColumnHeaderStyle.Nonclickable,
                            HideSelection = false,
                            Location = new Point(3, 3),
                            MultiSelect = false,
                            Size = new Size(284, 291),
                            TabIndex = 0,
                            UseCompatibleStateImageBehavior = false,                            
                            View = View.Details
                        };            

            _colNote = new OlvColumn("Note:", "Note")
                           {
                               CellPadding = null,
                               IsEditable = false,
                               Sortable = false,
                               Width = 300,
                               Hideable = true,
                               IsVisible = _userListType == UserListType.Notify
                           };

            _btnAdd = new Button
                          {
                              Location = new Point(293, 3),
                              Size = new Size(75, 23),
                              TabIndex = 1,
                              Tag = "ADD",
                              Text = @"Add",
                              UseVisualStyleBackColor = true
                          };

            _btnEdit = new Button
                           {
                               Enabled = false,
                               Location = new Point(293, 32),
                               Size = new Size(75, 23),
                               TabIndex = 2,
                               Tag = "EDIT",
                               Text = @"Edit",
                               UseVisualStyleBackColor = true
                           };

            _btnDelete = new Button
                             {
                                 Enabled = false,
                                 Location = new Point(293, 90),
                                 Size = new Size(75, 23),
                                 TabIndex = 3,
                                 Tag = "DELETE",
                                 Text = @"Delete",
                                 UseVisualStyleBackColor = true
                             };

            _btnClear = new Button
                            {
                                Enabled = false,
                                Location = new Point(293, 119),
                                Size = new Size(75, 23),
                                TabIndex = 4,
                                Tag = "CLEAR",
                                Text = @"Clear",
                                UseVisualStyleBackColor = true
                            };

            /* Image list for listview */
            _imageList = new ImageList {ColorDepth = ColorDepth.Depth32Bit, ImageSize = new Size(16, 16)};
            _imageList.Images.AddRange(new Image[]
                                           {
                                               Properties.Resources.notify.ToBitmap(),
                                               Properties.Resources.ignored.ToBitmap()
                                           });
            _list.SmallImageList = _imageList;

            Controls.AddRange(new Control[] {_list, _btnAdd, _btnEdit, _btnDelete, _btnClear});

            switch (_userListType)
            {
                case UserListType.Notify:
                    _colNick = new OlvColumn("Nick:", "Nick")
                                   {
                                       CellPadding = null,
                                       IsEditable = false,
                                       Sortable = false,
                                       Width = 120,
                                       ImageGetter = delegate { return 0; }
                                   };
                    _list.AddObjects(UserManager.UserList.Notify.Users);
                    break;

                case UserListType.Ignore:
                    _colNick = new OlvColumn("Address:", "Address")
                                   {
                                       CellPadding = null,
                                       IsEditable = false,
                                       Sortable = false,
                                       Width = 300,
                                       ImageGetter = delegate { return 1; }
                                   };
                    _list.AddObjects(UserManager.UserList.Ignore.Users);
                    break;
            }

            _list.AllColumns.AddRange(new[] { _colNick, _colNote });
            _list.Columns.AddRange(new ColumnHeader[] { _colNick, _colNote });
            _list.RebuildColumns();

            _btnClear.Enabled = _list.GetItemCount() > 0;

            _list.SelectionChanged += ListSelectionChanged;

            _btnAdd.Click += ButtonClick;
            _btnEdit.Click += ButtonClick;
            _btnDelete.Click += ButtonClick;
            _btnClear.Click += ButtonClick;
        }

        private void ListSelectionChanged(object sender, EventArgs e)
        {
            var enable = _list.SelectedObject != null;
            _btnEdit.Enabled = enable;
            _btnDelete.Enabled = enable;
        }

        private void ButtonClick(object sender, EventArgs e)
        {
            var btn = (Button) sender;
            if (btn == null)
            {
                return;
            }
            /* We will eventually have to make sure to update the WATCH list on the server/s */
            switch (btn.Tag.ToString())
            {
                case "ADD":
                    AddUser();
                    break;

                case "EDIT":
                    EditUser();               
                    break;

                case "DELETE":
                    DeleteUser();
                    break;

                case "CLEAR":
                    ClearUsers();
                    break;
            }
        }

        /* Private methods */
        private void AddUser()
        {
            var user = new User();
            switch (_userListType)
            {
                case UserListType.Notify:
                    using (var add = new FrmAddNotify(UserEditType.Add))
                    {
                        add.Text = @"Add new nick to notify list";
                        add.User = user;
                        if (add.ShowDialog(this) == DialogResult.OK)
                        {
                            UserManager.UserList.Notify.Users.Add(user);                            
                        }
                    }
                    break;

                case UserListType.Ignore:
                    using (var add = new FrmAddIgnore(UserEditType.Add))
                    {
                        add.Text = @"Add new nick/address to ignore list";
                        add.User = user;
                        if (add.ShowDialog(this) == DialogResult.OK)
                        {
                            UserManager.UserList.Ignore.Users.Add(user);
                        }
                    }
                    break;
            }
            if (string.IsNullOrEmpty(user.Nick))
            {
                return;
            }
            _list.AddObject(user);
            _btnClear.Enabled = _list.GetItemCount() > 0;
        }

        private void EditUser()
        {
            var user = (User) _list.SelectedObject;
            if (user == null)
            {
                return;
            }
            switch (_userListType)
            {
                case UserListType.Notify:
                    using (var edit = new FrmAddNotify(UserEditType.Edit))
                    {
                        edit.Text = @"Edit current user on notify list";
                        edit.User = user;
                        if (edit.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(user.Nick))
                        {
                            _list.RefreshObject(user);
                        }
                    }
                    break;

                case UserListType.Ignore:
                    using (var edit = new FrmAddIgnore(UserEditType.Edit))
                    {
                        edit.Text = @"Edit current user on ignore list";
                        edit.User = user;
                        if (edit.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(user.Nick))
                        {
                            _list.RefreshObject(user);
                        }
                    }
                    break;
            }
        }    

        private void DeleteUser()
        {
            var user = (User)_list.SelectedObject;
            if (user == null)
            {
                return;
            }
            switch (_userListType)
            {
                case UserListType.Notify:
                    UserManager.UserList.Notify.Users.Remove(user);
                    break;

                case UserListType.Ignore:
                    UserManager.UserList.Ignore.Users.Remove(user);
                    break;
            }
            _list.RemoveObject(user);
            _btnClear.Enabled = _list.GetItemCount() > 0;
        }

        private void ClearUsers()
        {
            switch (_userListType)
            {
                case UserListType.Notify:
                    UserManager.UserList.Notify.Users = new List<User>();
                    break;

                case UserListType.Ignore:
                    UserManager.UserList.Ignore.Users = new List<User>();
                    break;
            }
            _list.ClearObjects();
            _btnClear.Enabled = false;
        }
    }
}
