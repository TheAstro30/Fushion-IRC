/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using System.Collections.Generic;
using ircCore.Controls.ChildWindows.Input.ColorBox;

namespace ircCore.Controls.ChildWindows.Input
{
    public class InputWindow : UserControl
    {
        /* Input box control
           By: Jason James Newland
           ©2011 - KangaSoft Software
           All Rights Reserved
         */
        private readonly ContextMenuStrip _mnuContext;
        private readonly ColorTextBox _txtOut;

        private int _historyPoint;
        private readonly List<string> _history = new List<string>();

        /* Properties */
        public int MaximumHistoryCache { get; set; }
        public event Action<InputWindow> TabKeyPress;
        public new event Action<InputWindow> TextChanged;

        public bool ConfirmPaste
        {
            get { return _txtOut.ConfirmPaste; }
            set { _txtOut.ConfirmPaste = value; }
        }

        public int ConfirmPasteLines
        {
            get { return _txtOut.ConfirmPasteLines; }
            set { _txtOut.ConfirmPasteLines = value; }
        }

        public new Color BackColor
        {
            get { return base.BackColor; }
            set
            {
                _txtOut.BackColor = value;
                base.BackColor = value;
            }
        }

        public new Color ForeColor
        {
            get { return base.ForeColor; }
            set
            {
                _txtOut.ForeColor = value;
                base.ForeColor = value;
            }
        }

        public new Font Font
        {
            get { return base.Font; }
            set
            {
                if (DesignMode) { return; }
                /* This overcomes drawing 'errors' with using the TextRenderer (on IRC window and nicklist controls)
                  vs DrawString (+ 0.5f) */
                base.Font = new Font(value.Name, value.Size + 0.5f, value.Style);
                _txtOut.Font = base.Font;
                OnResize(new EventArgs());
            }
        }

        public new string Text
        {
            get { return _txtOut.Text; }
            set { _txtOut.Text = value; }
        }

        public int SelectionStart
        {
            get { return _txtOut.SelectionStart; }
            set { _txtOut.SelectionStart = value; }
        }

        public int SelectionLength
        {
            get { return _txtOut.SelectionLength; }
            set { _txtOut.SelectionLength = value; }
        }

        public void ScrollToCaret()
        {
            _txtOut.ScrollToCaret();
        }

        public bool IsMultiLinePaste
        {
            get { return _txtOut.IsMultiLinePaste; }
        }

        public InputWindow()
        {            
            _mnuContext = new ContextMenuStrip();
            _mnuContext.Opening += MnuContextOpening;

            _txtOut = new ColorTextBox
                          {
                              AcceptsTab = true,
                              BorderStyle = BorderStyle.None,
                              ContextMenuStrip = _mnuContext,
                              Font = new Font("Lucida Console", 10),
                              IsMultiLinePaste = false,
                              IsNormalTextbox = false,
                              Location = new Point(1, 1),
                              Multiline = true,
                              ProcessCodes = true,
                              Size = new Size(298, 20),
                              TabIndex = 0,
                              WordWrap = false,
                              AllowMultiLinePaste = true
                          };
            _txtOut.KeyDown += TxtOutKeyDown;
            _txtOut.KeyPress += TxtOutKeyPress;
            _txtOut.MouseWheel += TxtOutMouseWheel;
            _txtOut.TextChanged += TxtOutTextChanged;
            Controls.Add(_txtOut);

            BuildContextMenu();
        }

        protected override void OnResize(EventArgs e)
        {
            Height = GetTextHeight() + 6; /* Extra padding at top and bottom */
            _txtOut.SetBounds(0, 4, Width - 4, Height);
            base.OnResize(e);
        }

        /* Private methods */
        private void MnuContextOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _txtOut.Focus();
            BuildContextMenu();
        }

        private void BuildContextMenu()
        {
            _mnuContext.Items.Clear();
            _mnuContext.Items.AddRange(new ToolStripItem[]
                                           {
                                               new ToolStripMenuItem("Undo", null, ContextMenuOnClick)
                                                   {
                                                       ShortcutKeys = Keys.Control | Keys.Z,
                                                       Enabled = _txtOut.CanUndo,
                                                       Tag = "UNDO"
                                                   },
                                               new ToolStripSeparator(),
                                               new ToolStripMenuItem("Cut", null, ContextMenuOnClick)
                                                   {
                                                       ShortcutKeys = Keys.Control | Keys.X,
                                                       Enabled = _txtOut.SelectedText.Length > 0,
                                                       Tag = "CUT"
                                                   },
                                               new ToolStripMenuItem("Copy", null, ContextMenuOnClick)
                                                   {
                                                       ShortcutKeys = Keys.Control | Keys.C,
                                                       Enabled = _txtOut.SelectedText.Length > 0,
                                                       Tag = "COPY"
                                                   },
                                               new ToolStripMenuItem("Paste", null, ContextMenuOnClick)
                                                   {
                                                       ShortcutKeys = Keys.Control | Keys.V,
                                                       Enabled = Clipboard.GetText().Length > 0,
                                                       Tag = "PASTE"
                                                   },
                                               new ToolStripMenuItem("Delete", null, ContextMenuOnClick)
                                                   {
                                                       Enabled = _txtOut.SelectedText.Length > 0,
                                                       Tag = "DELETE"
                                                   },
                                               new ToolStripSeparator(),
                                               new ToolStripMenuItem("Select ALL", null, ContextMenuOnClick)
                                                   {
                                                       ShortcutKeys = Keys.Control | Keys.A,
                                                       Enabled = !string.IsNullOrEmpty(_txtOut.Text),
                                                       Tag = "ALL"
                                                   }
                                           });           
        }

        private void ContextMenuOnClick(object sender, EventArgs e)
        {
            if (!(sender is ToolStripItem)) { return; }
            var m = (ToolStripMenuItem)sender;
            switch (m.Tag.ToString())
            {
                case "UNDO":
                    _txtOut.Undo();
                    break;

                case "CUT":
                    _txtOut.Cut();
                    break;

                case "COPY":
                    _txtOut.Copy();
                    break;

                case "PASTE":
                    _txtOut.Paste();
                    break;

                case "DELETE":
                    /* Copy contents of clipboard */
                    var strTemp = Clipboard.GetText();
                    /* Remove selected text */
                    _txtOut.Cut();
                    /* Reset clipboard contents */
                    if (!string.IsNullOrEmpty(strTemp)) { Clipboard.SetText(strTemp); }
                    else { Clipboard.Clear(); }
                    break;

                case "ALL":
                    _txtOut.SelectAll();
                    break;
            }
        }

        /* Callback events */
        protected void TxtOutKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyValue)
            {
                case 13:
                    /* Return */
                    if (!string.IsNullOrEmpty(_txtOut.Text))
                    {
                        AddHistoryCache(_txtOut.Text);
                    }
                    else
                    {
                        SystemSounds.Beep.Play();
                    }
                    break;

                case 38:
                    /* Up arrow */
                    if (_historyPoint == _history.Count && !string.IsNullOrEmpty(_txtOut.Text))
                    {
                        /* We are moving up the list but the text box isn't empty - add contents to list */
                        AddHistoryCache(_txtOut.Text);
                        _historyPoint--;
                    }
                    _historyPoint--;
                    if (_historyPoint < 0)
                    {
                        SystemSounds.Beep.Play();
                        _historyPoint = 0;
                    }
                    if (_history.Count > 0)
                    {
                        _txtOut.Text = _history[_historyPoint];
                        if (_txtOut.Text.Length > 0)
                        {
                            _txtOut.SelectionStart = _txtOut.Text.Length + 1;
                        }
                    }
                    break;

                case 40:
                    /* Down arrow */
                    _historyPoint++;
                    if (_historyPoint > _history.Count - 1)
                    {
                        SystemSounds.Beep.Play();
                        if (!string.IsNullOrEmpty(_txtOut.Text))
                        {
                            AddHistoryCache(_txtOut.Text);
                        }
                        _txtOut.Text = string.Empty;
                        _historyPoint = _history.Count;
                        e.Handled = true;
                        return;
                    }
                    if (_historyPoint <= _history.Count - 1)
                    {
                        _txtOut.Text = _history[_historyPoint];
                        if (_txtOut.Text.Length > 0)
                        {
                            _txtOut.SelectionStart = _txtOut.Text.Length + 1;
                        }
                    }
                    break;
            }
            base.OnKeyDown(e);
        }

        private void TxtOutKeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case (char)9:
                    if (TabKeyPress != null) { TabKeyPress(this); }
                    e.Handled = true;
                    break;

                case (char)13:
                    e.Handled = true;
                    break;
            }
        }

        private void TxtOutMouseWheel(object sender, MouseEventArgs e)
        {
            OnMouseWheel(e);
        }

        private void TxtOutTextChanged(object sender, EventArgs e)
        {
            if (TextChanged != null)
            {
                TextChanged(this);
            }
        }

        /* Private helper methods */
        private int GetTextHeight()
        {
            /* Compensates the control height with the font height */
            var g = CreateGraphics();
            return Convert.ToInt32(g.MeasureString("gW", _txtOut.Font).Height);
        }

        private void AddHistoryCache(string text)
        {
            /* Make sure line isn't already in cache */
            foreach (var s in _history.Where(s => s.Equals(text, StringComparison.InvariantCultureIgnoreCase)))
            {
                _history.Remove(s);
                break;
            }
            _history.Add(_txtOut.Text);
            /* First in, first out */
            if (_history.Count > MaximumHistoryCache)
            {
                _history.RemoveAt(0);
            }
            _historyPoint = _history.Count;
        }
    }
}
