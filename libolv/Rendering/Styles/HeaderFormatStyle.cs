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
using System.ComponentModel;
using System.Drawing;

namespace libolv.Rendering.Styles
{
    public class HeaderFormatStyle : Component
    {
        public HeaderFormatStyle()
        {
            Hot = new HeaderStateStyle();
            Normal = new HeaderStateStyle();
            Pressed = new HeaderStateStyle();
        }

        [Category("Appearance"), Description("How should the header be drawn when the mouse is over it?")]
        public HeaderStateStyle Hot { get; set; }

        [Category("Appearance"), Description("How should a column header normally be drawn")]
        public HeaderStateStyle Normal { get; set; }

        [Category("Appearance"), Description("How should a column header be drawn when it is pressed")]
        public HeaderStateStyle Pressed { get; set; }

        public void SetFont(Font font)
        {
            Normal.Font = font;
            Hot.Font = font;
            Pressed.Font = font;
        }

        public void SetForeColor(Color color)
        {
            Normal.ForeColor = color;
            Hot.ForeColor = color;
            Pressed.ForeColor = color;
        }

        public void SetBackColor(Color color)
        {
            Normal.BackColor = color;
            Hot.BackColor = color;
            Pressed.BackColor = color;
        }
    }
}
