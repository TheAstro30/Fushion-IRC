/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using ircClient;

namespace ircScript.Classes.ScriptFunctions
{
    public static class Channel
    {
        public static string ParseComChan(ClientConnection client, string nick, int num)
        {
            /* Uses the internal address list (which already has the channels common to yourself) to return channels
             * common to nick */
            var c = client.Ial.Channels(nick);
            if (c == null || c.Count == 0)
            {
                return string.Empty;
            }
            return num == 0 ? c.Count.ToString() : (num > 0 && num <= c.Count ? c[num - 1] : string.Empty);
        }
    }
}
