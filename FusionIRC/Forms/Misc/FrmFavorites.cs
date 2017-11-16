/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ircCore.Controls;
using ircCore.Settings.Channels;

namespace FusionIRC.Forms.Misc
{
    public partial class FrmFavorites : FormEx
    {
        private readonly ImageList _imageList;

        public FrmFavorites()
        {
            InitializeComponent();

            _imageList = new ImageList {ColorDepth = ColorDepth.Depth32Bit, ImageSize = new Size(16, 16)};
            _imageList.Images.Add(Properties.Resources.channel);

            colChan.ImageGetter = delegate { return 0; };

            lvFave.SmallImageList = _imageList;

            lvFave.AddObjects(ChannelManager.Channels.Favorites.Favorite);

            btnClear.Enabled = lvFave.GetItemCount() > 0;
        }
    }
}
