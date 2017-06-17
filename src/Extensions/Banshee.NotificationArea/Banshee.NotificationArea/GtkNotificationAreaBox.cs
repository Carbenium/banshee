//
// GtkNotificationAreaBox.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2008 Novell, Inc.
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

using Banshee.Gui;
using Banshee.ServiceStack;
using Banshee.MediaEngine;

namespace Banshee.NotificationArea
{
    public class GtkNotificationAreaBox : StatusIcon, INotificationAreaBox
    {
#pragma warning disable 0067
        public event EventHandler Disconnected;
#pragma warning restore 0067
        public event EventHandler Activated;
        public event PopupMenuHandler PopupMenuEvent;

        private TrackInfoPopup _popup;
        private bool _canShowPopup;
        private bool _cursorOverTrayicon;
        private bool _hideDelayStarted;
        
        public Widget Widget => null;

        public GtkNotificationAreaBox (BaseClientWindow window)
        {
            Visible = false;
            IconName = (IconThemeUtils.HasIcon ("banshee-panel")) ?
                "banshee-panel" : ServiceStack.Application.IconName;

            HasTooltip = true;
            Activate += delegate {OnActivated ();};
            PopupMenu += delegate {OnPopupMenuEvent ();};
            _popup = new TrackInfoPopup ();
        }

        public void PositionMenu (Menu menu, out int x, out int y, out bool push_in)
        {
            StatusIcon.PositionMenu (menu, out x, out y, out push_in, Handle);
        }

        private void HidePopup ()
        {
            if (_popup == null) {
                return;
            }

            _popup.Hide ();
            _popup.EnterNotifyEvent -= OnPopupEnterNotify;
            _popup.LeaveNotifyEvent -= OnPopupLeaveNotify;
            _popup.Destroy ();
            _popup.Dispose ();
            _popup = null;
        }

        private void ShowPopup ()
        {
            if (_popup != null) {
                return;
            }

            _popup = new TrackInfoPopup ();
            _popup.EnterNotifyEvent += OnPopupEnterNotify;
            _popup.LeaveNotifyEvent += OnPopupLeaveNotify;

            PositionPopup ();

            _popup.Show ();
        }

        private void OnPopupEnterNotify (object o, EnterNotifyEventArgs args)
        {
            _hideDelayStarted = false;
        }

        private void OnPopupLeaveNotify (object o, LeaveNotifyEventArgs args)
        {
            Gdk.Rectangle rect;
            if (!_popup.Intersect (new Gdk.Rectangle ((int)args.Event.X, (int)args.Event.Y, 1, 1), out rect)) {
                OnLeaveNotifyEvent (o, args);
            }
        }

        private void PositionPopup ()
        {
            int x, y;
            Gdk.Screen screen;
            Gdk.Rectangle area;
            Orientation orientation;

            GetGeometry (out screen, out area, out orientation);

            Requisition popupMinSize, popupNaturalSize;
            _popup.GetPreferredSize (out popupMinSize, out popupNaturalSize);

            bool onBottom = area.Bottom + popupNaturalSize.Height >= screen.Height;

            y = onBottom
                ? area.Top - popupNaturalSize.Height - 5
                : area.Bottom + 5;

            int monitor = screen.GetMonitorAtPoint (area.Left, y);
            var monitorRect = screen.GetMonitorGeometry(monitor);

            x = area.Left - (popupNaturalSize.Width / 2) + (area.Width / 2);

            if (x + popupNaturalSize.Width >= monitorRect.Right - 5) {
                x = monitorRect.Right - popupNaturalSize.Width - 5;
            } else if (x < monitorRect.Left + 5) {
                x = monitorRect.Left + 5;
            }

            _popup.Move (x, y);
        }

        private void OnEnterNotifyEvent (object o, EnterNotifyEventArgs args)
        {
            _hideDelayStarted = false;
            _cursorOverTrayicon = true;
            if (_canShowPopup) {
                // only show the popup when the cursor is still over the
                // tray icon after 500ms
                GLib.Timeout.Add (500, delegate {
                    if (_cursorOverTrayicon && _canShowPopup) {
                        ShowPopup ();
                    }
                    return false;
                });
            }
        }

        private void OnLeaveNotifyEvent (object o, LeaveNotifyEventArgs args)
        {
            // Give the user half a second to move the mouse cursor to the popup.
            if (!_hideDelayStarted) {
                _hideDelayStarted = true;
                _cursorOverTrayicon = false;
                GLib.Timeout.Add (500, delegate {
                    if (_hideDelayStarted) {
                        _hideDelayStarted = false;
                        HidePopup ();
                    }
                    return false;
                });
            }
        }

        public void OnPlayerEvent (PlayerEventArgs args)
        {
            switch (args.Event) {
                case PlayerEvent.StartOfStream:
                    _canShowPopup = false;
                    break;

                case PlayerEvent.EndOfStream:
                    // only hide the popup when we don't play again after 250ms
                    GLib.Timeout.Add (250, delegate {
                        if (ServiceManager.PlayerEngine.CurrentState != PlayerState.Playing) {
                            _canShowPopup = false;
                            HidePopup ();
                         }
                         return false;
                    });
                    break;
            }
        }

        protected override bool OnScrollEvent (Gdk.EventScroll evnt)
        {
            switch (evnt.Direction) {
                case Gdk.ScrollDirection.Up:
                    if ((evnt.State & Gdk.ModifierType.ControlMask) != 0) {
                        ServiceManager.PlayerEngine.Volume += PlayerEngine.VolumeDelta;
                    } else if((evnt.State & Gdk.ModifierType.ShiftMask) != 0) {
                        ServiceManager.PlayerEngine.Position += PlayerEngine.SkipDelta;
                    } else {
                        ServiceManager.PlaybackController.Next ();
                    }
                    break;

                case Gdk.ScrollDirection.Down:
                    if ((evnt.State & Gdk.ModifierType.ControlMask) != 0) {
                        if (ServiceManager.PlayerEngine.Volume < PlayerEngine.VolumeDelta) {
                            ServiceManager.PlayerEngine.Volume = 0;
                        } else {
                            ServiceManager.PlayerEngine.Volume -= PlayerEngine.VolumeDelta;
                        }
                    } else if((evnt.State & Gdk.ModifierType.ShiftMask) != 0) {
                        ServiceManager.PlayerEngine.Position -= PlayerEngine.SkipDelta;
                    } else {
                        ServiceManager.PlaybackController.Previous ();
                    }
                    break;
            }
            return true;
        }

        protected override bool OnButtonPressEvent (Gdk.EventButton evnt)
        {
            if (evnt.Type != Gdk.EventType.ButtonPress) {
                return false;
            }

            switch (evnt.Button) {
                case 1:
                    if ((evnt.State & Gdk.ModifierType.ControlMask) != 0) {
                        ServiceManager.PlaybackController.Next ();
                    } else {
                        OnActivated ();
                    }
                    break;
                case 2:
                    ServiceManager.PlayerEngine.TogglePlaying ();
                    break;
                case 3:
                    if ((evnt.State & Gdk.ModifierType.ControlMask) != 0) {
                        ServiceManager.PlaybackController.Next ();
                    } else {
                        OnPopupMenuEvent ();
                    }
                    break;
            }
            return true;
        }


        protected override bool OnQueryTooltip (int x, int y, bool keyboardMode, Tooltip tooltip)
        {
            tooltip.Custom = _popup;
            return true;
        }

        public void Show ()
        {
            Visible = true;
        }

        public void Hide ()
        {
            Visible = false;
        }

        protected virtual void OnActivated ()
        {
            EventHandler handler = Activated;
            if (handler != null) {
                handler (this, EventArgs.Empty);
            }
        }

        protected virtual void OnPopupMenuEvent ()
        {
            PopupMenuHandler handler = PopupMenuEvent;
            if (handler != null) {
                handler (this, new PopupMenuArgs ());
            }
        }
    }
}
