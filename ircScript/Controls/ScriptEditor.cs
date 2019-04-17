using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UrielGuy.SyntaxHighlightingTextBox;

namespace ircScript.Controls
{
    /* This class MUST be the ONLY class initialized in an editing context */
    public partial class ScriptEditor : SyntaxHighlightingTextBox
    {
        public ScriptEditor()
        {
            InitializeComponent();
        }
    }
}
