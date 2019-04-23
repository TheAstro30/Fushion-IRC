﻿/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ircScript.Controls.SyntaxHighlight;

namespace ircScript.Controls
{
    public sealed class ScriptEditor : FastColoredTextBox
    {
        private readonly Regex _commentPrefix = new Regex(@"//.*$", RegexOptions.Multiline | RegexOptions.Compiled);

        private readonly Regex _keywordPrefix = new Regex(@"\b(alias|if|elseif|else|while|on)\b",
                                                          RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly Regex _commandPrefix = new Regex(@"\b(set|var|inc|dec|unset|echo)\b",
                                                          RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly Style _comment= new TextStyle(Brushes.Green, null, FontStyle.Regular);
        private readonly Style _variableStyle= new TextStyle(Brushes.Red, null, FontStyle.Regular);
        private readonly Style _identifierStyle= new TextStyle(Brushes.DeepPink, null, FontStyle.Regular);
        private readonly Style _keywordsStyle= new TextStyle(Brushes.Blue, null, FontStyle.Regular);
        private readonly Style _commandStyle = new TextStyle(Brushes.BlueViolet, null, FontStyle.Regular);
        private readonly Style _brownStyle = new TextStyle(Brushes.Brown, null, FontStyle.Regular);

        private bool _enableSyntaxHighlight;

        public new event Action<object, EventArgs> TextChanged;

        public bool EnableSyntaxHighlight
        {
            get { return _enableSyntaxHighlight; }
            set
            {
                _enableSyntaxHighlight = value;
                Highlight();
            }
        }

        public ScriptEditor()
        {
            Language = Language.Custom;

            SyntaxHighlighter.CommentStyle = new TextStyle(Brushes.Green, null, FontStyle.Italic);

            Range.tb.CommentPrefix = "//";
            Range.tb.LeftBracket = '(';
            Range.tb.RightBracket = ')';
            Range.tb.LeftBracket2 = '{';
            Range.tb.RightBracket2 = '}';
            Range.tb.BracketsHighlightStrategy = BracketsHighlightStrategy.Strategy2;
            AutoIndent = false;
            Cursor = Cursors.IBeam;
            CommentPrefix = "//";
            HighlightingRangeType = HighlightingRangeType.VisibleRange;
            DelayedEventsInterval = 1000;
            DelayedTextChangedInterval = 500;
            DisabledColor = Color.FromArgb(100, 180, 180, 180);
            SelectionColor = Color.FromArgb(50, 0, 0, 255);
            Paddings = new Padding(0);
            AutoScrollMinSize = new Size(375, 75);
            BackBrush = null;
            FoldingIndicatorColor = Color.Green;
            HighlightFoldingIndicator = true;
            LineNumberColor = Color.Teal;
            SelectionHighlightingForLineBreaksEnabled = true;
            ShowScrollBars = true;

            base.TextChanged += OnTextChanged;
            TextChangedDelayed += OnTextChangedDelayed;
        }

        /* Handlers */
        private void OnTextChanged(object sender, EventArgs e)
        {
            if (TextChanged != null)
            {
                TextChanged(sender, e);
            }
        }

        private void OnTextChangedDelayed(object sender, EventArgs e)
        {
            Highlight();
        }

        /* Text formatting */
        public void Indent()
        {
            /* Custom indent, the control's version doesn't quite work correctly and I can't be bothered fixing it -
             * First get selection start */
            var sel = SelectionStart;
            /* Indents text as by { and } */
            var indent = 0;
            var lines = new List<string>(Lines);
            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i].TrimStart(); /* Remove any leading spacing already */
                if (line.EndsWith("{"))
                {
                    lines[i] = string.Format("{0}{1}", new String(' ', indent * 2), line);
                    indent++;
                    continue;
                }
                if (line.EndsWith("}"))
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
            Text = string.Join(Environment.NewLine, lines);
            /* Re-set selection start */
            SelectionStart = sel;
            SelectionLength = 0;
        }

        /* Private helper method */
        private void Highlight()
        {            
            Range.ClearFoldingMarkers();
            Range.SetFoldingMarkers("{", "}"); /* Allow to collapse brackets block */
            if (!_enableSyntaxHighlight)
            {
                return;
            }
            Range.ClearStyle(_comment, _keywordsStyle, _variableStyle, _identifierStyle, _brownStyle, _commandStyle);
            Range.SetStyle(_keywordsStyle, _keywordPrefix);
            Range.SetStyle(_comment, _commentPrefix);
            Range.SetStyle(_commandStyle, _commandPrefix); 
            foreach (var found in GetRanges(@"%\w+|\$\w+|:(?<range>\w+):|#"))
            {
                switch (found.Text[0])
                {
                    case '$':
                        Range.SetStyle(_identifierStyle, Regex.Escape(found.Text));
                        break;

                    case '%':
                        Range.SetStyle(_variableStyle, found.Text);
                        break;

                    case '#':
                        Range.SetStyle(_brownStyle, found.Text);
                        break;

                    default:
                        /* on *:<x>:*:{ etc */
                        Range.SetStyle(_keywordsStyle, found.Text);
                        break;
                }
            }
        }
    }
}