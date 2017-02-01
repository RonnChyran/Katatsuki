using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Katatsuki.API
{
    public enum TrackFileType
    {
        [Description("FLAC")]
        FLAC,
        [Description("FLAC (4-bit)")]
        FLAC_4,
        [Description("FLAC (8-bit)")]
        FLAC_8,
        [Description("FLAC (16-bit)")]
        FLAC_16,
        [Description("FLAC (24-bit)")]
        FLAC_24,
        [Description("FLAC (32-bit)")]
        FLAC_32,
        [Description("Apple Lossless")]
        ALAC,
        [Description("MP3 (Constant Bitrate)")]
        MP3_CBR,
        [Description("MP3 (Variable Bitrate)")]
        MP3_VBR,
        [Description("AAC")]
        AAC,
        [Description("Unknown")]
        UNKNOWN
    }
}
