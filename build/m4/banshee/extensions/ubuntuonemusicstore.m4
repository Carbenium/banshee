AC_DEFUN([BANSHEE_CHECK_EXTENSION_UBUNTUONEMUSICSTORE],
[
	AC_ARG_ENABLE(ubuntuonemusicstore, AC_HELP_STRING([--enable-ubuntuonemusicstore], [Build UbuntuOneMusicStore]),, enable_ubuntuonemusicstore="no")
	AM_CONDITIONAL(ENABLE_UBUNTUONEMUSICSTORE, test "x$enable_ubuntuonemusicstore" = "xyes")
])
