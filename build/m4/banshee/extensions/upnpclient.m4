AC_DEFUN([BANSHEE_CHECK_EXTENSION_UPNPCLIENT],
[
	AC_ARG_ENABLE(upnpclient, AC_HELP_STRING([--enable-upnpclient], [Build UPnPClient]),, enable_upnpclient="no")
	AM_CONDITIONAL(ENABLE_UPNPCLIENT, test "x$enable_upnpclient" = "xyes")
])
