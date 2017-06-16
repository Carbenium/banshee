AC_DEFUN([BANSHEE_CHECK_WINDOWS],
[
    enable_windows="no"
    if test "x${host_os%${host_os#???????}}" = "xWindows"; then
        enable_windows="yes"
    fi

    AM_CONDITIONAL([PLATFORM_WINDOWS], [test "x$enable_windows" = "xyes"])
])
