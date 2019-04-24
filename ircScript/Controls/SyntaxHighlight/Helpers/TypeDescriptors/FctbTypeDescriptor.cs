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
using System.ComponentModel;
using System.Windows.Forms;

namespace ircScript.Controls.SyntaxHighlight.Helpers.TypeDescriptors
{
    public class FctbTypeDescriptor : CustomTypeDescriptor
    {
        private readonly object _instance;

        public FctbTypeDescriptor(ICustomTypeDescriptor parent, object instance) : base(parent)
        {
            _instance = instance;
        }

        public override string GetComponentName()
        {
            var ctrl = (_instance as Control);
            return ctrl == null ? null : ctrl.Name;
        }

        public override EventDescriptorCollection GetEvents()
        {
            var coll = base.GetEvents();
            var list = new EventDescriptor[coll.Count];
            for (var i = 0; i < coll.Count; i++)
            {
                if (coll[i].Name == "TextChanged")//instead of TextChanged slip BindingTextChanged for binding
                {
                    list[i] = new FooTextChangedDescriptor(coll[i]);
                }
                else
                {
                    list[i] = coll[i];
                }
            }
            return new EventDescriptorCollection(list);
        }
    }
}
