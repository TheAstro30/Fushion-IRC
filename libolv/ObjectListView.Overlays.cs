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
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using libolv.Implementation;
using libolv.Rendering.Decoration;
using libolv.Rendering.Overlays;
using libolv.SubControls;

namespace libolv
{
    public partial class ObjectListView
    {
        public virtual void AddDecoration(IDecoration decoration)
        {
            if (decoration == null)
            {
                return;
            }
            Decorations.Add(decoration);
            Invalidate();
        }

        public virtual void AddOverlay(IOverlay overlay)
        {
            if (overlay == null)
            {
                return;
            }
            Overlays.Add(overlay);
            Invalidate();
        }

        protected virtual void DrawAllDecorations(Graphics g, List<OlvListItem> drawnItems)
        {
            g.TextRenderingHint = TextRenderingHint;
            g.SmoothingMode = SmoothingMode;
            var contentRectangle = ContentRectangle;
            if (HasEmptyListMsg && GetItemCount() == 0)
            {
                EmptyListMsgOverlay.Draw(this, g, contentRectangle);
            }
            /* Let the drop sink draw whatever feedback it likes */
            if (DropSink != null)
            {
                DropSink.DrawFeedback(g, contentRectangle);
            }
            /* Draw our item and subitem decorations */
            foreach (var olvi in drawnItems)
            {
                if (olvi.HasDecoration)
                {
                    foreach (var d in olvi.Decorations)
                    {
                        d.ListItem = olvi;
                        d.SubItem = null;
                        d.Draw(this, g, contentRectangle);
                    }
                }
                foreach (OlvListSubItem subItem in olvi.SubItems)
                {
                    if (!subItem.HasDecoration) { continue; }
                    foreach (var d in subItem.Decorations)
                    {
                        d.ListItem = olvi;
                        d.SubItem = subItem;
                        d.Draw(this, g, contentRectangle);
                    }
                }
                if (SelectedRowDecoration == null || !olvi.Selected) { continue; }
                SelectedRowDecoration.ListItem = olvi;
                SelectedRowDecoration.SubItem = null;
                SelectedRowDecoration.Draw(this, g, contentRectangle);
            }
            /* Now draw the specifically registered decorations */
            foreach (var decoration in Decorations)
            {
                decoration.ListItem = null;
                decoration.SubItem = null;
                decoration.Draw(this, g, contentRectangle);
            }
            /* Finally, draw any hot item decoration */
            if (UseHotItem && HotItemStyle != null && HotItemStyle.Decoration != null)
            {
                var hotItemDecoration = HotItemStyle.Decoration;
                hotItemDecoration.ListItem = GetItem(HotRowIndex);
                hotItemDecoration.SubItem = hotItemDecoration.ListItem == null ? null : hotItemDecoration.ListItem.GetSubItem(HotColumnIndex);
                hotItemDecoration.Draw(this, g, contentRectangle);
            }
            /* If we are in design mode, we don't want to use the glass panels,
             * so we draw the background overlays here */
            if (!DesignMode) { return; }
            foreach (var overlay in Overlays)
            {
                overlay.Draw(this, g, contentRectangle);
            }
        }

        public virtual bool HasDecoration(IDecoration decoration)
        {
            return Decorations.Contains(decoration);
        }

        public virtual bool HasOverlay(IOverlay overlay)
        {
            return Overlays.Contains(overlay);
        }

        public virtual void HideOverlays()
        {
            foreach (var glassPanel in _glassPanels)
            {
                glassPanel.HideGlass();
            }
        }

        protected virtual void InitializeEmptyListMsgOverlay()
        {
            var overlay = new TextOverlay
                              {
                                  Alignment = ContentAlignment.MiddleCenter,
                                  TextColor = SystemColors.ControlDarkDark,
                                  BackColor = Color.BlanchedAlmond,
                                  BorderColor = SystemColors.ControlDark,
                                  BorderWidth = 2.0f
                              };
            EmptyListMsgOverlay = overlay;
        }

        protected virtual void InitializeStandardOverlays()
        {
            OverlayImage = new ImageOverlay();
            AddOverlay(OverlayImage);
            OverlayText = new TextOverlay();
            AddOverlay(OverlayText);
        }

        public virtual void ShowOverlays()
        {
            /* If we shouldn't show overlays, then don't create glass panels */
            if (!ShouldShowOverlays())
            {
                return;
            }
            /* Make sure that each overlay has its own glass panels */
            if (Overlays.Count != _glassPanels.Count)
            {
                foreach (var overlay in Overlays)
                {
                    var glassPanel = FindGlassPanelForOverlay(overlay);
                    if (glassPanel != null) { continue; }
                    glassPanel = new GlassPanelForm();
                    glassPanel.Bind(this, overlay);
                    _glassPanels.Add(glassPanel);
                }
            }
            foreach (var glassPanel in _glassPanels)
            {
                glassPanel.ShowGlass();
            }
        }

        private bool ShouldShowOverlays()
        {
            /* If we are in design mode, we dont show the overlays 
             * If we are explicitly not using overlays, also don't show them 
             * If there are no overlays, guess...
             * If we don't have 32-bit display, alpha blending doesn't work, so again, no overlays */
            return !DesignMode && UseOverlays && HasOverlays && Screen.PrimaryScreen.BitsPerPixel >= 32;
        }

        private GlassPanelForm FindGlassPanelForOverlay(IOverlay overlay)
        {
            return _glassPanels.Find(x => x.Overlay == overlay);
        }

        public virtual void RefreshOverlays()
        {
            foreach (var glassPanel in _glassPanels)
            {
                glassPanel.Invalidate();
            }
        }

        public virtual void RefreshOverlay(IOverlay overlay)
        {
            var glassPanel = FindGlassPanelForOverlay(overlay);
            if (glassPanel != null)
            {
                glassPanel.Invalidate();
            }
        }

        public virtual void RemoveDecoration(IDecoration decoration)
        {
            if (decoration == null)
            {
                return;
            }
            Decorations.Remove(decoration);
            Invalidate();
        }

        public virtual void RemoveOverlay(IOverlay overlay)
        {
            if (overlay == null)
            {
                return;
            }
            Overlays.Remove(overlay);
            var glassPanel = FindGlassPanelForOverlay(overlay);
            if (glassPanel == null) { return; }
            _glassPanels.Remove(glassPanel);
            glassPanel.Unbind();
            glassPanel.Dispose();
        }
    }
}
