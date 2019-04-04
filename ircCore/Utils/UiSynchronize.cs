/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.ComponentModel;

namespace ircCore.Utils
{
    public class UiSynchronize
    {
        /* This class eliminates the need for delegates when calling InvokeRequired/BeginInvoke invocation on UI objects */
        private readonly ISynchronizeInvoke _sync;

        public UiSynchronize(ISynchronizeInvoke sync)
        {
            _sync = sync;
        }

        public void Execute(Action action)
        {
            if (_sync == null)
            {
                /* It shouldn't be null, as the constructor forces us to use a synchronous object */
                return;
            }
            _sync.BeginInvoke(action, null);
        }
    }
}
