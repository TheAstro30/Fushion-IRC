/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Collections.Generic;
using System.Linq;

namespace ircClient.Parsing.Helpers
{
    public class ChannelTypes
    {
        /* Helper class for storing and retreiving channel prefixes -
         * it doesn't matter if the connecting server DOESN'T send RAW 005 (protocols), or
         * even if the CHANTYPES= tag is not in the protocols, this class will use the default
         * channel prefix '#' when using MatchChannelType() */
        private readonly List<char> _knownChannelTypes;

        public ChannelTypes()
        {
            _knownChannelTypes = new List<char>();
        }

        public ChannelTypes(IEnumerable<char> types)
        {
            _knownChannelTypes = new List<char>(types);
        }

        public char MatchChannelType(char c)
        {
            foreach (var t in _knownChannelTypes.Where(t => t == c))
            {
                return t;
            }
            return '#'; /* Default channel prefix */
        }
    }
}
