AC_DEFUN([BANSHEE_CHECK_EXTENSION_BOOSCRIPT],
[
	AC_ARG_ENABLE(booscript, AC_HELP_STRING([--enable-booscript], [Build BooScript]),, enable_booscript="no")
	AM_CONDITIONAL(ENABLE_BOOSCRIPT, test "x$enable_booscript" = "xyes")
])
