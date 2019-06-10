/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.ComponentModel;
using ircCore.Dcc;

namespace ircClient.Tcp
{
    public enum DccType
    {
        DccChat = 0,
        DccFileTransfer = 1        
    }

    public class Dcc
    {
        /* This class handles both direct connection communication plus file transfers */
        private ClientSock _sock;
        private readonly ISynchronizeInvoke _sync;

        public DccType DccType { get; set; }
        public DccFile DccFile { get; set; }

        public Dcc(ISynchronizeInvoke syncObject)
        {
            _sync = syncObject;
            _sock = new ClientSock(_sync);
            /* Penis */
        }
    }
}
