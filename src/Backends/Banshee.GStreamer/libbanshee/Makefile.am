AM_CPPFLAGS = \
	-I$(top_srcdir) \
	-Wall \
	-ggdb3 \
	-D_FORTIFY_SOURCE=2 \
	$(LIBBANSHEE_CFLAGS) \
	$(GST_CFLAGS) 

bansheelibdir = $(pkglibdir)

bansheelib_LTLIBRARIES = 
if ENABLE_GST_NATIVE
bansheelib_LTLIBRARIES += libbanshee.la
all: $(top_builddir)/bin/libbanshee.so
endif

libbanshee_la_LDFLAGS = -avoid-version -module
libbanshee_la_SOURCES =  \
	banshee-bpmdetector.c \
	banshee-gst.c \
	banshee-player.c \
	banshee-player-cdda.c \
	banshee-player-equalizer.c \
	banshee-player-missing-elements.c \
	banshee-player-pipeline.c \
	banshee-player-replaygain.c \
	banshee-player-video.c \
	banshee-player-vis.c \
	banshee-ripper.c \
	banshee-tagger.c \
	banshee-transcoder.c

noinst_DATA =

noinst_HEADERS =  \
	banshee-gst.h \
	banshee-player-cdda.h \
	banshee-player-equalizer.h \
	banshee-player-missing-elements.h \
	banshee-player-pipeline.h \
	banshee-player-private.h \
	banshee-player-replaygain.h \
	banshee-player-video.h \
	banshee-player-vis.h \
	banshee-tagger.h

libbanshee_la_LIBADD = \
	$(LIBBANSHEE_LIBS) \
	$(GST_LIBS)

$(top_builddir)/bin/libbanshee.so: libbanshee.la
	mkdir -p $(top_builddir)/bin
	cp -f .libs/libbanshee.so $@

CLEANFILES = $(top_builddir)/bin/libbanshee.so
MAINTAINERCLEANFILES = Makefile.in
EXTRA_DIST = $(libbanshee_la_SOURCES)
