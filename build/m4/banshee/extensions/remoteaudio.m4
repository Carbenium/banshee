AC_DEFUN([BANSHEE_CHECK_EXTENSION_REMOTEAUDIO],
[
	AC_ARG_ENABLE(remoteaudio, AC_HELP_STRING([--enable-remoteaudio], [Build RemoteAudio]),, enable_remoteaudio="no")
	AM_CONDITIONAL(ENABLE_REMOTEAUDIO, test "x$enable_remoteaudio" = "xyes")
])
