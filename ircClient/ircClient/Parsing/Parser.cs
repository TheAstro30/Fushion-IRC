/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ircClient.Tcp;

namespace ircClient.Parsing
{
    public class Parser
    {
        /* This class does most of the work of parsing IRC messages */
        public ClientSock Sock = new ClientSock();
    }
}
