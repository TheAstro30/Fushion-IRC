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
        private readonly LineNumberStrip _strip;

        public SyntaxHighlight EditBox;
       
        public ScriptEditor()
        {
            EditBox = new SyntaxHighlight
                        {
                            Dock = DockStyle.Fill,
                            BorderStyle = BorderStyle.None,
                            WordWrap = false,
                            ScrollBars = RichTextBoxScrollBars.Both,
                            MaxUndoRedoSteps = 500 /* Should be more than adequate */
                        };

            /* These break up tokens if used between them */
            EditBox.Seperators.AddRange(new[] {'\r', '\n', ' ', ',', '.', '-', '+', '(', ')', '{', '}', '<', '>', '=', '!'});
            /* Highlighted "words" - note: might have to modify this to only highlight first word in some instances */
            EditBox.HighlightDescriptors.AddRange(new[]
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
                                                          new HighlightDescriptor("var", Color.Blue, null,
                                                                                  DescriptorType.Word,
                                                                                  DescriptorRecognition.StartsWith),
                                                          new HighlightDescriptor("inc", Color.Blue, null,
                                                                                  DescriptorType.Word,
                                                                                  DescriptorRecognition.StartsWith),
                                                          new HighlightDescriptor("dec", Color.Blue, null,
                                                                                  DescriptorType.Word,
                                                                                  DescriptorRecognition.StartsWith),
                                                          new HighlightDescriptor("set", Color.Blue, null,
                                                                                  DescriptorType.Word,
                                                                                  DescriptorRecognition.StartsWith),
                                                          new HighlightDescriptor("unset", Color.Blue, null,
                                                                                  DescriptorType.Word,
                                                                                  DescriptorRecognition.StartsWith),
                                                          new HighlightDescriptor("echo", Color.Salmon, null,
                                                                                  DescriptorType.Word,
                                                                                  DescriptorRecognition.StartsWith)
                                                      });

            _strip = new LineNumberStrip(EditBox);
            Controls.AddRange(new Control[] {EditBox, _strip});
            BorderStyle = BorderStyle.Fixed3D;
            BackColor = EditBox.BackColor;            
        }

        /* Text formatting */
        public void Indent()
        {
            /* First get selection start */
            var sel = EditBox.SelectionStart;
            /* Indents text as by { and } */
            var indent = 0;
            var lines = EditBox.Lines;
            for (var i = 0; i < lines.Length; i++)
            {
                var line = EditBox.Lines[i].TrimStart(); /* Remove any leading spacing already */
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
            EditBox.Lines = lines;
            /* Re-set selection start */
            EditBox.SelectionStart = sel;
            EditBox.SelectionLength = 0;
        }
    }
}
