AC_DEFUN([BANSHEE_CHECK_GSTREAMERSHARP],
[
    GSTREAMER_SHARP_REQUIRED_VERSION=0.99.0

    AC_ARG_ENABLE(gst_sharp, AC_HELP_STRING([--enable-gst-sharp], [Enable Gst# backend]), , enable_gst_sharp="no")

    if test "x$enable_gst_sharp" = "xyes"; then
        PKG_CHECK_MODULES(GST_SHARP, gstreamer-sharp-1.0 >= $GSTREAMER_SHARP_REQUIRED_VERSION)
        AC_SUBST(GST_SHARP_LIBS)

        AM_CONDITIONAL(ENABLE_GST_SHARP, true)
    else
        AM_CONDITIONAL(ENABLE_GST_SHARP, false)
    fi
])

