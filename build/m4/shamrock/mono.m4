AC_DEFUN([SHAMROCK_FIND_MONO_COMPILER],
[
	SHAMROCK_FIND_PROGRAM_OR_BAIL(MCS, mcs)
])

AC_DEFUN([SHAMROCK_FIND_MONO_RUNTIME],
[
	SHAMROCK_FIND_PROGRAM_OR_BAIL(MONO, mono)
])

AC_DEFUN([_SHAMROCK_CHECK_MONO_MODULE],
[
	PKG_CHECK_MODULES(MONO_MODULE, $1 >= $2)
])

AC_DEFUN([SHAMROCK_CHECK_MONO_MODULE],
[
	_SHAMROCK_CHECK_MONO_MODULE(mono, $1)
])

AC_DEFUN([SHAMROCK_CHECK_MONO2_MODULE],
[
	_SHAMROCK_CHECK_MONO_MODULE(mono-2, $1)
])

AC_DEFUN([_SHAMROCK_CHECK_MONO_MODULE_NOBAIL],
[
	PKG_CHECK_MODULES(MONO_MODULE, $2 >= $1,
		HAVE_MONO_MODULE=yes, HAVE_MONO_MODULE=no)
	AC_SUBST(HAVE_MONO_MODULE)
])

AC_DEFUN([SHAMROCK_CHECK_MONO_MODULE_NOBAIL],
[
	_SHAMROCK_CHECK_MONO_MODULE_NOBAIL(mono, $1)
])

AC_DEFUN([SHAMROCK_CHECK_MONO2_MODULE_NOBAIL],
[
	_SHAMROCK_CHECK_MONO_MODULE_NOBAIL(mono-2, $1)
])

AC_DEFUN([_SHAMROCK_CHECK_MONO_GAC_ASSEMBLIES],
[
	for asm in $(echo "$*" | cut -d, -f3- | sed 's/\,/ /g')
	do
		AC_MSG_CHECKING([for Mono $2 GAC for $asm.dll])
		if test \
			-e "$($PKG_CONFIG --variable=libdir $1)/mono/$2/$asm.dll" -o \
			-e "$($PKG_CONFIG --variable=prefix $1)/lib/mono/$2/$asm.dll"; \
			then \
			AC_MSG_RESULT([found])
		else
			AC_MSG_RESULT([not found])
			AC_MSG_ERROR([missing required Mono $2 assembly: $asm.dll])
		fi
	done
])

AC_DEFUN([SHAMROCK_CHECK_MONO2_4_5_GAC_ASSEMBLIES],
[
	_SHAMROCK_CHECK_MONO_GAC_ASSEMBLIES(mono-2, 4.5, $*)
])

