/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using FusionIRC.Forms.DirectClientConnection.Helper;
using FusionIRC.Properties;
using ircClient.Tcp;
using ircCore.Settings;
using ircCore.Utils;
using libolv;
using libolv.Implementation.Events;
using libolv.Rendering.Renderers;

namespace FusionIRC.Forms.DirectClientConnection
{
    public partial class FrmDccManager : Form
    {
        /* This form is created and hidden when the client form (main window) is created.
         * We just hide/show the form when needed. */
        private readonly OlvColumn _colFileName;
        private readonly OlvColumn _colUserName;
        private readonly OlvColumn _colProgress;
        private readonly OlvColumn _colSpeed;
        private readonly OlvColumn _colRemain;
        private readonly OlvColumn _colStatus;

        private readonly BarRenderer _barRenderer;

        private readonly ImageList _imageList;

        private readonly List<Dcc> _transfers;

        private readonly ContextMenuStrip _popupFile;

        private readonly bool _initialize;
        
        /* Constructor - create ObjectListView and columns */
        public FrmDccManager(List<Dcc> transfers)
        {
            _initialize = true;
            _transfers = transfers;
            InitializeComponent();
            Icon = Resources.dccManager;
            /* Image list */
            _imageList = new ImageList {ColorDepth = ColorDepth.Depth32Bit, ImageSize = new Size(16, 16)};
            _imageList.Images.AddRange(new[]
                                           {
                                               Resources.dccDown.ToBitmap(),
                                               Resources.dccUp.ToBitmap()
                                           });

            olvFiles.SmallImageList = _imageList;
            /* Load the list */
            _colFileName = new OlvColumn("File name:", "FileName")
                               {
                                   Sortable = false,
                                   IsEditable = false,
                                   IsVisible = true,
                                   Width = 150,
                                   /* Delegate to allow images upload/download in list */
                                   ImageGetter = row => (int) ((Dcc) row).DccFileType,
                                   FillsFreeSpace = true
                               };            

            _colUserName = new OlvColumn("User name:", "UserNameToString")
                               {
                                   Sortable = false,
                                   IsEditable = false,
                                   IsVisible = true,
                                   Width = 180
                               };

            _barRenderer = new BarRenderer {MaximumValue = 100, UseStandardBar = true};

            _colProgress = new OlvColumn("Progress:", "Progress")
                               {
                                   Sortable = false,
                                   IsEditable = false,
                                   IsVisible = true,
                                   Renderer = _barRenderer,
                                   Width = 80
                               };

            _colSpeed = new OlvColumn("Speed:", "SpeedToString")
                            {
                                Sortable = false,
                                IsEditable = false,
                                IsVisible = true,
                                Width = 70
                            };

            _colRemain = new OlvColumn("ETA:", "RemainingToString")
                             {
                                 Sortable = false,
                                 IsEditable = false,
                                 IsVisible = true,
                                 Width = 80
                             };

            _colStatus = new OlvColumn("Status:", "Status")
                             {
                                 Sortable = false,
                                 IsEditable = false,
                                 IsVisible = true,
                                 Width = 90
                             };

            olvFiles.AllColumns.AddRange(new[] {_colFileName, _colUserName, _colProgress, _colSpeed, _colRemain, _colStatus});
            olvFiles.Columns.AddRange(new[] {_colFileName, _colUserName, _colProgress, _colSpeed, _colRemain, _colStatus});
            olvFiles.RebuildColumns();

            olvFiles.CellToolTipShowing += OnCellToolTipShowing;
            olvFiles.MouseUp += OnListMouseUp;

            _popupFile = new ContextMenuStrip();
            BuildPopup();
            _popupFile.Opening += OnPopupOpening;

            /* Set window position and size */
            var w = SettingsManager.GetWindowByName("dcc-manager");
            Size = w.Size;
            Location = w.Position;
            WindowState = w.Maximized ? FormWindowState.Maximized : FormWindowState.Normal;

            olvFiles.SetObjects(_transfers);
            _initialize = false;
        }

        /* Form overrides */
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.FormOwnerClosing)
            {
                /* Ignore close event unless application is closing, otherwise just hide the window */
                Hide();
                e.Cancel = true;
                return;
            }
            base.OnFormClosing(e);
        }

        protected override void OnMove(EventArgs e)
        {
            if (!_initialize)
            {
                var w = SettingsManager.GetWindowByName("dcc-manager");
                if (WindowState == FormWindowState.Normal)
                {
                    w.Position = Location;
                }
            }
            base.OnMove(e);
        }

        protected override void OnResize(EventArgs e)
        {
            if (!_initialize)
            {
                var w = SettingsManager.GetWindowByName("dcc-manager");
                if (WindowState == FormWindowState.Normal)
                {
                    w.Size = Size;
                    w.Position = Location;
                }
                w.Maximized = WindowState == FormWindowState.Maximized;
            }
            base.OnResize(e);
        }

        /* Public exposed method called via static method */
        public void AddTransfer(Dcc file)
        {
            /* Add to DCC file manager list and update this list */
            olvFiles.SetObjects(_transfers);
            olvFiles.SelectedObject = file;
            /* If the form is currently hidden, show it */
            if (!Visible)
            {
                Show(Parent);
            }
        }

        public void UpdateTransferData()
        {
            /* Used to update the list output (if visible) to show progress, speed, etc. changes */
            olvFiles.RefreshObjects(_transfers);
        }

        /* Private methods */
        private void OnListMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var p = new Point(ClientRectangle.Left + e.X, ClientRectangle.Top + e.Y);
                _popupFile.Show(olvFiles, p);
            }            
        }

        private void OnPopupOpening(object sender, EventArgs e)
        {
            _popupFile.Items.Clear();
            BuildPopup();            
        }

        private static void OnCellToolTipShowing(object sender, ToolTipShowingEventArgs e)
        {
            /* Tooltip handler */
            var data = (Dcc) e.Model;
            e.Title = data.FileName;
            e.Text = data.Status == DccFileStatus.Downloading || data.Status == DccFileStatus.Uploading
                         ? string.Format(
                             "DCC type: {0}\r\nUser name: {1}\r\nStatus: {2}\r\nCompleted: {3}%\r\nSpeed: {4}",
                             data.DccFileType, data.UserNameToString, data.Status, data.Progress, data.SpeedToString)
                         : string.Format("DCC type: {0}\r\nUser name: {1}\r\nStatus: {2}", data.DccFileType,
                                         data.UserNameToString, data.Status);
            e.IsBalloon = true;
            e.Handled = true;
        }

        private void OnPopupMenuItemClick(object sender, EventArgs e)
        {
            var t = (ToolStripMenuItem) sender;
            if (t == null)
            {
                return;
            }
            var dcc = (Dcc)olvFiles.SelectedObject;
            switch (t.Name)
            {
                case "OPEN":
                    Functions.OpenProcess(string.Format(@"{0}\{1}", dcc.DccFolder, dcc.FileName));
                    break;

                case "OPENLOCATION":
                    Functions.OpenProcess(dcc.DccFolder);
                    break;

                case "CANCEL":
                    dcc.Disconnect();
                    break;

                case "REMOVE":
                    dcc.Disconnect();
                    _transfers.Remove(dcc);
                    olvFiles.RemoveObject(dcc);
                    break;

                case "RESEND":
                    var add = SettingsManager.Settings.Connection.LocalInfo.HostInfo.Address;
                    var ip = DccManager.IpConvert(add, false);
                    var fs = new FileInfo(string.Format(@"{0}\{1}", dcc.DccFolder, dcc.FileName));
                    var file = dcc.FileName.Replace(" ", "_");
                    dcc.Client.Send(
                        string.Format(
                            "NOTICE {0} :DCC Send {1} ({2}){3}PRIVMSG {0} :\u0001DCC SEND {1} {4} {5} {6}\u0001",
                            dcc.UserName, file, add, Environment.NewLine, ip, dcc.Port, fs.Length));
                    dcc.BeginConnect();
                    break;

                case "CLEAR":
                    for (var i = _transfers.Count - 1; i >= 0; i--)
                    {
                        dcc = _transfers[i];
                        DccManager.RemovePort(dcc.Port);
                        DccManager.RemoveTransfer(dcc);
                    }
                    olvFiles.SetObjects(_transfers);
                    break;
            }
        }

        private void BuildPopup()
        {
            if (olvFiles.SelectedObject != null)
            {
                var dcc = (Dcc) olvFiles.SelectedObject;
                if (dcc.DccFileType == DccFileType.Download)
                {
                    switch (dcc.Status)
                    {
                        case DccFileStatus.Completed:
                            _popupFile.Items.AddRange(new ToolStripItem[]
                                                          {
                                                              new ToolStripMenuItem("Open file", null,
                                                                                    OnPopupMenuItemClick,
                                                                                    "OPEN"),
                                                              new ToolStripMenuItem("Open file location", null,
                                                                                    OnPopupMenuItemClick, "OPENLOCATION"),
                                                              new ToolStripSeparator()
                                                          });
                            break;

                        case DccFileStatus.Waiting:
                        case DccFileStatus.Downloading:
                            _popupFile.Items.Add(new ToolStripMenuItem("Cancel", null, OnPopupMenuItemClick, "CANCEL"));
                            break;
                    }
                }
                else
                {
                    switch (dcc.Status)
                    {
                        case DccFileStatus.Waiting:
                        case DccFileStatus.Uploading:
                            _popupFile.Items.Add(new ToolStripMenuItem("Cancel", null, OnPopupMenuItemClick, "CANCEL"));
                            break;

                        case DccFileStatus.Failed:
                            _popupFile.Items.Add(new ToolStripMenuItem("Resend", null, OnPopupMenuItemClick, "RESEND"));
                            break;
                    }
                }
                _popupFile.Items.Add(new ToolStripMenuItem("Remove", null, OnPopupMenuItemClick, "REMOVE"));
            }
            else
            {
                _popupFile.Items.AddRange(new ToolStripItem[]
                                              {
                                                  new ToolStripMenuItem("Clear", null, OnPopupMenuItemClick, "CLEAR")
                                              });
            }
        }
    }
}
