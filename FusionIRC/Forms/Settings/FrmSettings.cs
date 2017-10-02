/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Drawing;
using System.Windows.Forms;
using FusionIRC.Forms.Settings.Controls;
using ircCore.Controls;

namespace FusionIRC.Forms.Settings
{
    public partial class FrmSettings : FormEx
    {
        private readonly SettingsServer _servers;

        public FrmSettings()
        {
            InitializeComponent();

            _servers = new SettingsServer {Location = new Point(168, 12)};

            Controls.AddRange(new Control[] {_servers});
        }
    }
}
