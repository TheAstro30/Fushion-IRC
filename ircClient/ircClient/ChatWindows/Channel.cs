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
    /* Not sure I'm using this...*/
    public class Channel: Form, IChatWindow
    {
        public OutputWindow Output { get; set; }

        public Nicklist Nicklist { get; set; }

        public Channel()
        {
            Output = new OutputWindow();
            Nicklist = new Nicklist();
        }    
    }
}
