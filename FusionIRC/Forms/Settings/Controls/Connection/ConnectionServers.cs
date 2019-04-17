/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using FusionIRC.Forms.Settings.Controls.Base;
using FusionIRC.Forms.Settings.Editing;
using ircCore.Settings.Networks;
using libolv;

namespace FusionIRC.Forms.Settings.Controls.Connection
{
    public class ConnectionServers : BaseControlRenderer, ISettings
    {
        private readonly TreeListView _lvServers;
        private readonly OlvColumn _colName;
        private readonly Button _btnAdd;
        private readonly Button _btnEdit;
        private readonly Button _btnDelete;
        private readonly Button _btnSort;
        private readonly Button _btnClear;

        private readonly Servers _servers;

        public event Action OnSettingsChanged;

        public bool SettingsChanged { get; set; }

        public ConnectionServers()
        {
            Header = "Servers";

            _lvServers = new TreeListView
                             {
                                 FullRowSelect = true,
                                 HeaderStyle = ColumnHeaderStyle.Nonclickable,
                                 HideSelection = false,
                                 Location = new Point(3, 37),
                                 MultiSelect = false,
                                 OwnerDraw = true,
                                 ShowGroups = false,
                                 ShowItemToolTips = true,
                                 Size = new Size(340, 307),
                                 TabIndex = 1,
                                 UseCompatibleStateImageBehavior = false,
                                 View = View.Details,
                                 EmptyListMsg = @"No servers currently installed. Click 'Add' to create some servers.",
                                 EmptyListMsgFont = base.Font,                                 
                                 VirtualMode = true
                             };

            _colName = new OlvColumn(@"Installed IRC servers:", "DisplayName")
                           {
                               CellPadding = null,
                               IsEditable = false,
                               Sortable = false,
                               Width = 360
                           };

            _lvServers.AllColumns.Add(_colName);
            _lvServers.Columns.Add(_colName);

            _btnAdd = new Button
                          {
                              Location = new Point(349, 37),
                              Size = new Size(75, 23),
                              TabIndex = 2,
                              Tag = "ADD",
                              Text = @"Add",
                              UseVisualStyleBackColor = true
                          };

            _btnEdit = new Button
                           {
                               Location = new Point(349, 79),
                               Size = new Size(75, 23),
                               TabIndex = 3,
                               Tag = "EDIT",
                               Text = @"Edit",
                               UseVisualStyleBackColor = true,
                               Enabled = false
                           };

            _btnDelete = new Button
                             {
                                 Location = new Point(349, 108),
                                 Size = new Size(75, 23),
                                 TabIndex = 4,
                                 Tag = "DELETE",
                                 Text = @"Delete",
                                 UseVisualStyleBackColor = true,
                                 Enabled = false
                             };

            _btnSort = new Button
                           {
                               Location = new Point(349, 151),
                               Size = new Size(75, 23),
                               TabIndex = 5,
                               Tag = "SORT",
                               Text = @"Sort",
                               UseVisualStyleBackColor = true,
                               Enabled = false
                           };

            _btnClear = new Button
                            {
                                Location = new Point(349, 180),
                                Name = "btnClear",
                                Size = new Size(75, 23),
                                TabIndex = 6,
                                Tag = "CLEAR",
                                Text = @"Clear",
                                UseVisualStyleBackColor = true,
                                Enabled = false
                            };

            Controls.AddRange(new Control[] { _lvServers, _btnAdd, _btnEdit, _btnDelete, _btnSort, _btnClear });

            /* Root item (network name) */
            _lvServers.CanExpandGetter = x => x is NetworkData;

            /* Children of each root item (server data) */
            _lvServers.ChildrenGetter = delegate(object x)
                                           {
                                               var sd = (NetworkData) x;
                                               return sd.Server;
                                           };

            /* Images for root or children */
            _colName.ImageGetter = x => x is NetworkData
                                            ? Properties.Resources.network.ToBitmap()
                                            : Properties.Resources.server.ToBitmap();

            /* Change the color of treelines */
            _lvServers.TreeColumnRenderer.LinePen = new Pen(Color.FromArgb(190, 190, 190), 0.5F)
                                                       {
                                                           DashStyle = DashStyle.Dot
                                                       };

            _servers = new Servers(ServerManager.Servers);
            
            _lvServers.AddObjects(_servers.Networks.Network);
            _lvServers.SelectedIndexChanged += SelectedObjectChanged;

            var enable = _lvServers.GetItemCount() > 0;
            _btnSort.Enabled = enable;
            _btnClear.Enabled = enable;

            _btnAdd.Click += ButtonClickHandler;
            _btnEdit.Click += ButtonClickHandler;
            _btnDelete.Click += ButtonClickHandler;
            _btnSort.Click += ButtonClickHandler;
            _btnClear.Click += ButtonClickHandler;
        }
        
        public void SaveSettings()
        {
            ServerManager.Servers = new Servers(_servers);
            ServerManager.Save();
            SettingsChanged = false;
        }

        /* Callbacks */
        private void SelectedObjectChanged(object sender, EventArgs e)
        {
            var enable = _lvServers.SelectedObject != null;
            _btnEdit.Enabled = enable;
            _btnDelete.Enabled = enable;
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
                    AddServer();
                    break;

                case "EDIT":
                    EditServer();
                    break;

                case "DELETE":
                    DeleteServer();
                    break;

                case "SORT":
                    SortServers();
                    break;

                case "CLEAR":
                    _lvServers.RemoveObjects(_servers.Networks.Network);
                    _servers.Networks = new Servers.NetworkList();
                    _lvServers.RefreshObjects(_servers.Networks.Network);
                    _btnEdit.Enabled = false;
                    _btnDelete.Enabled = false;
                    _btnSort.Enabled = false;
                    _btnClear.Enabled = false;
                    /* Servers modified */
                    SettingsChanged = true;
                    if (OnSettingsChanged != null)
                    {
                        OnSettingsChanged();
                    }
                    break;
            }
        }

        /* Private methods */
        private void AddServer()
        {
            using (var d = new FrmAddServer(ServerEditType.Add))
            {
                var network = (_lvServers.SelectedObject is NetworkData
                                   ? (NetworkData) _lvServers.SelectedObject
                                   : _servers.GetNetworkByServer((ServerData) _lvServers.SelectedObject)) ??
                              new NetworkData();
                var server = new ServerData();
                d.Text = @"Add new server";
                d.Network = new NetworkData
                                {
                                    NetworkName = network.NetworkName
                                };
                d.Server = server;
                if (d.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                /* Create a new server - first determine if a network name of "network" all ready exists */
                if (string.Compare(d.Network.NetworkName, network.NetworkName, StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    var n = _servers.GetNetworkByName(d.Network.NetworkName);
                    if (n != null)
                    {
                        /* Found the network, add the server to it */
                        n.Server.Add(server);
                        _lvServers.RefreshObject(n);
                        _lvServers.Expand(n);
                    }
                    else
                    {                        
                        d.Network.Server.Add(server);
                        _servers.Networks.Network.Add(d.Network);
                        _lvServers.AddObject(d.Network);
                        _lvServers.Expand(d.Network);
                    }
                }
                else
                {                    
                    /* Doesn't exist, just add network to list and server to network */
                    network.Server.Add(server);
                    _lvServers.RefreshObject(network);
                }
                _lvServers.SelectObject(server);               
                _btnEdit.Enabled = true;
                _btnDelete.Enabled = true;
                _btnSort.Enabled = true;
                _btnClear.Enabled = true;
                /* Servers modified */
                SettingsChanged = true;
                if (OnSettingsChanged != null)
                {
                    OnSettingsChanged();
                }
            }
        }

        private void EditServer()
        {
            var server = _lvServers.SelectedObject is ServerData ? (ServerData) _lvServers.SelectedObject : null;
            if (server == null)
            {
                return;
            }
            var network = _servers.GetNetworkByServer(server);
            if (network == null)
            {
                return;
            }
            using (var d = new FrmAddServer(ServerEditType.Edit))
            {
                d.Text = @"Edit selected server";
                d.Network = new NetworkData
                                {
                                    NetworkName = network.NetworkName
                                };
                d.Server = server;
                if (d.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                /* Determine if a network name of "network" has changed */
                if (string.Compare(d.Network.NetworkName, network.NetworkName, StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    /* The network was changed - we remove the server from current network and find/create new network */
                    network.Server.Remove(server);
                    _lvServers.RemoveObject(server);
                    if (network.Server.Count == 0)
                    {
                        /* Remove empty networks */
                        _servers.Networks.Network.Remove(network);
                        _lvServers.RemoveObject(network);
                    }
                    var n = _servers.GetNetworkByName(d.Network.NetworkName);
                    if (n != null)
                    {
                        n.Server.Add(server);
                        _lvServers.RefreshObject(n);
                        _lvServers.Expand(n);
                    }
                    else
                    {
                        /* Doesn't exist, just add network to list and server to network */
                        network = d.Network;
                        network.Server.Add(server);
                        _servers.Networks.Network.Add(network);
                        _lvServers.AddObject(network);
                        _lvServers.Expand(network);
                    }
                }
                else
                {
                    /* We just update the server data of selected network */
                    _lvServers.RefreshObject(network);
                }
                _lvServers.SelectObject(server);
                _btnEdit.Enabled = true;
                _btnDelete.Enabled = true;
                _btnSort.Enabled = true;
                _btnClear.Enabled = true;
                /* Servers modified */
                SettingsChanged = true;
                if (OnSettingsChanged != null)
                {
                    OnSettingsChanged();
                }
            }
        }

        private void DeleteServer()
        {
            if (_lvServers.SelectedObject == null)
            {
                return;
            }
            if (_lvServers.SelectedObject is NetworkData)
            {
                _servers.Networks.Network.Remove((NetworkData)_lvServers.SelectedObject);
                _lvServers.RemoveObject(_lvServers.SelectedObject);
            }
            else if (_lvServers.SelectedObject is ServerData)
            {
                var server = (ServerData) _lvServers.SelectedObject;
                var n = _servers.GetNetworkByServer(server);
                n.Server.Remove(server);
                _lvServers.RemoveObject(server);
                if (n.Server.Count == 0)
                {
                    /* Remove empty network */
                    _servers.Networks.Network.Remove(n);
                    _lvServers.RemoveObject(n);
                }
            }
            /* Refresh list */
            _lvServers.RefreshObjects(_servers.Networks.Network);
            _btnEdit.Enabled = false;
            _btnDelete.Enabled = false;
            /* Servers modified */
            SettingsChanged = true;
            if (OnSettingsChanged != null)
            {
                OnSettingsChanged();
            }
            if (_lvServers.GetItemCount() != 0)
            {
                return;
            }
            _btnSort.Enabled = false;
            _btnClear.Enabled = false;
        }

        private void SortServers()
        {
            if (_lvServers.SelectedObject == null)
            {
                /* Nothing selected, sort ALL networks - doesn't sort servers */
                _servers.Networks.Network.Sort();
                _lvServers.SetObjects(_servers.Networks.Network);
            }
            else
            {
                /* This won't work if the selected object is the server itself...*/
                NetworkData n;
                var sel = _lvServers.SelectedObject;
                if (sel.GetType() == typeof(ServerData))
                {
                    /* Get the network group associated with this server */
                    n = _servers.GetNetworkByServer((ServerData) sel);
                }
                else
                {
                    /* It's already a network */
                    n = (NetworkData) sel;
                }
                if (n == null)
                {
                    /* It shouldn't be null, but you never know ;) */
                    return;
                }
                n.Server.Sort();
                _lvServers.RefreshObject(n);
            }
            /* Servers modified */
            SettingsChanged = true;
            if (OnSettingsChanged != null)
            {
                OnSettingsChanged();
            }
        }
    }
}
