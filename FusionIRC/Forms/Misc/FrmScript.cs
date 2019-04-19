/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using ircCore.Settings;
using ircCore.Utils;
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

            /* Menubar */
            menu.Items.Add(new ToolStripButton("{ }", null, MenuButtonClick)
                                     {
                                         Alignment = ToolStripItemAlignment.Right
                                     });
            mnuEdit.DropDownItems.AddRange(new ToolStripItem[]
                                               {
                                                   new ToolStripMenuItem("Undo", null, MenuItemOnClick, Keys.Control | Keys.Z),
                                                   new ToolStripMenuItem("Redo", null, MenuItemOnClick, Keys.Control | Keys.Y),
                                                   new ToolStripSeparator(), 
                                                   new ToolStripMenuItem("Cut", null, MenuItemOnClick, Keys.Control | Keys.X),
                                                   new ToolStripMenuItem("Copy", null, MenuItemOnClick, Keys.Control | Keys.C),
                                                   new ToolStripMenuItem("Paste", null, MenuItemOnClick, Keys.Control | Keys.V),
                                                   new ToolStripMenuItem("Delete", null, MenuItemOnClick, Keys.None)
                                               });
            mnuEdit.DropDownOpening += MenuDropDownOpening;

            btnClose.Click += ButtonClickHandler;

            _initialize = false;
        }

        /* Overrides */
        protected override void OnLoad(EventArgs e)
        {
            /* Import alias data to text box - currently uses first file */
            //txtEdit.SetBuffer(string.Join(Environment.NewLine, ScriptManager.Aliases.AliasData));  
            txtEdit.Lines = ScriptManager.AliasData[0].RawScriptData.Select(data => data.ToString()).ToArray(); //ScriptManager.Aliases.AliasData.Select(data => data.ToString()).ToArray();
            txtEdit.Indent();
            base.OnLoad(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            /* This will change as stuff progresses, currently we only have an "OK" button */
            /* Save alias data */
            //ScriptManager.Aliases.AliasData.Clear();
            ScriptManager.AliasData[0].RawScriptData = new List<string>(txtEdit.Lines);
            //foreach (var line in txtEdit.Lines)
            //{
            //    //ScriptManager.AddScript(ScriptType.Alias, line.TrimStart());
            //}
            //ScriptManager.SaveScript(ScriptType.Alias, @"scripts\aliases.xml");
            ScriptManager.SaveScript(ScriptManager.AliasData[0], Functions.MainDir(@"scripts\aliases.xml", false));
            /* Build script data */
            ScriptManager.BuildScripts(ScriptManager.AliasData, ScriptManager.Aliases);
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
            base.OnResize(e);
        }

        /* Handler callbacks */
        private void MenuDropDownOpening(object sender,EventArgs e)
        {
            var dd = (ToolStripMenuItem) sender;
            if (dd == null)
            {
                return;
            }
            switch (dd.Text.ToUpper())
            {
                case "&EDIT":
                    /* Check ability to undo/redo */
                    mnuEdit.DropDownItems[0].Enabled = txtEdit.CanUndo;
                    mnuEdit.DropDownItems[1].Enabled = txtEdit.CanRedo;
                    mnuEdit.DropDownItems[3].Enabled = txtEdit.CanCopy;
                    mnuEdit.DropDownItems[4].Enabled = txtEdit.CanCopy;
                    mnuEdit.DropDownItems[5].Enabled = Clipboard.ContainsText();
                    mnuEdit.DropDownItems[6].Enabled = txtEdit.CanCopy;
                    break;
            }
        }

        private void MenuButtonClick(object sender, EventArgs e)
        {
            var btn = (ToolStripButton) sender;
            if (btn == null)
            {
                return;
            }
            /* Reformat text */
            txtEdit.Indent();
        }

        private void MenuItemOnClick(object sender, EventArgs e)
        {
            var di = (ToolStripMenuItem) sender;
            if (di == null)
            {
                return;
            }
            switch (di.Text.ToUpper())
            {
                case "UNDO":
                    txtEdit.Undo();
                    break;

                case "REDO":
                    txtEdit.Redo();
                    break;

                case "CUT":
                    txtEdit.Cut();
                    break;

                case "COPY":
                    txtEdit.Copy();
                    break;

                case "PASTE":
                    txtEdit.Paste();
                    break;

                case "DELETE":
                    /* Copy contents of clipboard */
                    var clipText = Clipboard.GetText();
                    /* Remove selected text */
                    txtEdit.Cut();
                    /* Reset clipboard contents */
                    if (!string.IsNullOrEmpty(clipText))
                    {
                        Clipboard.SetText(clipText);
                    }
                    else
                    {
                        Clipboard.Clear();
                    }
                    break;
            }
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
                case "CLOSE":
                    Close();
                    break;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            
            base.OnKeyDown(e);
        }
    }
}
