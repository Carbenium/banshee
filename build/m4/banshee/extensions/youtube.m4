AC_DEFUN([BANSHEE_CHECK_EXTENSION_YOUTUBE],
[
	AC_ARG_ENABLE(youtube, AC_HELP_STRING([--enable-youtube], [Build Youtube]),, enable_youtube="no")
	AM_CONDITIONAL(ENABLE_YOUTUBE, test "x$enable_youtube" = "xyes")
])
