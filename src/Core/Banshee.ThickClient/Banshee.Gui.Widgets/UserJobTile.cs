// 
// UserJobTile.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2007 Novell, Inc.
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
using Mono.Unix;
using Gtk;

using Banshee.Base;
using Banshee.ServiceStack;

namespace Banshee.Gui.Widgets
{
    public class UserJobTile : Table
    {
        private IUserJob job;
        
        private string [] icon_names;
        private string title;
        private string status;
        
        private Image icon;
        private Gdk.Pixbuf icon_pixbuf;
        private Label title_label;
        private Label status_label;
        private ProgressBar progress_bar;
        private Button cancel_button;
        private uint update_delay_id;
        private uint progress_bounce_id;
        
        Banshee.Widgets.HigMessageDialog cancel_dialog;
        
        public UserJobTile (IUserJob job) : base (3, 2, false)
        {
            this.job = job;
            this.job.Updated += OnJobUpdated;
            
            BuildWidget ();
            UpdateFromJob ();
        }
        
        private void BuildWidget ()
        {
            ColumnSpacing = 5;
            RowSpacing = 2;
            
            icon = new Image ();
            
            title_label = new Label ();
            title_label.Xalign = 0.0f;
            title_label.Ellipsize = Pango.EllipsizeMode.End;
            
            status_label = new Label ();
            status_label.Xalign = 0.0f;
            status_label.Ellipsize = Pango.EllipsizeMode.End;
            
            progress_bar = new ProgressBar ();
            progress_bar.SetSizeRequest (0, -1);
            progress_bar.Text = " ";
            progress_bar.Show ();
            
            cancel_button = new Button (new Image (Stock.Close, IconSize.Menu));
            cancel_button.Relief = ReliefStyle.None;
            cancel_button.ShowAll ();
            cancel_button.Clicked += OnCancelClicked;
            
            Attach (title_label, 0, 3, 0, 1, 
                AttachOptions.Expand | AttachOptions.Fill,
                AttachOptions.Expand | AttachOptions.Fill, 0, 0);
            
            Attach (status_label, 0, 3, 1, 2, 
                AttachOptions.Expand | AttachOptions.Fill,
                AttachOptions.Expand | AttachOptions.Fill, 0, 0);
                
            Attach (icon, 0, 1, 2, 3, 
                AttachOptions.Shrink | AttachOptions.Fill,
                AttachOptions.Shrink | AttachOptions.Fill, 0, 0);
                
            Attach (progress_bar, 1, 2, 2, 3,
                AttachOptions.Expand | AttachOptions.Fill,
                AttachOptions.Shrink, 0, 0);
                
            Attach (cancel_button, 2, 3, 2, 3,
                AttachOptions.Shrink | AttachOptions.Fill,
                AttachOptions.Shrink | AttachOptions.Fill, 0, 0);
        }
        
        protected override void OnStyleSet (Style style)
        {
            base.OnStyleSet (style);
            UpdateIcons ();
        }

        private void OnCancelClicked (object o, EventArgs args)
        {
            if (cancel_dialog != null) {
                return;
            }
            
            Window parent = null;
            if (ServiceManager.Contains ("GtkElementsService")) {
                parent = ServiceManager.Get<GtkElementsService> ("GtkElementsService").PrimaryWindow;
            }
            
            cancel_dialog = new Banshee.Widgets.HigMessageDialog (parent, 
                DialogFlags.Modal, MessageType.Question, ButtonsType.None,
                job.Name == null
                    ? Catalog.GetString ("Stop Operation")
                    : String.Format (Catalog.GetString ("Stop {0}"), job.Name),
                job.CancelMessage == null 
                    ? (job.Name == null 
                        ? Catalog.GetString ("This operation is still performing work. Would you like to stop it?")
                        : String.Format (Catalog.GetString (
                            "The '{0}' operation is still performing work. Would you like to stop it?"), job.Name))
                    : job.CancelMessage);
                        
            cancel_dialog.AddButton (job.Name == null 
                ? Catalog.GetString ("Continue")
                : String.Format (Catalog.GetString ("Continue {0}"), job.Name), 
                ResponseType.No, true);
            cancel_dialog.AddButton (Stock.Stop, ResponseType.Yes, false);
            cancel_dialog.DefaultResponse = ResponseType.Cancel;
                
            if(cancel_dialog.Run () == (int)ResponseType.Yes) {
                if (job.CanCancel) {
                    job.Cancel ();
                }
            }
        
            cancel_dialog.Destroy();
            cancel_dialog = null;
        }
        
        private void UpdateFromJob ()
        {
            if (cancel_dialog != null && !job.CanCancel) {
                cancel_dialog.Respond (Gtk.ResponseType.Cancel);
            }
            
            if (title != job.Title) {
                if (String.IsNullOrEmpty (job.Title)) {
                    title_label.Hide ();
                } else {
                    title_label.Markup = String.Format ("<small><b>{0}</b></small>", 
                        GLib.Markup.EscapeText (job.Title));
                    title_label.Show ();
                }
                title = job.Title;
            }
            
            if (status != job.Status) {
                if (String.IsNullOrEmpty (job.Status)) {
                    status_label.Hide ();
                } else {
                    status_label.Markup = String.Format ("<small>{0}</small>", GLib.Markup.EscapeText (job.Status));
                    status_label.Show ();
                }
                status = job.Status;
            }
            
            if (icon_names == null || icon_names.Length != job.IconNames.Length) {
                UpdateIcons ();
            } else {
                for (int i = 0; i < job.IconNames.Length; i++) {
                    if (!icon_names[i].Equals (job.IconNames[i])) {
                        UpdateIcons ();
                        break;
                    }
                }
            }
            
            cancel_button.Sensitive = job.CanCancel;
            progress_bar.Fraction = job.Progress;
            
            if (job.Progress == 0.0 && progress_bounce_id == 0) {
                progress_bounce_id = GLib.Timeout.Add (100, delegate {
                    progress_bar.Text = " ";
                    progress_bar.Pulse ();
                    return true;
                });
            } else if (job.Progress > 0.0) {
                if (progress_bounce_id > 0) {
                    GLib.Source.Remove (progress_bounce_id);
                    progress_bounce_id = 0;
                }
                
                progress_bar.Text = String.Format("{0}%", (int)(job.Progress * 100.0));
            }
        }
        
        private void UpdateIcons ()
        {
            icon_names = job.IconNames;
            
            if (icon_pixbuf != null) {
                icon_pixbuf.Dispose ();
                icon_pixbuf = null;
            }

            if (icon_names == null || icon_names.Length == 0) {
                icon.Hide ();
                return;
            }
            
            icon_pixbuf = IconThemeUtils.LoadIcon (22, icon_names);
            if (icon_pixbuf != null) {
                icon.Pixbuf = icon_pixbuf;
                icon.Show ();
            }
        }
        
        private void OnJobUpdated (object o, EventArgs args)
        {
            if (update_delay_id == 0) {
                GLib.Timeout.Add (100, UpdateFromJobTimeout);
            }
        }
        
        private bool UpdateFromJobTimeout ()
        {
            UpdateFromJob ();
            update_delay_id = 0;
            return false;
        }
    }
}
