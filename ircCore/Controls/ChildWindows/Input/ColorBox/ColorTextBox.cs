/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using ircCore.Controls.ChildWindows.Helpers;
using ircCore.Forms;

namespace ircCore.Controls.ChildWindows.Input.ColorBox
{
    public class ColorTextBox : TextBox
    {
        /* Control codes textbox class
           By: Jason James Newland
           ©2010 - 2011 - KangaSoft Software
           All Rights Reserved
         */
        private FrmColorIndex _frmCol;
        private bool _isItalic;

        protected const int WmPaste = 0x302;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetCaretPos(ref Point lpPoint);

        public ColorTextBox()
        {
            ProcessCodes = true;
        }

        public bool ProcessCodes { get; set; }
        public bool AllowMultiLinePaste { get; set; }
        public bool IsMultiLinePaste { get; set; }
        public bool IsNormalTextbox { get; set; }

        protected override void WndProc(ref Message m)
        {
            if (!IsNormalTextbox)
            {
                if (m.Msg == WmPaste)
                {
                    ProcessPaste();
                    return;
                }
            }
            base.WndProc(ref m);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (!IsNormalTextbox && keyData == (Keys.Control | Keys.V))
            {
                ProcessPaste();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.A:
                        SelectAll();
                        break;

                    case Keys.K:
                        if (ProcessCodes)
                        {
                            /* Color */
                            InsertChar(((char) ControlByte.Color).ToString());
                            if (_frmCol == null)
                            {
                                var pt = Point.Empty;
                                /* Init new color form */
                                _frmCol = new FrmColorIndex { Box = this, Start = SelectionStart };
                                /* Get cursor position */
                                GetCaretPos(ref pt);
                                var x = PointToScreen(pt).X - (_frmCol.Size.Width / 2);
                                var y = PointToScreen(pt).Y - ((_frmCol.Height - _frmCol.ClientRectangle.Height) - SystemInformation.CaptionHeight);
                                /* Check form isn't off the screen */
                                var i = Screen.PrimaryScreen.Bounds.Width - _frmCol.Size.Width;
                                if (x < 0) { x = 0; }
                                if (x > i) { x = i; }
                                /* Set bounds */
                                _frmCol.SetBounds(x, y - _frmCol.Size.Height, _frmCol.Size.Width, _frmCol.Size.Height);
                                /* Show and set focus back to this textbox */
                                _frmCol.Show(Parent);
                                Focus();
                            }
                            return;
                        }
                        break;

                    case Keys.B:
                        if (ProcessCodes)
                        {
                            InsertChar(((char) ControlByte.Bold).ToString());
                            return;
                        }
                        break;

                    case Keys.U:
                        if (ProcessCodes)
                        {
                            InsertChar(((char) ControlByte.Underline).ToString());
                            return;
                        }
                        break;

                    case Keys.R:
                        if (ProcessCodes)
                        {
                            InsertChar(((char) ControlByte.Reverse).ToString());
                            return;
                        }
                        break;

                    case Keys.I:
                        if (ProcessCodes)
                        {
                            _isItalic = true;
                            InsertChar(((char) ControlByte.Italic).ToString());
                            return;
                        }
                        break;

                    case Keys.O:
                        if (ProcessCodes)
                        {
                            InsertChar(((char) ControlByte.Normal).ToString());
                            return;
                        }
                        break;
                }
            }
            if (_frmCol != null)
            {
                _frmCol.Close();
                _frmCol.Dispose();
                _frmCol = null;
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (!ProcessCodes)
            {
                return;
            }
            switch (e.KeyChar)
            {
                case (char) 1:
                case (char) ControlByte.Bold:
                case (char) 10:
                case (char) 11:
                case (char) 15:
                case (char) 18:
                case (char) 21:
                    /* Stops the default beeping */
                    e.Handled = true;
                    break;

                case (char) 13:
                    if (Multiline)
                    {
                        base.OnKeyPress(e);
                        return;
                    }
                    e.Handled = true;
                    break;

                case (char) 9:
                    if (_isItalic)
                    {
                        _isItalic = false;
                    }
                    else
                    {
                        base.OnKeyPress(e);
                    }
                    e.Handled = true;
                    break;

                default:
                    base.OnKeyPress(e);
                    break;
            }
        }

        private void InsertChar(string c)
        {
            if (Text.Length > 0)
            {
                var iStart = SelectionStart;
                Text = Text.Substring(0, iStart) + c + Text.Substring(iStart);
                SelectionStart = iStart + 1;
            }
            else
            {
                Text = c;
                SelectionStart = Text.Length;
            }
            ScrollToCaret();
        }

        private void ProcessPaste()
        {
            /* Handle Ctrl+V directly here */
            var strTemp = InternalClipboard.GetText();
            if (string.IsNullOrEmpty(strTemp))
            {
                return;
            }
            var sTmp = strTemp.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
            int start;
            if (AllowMultiLinePaste && sTmp.Length > 1)
            {
                //var lines = IrcMain.Settings.ConfirmPasteLines;
                //if (IrcMain.Settings.GetOption(IrcMain.Settings.ConfirmOptions, "CONFIRMPASTE") && sTmp.Length >= lines)
                //{
                //    if (MessageBox.Show(string.Format(@"You are about to paste more than {0} of text.{1}{1}Do you wish to paste anyway?", lines, Environment.NewLine), @"dIRC 7 Confirm Paste", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.No)
                //    {
                //        return;
                //    }
                //}
                IsMultiLinePaste = true;
                foreach (var s in sTmp)
                {
                    start = SelectionStart;
                    Text = Text.Substring(0, start) + s + Text.Substring(start);
                    var eKey = new KeyEventArgs(Keys.Return);
                    OnKeyDown(eKey);
                }
                IsMultiLinePaste = false;
            }
            else
            {
                start = SelectionStart;
                Text = Text.Substring(0, start) + sTmp[0] + Text.Substring(start);
                SelectionStart = start + sTmp[0].Length;
            }
        }
    }
}
