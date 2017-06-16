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
