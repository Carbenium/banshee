ASSEMBLY = Banshee.UbuntuOneMusicStore
TARGET = library
LINK = $(REF_EXTENSION_UBUNTUONEMUSICSTORE)
INSTALL_DIR = $(EXTENSIONS_INSTALL_DIR)

SOURCES =  \
	Banshee.UbuntuOneMusicStore/Tests/MusicStoreTests.cs \
	Banshee.UbuntuOneMusicStore/UbuntuOneMusicStoreSource.cs

RESOURCES = Banshee.UbuntuOneMusicStore.addin.xml

if ENABLE_UBUNTUONE
if ENABLE_UBUNTUONEMUSICSTORE
include $(top_srcdir)/build/build.mk
else
include $(top_srcdir)/build/build.dist.mk
endif
else
include $(top_srcdir)/build/build.dist.mk
endif
