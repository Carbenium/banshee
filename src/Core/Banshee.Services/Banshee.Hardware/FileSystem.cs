//
// FileSystem.cs
//
// Author:
//   Nicholas Little <arealityfarbetween@googlemail.com>
//
// Copyright 2013 Nicholas Little
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
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Banshee.Hardware
{
    public static class FileSystem
    {
        public static IEnumerable<string> Safe (string head, params string[] tail)
        {
            var path = new [] { head }.Concat (tail);

            return path.Select (Safe);
        }

        public static string Safe (string path)
        {
            return ch_unsafe.Aggregate (path, Wipe);
        }

        private static string Wipe(string input, char wipe)
        {
            return input.Replace (wipe, ch_escape);
        }

        public static bool EqualsNoCase(string x, string y)
        {
            return string.Equals (x, y, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsCaseSensitive (IVolume volume)
        {
            var drive = new DriveInfo (volume.MountPoint);
            return !fs_nocase.Contains (drive.DriveFormat);
        }

        private static readonly char     ch_escape = '_';
        private static readonly char[]   ch_unsafe = { '/', '|', '\\', '<', '>', '?', '*', '"' };
        private static readonly string[] fs_nocase = { "vfat", "msdos", "ntfs" };
    }
}

