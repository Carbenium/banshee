//
// banshee-player-video.c
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//   Julien Moutte <julien@fluendo.com>
//
// Copyright (C) 2005-2008 Novell, Inc.
// Copyright (C) 2010 Fluendo S.A.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#include "banshee-player-video.h"

// ---------------------------------------------------------------------------
// Private Functions
// ---------------------------------------------------------------------------

#if defined(GDK_WINDOWING_X11) || defined(GDK_WINDOWING_WIN32)

static gboolean
bp_video_find_video_overlay (BansheePlayer *player)
{
    GstElement *video_sink = NULL;
    GstElement *video_overlay;
    GstVideoOverlay *previous_video_overlay;
    gboolean    found_video_overlay;

    g_object_get (player->playbin, "video-sink", &video_sink, NULL);

    g_mutex_lock (player->video_mutex);
    previous_video_overlay = player->video_overlay;

    if (video_sink == NULL) {
        player->video_overlay = NULL;
        if (previous_video_overlay != NULL) {
            gst_object_unref (previous_video_overlay);
        }
        g_mutex_unlock (player->video_mutex);
        return FALSE;
    }
   
    video_overlay = GST_IS_BIN (video_sink)
        ? gst_bin_get_by_interface (GST_BIN (video_sink), GST_TYPE_VIDEO_OVERLAY)
        : video_sink;
    
    player->video_overlay = GST_IS_VIDEO_OVERLAY (video_overlay) ? GST_VIDEO_OVERLAY (video_overlay) : NULL;
    
    if (previous_video_overlay != NULL) {
        gst_object_unref (previous_video_overlay);
    }
        
#if !defined(GDK_WINDOWING_WIN32) // We can't rely on aspect ratio from dshowvideosink
    if (player->video_overlay != NULL && g_object_class_find_property (
        G_OBJECT_GET_CLASS (player->video_overlay), "force-aspect-ratio")) {
        g_object_set (G_OBJECT (player->video_overlay), "force-aspect-ratio", TRUE, NULL);
    }
#endif
    
    if (player->video_overlay != NULL && g_object_class_find_property (
        G_OBJECT_GET_CLASS (player->video_overlay), "handle-events")) {
        g_object_set (G_OBJECT (player->video_overlay), "handle-events", FALSE, NULL);
    }

    gst_object_unref (video_sink);
    found_video_overlay = (player->video_overlay != NULL) ? TRUE : FALSE;

    g_mutex_unlock (player->video_mutex);
    return found_video_overlay;
}

#endif /* GDK_WINDOWING_X11 || GDK_WINDOWING_WIN32 */

static void
bp_video_sink_element_added (GstBin *videosink, GstElement *element, BansheePlayer *player)
{
    g_return_if_fail (IS_BANSHEE_PLAYER (player));

    #if defined(GDK_WINDOWING_X11) || defined(GDK_WINDOWING_WIN32)
    bp_video_find_video_overlay (player);
    #endif
}

static void
bp_video_bus_element_sync_message (GstBus *bus, GstMessage *message, BansheePlayer *player)
{
    gboolean found_video_overlay;
    
    g_return_if_fail (IS_BANSHEE_PLAYER (player));

    #if defined(GDK_WINDOWING_X11) || defined(GDK_WINDOWING_WIN32)

    if (!gst_is_video_overlay_prepare_window_handle_message (message)) {
        return;
    }

    found_video_overlay = bp_video_find_video_overlay (player);

    if (found_video_overlay) {
        gst_video_overlay_set_window_handle (player->video_overlay, player->video_window_xid);
        gst_video_overlay_handle_events (player->video_overlay, TRUE);
    }

    #endif
}

// ---------------------------------------------------------------------------
// Internal Functions
// ---------------------------------------------------------------------------

static void
cb_caps_set (GObject *obj, GParamSpec *pspec, BansheePlayer *p)
{
    GstStructure * s = NULL;
    GstCaps * caps = gst_pad_get_current_caps (GST_PAD (obj));

    if (G_UNLIKELY (!caps)) {
        return;
    }

    /* Get video decoder caps */
    s = gst_caps_get_structure (caps, 0);
    if (s) {
        const GValue *par;

        /* We need at least width/height and framerate */
        if (!(gst_structure_get_fraction (s, "framerate", &p->fps_n, &p->fps_d) &&
            gst_structure_get_int (s, "width", &p->width) && gst_structure_get_int (s, "height", &p->height))) {
            return;
        }

        /* Get the PAR if available */
        par = gst_structure_get_value (s, "pixel-aspect-ratio");
        if (par) {
            p->par_n = gst_value_get_fraction_numerator (par);
            p->par_d = gst_value_get_fraction_denominator (par);
        }
        else { /* Square pixels */
            p->par_n = 1;
            p->par_d = 1;
        }
    }

    gst_caps_unref (caps);
}

void
_bp_parse_stream_info (BansheePlayer *player)
{
    gint audios_streams, video_streams, text_streams;
    GstPad *vpad = NULL;

    g_object_get (G_OBJECT (player->playbin), "n-audio", &audios_streams,
        "n-video", &video_streams, "n-text", &text_streams, NULL);

    if (video_streams) {
        gint i;
        /* Try to obtain a video pad */
        for (i = 0; i < video_streams && vpad == NULL; i++) {
            g_signal_emit_by_name (player->playbin, "get-video-pad", i, &vpad);
        }
    }

    if (G_LIKELY (vpad)) {
        GstCaps *caps = gst_pad_get_current_caps (vpad);
        if (G_LIKELY (caps)) {
            cb_caps_set (G_OBJECT (vpad), NULL, player);
            gst_caps_unref (caps);
        }
        g_signal_connect (vpad, "notify::caps", G_CALLBACK (cb_caps_set), player);
        gst_object_unref (vpad);
    }
}

void
_bp_video_pipeline_setup (BansheePlayer *player, GstBus *bus)
{
    GstElement *videosink;
    
    g_return_if_fail (IS_BANSHEE_PLAYER (player));
    
    if (player->video_pipeline_setup_cb != NULL) {
        videosink = player->video_pipeline_setup_cb (player, bus);
        if (videosink != NULL && GST_IS_ELEMENT (videosink)) {
            g_object_set (G_OBJECT (player->playbin), "video-sink", videosink, NULL);
            return;
        }
    }
    
    videosink = gst_element_factory_make ("autovideosink", "videosink");
    if (videosink == NULL) {
        videosink = gst_element_factory_make ("fakesink", "videosink");
        if (videosink != NULL) {
            g_object_set (G_OBJECT (videosink), "sync", TRUE, NULL);
        }
    }
    
    g_object_set (G_OBJECT (player->playbin), "video-sink", videosink, NULL);
    
    gst_bus_set_sync_handler (bus, gst_bus_sync_signal_handler, player, NULL);
    g_signal_connect (bus, "sync-message::element", G_CALLBACK (bp_video_bus_element_sync_message), player);
        
    if (GST_IS_BIN (videosink)) {
        g_signal_connect (videosink, "element-added", G_CALLBACK (bp_video_sink_element_added), player);
    }

    #ifndef WIN32

    videosink = gst_element_factory_make ("fakesink", "videosink");
    if (videosink != NULL) {
        g_object_set (G_OBJECT (videosink), "sync", TRUE, NULL);
    }
    
    g_object_set (G_OBJECT (player->playbin), "video-sink", videosink, NULL);

    #endif

    if (player->video_prepare_window_cb != NULL) {
        player->video_prepare_window_cb (player);
    }
}

P_INVOKE void
bp_set_video_pipeline_setup_callback (BansheePlayer *player, BansheePlayerVideoPipelineSetupCallback cb)
{
    SET_CALLBACK (video_pipeline_setup_cb);
}

P_INVOKE void
bp_set_video_prepare_window_callback (BansheePlayer *player, BansheePlayerVideoPrepareWindowCallback cb)
{
    SET_CALLBACK (video_prepare_window_cb);
}
