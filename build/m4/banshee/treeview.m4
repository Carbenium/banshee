AC_DEFUN([BANSHEE_CHECK_TREEVIEW],
[
	AC_ARG_ENABLE(treeview, AC_HELP_STRING([--enable-treeview], [Use TreeView]),, enable_treeview="no")
	AM_CONDITIONAL(ENABLE_TREEVIEW, test "x$enable_treeview" = "xyes")
])
