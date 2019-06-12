/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using FusionIRC.Helpers;
using FusionIRC.Properties;
using ircCore.Dcc;
using ircCore.Settings;
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
        private readonly OlvColumn _colStatus;

        private readonly BarRenderer _barRenderer;

        private readonly ImageList _imageList;

        private readonly bool _initialize;
        
        /* Constructor - create ObjectListView and columns */
        public FrmDccManager()
        {
            _initialize = true;
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
                                   ImageGetter = row => (int) ((DccFile) row).FileType,
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
                                Width = 60
                            };

            _colStatus = new OlvColumn("Status:", "Status")
                             {
                                 Sortable = false,
                                 IsEditable = false,
                                 IsVisible = true,
                                 Width = 90
                             };

            olvFiles.AllColumns.AddRange(new[] {_colFileName, _colUserName, _colProgress, _colSpeed, _colStatus});
            olvFiles.Columns.AddRange(new[] {_colFileName, _colUserName, _colProgress, _colSpeed, _colStatus});
            olvFiles.RebuildColumns();

            olvFiles.CellToolTipShowing += OnCellToolTipShowing;

            /* Set window position and size */
            var w = SettingsManager.GetWindowByName("dcc-manager");
            Size = w.Size;
            Location = w.Position;
            WindowState = w.Maximized ? FormWindowState.Maximized : FormWindowState.Normal;
            
            LoadFiles();
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
        public void AddFile(DccFile file)
        {
            /* Add to DCC file manager list and update this list */
            DccManager.AddFile(file);
            olvFiles.SetObjects(DccManager.DccTransfers.FileData);
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
            olvFiles.RefreshObjects(DccManager.DccTransfers.FileData);
        }

        /* Private methods */
        private void LoadFiles()
        {
            if (DccManager.DccTransfers.FileData.Count == 0)
            {
                return;
            }
            olvFiles.SetObjects(DccManager.DccTransfers.FileData);
        }

        private static void OnCellToolTipShowing(object sender, ToolTipShowingEventArgs e)
        {
            /* Tooltip handler */
            var data = (DccFile) e.Model;
            e.Title = data.FileName;
            e.Text = data.Status == DccFileStatus.Downloading || data.Status == DccFileStatus.Uploading
                         ? string.Format(
                             "DCC type: {0}\r\nUser name: {1}\r\nStatus: {2}\r\nCompleted: {3}%\r\nSpeed: {4}",
                             data.FileType, data.UserNameToString, data.Status, data.Progress, data.SpeedToString)
                         : string.Format("DCC type: {0}\r\nUser name: {1}\r\nStatus: {2}", data.FileType,
                                         data.UserNameToString, data.Status);
            e.IsBalloon = true;
            e.Handled = true;
        }
    }
}
