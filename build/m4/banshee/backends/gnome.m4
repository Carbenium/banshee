AC_DEFUN([BANSHEE_CHECK_GNOME],
[
    AC_ARG_ENABLE(gnome, AC_HELP_STRING([--enable-gnome], [Enable GNOME support]), , enable_gnome="no")
    if test "x$enable_gnome" = "xyes"; then
        BANSHEE_CHECK_GNOME_SHARP
        BANSHEE_CHECK_GCONF
        AM_CONDITIONAL(ENABLE_GNOME, true)
    else
        AM_CONDITIONAL(GCONF_SCHEMAS_INSTALL, false)
        AM_CONDITIONAL(ENABLE_GNOME, false)
    fi
    AM_CONDITIONAL([PLATFORM_GNOME], [test "x$enable_gnome" = "xyes"])
])
