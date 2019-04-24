//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE.
//
//  License: GNU Lesser General Public License (LGPLv3)
//
//  Email: pavel_torgashov@ukr.net
//
//  Copyright (C) Pavel Torgashov, 2011-2016.
using System;
using System.ComponentModel;

namespace ircScript.Controls.SyntaxHighlight.Helpers.TypeDescriptors
{
    public class FctbDescriptionProvider : TypeDescriptionProvider
    {
        public FctbDescriptionProvider(Type type) : base(GetDefaultTypeProvider(type))
        {
            /* Empty */
        }

        private static TypeDescriptionProvider GetDefaultTypeProvider(Type type)
        {
            return TypeDescriptor.GetProvider(type);
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            var defaultDescriptor = base.GetTypeDescriptor(objectType, instance);
            return new FctbTypeDescriptor(defaultDescriptor, instance);
        }
    }
}
