//
// Extensions.cs
//
// Author:
//   nicholas <>
//
// Copyright 2016 nicholas
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

using System;
using System.Runtime.InteropServices;

namespace Gtk
{
    public static class _Extensions
    {
        [DllImport ("libgtk-3.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void gtk_window_set_titlebar (IntPtr window, IntPtr widget);

        public static void SetTitlebar (this Window window, Widget widget)
        {
            gtk_window_set_titlebar (window.Handle, widget.Handle);
        }

        public static void ForHeaderBar (this Toolbar toolbar)
        {
            toolbar.OverrideBackgroundColor (StateFlags.Normal, new Gdk.RGBA { Alpha = 0.0 });
            toolbar.StyleContext.AddProvider (HorizontalZeroPadding, 600);
        }

        public static void ResetAfterHeaderBar (this Toolbar toolbar)
        {
            toolbar.StyleContext.RemoveProvider (HorizontalZeroPadding);
        }

        static _Extensions ()
        {
            HorizontalZeroPadding = new Gtk.CssProvider ();

            HorizontalZeroPadding.LoadFromData (".horizontal { padding-top: 0px; padding-bottom: 5px; padding-left: 0px; padding-right: 0px }");
        }

        static readonly CssProvider HorizontalZeroPadding;
    }
}