//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE.
//
//  License: GNU Lesser General Public License (LGPLv3)
//
//  Email: pavel_torgashov@ukr.net
//
//  Copyright (C) Pavel Torgashov, 2011-2016.
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ircScript.Controls.SyntaxHighlight.Controls.AutoComplete;
using ircScript.Controls.SyntaxHighlight.Helpers.TextRange;

namespace ircScript.Controls.SyntaxHighlight.Controls
{
    [Browsable(false)]
    public sealed class AutoCompleteMenu : ToolStripDropDown
    {
        public AutoCompleteMenu(FastColoredTextBox tb)
        {
            /* Create a new popup and add the list view to it  */
            AutoClose = false;
            AutoSize = false;
            Margin = Padding.Empty;
            Padding = Padding.Empty;
            BackColor = Color.White;
            ListView = new AutocompleteListView(tb);
            Host = new ToolStripControlHost(ListView)
                       {
                           Margin = new Padding(2, 2, 2, 2),
                           Padding = Padding.Empty,
                           AutoSize = false,
                           AutoToolTip = false
                       };
            CalcSize();
            base.Items.Add(Host);
            ListView.Parent = this;
            SearchPattern = @"[\w\.]";
            MinFragmentLength = 2;
        }

        public AutocompleteListView ListView { get; set; }
        public ToolStripControlHost Host { get; set; }
        public Range Fragment { get; internal set; }
        public string SearchPattern { get; set; }
        public int MinFragmentLength { get; set; }

        public bool AllowTabKey
        {
            get { return ListView.AllowTabKey; }
            set { ListView.AllowTabKey = value; }
        }

        public int AppearInterval
        {
            get { return ListView.AppearInterval; }
            set { ListView.AppearInterval = value; }
        }

        public Size MaxTooltipSize
        {
            get { return ListView.MaxToolTipSize; }
            set { ListView.MaxToolTipSize = value; }
        }

        public bool AlwaysShowTooltip
        {
            get { return ListView.AlwaysShowTooltip; }
            set { ListView.AlwaysShowTooltip = value; }
        }

        [DefaultValue(typeof (Color), "Orange")]
        public Color SelectedColor
        {
            get { return ListView.SelectedColor; }
            set { ListView.SelectedColor = value; }
        }

        [DefaultValue(typeof (Color), "Red")]
        public Color HoveredColor
        {
            get { return ListView.HoveredColor; }
            set { ListView.HoveredColor = value; }
        }

        public new Font Font
        {
            get { return ListView.Font; }
            set { ListView.Font = value; }
        }

        public new AutocompleteListView Items
        {
            get { return ListView; }
        }

        public new Size MinimumSize
        {
            get { return Items.MinimumSize; }
            set { Items.MinimumSize = value; }
        }

        public new ImageList ImageList
        {
            get { return Items.ImageList; }
            set { Items.ImageList = value; }
        }

        public int ToolTipDuration
        {
            get { return Items.ToolTipDuration; }
            set { Items.ToolTipDuration = value; }
        }

        public ToolTip ToolTip
        {
            get { return Items.ToolTip; }
            set { Items.ToolTip = value; }
        }

        public event EventHandler<SelectingEventArgs> Selecting;
        public event EventHandler<SelectedEventArgs> Selected;
        public new event EventHandler<CancelEventArgs> Opening;

        internal new void OnOpening(CancelEventArgs args)
        {
            if (Opening != null)
            {
                Opening(this, args);
            }
        }

        public new void Close()
        {
            ListView.ToolTip.Hide(ListView);
            base.Close();
        }

        internal void CalcSize()
        {
            Host.Size = ListView.Size;
            Size = new Size(ListView.Size.Width + 4, ListView.Size.Height + 4);
        }

        public void OnSelecting()
        {
            ListView.OnSelecting();
        }

        public void SelectNext(int shift)
        {
            ListView.SelectNext(shift);
        }

        internal void OnSelecting(SelectingEventArgs args)
        {
            if (Selecting != null)
            {
                Selecting(this, args);
            }
        }

        public void OnSelected(SelectedEventArgs args)
        {
            if (Selected != null)
            {
                Selected(this, args);
            }
        }

        public void Show(bool forced)
        {
            Items.DoAutocomplete(forced);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (ListView != null && !ListView.IsDisposed)
            {
                ListView.Dispose();
            }
        }
    }   
}