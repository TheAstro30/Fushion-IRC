/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ircCore.Controls.ChildWindows.Helpers;
using ircCore.Utils;
using ircScript.Controls.SyntaxHighlight;
using ircScript.Controls.SyntaxHighlight.Highlight;
using ircScript.Controls.SyntaxHighlight.Styles;

namespace ircScript.Controls
{
    public class ScriptEditor : FastColoredTextBox
    {
        private readonly Regex _commentPrefix = new Regex(@"//.*$", RegexOptions.Multiline | RegexOptions.Compiled);

        private readonly Regex _keywordPrefix = new Regex(@"\b(alias|if|elseif|else|while|on|return|break|halt|isnull|isnum)\b",
                                                          RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly Regex _identifierPrefix =
            new Regex(
                @"\$input|\$\+|\$me|\$chan|\$nick|\$active|\$gettok|" +
                @"\$addtok|\$deltok|\$cid|\$asctime|\$ctime|\$duration|" +
                @"\$calc|\$iif|\$encode|\$decode|\$appdir|\$chr|\$asc|\$readini|" +
                @"\$read|\$server|\$network|\$address|\$comchan|" +
                @"\$len|\$left|\$mid|\$right|\$upper|\$lower");

        private readonly Regex _commandPrefix = new Regex(@"\b(set|var|inc|dec|unset|echo|writeini|write)\b",
                                                          RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly Regex _ircCommandPrefix = new Regex(@"\b(me|say|amsg|ame|msg|join|part|quit|nick|kick|mode|ignore)\b");

        private static readonly SolidBrush CommentBrush = new SolidBrush(Color.Green);
        private static readonly SolidBrush KeywordBrush = new SolidBrush(Color.Blue);
        private static readonly SolidBrush CustomIdentifierBrush = new SolidBrush(Color.DeepPink);
        private static readonly SolidBrush IdentifierBrush = new SolidBrush(Color.DarkCyan);
        private static readonly SolidBrush MiscBrush = new SolidBrush(Color.Brown);
        private static readonly SolidBrush CommandBrush = new SolidBrush(Color.BlueViolet);
        private static readonly SolidBrush VariableBrush = new SolidBrush(Color.Red);

        private readonly Style _comment= new TextStyle(CommentBrush, null, FontStyle.Regular);
        private readonly Style _variableStyle= new TextStyle(VariableBrush, null, FontStyle.Regular);
        private readonly Style _identifierStyle = new TextStyle(IdentifierBrush, null, FontStyle.Regular);
        private readonly Style _customIdentifierStyle= new TextStyle(CustomIdentifierBrush, null, FontStyle.Regular);
        private readonly Style _keywordsStyle= new TextStyle(KeywordBrush, null, FontStyle.Regular);
        private readonly Style _commandStyle = new TextStyle(CommandBrush, null, FontStyle.Regular);
        private readonly Style _miscStyle = new TextStyle(MiscBrush, null, FontStyle.Regular);

        private readonly Timer _unlockMouse;
        private bool _lockMouse;

        private bool _enableSyntaxHighlight;

        public new event Action<object, EventArgs> TextChanged;

        public bool EnableSyntaxHighlight
        {
            get { return _enableSyntaxHighlight; }
            set
            {
                _enableSyntaxHighlight = value;
                switch (value)
                {
                    case true:
                        SetStyles();
                        break;

                    case false:
                        ClearStyles();
                        break;
                }
            }
        }

        public Color CommentColor
        {
            get { return CommentBrush.Color; }
            set { CommentBrush.Color = value; }
        }

        public Color CommandColor
        {
            get { return CommandBrush.Color; }
            set { CommandBrush.Color = value; }
        }

        public Color VariableColor
        {
            get { return VariableBrush.Color; }
            set { VariableBrush.Color = value; }
        }

        public Color IdentifierColor
        {
            get { return IdentifierBrush.Color; }
            set { IdentifierBrush.Color = value; }
        }

        public Color CustomIdentifierColor
        {
            get { return CustomIdentifierBrush.Color; }
            set { CustomIdentifierBrush.Color = value; }
        }

        public Color KeywordColor
        {
            get { return KeywordBrush.Color; }
            set { KeywordBrush.Color = value; }
        }

        public Color MiscColor
        {
            get { return MiscBrush.Color; }
            set { MiscBrush.Color = value; }
        }

        public ScriptEditor()
        {
            Language = Language.Custom;
            Range.TextBox.CommentPrefix = "//";
            HighlightingRangeType = HighlightingRangeType.VisibleRange;
            DelayedEventsInterval = 500;
            DelayedTextChangedInterval = 200;
            ShowScrollBars = true;
            AutoIndent = false;
            TabLength = 2;
            base.TextChanged += OnTextChanged;
            TextChangedDelayed += OnTextChangedDelayed;
            _unlockMouse = new Timer {Interval = 1};
            _unlockMouse.Tick += TimerSelection;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            /* Over comes an issue with having the color selection dialog open, clicking a color, and this stupid window
             * having a selection range... */
            if (_lockMouse)
            {
                return;
            }
            base.OnMouseMove(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.B:
                        InsertChar((char) ControlByte.Bold);
                        return;

                    case Keys.I:
                        InsertChar((char) ControlByte.Italic);
                        return;

                    case Keys.U:
                        InsertChar((char) ControlByte.Underline);
                        return;

                    case Keys.R:
                        InsertChar((char) ControlByte.Reverse);
                        return;

                    case Keys.O:
                        InsertChar((char) ControlByte.Normal);
                        return;

                    case Keys.K:
                        _lockMouse = true;
                        InsertChar((char) ControlByte.Color);
                        Functions.ShowColorIndexBox(this, SelectionStart).SelectedIndexChanged += ColorIndexSelection;
                        return;
                }
            }
            Functions.DestroyColorIndexBox();
            _lockMouse = false;
            base.OnKeyDown(e);
        }

        /* Handlers */
        private void ColorIndexSelection(string color)
        {    
            var c = color.ToCharArray();
            foreach (var ch in c)
            {
                InsertChar(ch);
            }          
            _unlockMouse.Enabled = true;
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            if (TextChanged != null)
            {
                TextChanged(sender, e);
            }
        }

        private void OnTextChangedDelayed(object sender, EventArgs e)
        {
            SetStyles();
        }

        /* Text formatting - Indents text as by { and } */
        public void Indent()
        {
            /* Custom indent, the control's version doesn't quite work correctly and I can't be bothered fixing it */            
            var indent = 0;
            var lines = new List<string>(Lines);
            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i].TrimStart(); /* Remove any leading spacing already */
                if (!line.StartsWith("//") && line.EndsWith("{"))
                {
                    lines[i] = string.Format("{0}{1}", new String(' ', indent * 2), line);
                    indent++;
                    continue;
                }
                if (line.Length > 0 && line.Trim()[0] == '}')
                {
                    indent--;
                }
                if (indent < 0)
                {
                    indent = 0;
                }
                lines[i] = string.Format("{0}{1}", new String(' ', indent * 2), line);
            }
            /* Re-set lines in RTB */
            TextSource.CurrentTextBox.Clear();
            TextSource.CurrentTextBox.Text = string.Join(Environment.NewLine, lines);
        }

        public void SetStyles()
        {                        
            if (!_enableSyntaxHighlight)
            {
                return;
            }
            ClearStyles();
            Range.SetStyle(_keywordsStyle, _keywordPrefix);
            Range.SetStyle(_comment, _commentPrefix);
            Range.SetStyle(_commandStyle, _commandPrefix);
            Range.SetStyle(_identifierStyle, _identifierPrefix);
            Range.SetStyle(_miscStyle, _ircCommandPrefix);
            foreach (var found in GetRanges(@"%\w+|\$\w+|\$(?<range>)\w+[\(^]|(?<range>\w+):|#"))
            {
                switch (found.Text[0])
                {
                    case '$':
                        Range.SetStyle(_customIdentifierStyle, Regex.Escape(found.Text));
                        break;

                    case '%':
                        Range.SetStyle(_variableStyle, found.Text);
                        break;

                    case '#':
                        Range.SetStyle(_miscStyle, found.Text);
                        break;

                    default:
                        /* on <x>:*:{ etc */
                        Range.SetStyle(_keywordsStyle, found.Text);
                        break;
                }
            }            
        }

        private void ClearStyles()
        {
            Range.ClearFoldingMarkers();
            Range.SetFoldingMarkers("{", "}"); /* Allow to collapse brackets block */
            Range.ClearStyle(_comment, _keywordsStyle,
                             _variableStyle, _customIdentifierStyle,
                             _identifierStyle, _miscStyle, _commandStyle);
        }

        private void TimerSelection(object sender, EventArgs e)
        {
            _unlockMouse.Enabled = false; 
            _lockMouse = false;
        }
    }
}