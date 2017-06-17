SUBDIRS = \
	Banshee.Audiobook \
	Banshee.BooScript \
	Banshee.Bpm \
	Banshee.CoverArt \
	Banshee.Daap \
	Banshee.Emusic \
	Banshee.FileSystemQueue \
	Banshee.InternetArchive \
	Banshee.Fixup \
	Banshee.InternetRadio \
	Banshee.Lastfm \
	Banshee.LastfmStreaming \
	Banshee.LibraryWatcher \
	Banshee.MiniMode \
	Banshee.Mpris \
	Banshee.MultimediaKeys \
	Banshee.NotificationArea \
	Banshee.OpticalDisc \
	Banshee.PlayerMigration \
	Banshee.PlayQueue \
	Banshee.MediaPanel \
	Banshee.Podcasting \
	Banshee.Sample \
	Banshee.SoundMenu \
	Banshee.SqlDebugConsole \
	Banshee.Torrent \
	Banshee.RemoteAudio \
	Banshee.UbuntuOneMusicStore \
	Banshee.UPnPClient \
	Banshee.YouTube

MAINTAINERCLEANFILES = Makefile.in

uninstall-hook:
	if test -d "$(pkglibdir)/Extensions"; then \
		rmdir "$(pkglibdir)/Extensions"; \
	fi
