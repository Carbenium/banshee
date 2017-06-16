//
// FilteredListSourceContents.cs
//
// Authors:
//   Aaron Bockover <abockover@novell.com>
//   Gabriel Burt <gburt@novell.com>
//
// Copyright (C) 2007-2008 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using System.Collections.Generic;

using Gtk;
using Hyena.Data;
using Hyena.Data.Gui;

using Banshee.Collection;
using Banshee.Collection.Gui;

namespace Banshee.Sources.Gui
{
    public class SimpleListSourceContents : ScrolledWindow, ITrackModelSourceContents
    {
        readonly TrackListView track_view;

        public SimpleListSourceContents ()
        {
            track_view = new TrackListView ();
            Add (track_view);
            track_view.Show ();
        }

        IListView<TrackInfo> ITrackModelSourceContents.TrackView {
            get { return track_view; }
        }

        public TrackListView TrackView {
            get { return track_view; }
        }

        public TrackListModel TrackModel {
            get { return (TrackListModel)track_view.Model; }
        }

        protected bool ActiveSourceCanHasBrowser {
            get { return false; }
        }

        #region Implement ISourceContents

        protected ISource source;

        public ISource Source {
            get { return source; }
        }

        public Widget Widget {
            get { return this; }
        }

        public bool SetSource (ISource source)
        {
            var track_source = source as ITrackModelSource;
            if (track_source == null) {
                return false;
            }

            this.source = source;

            SetModel (track_view, track_source.TrackModel);

            track_view.HeaderVisible = true;
            return true;
        }

        public void ResetSource ()
        {
            source = null;
            SetModel (track_view, null);
            track_view.HeaderVisible = false;
        }

        #endregion

        protected void SetModel<T> (IListView<T> view, IListModel<T> model)
        {
            if (model == null) {
                view.SetModel (null);
                return;
            }

            view.SetModel (model, 0.0);
        }
    }
}
