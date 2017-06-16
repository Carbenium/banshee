AC_DEFUN([BANSHEE_CHECK_EXTENSION_TORRENT],
[
	AC_ARG_ENABLE(torrent, AC_HELP_STRING([--enable-torrent], [Build Torrent]),, enable_torrent="no")
	AM_CONDITIONAL(ENABLE_TORRENT, test "x$enable_torrent" = "xyes")
])
