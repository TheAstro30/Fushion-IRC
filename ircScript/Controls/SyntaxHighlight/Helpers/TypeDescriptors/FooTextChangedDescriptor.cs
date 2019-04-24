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
    class FooTextChangedDescriptor : EventDescriptor
    {
        public FooTextChangedDescriptor(MemberDescriptor desc) : base(desc)
        {
            /* Empty */
        }

        public override void AddEventHandler(object component, Delegate value)
        {
            ((FastColoredTextBox) component).BindingTextChanged += value as EventHandler;
        }

        public override Type ComponentType
        {
            get { return typeof(FastColoredTextBox); }
        }

        public override Type EventType
        {
            get { return typeof(EventHandler); }
        }

        public override bool IsMulticast
        {
            get { return true; }
        }

        public override void RemoveEventHandler(object component, Delegate value)
        {
            var fastColoredTextBox = component as FastColoredTextBox;
            if (fastColoredTextBox != null)
            {
                fastColoredTextBox.BindingTextChanged -= value as EventHandler;
            }
        }
    }
}
