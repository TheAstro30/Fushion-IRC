/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ircClient.Tcp
{
    public enum DccType
    {
        DccFileTransfer = 0,
        DccChat = 1
    }

    public class Dcc
    {
        /* This class handles both direct connection communication plus file transfers */
        private ClientSock _sock;
        private ISynchronizeInvoke _sync;

        public DccType DccType { get; set; }
        public int FileProgress { get; private set; }
        public string FileName { get; set; }

        public Dcc(ISynchronizeInvoke syncObject)
        {
            _sync = syncObject;
            _sock = new ClientSock(_sync);
        }
    }
}
