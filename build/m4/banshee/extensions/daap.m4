AC_DEFUN([BANSHEE_CHECK_EXTENSION_DAAP],
[
	AC_ARG_ENABLE(daap, AC_HELP_STRING([--enable-daap], [Build Daap]),, enable_daap="no")
	AM_CONDITIONAL(ENABLE_DAAP, test "x$enable_daap" = "xyes")
])
