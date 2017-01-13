/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Windows.Forms;
using ircCore.Controls.ChildWindows.IrcWindow;
using ircCore.Controls.ChildWindows.Nicklist;

namespace ircClient.ChatWindows
{
    public class Private: Form, IChatWindow
    {
        public OutputWindow Output { get; set; }

        public Nicklist Nicklist { get; set; }

        public Private()
        {
            Output = new OutputWindow();
            Nicklist = null; /* Nicklist isn't used */
        }   
    }
}
