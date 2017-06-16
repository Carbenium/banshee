//
// PlayerInterfaceLite.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Banshee.Collection;
using Banshee.Configuration;
using Banshee.Gui;
using Banshee.Gui.Widgets;
using Banshee.I18n;
using Banshee.ServiceStack;
using Banshee.Sources;
using Banshee.Sources.Gui;
using Gtk;

namespace Nereid
{
    public class PlayerInterfaceLite : BaseClientWindow, IClientWindow, IDBusObjectName, IService, IDisposable, IHasSourceView
    {
        const string CONFIG_NAMESPACE = "player_window_lite";

        static readonly SchemaEntry<int> WidthSchema = WindowConfiguration.NewWidthSchema (CONFIG_NAMESPACE, 1024);
        static readonly SchemaEntry<int> HeightSchema = WindowConfiguration.NewHeightSchema (CONFIG_NAMESPACE, 700);
        static readonly SchemaEntry<int> XPosSchema = WindowConfiguration.NewXPosSchema (CONFIG_NAMESPACE);
        static readonly SchemaEntry<int> YPosSchema = WindowConfiguration.NewYPosSchema (CONFIG_NAMESPACE);
        static readonly SchemaEntry<bool> MaximizedSchema = WindowConfiguration.NewMaximizedSchema (CONFIG_NAMESPACE);

        VBox _source_box;
        SourceView _source_view;
        ViewContainer _view_container;

        ISourceContents _composite;
        Label _status_label;
        HeaderBar _header;

        Grid _status;

        Toolbar _main_bar, _source_bar;

        List<System.Action> _ready = new List<System.Action> ();

        IDBusExportable IDBusExportable.Parent {
            get { return null; }
        }

        public string ServiceName {
            get { return "NereidPlayerInterfaceLite"; }
        }

        public string ExportObjectName {
            get { return "ClientWindow"; }
        }

        public Source HighlightedSource {
            get { return _source_view.HighlightedSource; }
        }

        public void ResetHighlight ()
        {
            _source_view.ResetHighlight ();
        }

        public void BeginRenameSource (Source source)
        {
            _source_view.BeginRenameSource (source);
        }


        protected PlayerInterfaceLite (IntPtr ptr) : base (ptr)
        {
        }

        public PlayerInterfaceLite () :
            base (Catalog.GetString ("Banshee Media Player"),
                  new WindowConfiguration (WidthSchema, HeightSchema, XPosSchema, YPosSchema, MaximizedSchema))
        {
        }

        protected override void Initialize ()
        {
            InitialShowPresent ();
        }

        protected override void OnShown ()
        {
            var vbox = new VBox ();

            var views_pane = new HPaned ();
            PersistentPaneController.Control (views_pane, SourceViewWidth);
            _view_container = new ViewContainer ();

            _source_view = new SourceView ();

            var source_scroll = new ScrolledWindow ();

            _view_container.Content = _composite = new SimpleListSourceContents ();

            var toolbar = _Toolbar ();
            _status = _Statusbar ();

            source_scroll.Add (_source_view);

            _source_box = new VBox ();
            _source_box.PackStart (source_scroll, true, true, 0);
            _source_box.PackEnd (_cover_art, false, false, 0);

            views_pane.Pack1 (_source_box, true, true);
            views_pane.Pack2 (_view_container, true, true);

            vbox.PackStart (_status, false, false, 0);
            vbox.PackStart (views_pane, true, true, 0);
            vbox.PackStart (toolbar, false, false, 0);

            Add (vbox);

            foreach (var _ in _ready) {
                _ ();
            }

            _ready.Clear ();

            _composite.Widget.Show ();
            _view_container.Show ();
            _source_view.Show ();
            source_scroll.Show ();
            _source_box.Show ();

            CoverArt ();

            _status.Show ();
            views_pane.Show ();
            toolbar.Show ();

            vbox.Show ();

            ConnectEvents ();

            base.OnShown ();
        }

        Grid _Toolbar ()
        {
            var grid = new Grid { ColumnHomogeneous = true, MarginLeft = 10, MarginRight = 10 };

            var task_status = new TaskStatusIcon { ShowOnlyBackgroundTasks = false };

            var toolbar = (Toolbar)ActionService.UIManager.GetWidget ("/ControlToolbar");

            toolbar.ShowArrow = false;
            toolbar.ToolbarStyle = ToolbarStyle.Icons;
            toolbar.IconSize = IconSize.SmallToolbar;
            toolbar.Margin = 0;

            var next_button = new NextButton (ActionService) { IconSize = IconSize.SmallToolbar };
            ActionService.PopulateToolbarPlaceholder (toolbar, "/ControlToolbar/NextArrowButton", next_button);

            var repeat_button = new RepeatButton { Relief = ReliefStyle.None };
            ActionService.PopulateToolbarPlaceholder (toolbar, "/ControlToolbar/RepeatButton", repeat_button);

            var seek_slider = new ConnectedSeekSlider ();

            var tools_align = new Alignment (0.5f, 0.5f, 0f, 0f);
            tools_align.Child = toolbar;

            var search_align = new Alignment (0.5f, 0.5f, 0f, 0f);
            search_align.Child = _view_container.SearchEntry;

            var lbox = new HBox ();
            lbox.PackStart (task_status, false, false, 0);
            lbox.PackStart (tools_align, true, true, 0);

            var rbox = new HBox ();
            rbox.PackEnd (search_align, true, true, 0);

            grid.Attach (lbox,        0, 0, 1, 1);
            grid.Attach (seek_slider, 1, 0, 1, 1);
            grid.Attach (rbox,        2, 0, 1, 1);

            _ready.Add (() => {
                task_status.Show ();
                next_button.Show ();
                repeat_button.Show ();
                tools_align.Show ();
                search_align.Show ();

                lbox.Show ();
                seek_slider.Show ();
                rbox.Show ();
            });

            return grid;
        }

        Grid _Statusbar ()
        {
            _header = new HeaderBar { Title = Title, ShowCloseButton = true };

            this.SetTitlebar (_header);

            _main_bar = new Toolbar ();
            _main_bar.ToolbarStyle = ToolbarStyle.Icons;
            _main_bar.IconSize = IconSize.SmallToolbar;
            _main_bar.Halign = Align.Start;

            var menu_button = new MainMenuButton ();
            var menu_item = new ToolItem { Child = menu_button };
            _main_bar.Insert (menu_item, 0);

            _source_bar = (Toolbar)ActionService.UIManager.GetWidget ("/SourceToolbar");
            _source_bar.ToolbarStyle = ToolbarStyle.Icons;
            _source_bar.IconSize = IconSize.SmallToolbar;
            _source_bar.Halign = Align.End;

            _status_label = new Label ();

            var grid = new Grid { ColumnHomogeneous = true };

            grid.Attach (_status_label, 1, 0, 1, 1);

            _ready.Add (() =>
            {
                menu_button.Show ();
                menu_item.Show ();

                _main_bar.Show ();
                _status_label.Show ();
                _source_bar.Show ();

                _header.Show ();
            });

            return grid;
        }

        void Setup (Gdk.WindowState state)
        {
            var stat = _status.Visible;
            var full = state.HasFlag (Gdk.WindowState.Fullscreen);

            if (full ^ !stat)
            {
                return;
            }

            if (full)
            {
                _header.Remove (_main_bar);
                _header.Remove (_source_bar);

                _main_bar.ResetAfterHeaderBar ();
                _source_bar.ResetAfterHeaderBar ();

                _status.Attach (_main_bar,   0, 0, 1, 1);
                _status.Attach (_source_bar, 2, 0, 1, 1);
                _status.Show ();
            }
            else
            {
                _status.Remove (_main_bar);
                _status.Remove (_source_bar);
                _status.Hide ();

                _main_bar.ForHeaderBar ();
                _source_bar.ForHeaderBar ();

                _header.PackStart (_main_bar);
                _header.PackEnd (_source_bar);
            }
        }

        protected override bool OnWindowStateEvent (Gdk.EventWindowState evnt)
        {
            Setup (evnt.NewWindowState);

            return base.OnWindowStateEvent (evnt);
        }

        protected override void ConnectEvents ()
        {
            base.ConnectEvents ();

            ActionService.SourceActions.SourceView = this;

            ServiceManager.SourceManager.ActiveSourceChanged += OnActiveSourceChanged;
            ServiceManager.SourceManager.SourceUpdated += OnSourceUpdated;

            _view_container.SearchEntry.Changed += (o, x) => {
                var source = ServiceManager.SourceManager.ActiveSource;
                if (source == null)
                    return;

                source.FilterType = (TrackFilterType)_view_container.SearchEntry.ActiveFilterID;
                source.FilterQuery = _view_container.SearchEntry.Query;
            };

            _source_view.RowActivated += (o, x) => {
                Source source = ServiceManager.SourceManager.ActiveSource;
                var handler = source.Properties.Get<System.Action> ("ActivationAction");
                if (handler != null) {
                    handler ();
                } else if (source is ITrackModelSource) {
                    ServiceManager.PlaybackController.NextSource = (ITrackModelSource)source;
                    ServiceManager.PlaybackController.Next ();
                }
            };

            ((ToggleAction)ActionService.ViewActions ["ShowCoverArtAction"]).Active = ShowCoverArt.Get ();
            ActionService.ViewActions ["ShowCoverArtAction"].Activated += (o, a) => {
                ShowCoverArt.Set ((o as Gtk.ToggleAction).Active);

                CoverArt ();
            };

            _source_box.Parent.SizeAllocated += (o, x) => {
                _cover_art.HeightRequest = _source_box.Allocation.Width;
            };
        }

        void CoverArt ()
        {
            if (ShowCoverArt.Get ()) {
                _cover_art.Child = new CoverArtDisplay ();
                _cover_art.Child.Show ();
                _cover_art.Show ();
            } else if (null != _cover_art.Child) {
                var widget = _cover_art.Child;
                _cover_art.Remove (_cover_art.Child);
                _cover_art.Hide ();
                widget.Dispose ();
            }
        }

        readonly Alignment _cover_art = new Alignment (0.0f, 0.0f, 1f, 1f);

        bool _accel_disabled;

        protected bool AccelDisabled {
            get {
                return _accel_disabled;
            }
            set {
                if (value == _accel_disabled) {
                    return;
                }

                _accel_disabled = value;

                if (value) {
                    RemoveAccelGroup (ActionService.UIManager.AccelGroup);
                } else {
                    AddAccelGroup (ActionService.UIManager.AccelGroup);
                }
            }
        }

        protected override void OnSetFocus (Widget focus)
        {
            AccelDisabled = focus is Entry || focus is IDisableKeybindings;

            base.OnSetFocus (focus);
        }

        protected override void UpdateTitle ()
        {
            var title = _view_container.Content.Source?.Name ?? Catalog.GetString ("Banshee Media Player");

            _header.Title = Title = title;

            OnTitleChanged ();
        }

        private bool _disposed;

        protected override void Dispose (bool disposing)
        {
            if (_disposed) return;

            _disposed = true;

            ServiceManager.SourceManager.ActiveSourceChanged -= OnActiveSourceChanged;
            ServiceManager.SourceManager.SourceUpdated -= OnSourceUpdated;

            base.Dispose (disposing);
        }

        void OnActiveSourceChanged (SourceEventArgs args)
        {
            Set (args.Source);
        }

        void OnSourceUpdated (SourceEventArgs args)
        {
            Widgets (args.Source);
            Update (args.Source);
        }

        void Update (Source source)
        {
            if (source != _view_container.Content.Source) {
                return;
            }

            UpdateTitle ();

            _status_label.Text = source.GetStatusText ();

            var msg = source.Properties.Get<string> ("SearchEntryDescription") ?? Catalog.GetString ("Search");
            _view_container.SearchEntry.EmptyMessage = msg;
            _view_container.SearchEntry.TooltipText = msg;

            _header.Subtitle = _status_label.Text;
        }

        void Widgets (Source source)
        {
            if (null == source)
            {
                return;
            }

            if (ServiceManager.SourceManager.ActiveSource != source)
            {
                return;
            }

            var contents = source.GetProperty<ISourceContents> ("Nereid.SourceContents", source.GetInheritedProperty<bool> ("Nereid.SourceContentsPropagate"));

            if (null == contents)
            {
                contents = source is ITrackModelSource ? _composite : new _SourceContents ();
            }

            if (contents != _view_container.Content)
            {
                _view_container.Content = contents;
            }

            if (source != _view_container.Content.Source)
            {
                _view_container.Content.ResetSource ();
                _view_container.Content.SetSource (source);

                if (_view_container.Content is ITrackModelSourceContents)
                {
                    var track_content = (ITrackModelSourceContents) _view_container.Content;
                    source.Properties.Set ("Track.IListView", track_content.TrackView);
                }
            }

            var header = source.Properties.Get<Widget> ("Nereid.SourceContents.HeaderWidget");
            var footer = source.Properties.Get<Widget> ("Nereid.SourceContents.FooterWidget");

            if (null == header?.Parent)
            {
                _view_container.ClearHeaderWidget ();
                _view_container.SetHeaderWidget (header);
            }

            if (null == footer?.Parent)
            {
                _view_container.ClearFooter ();
                _view_container.SetFooter (footer);
            }
        }

        void Set (Source source)
        {
            if (source == null) {
                return;
            }

            if (ServiceManager.SourceManager.ActiveSource != source) {
                return;
            }

            if (source == _view_container.Content.Source) {
                return;
            }

            Widgets (source);
            Update (source);

            _view_container.SearchEntry.Ready = false;
            _view_container.SearchEntry.CancelSearch ();

            if (source.FilterQuery != null) {
                _view_container.SearchEntry.Query = source.FilterQuery;
                _view_container.SearchEntry.ActivateFilter ((int)source.FilterType);
            }

            _view_container.SearchSensitive = source.CanSearch;
            _view_container.SearchEntry.Ready = true;
        }

        class _SourceContents : ScrolledWindow, ISourceContents
        {
            public ISource Source { get; private set; }

            public Widget Widget {
                get { return this; }
            }

            public void ResetSource ()
            {
                Source = null;
            }

            public bool SetSource (ISource source)
            {
                Source = source;

                return true;
            }
        }

        static readonly SchemaEntry<int> SourceViewWidth = new SchemaEntry<int> (
            CONFIG_NAMESPACE, "source_view_width",
            175,
            "Source View Width",
            "Width of Source View Column."
        );

        static readonly SchemaEntry<bool> ShowCoverArt = new SchemaEntry<bool> (
            CONFIG_NAMESPACE, "show_cover_art",
            false,
            "Show cover art",
            "Show cover art below source view if available"
        );
    }
}