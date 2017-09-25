/* Object List View
 * Copyright (C) 2006-2012 Phillip Piper
 * Refactored by Jason James Newland - 2014
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * If you wish to use this code in a closed source application, please contact phillip_piper@bigfoot.com.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Threading.Timer;

namespace libolv.Rendering.Renderers
{
    public class ImageRenderer : BaseRenderer
    {
        internal class AnimationState
        {
            private const int PropertyTagFrameDelay = 0x5100;

            internal int CurrentFrame;
            internal long CurrentFrameExpiresAt;
            internal Image Image;
            internal List<int> ImageDuration;
            internal int FrameCount;

            public AnimationState()
            {
                ImageDuration = new List<int>();
            }

            public AnimationState(Image image) : this()
            {
                if (!IsAnimation(image))
                {
                    return;
                }
                /* How many frames in the animation? */
                Image = image;
                FrameCount = Image.GetFrameCount(FrameDimension.Time);
                /* Find the delay between each frame.
                 * The delays are stored an array of 4-byte ints. Each int is the
                 * number of 1/100th of a second that should elapsed before the frame expires */
                foreach (var pi in Image.PropertyItems.Where(pi => pi.Id == PropertyTagFrameDelay))
                {
                    for (var i = 0; i < pi.Len; i += 4)
                    {
                        /* TODO: There must be a better way to convert 4-bytes to an int */
                        var delay = (pi.Value[i + 3] << 24) + (pi.Value[i + 2] << 16) + (pi.Value[i + 1] << 8) +
                                    pi.Value[i];
                        ImageDuration.Add(delay * 10); /* store delays as milliseconds */
                    }
                    break;
                }
                /* There should be as many frame durations as frames */
                Debug.Assert(ImageDuration.Count == FrameCount, "There should be as many frame durations as there are frames.");
            }

            public static bool IsAnimation(Image image)
            {
                return image != null && (new List<Guid>(image.FrameDimensionsList)).Contains(FrameDimension.Time.Guid);
            }

            public bool IsValid
            {
                get { return (Image != null && FrameCount > 0); }
            }

            public void AdvanceFrame(long millisecondsNow)
            {
                CurrentFrame = (CurrentFrame + 1) % FrameCount;
                CurrentFrameExpiresAt = millisecondsNow + ImageDuration[CurrentFrame];
                Image.SelectActiveFrame(FrameDimension.Time, CurrentFrame);
            }
        }

        private Timer _tickler; /* timer used to tickle the animations */
        private readonly Stopwatch _stopwatch; /* clock used to time the animation frame changes */
        private bool _isPaused = true;

        public ImageRenderer()
        {
            _stopwatch = new Stopwatch();
        }

        public ImageRenderer(bool startAnimations) : this()
        {
            Paused = !startAnimations;
        }

        protected override void Dispose(bool disposing)
        {
            Paused = true;
            base.Dispose(disposing);
        }

        /* Properties */
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Paused
        {
            get { return _isPaused; }
            set
            {
                if (_isPaused == value)
                {
                    return;
                }
                _isPaused = value;
                if (_isPaused)
                {
                    StopTickler();
                    _stopwatch.Stop();
                }
                else
                {
                    Tickler.Change(1, Timeout.Infinite);
                    _stopwatch.Start();
                }
            }
        }
        
        protected Timer Tickler
        {
            get
            {
                return _tickler ?? (_tickler = new Timer(OnTimer, null, Timeout.Infinite, Timeout.Infinite));
            }
        }

        /* Commands */
        private void StopTickler()
        {
            Tickler.Change(Timeout.Infinite, Timeout.Infinite);
            Tickler.Dispose();
            _tickler = null;
        }

        public void Pause()
        {
            Paused = true;
        }

        public void Unpause()
        {
            Paused = false;
        }

        /* Drawing */
        public override void Render(Graphics g, Rectangle r)
        {
            DrawBackground(g, r);
            if (Aspect == null || Aspect == DBNull.Value)
            {
                return;
            }
            r = ApplyCellPadding(r);
            if (Aspect is Byte[])
            {
                DrawAlignedImage(g, r, GetImageFromAspect());
            }
            else
            {
                var imageSelectors = Aspect as ICollection;
                if (imageSelectors == null)
                {
                    DrawAlignedImage(g, r, GetImageFromAspect());
                }
                else
                {
                    DrawImages(g, r, imageSelectors);
                }
            }
        }

        protected Image GetImageFromAspect()
        {
            /* If we've already figured out the image, don't do it again */
            if (OlvSubItem != null && OlvSubItem.ImageSelector is Image)
            {
                return OlvSubItem.AnimationState == null
                           ? (Image)OlvSubItem.ImageSelector
                           : OlvSubItem.AnimationState.Image;
            }
            /* Try to convert our Aspect into an Image
             * If its a byte array, we treat it as an in-memory image
             * If it's an int, we use that as an index into our image list
             * If it's a string, we try to find a file by that name.
             * If we can't, we use the string as an index into our image list. */
            Image image = null;
            if (Aspect is Byte[])
            {
                using (var stream = new MemoryStream((Byte[])Aspect))
                {
                    try
                    {
                        image = Image.FromStream(stream);
                    }
                    catch (ArgumentException)
                    {
                        /* Ignore */
                    }
                }
            }
            else if (Aspect is Int32)
            {
                image = GetImage(Aspect);
            }
            else
            {
                var str = Aspect as String;
                if (!string.IsNullOrEmpty(str))
                {
                    try
                    {
                        image = Image.FromFile(str);
                    }
                    catch (FileNotFoundException)
                    {
                        image = GetImage(Aspect);
                    }
                    catch (OutOfMemoryException)
                    {
                        image = GetImage(Aspect);
                    }
                }
            }
            /* If this image is an animation, initialize the animation process */
            if (OlvSubItem != null && AnimationState.IsAnimation(image))
            {
                OlvSubItem.AnimationState = new AnimationState(image);
            }
            /* Cache the image so we don't repeat this dreary process */
            if (OlvSubItem != null)
            {
                OlvSubItem.ImageSelector = image;
            }
            return image;
        }

        /* Events */
        public void OnTimer(Object state)
        {
            if (ListView == null || Paused)
            {
                return;
            }
            if (ListView.InvokeRequired)
            {
                ListView.Invoke((MethodInvoker)(() => OnTimer(state)));
            }
            else
            {
                OnTimerInThread();
            }
        }

        protected void OnTimerInThread()
        {
            /* MAINTAINER NOTE: This method must renew the tickler. If it doesn't the animations will stop.
             * If this listview has been destroyed, we can't do anything, so we return without
             * renewing the tickler, effectively killing all animations on this renderer */
            if (ListView == null || Paused || ListView.IsDisposed)
            {
                return;
            }
            /* If we're not in Detail view or our column has been removed from the list,
             * we can't do anything at the moment, but we still renew the tickler because the view may change later. */
            if (ListView.View != View.Details || Column == null || Column.Index < 0)
            {
                Tickler.Change(1000, Timeout.Infinite);
                return;
            }
            var elapsedMilliseconds = _stopwatch.ElapsedMilliseconds;
            var subItemIndex = Column.Index;
            var nextCheckAt = elapsedMilliseconds + 1000; /* wait at most one second before checking again */
            var updateRect = new Rectangle(); /* what part of the view must be updated to draw the changed gifs? */
            /* Run through all the subitems in the view for our column, and for each one that
             * has an animation attached to it, see if the frame needs updating. */
            for (var i = 0; i < ListView.GetItemCount(); i++)
            {
                var lvi = ListView.GetItem(i);
                /* Get the animation state from the subitem. If there isn't an animation state, skip this row. */
                var lvsi = lvi.GetSubItem(subItemIndex);
                var state = lvsi.AnimationState;
                if (state == null || !state.IsValid)
                {
                    continue;
                }
                /* Has this frame of the animation expired? */
                if (elapsedMilliseconds >= state.CurrentFrameExpiresAt)
                {
                    state.AdvanceFrame(elapsedMilliseconds);
                    /* Track the area of the view that needs to be redrawn to show the changed images */
                    updateRect = updateRect.IsEmpty ? lvsi.Bounds : Rectangle.Union(updateRect, lvsi.Bounds);
                }
                /* Remember the minimum time at which a frame is next due to change */
                nextCheckAt = Math.Min(nextCheckAt, state.CurrentFrameExpiresAt);
            }
            /* Update the part of the listview where frames have changed */
            if (!updateRect.IsEmpty)
            {
                ListView.Invalidate(updateRect);
            }
            /* Renew the tickler in time for the next frame change */
            Tickler.Change(nextCheckAt - elapsedMilliseconds, Timeout.Infinite);
        }
    }
}
