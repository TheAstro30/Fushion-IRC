/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using ircCore.Controls.ChildWindows.Nicklist;
using ircCore.Controls.ChildWindows.OutputDisplay;

namespace ircClient.ChatWindows
{
    public interface IChatWindow
    {
        OutputWindow Output { get; set; }

        Nicklist Nicklist { get; set; }


    }
}
