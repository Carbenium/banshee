if !EXTERNAL_NOTIFY_SHARP
MCS_FLAGS+="-define:INTERNAL_NOTIFY_SHARP"
endif

ASSEMBLY = Banshee.NotificationArea
TARGET = library
LINK = $(REF_EXTENSION_NOTIFICATIONAREA) $(NOTIFY_SHARP_LIBS)
INSTALL_DIR = $(EXTENSIONS_INSTALL_DIR)

SOURCES =  \
	Banshee.NotificationArea/GtkNotificationAreaBox.cs \
	Banshee.NotificationArea/INotificationAreaBox.cs \
	Banshee.NotificationArea/NotificationAreaService.cs \
	Banshee.NotificationArea/TrackInfoPopup.cs \
	Notifications/Notification.cs \
	Notifications/Notifications.cs

RESOURCES =  \
	Banshee.NotificationArea.addin.xml \
	Resources/NotificationAreaMenu.xml

if PLATFORM_DARWIN
include $(top_srcdir)/build/build.dist.mk
else
if ENABLE_NOTIFICATIONAREA
include $(top_srcdir)/build/build.mk
module_SCRIPTS += Banshee.NotificationArea.dll.config
else
include $(top_srcdir)/build/build.dist.mk
endif
endif

EXTRA_DIST += Banshee.NotificationArea.dll.config
