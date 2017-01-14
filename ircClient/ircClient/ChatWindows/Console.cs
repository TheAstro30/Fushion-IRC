/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Windows.Forms;
using ircCore.Controls.ChildWindows.Nicklist;
using ircCore.Controls.ChildWindows.OutputDisplay;

namespace ircClient.ChatWindows
{
    public class Console : Form, IChatWindow
    {
        public OutputWindow Output { get; set; }

        public Nicklist Nicklist { get; set; }

        public Console()
        {
            Output = new OutputWindow();
            Nicklist = null; /* Nicklist isn't used */
        }
    }
}