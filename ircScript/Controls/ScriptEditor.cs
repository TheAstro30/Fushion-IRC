/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */

using System;
using System.Drawing;
using System.Windows.Forms;
using ircScript.Controls.SyntaxHightlight;
using ircScript.Controls.SyntaxHightlight.BaseControl;
using ircScript.Controls.SyntaxHightlight.Helpers;

namespace ircScript.Controls
{
    /* This class MUST be the ONLY class initialized in an editing context */

    public sealed class ScriptEditor : UserControl
    {
        private readonly SyntaxHighlight _text;
        private readonly LineNumberStrip _strip;

        /* Only external properties required by this control to the internal RTB/SyntaxHighlight */
        public string[] Lines
        {
            get { return _text.Lines; }
            set { _text.Lines = value; }
        }

        public bool CanUndo
        {
            get { return _text.CanUndo; }
        }

        public bool CanRedo
        {
            get { return _text.CanRedo; }
        }

        public bool CanCopy
        {
            get { return _text.CanCopy; }
        }

        public ScriptEditor()
        {
            _text = new SyntaxHighlight
                        {
                            Dock = DockStyle.Fill,
                            BorderStyle = BorderStyle.None,
                            WordWrap = false,
                            ScrollBars = RichTextBoxScrollBars.Both,
                            MaxUndoRedoSteps = 500 /* Should be more than adequate */
                        };

            /* These break up tokens if used between them */
            _text.Seperators.AddRange(new[] {'\r', '\n', ' ', ',', '.', '-', '+', '(', ')', '{', '}', '<', '>', '=', '!'});
            /* Highlighted "words" - note: might have to modify this to only highlight first word in some instances */
            _text.HighlightDescriptors.AddRange(new[]
                                                    {
                                                        new HighlightDescriptor("/*", "*/", Color.Green, null,
                                                                                DescriptorType.ToCloseToken,
                                                                                DescriptorRecognition.StartsWith),
                                                        new HighlightDescriptor("$", Color.DarkCyan, null,
                                                                                DescriptorType.Word,
                                                                                DescriptorRecognition.StartsWith),
                                                        new HighlightDescriptor("%", Color.Red, null,
                                                                                DescriptorType.Word,
                                                                                DescriptorRecognition.StartsWith),
                                                        new HighlightDescriptor("//", Color.Green, null,
                                                                                DescriptorType.ToEol,
                                                                                DescriptorRecognition.StartsWith),
                                                        new HighlightDescriptor("if", Color.Blue, null,
                                                                                DescriptorType.Word,
                                                                                DescriptorRecognition.WholeWord),
                                                        new HighlightDescriptor("elseif", Color.Blue, null,
                                                                                DescriptorType.Word,
                                                                                DescriptorRecognition.WholeWord),
                                                        new HighlightDescriptor("else", Color.Blue, null,
                                                                                DescriptorType.Word,
                                                                                DescriptorRecognition.WholeWord),
                                                        new HighlightDescriptor("while", Color.Blue, null,
                                                                                DescriptorType.Word,
                                                                                DescriptorRecognition.WholeWord),
                                                        new HighlightDescriptor("alias", Color.Purple, null,
                                                                                DescriptorType.Word,
                                                                                DescriptorRecognition.StartsWith),
                                                        new HighlightDescriptor("on", Color.Purple, null,
                                                                                DescriptorType.Word,
                                                                                DescriptorRecognition.StartsWith),
                                                        new HighlightDescriptor("inc", Color.SkyBlue, null,
                                                                                DescriptorType.Word,
                                                                                DescriptorRecognition.StartsWith),
                                                        new HighlightDescriptor("dec", Color.SkyBlue, null,
                                                                                DescriptorType.Word,
                                                                                DescriptorRecognition.StartsWith),
                                                        new HighlightDescriptor("set", Color.SkyBlue, null,
                                                                                DescriptorType.Word,
                                                                                DescriptorRecognition.StartsWith),
                                                        new HighlightDescriptor("unset", Color.SkyBlue, null,
                                                                                DescriptorType.Word,
                                                                                DescriptorRecognition.StartsWith)
                                                    });          

            _strip = new LineNumberStrip(_text);
            Controls.AddRange(new Control[] {_text, _strip});
            BorderStyle = BorderStyle.Fixed3D;
            BackColor = _text.BackColor;            
        }

        /* Edting methods - because we can't inherit directly from the SyntaxHighlight class
         * (line numbering won't align correctly), we expose some methods to do edit operations which
         * are tied back to the SyntaxHighlight class */
        public void Undo()
        {
            _text.Undo();
        }

        public void Redo()
        {
            _text.Redo();
        }

        public void Cut()
        {
            _text.Cut();
        }

        public void Copy()
        {
            _text.Copy();
        }

        public void Paste()
        {
            _text.Paste();
        }

        /* Text formatting */
        public void Indent()
        {
            /* First get selection start */
            var sel = _text.SelectionStart;
            /* Indents text as by { and } */
            var indent = 0;
            var lines = _text.Lines;
            for (var i = 0; i < lines.Length; i++)
            {
                var line = _text.Lines[i].TrimStart(); /* Remove any leading spacing already */
                if (line.EndsWith("{"))
                {
                    lines[i] = string.Format("{0}{1}", new String(' ', indent*4), line);
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
                lines[i] = string.Format("{0}{1}", new String(' ', indent*4), line);             
            }
            /* Re-set lines in RTB */
            _text.Lines = lines;
            /* Re-set selection start */
            _text.SelectionStart = sel;
            _text.SelectionLength = 0;
        }
    }
}
