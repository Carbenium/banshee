//
// MtpSource.cs
//
// Author:
//   Gabriel Burt <gburt@novell.com>
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
using System.Collections.Generic;
using System.Threading;
using Mono.Unix;

using Hyena;
using Hyena.Collections;
using Mtp;

using Banshee.Base;
using Banshee.Dap;
using Banshee.ServiceStack;
using Banshee.Library;
using Banshee.Sources;
using Banshee.Configuration;
using Banshee.Collection;
using Banshee.Collection.Database;
using Banshee.Hardware;

namespace Banshee.Dap.Mtp
{
    public class MtpSource : DapSource
    {
        // libmtp only lets us have one device connected at a time
        private static MtpSource mtp_source;

		private MtpDevice mtp_device;
        //private bool supports_jpegs = false;
        private Dictionary<int, Track> track_map;

        private Dictionary<string, Album> album_cache = new Dictionary<string, Album> ();

        private bool supports_jpegs = false;
        private bool can_sync = NeverSyncAlbumArtSchema.Get () == false;
        private int thumb_width = AlbumArtWidthSchema.Get ();

        public override void DeviceInitialize (IDevice device)
        {
            base.DeviceInitialize (device);
            
            if (MediaCapabilities == null || !MediaCapabilities.IsType ("mtp")) {
                throw new InvalidDeviceException ();
            }

            // libmtp only allows us to have one MTP device active
            if (mtp_source != null) {
                Log.Information (
                    Catalog.GetString ("MTP Support Ignoring Device"),
                    Catalog.GetString ("Banshee's MTP audio player support can only handle one device at a time."),
                    true
                );
                throw new InvalidDeviceException ();
            }

			List<MtpDevice> devices = null;
			try {
				devices = MtpDevice.Detect ();
			} catch (TypeInitializationException e) {
                Log.Exception (e);
				Log.Error (
                    Catalog.GetString ("Error Initializing MTP Device Support"),
                    Catalog.GetString ("There was an error intializing MTP device support.  See http://www.banshee-project.org/Guide/DAPs/MTP for more information."), true
                );
                throw new InvalidDeviceException ();
			} catch (Exception e) {
                Log.Exception (e);
				//ShowGeneralExceptionDialog (e);
                throw new InvalidDeviceException ();
			}

            if (devices == null || devices.Count == 0) {
				Log.Error (
                    Catalog.GetString ("Error Finding MTP Device Support"),
                    Catalog.GetString ("An MTP device was detected, but Banshee was unable to load support for it."), true
                );
            } else {
                string mtp_serial = devices[0].SerialNumber;
                if (!String.IsNullOrEmpty (mtp_serial)) {
                    if (mtp_serial.Contains (device.Serial)) {
                        mtp_device = devices[0];
                        mtp_source = this;
                    }
                }

                if (mtp_device == null) {
                    Log.Information(
                        Catalog.GetString ("MTP Support Ignoring Device"),
                        Catalog.GetString ("Banshee's MTP audio player support can only handle one device at a time."),
                        true
                    );
                }
            }

            if (mtp_device == null) {
                throw new InvalidDeviceException ();
            }

            Name = mtp_device.Name;
            Initialize ();

            List<string> mimetypes = new List<string> ();
            foreach (FileType format in mtp_device.GetFileTypes ()) {
                if (format == FileType.JPEG) {
                    supports_jpegs = true;
                } else {
                    string mimetype = MtpDevice.GetMimeTypeFor (format);
                    if (mimetype != null) {
                        mimetypes.Add (mimetype);
                    }
                }
            }
            AcceptableMimeTypes = mimetypes.ToArray ();

            AddDapProperty (Catalog.GetString ("Serial number"), mtp_device.SerialNumber);
            AddDapProperty (Catalog.GetString ("Version"), mtp_device.Version);
            AddDapProperty (Catalog.GetString ("Battery level"), String.Format ("{0:0%}", mtp_device.BatteryLevel/100.0));
        }

        protected override void LoadFromDevice ()
        {
            track_map = new Dictionary<int, Track> ();
            try {
                List<Track> files = mtp_device.GetAllTracks (delegate (ulong current, ulong total, IntPtr data) {
                    //user_event.Progress = (double)current / total;
                    // Translators: {0} is the name of the MTP audio device (eg Gabe's Zen Player), {1} is the
                    // current track being loaded, and {2} is the total # of tracks that will be loaded.
                    SetStatus (String.Format (Catalog.GetString ("Loading {0} - {1} of {2}"), Name, current, total), false);
                    return 0;
                });
                
                /*if (user_event.IsCancelRequested) {
                    return;
                }*/
                
                int [] source_ids = new int [] { DbId };
                foreach (Track mtp_track in files) {
                    int track_id;
                    if ((track_id = DatabaseTrackInfo.GetTrackIdForUri (MtpTrackInfo.GetPathFromMtpTrack (mtp_track), source_ids)) > 0) {
                        track_map[track_id] = mtp_track;
                    } else {
                        MtpTrackInfo track = new MtpTrackInfo (mtp_device, mtp_track);
                        track.PrimarySource = this;
                        track.Save (false);
                        track_map[track.TrackId] = mtp_track;
                    }
                }
            } catch (Exception e) {
                Log.Exception (e);
            }
        }

        public override void Import ()
        {
            Log.Information ("Import to Library is not implemented for MTP devices yet", true);
            //new LibraryImportManager (true).QueueSource (BaseDirectory);
        }

        public override void CopyTrackTo (DatabaseTrackInfo track, SafeUri uri, BatchUserJob job)
        {
            if (track_map.ContainsKey (track.TrackId)) {
                track_map[track.TrackId].Download (uri.LocalPath, delegate (ulong current, ulong total, IntPtr data) {
                    job.DetailedProgress = (double) current / total;
                    return 0;
                });
            } else {
                throw new Exception ("Error copying track from MTP device");
            }
        }

        public override bool CanRename {
            get { return !(IsAdding || IsDeleting); }
        }

        private static SafeUri empty_file = new SafeUri (Paths.Combine (Paths.ApplicationCache, "mtp.mp3"));
        protected override void OnTracksDeleted ()
        {
            // Hack to get the disk usage indicate to be accurate, which seems to
            // only be updated when tracks are added, not removed.
            try {
                lock (mtp_device) {
                    using (System.IO.TextWriter writer = new System.IO.StreamWriter (Banshee.IO.File.OpenWrite (empty_file, true))) {
                        writer.Write ("foo");
                    }
                    Track mtp_track = new Track (System.IO.Path.GetFileName (empty_file.LocalPath), 3);

                    mtp_device.UploadTrack (empty_file.AbsolutePath, mtp_track, mtp_device.MusicFolder);
                    mtp_device.Remove (mtp_track);
                    Banshee.IO.File.Delete (empty_file);
                }
            } catch {}
            base.OnTracksDeleted ();
        }

        public override void Rename (string newName)
        {
            base.Rename (newName);
            lock (mtp_device) {
                mtp_device.Name = newName;
            }
        }

        public override long BytesUsed {
            get {
				long count = 0;
                lock (mtp_device) {
                    foreach (DeviceStorage s in mtp_device.GetStorage ()) {
                        count += (long) s.MaxCapacity - (long) s.FreeSpaceInBytes;
                    }
                }
				return count;
            }
        }
        
        public override long BytesCapacity {
            get {
				long count = 0;
                lock (mtp_device) {
                    foreach (DeviceStorage s in mtp_device.GetStorage ()) {
                        count += (long) s.MaxCapacity;
                    }
                }
				return count;
            }
        }

        public override bool IsReadOnly {
            get { return false; }
        }

        protected override void AddTrackToDevice (DatabaseTrackInfo track, SafeUri fromUri)
        {
            if (track.PrimarySourceId == DbId)
                return;

            Track mtp_track = TrackInfoToMtpTrack (track, fromUri);
            bool video = (track.MediaAttributes & TrackMediaAttributes.VideoStream) != 0;
            Console.WriteLine ("Sending file {0}, is video? {1}", fromUri.LocalPath, video);
            lock (mtp_device) {
                mtp_device.UploadTrack (fromUri.LocalPath, mtp_track, GetFolderForTrack (track), OnUploadProgress);
            }

            // Add/update album art
            if (!video) {
                string key = MakeAlbumKey (track.ArtistName, track.AlbumTitle);
                if (!album_cache.ContainsKey (key)) {
                    Album album = new Album (mtp_device, track.AlbumTitle, track.ArtistName, track.Genre);
                    album.AddTrack (mtp_track);

                    if (supports_jpegs && can_sync) {
                        try {
                            Gdk.Pixbuf pic = ServiceManager.Get<Banshee.Collection.Gui.ArtworkManager> ().LookupScale (
                                CoverArtSpec.CreateArtistAlbumId (track.ArtistName, track.AlbumTitle), thumb_width
                            );
                            if (pic != null) {
                                byte [] bytes = pic.SaveToBuffer ("jpeg");
                                album.Save (bytes, (uint)pic.Width, (uint)pic.Height);
                                pic.Dispose ();
                            }
                            album_cache[key] = album;
                        } catch {}
                    }
                } else {
                    Album album = album_cache[key];
                    album.AddTrack (mtp_track);
                    album.Save ();
                }
            }

            MtpTrackInfo new_track = new MtpTrackInfo (mtp_device, mtp_track);
            new_track.PrimarySource = this;
            new_track.Save (false);
            track_map[new_track.TrackId] = mtp_track;
        }

        private Folder GetFolderForTrack (TrackInfo track)
        {
            if (track.HasAttribute (TrackMediaAttributes.Podcast))
                return mtp_device.PodcastFolder;
            else if (track.HasAttribute (TrackMediaAttributes.VideoStream))
                return mtp_device.VideoFolder;
            else
                return mtp_device.MusicFolder;
        }

        private int OnUploadProgress (ulong sent, ulong total, IntPtr data)
        {
            AddTrackJob.DetailedProgress = (double) sent / (double) total;
            return 0;
        }

        protected override void DeleteTrack (DatabaseTrackInfo track)
        {
            lock (mtp_device) {
                Track mtp_track = track_map [track.TrackId];
                track_map.Remove (track.TrackId);

                // Remove from device
                mtp_device.Remove (mtp_track);

                // Remove track from album, and remove album from device if it no longer has tracks
                string key = MakeAlbumKey (track.ArtistName, track.AlbumTitle);
                if (album_cache.ContainsKey (key)) {
                    Album album = album_cache[key];
                    album.RemoveTrack (mtp_track);
                    if (album.TrackCount == 0) {
                        album.Remove ();
                        album_cache.Remove (key);
                    }
                }
            }
        }

        public Track TrackInfoToMtpTrack (TrackInfo track, SafeUri fromUri)
        {
			Track f = new Track (System.IO.Path.GetFileName (fromUri.LocalPath), (ulong) Banshee.IO.File.GetSize (fromUri));
            MtpTrackInfo.ToMtpTrack (track, f);
            return f;
        }

        public override void Dispose ()
        {
			base.Dispose ();

            if (mtp_device != null) {
                lock (mtp_device) {
                    mtp_device.Dispose ();
                }
            }

            ServiceManager.SourceManager.RemoveSource (this);

            mtp_device = null;
            mtp_source = null;
        }

        protected override void Eject ()
        {
            Dispose ();
        }

        private static string MakeAlbumKey (string artist, string album)
        {
            return String.Format ("{0}_{1}", artist, album);
        }

        public static readonly SchemaEntry<bool> NeverSyncAlbumArtSchema = new SchemaEntry<bool>(
            "plugins.mtp", "never_sync_albumart",
            false,
            "Album art disabled",
            "Regardless of device's capabilities, do not sync album art"
        );

        public static readonly SchemaEntry<int> AlbumArtWidthSchema = new SchemaEntry<int>(
            "plugins.mtp", "albumart_max_width",
            170,
            "Album art max width",
            "The maximum width to allow for album art."
        );
    }
}
