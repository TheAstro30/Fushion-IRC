using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ircScript.Controls.SyntaxHightlight;
using ircScript.Controls.SyntaxHightlight.BaseControl;
using ircScript.Controls.SyntaxHightlight.Helpers;

namespace ircScript.Controls
{
    /* This class MUST be the ONLY class initialized in an editing context */
    public partial class ScriptEditor : SyntaxHighlight
    {
        public ScriptEditor()
        {
            InitializeComponent();
            
            WordWrap = false;
            ScrollBars = RichTextBoxScrollBars.Both;
            
            FilterAutoComplete = true;

            /* These break up tokens if used between them */
            Seperators.AddRange(new[] { ' ', '\r', '\n', ',', '.', '-', '+' });

            HighlightDescriptors.Add(new HighlightDescriptor("$", Color.Blue, null, DescriptorType.Word,
                                                                  DescriptorRecognition.StartsWith, true));
            HighlightDescriptors.Add(new HighlightDescriptor("%", Color.Red, null, DescriptorType.Word,
                                                                  DescriptorRecognition.StartsWith, false));
            HighlightDescriptors.Add(new HighlightDescriptor(";", Color.Green, null, DescriptorType.ToEol,
                                                             DescriptorRecognition.StartsWith, false));
        }
    }
}
