/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Windows.Forms;

namespace FusionIRC.Controls.SwitchView.Base
{
    public class TreeNodeEx : TreeNode
    {
        /* IRC Treeview Control - Node Extension Class
           By: Jason James Newland
           ©2010-2012 - KangaSoft Software
           All Rights Reserved
         */
        public TreeNodeEx(string key, string text, int imageIndex, int selectedImageIndex) : base(text, imageIndex, selectedImageIndex)
        {
            Name = key;
        }

        public TreeNodeEx(string text) : base(text)
        {
            /* No implementation */
        }

        public TreeNodeEx AddSorted(string key, string text, int imageIndex, int selectedImageIndex)
        {
            TreeNodeEx t;
            for (var i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i].Text.Equals(text, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }
                t = new TreeNodeEx(key, text, imageIndex, selectedImageIndex);
                Nodes.Insert(i, t);
                return t;
            }
            t = new TreeNodeEx(key, text, imageIndex, selectedImageIndex);
            Nodes.Add(t);
            return t;
        }
    }
}
