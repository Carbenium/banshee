//
// MainMenuButton.cs
//
// Author:
//   Nicholas Little <arfbtwn@openmailbox.org>
//
// Copyright 2016
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Gtk;
using Hyena.Gui;
using Hyena.Widgets;

namespace Banshee.Gui
{
    public class MainMenuButton : MenuButton
    {
        static Image Logo {
            get {
                int w, h;
                Icon.SizeLookup (IconSize.LargeToolbar, out w, out h);

                using (var img = new Cairo.ImageSurface (Cairo.Format.Rgb24, w, h))
                using (var cr = new Cairo.Context (img))
                {
                    var inner = CairoExtensions.RgbaToColor (0x00000044);
                    var outer = CairoExtensions.RgbaToColor (0x00000055);

                    CairoGlyphs.BansheeLineLogo.Render (cr, new Cairo.Rectangle (0, 0, w, h), inner, outer);

                    var pix = new Gdk.Pixbuf (img.Data, Gdk.Colorspace.Rgb, true, 8, w, h, img.Stride);

                    return new Image (pix);
                }
            }
        }

        readonly Menu _menu;

        public MainMenuButton() : this (Logo)
        {
        }

        public MainMenuButton (Widget button)
        {
            var _ = ServiceStack.ServiceManager.Get<InterfaceActionService> ();

            _menu = (Menu) _.UIManager.GetWidget ("/MainMenuButton");

            ((PlaybackRepeatActions)_.FindActionGroup ("PlaybackRepeat")).AttachSubmenu (
                "/MainMenuButton/PlaybackMenu/RepeatMenu");

            ((PlaybackShuffleActions)_.FindActionGroup ("PlaybackShuffle")).AttachSubmenu (
                "/MainMenuButton/PlaybackMenu/ShuffleMenu");

            ((PlaybackSubtitleActions)_.FindActionGroup ("PlaybackSubtitle")).AttachSubmenu (
                "/MainMenuButton/PlaybackMenu/SubtitleMenu");

            Construct (button, _menu, false);
        }
    }
}

