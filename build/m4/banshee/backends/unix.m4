AC_DEFUN([BANSHEE_CHECK_UNIX],
[
    enable_unix="no"

    if test "x${host_os%${host_os#?????????}}" = "xlinux-gnu"; then
        enable_unix="yes"
    fi

    AM_CONDITIONAL([PLATFORM_UNIX], [test "x$enable_unix" = "xyes"])
])
