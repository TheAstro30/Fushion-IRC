/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Windows.Forms;
using ircCore.Settings;
using ircScript;

namespace FusionIRC.Forms.Misc
{
    public partial class FrmScript : Form
    {
        private readonly bool _initialize;

        public FrmScript()
        {
            _initialize = true;
            InitializeComponent();

            /* Set window position and size */
            var w = SettingsManager.GetWindowByName("editor");
            Size = w.Size;
            Location = w.Position;
            WindowState = w.Maximized ? FormWindowState.Maximized : FormWindowState.Normal;

            btnClose.Click += ButtonClickHandler;

            _initialize = false;
        }

        /* Overrides */
        protected override void OnLoad(EventArgs e)
        {
            /* Import alias data to text box */
            txtEdit.SetBuffer(string.Join(Environment.NewLine, ScriptManager.Aliases.AliasData));         
            txtEdit.MaxUndoRedoSteps = 500;
            base.OnLoad(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            /* This will change as stuff progresses, currently we only have an "OK" button */
            /* Save alias data */
            ScriptManager.Aliases.AliasData.Clear();
            foreach (var line in txtEdit.Lines)
            {
                ScriptManager.AddScript(ScriptType.Alias, line);
            }
            ScriptManager.SaveScript(ScriptType.Alias, @"scripts\aliases.xml");
            base.OnFormClosing(e);
        }

        protected override void OnMove(EventArgs e)
        {
            if (!_initialize)
            {
                var w = SettingsManager.GetWindowByName("editor");
                if (WindowState != FormWindowState.Maximized)
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
                var w = SettingsManager.GetWindowByName("editor");
                if (WindowState != FormWindowState.Maximized)
                {
                    w.Size = Size;
                    w.Position = Location;
                }
                w.Maximized = WindowState == FormWindowState.Maximized;
            }
            /* Move controls */
            txtEdit.SetBounds(12, 12, ClientRectangle.Width - 24, ClientRectangle.Height - 55);
            btnClose.SetBounds(ClientRectangle.Width - btnClose.Width - 12,
                               ClientRectangle.Height - btnClose.Height - 12, btnClose.Width, btnClose.Height);                        
            base.OnResize(e);
        }

        /* Handler callbacks */
        private void ButtonClickHandler(object sender, EventArgs e)
        {
            var btn = (Button) sender;
            if (btn == null)
            {
                return;
            }
            switch (btn.Text.ToUpper())
            {
                case "CLOSE":
                    Close();
                    break;
            }
        }
    }
}
