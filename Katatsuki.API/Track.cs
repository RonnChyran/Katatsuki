using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib.Flac;
using Fasterflect;
using System.IO;

namespace Katatsuki.API
{
    public class Track
    {
        public string FilePath { get; }
        public string Title { get; }
        public string Artist { get; }
        public IList<string> AlbumArtists { get; }
        public string Album { get; }
        public uint Year { get; }
        public uint TrackNumber { get; }
        public string MusicBrainzTrackId { get; }
        public bool HasFrontCover { get; }
        public int FrontCoverHeight { get; } = 0;
        public int FrontCoverWidth { get; } = 0;
        public int Bitrate { get; }
        public int BitDepth { get; }
        public int SampleRate { get; }
        public string Codec { get; }
        public string Source { get; } = "None";
        public TimeSpan Duration { get; }
        public TrackFileType FileType { get; } 
        public Track(string filename, string source = "None")
        {
            this.FilePath = Path.GetFullPath(filename);
            this.Source = source;
            
            var file = TagLib.File.Create(filename);
            switch(file.MimeType)
            {
                case "taglib/flac":
                    this.FileType = TrackFileType.FLAC;
                    break;
                case "taglib/mp3":
                    if (file.Properties.Description == "MPEG Version 1 Audio, Layer 3 VBR")
                    {
                        this.FileType = TrackFileType.MP3_VBR;
                        break;
                    }
                    this.FileType = TrackFileType.MP3_CBR;
                    break;
                case "taglib/m4a":
                    if (file.Properties.Description == "MPEG-4 Audio (alac)")
                    {
                        this.FileType = TrackFileType.ALAC;
                    }
                    this.FileType = TrackFileType.AAC_CBR;

                    break;
                default:
                    this.FileType = TrackFileType.UNKNOWN;
                    break;

            }
            this.Duration = file.Properties.Duration;
            this.Bitrate = file.Properties.AudioBitrate;
            this.SampleRate = file.Properties.AudioSampleRate;
            this.BitDepth = file.Properties.BitsPerSample;
            this.Album = file.Tag.Album;
            this.AlbumArtists = file.Tag.AlbumArtists.ToList();
            this.Artist = file.Tag.FirstPerformer;
            this.TrackNumber = file.Tag.Track;
            this.Year = file.Tag.Year;
            this.MusicBrainzTrackId = file.Tag.MusicBrainzTrackId;
            this.Title = file.Tag.Title;
            var frontAlbum = from picture in file.Tag.Pictures
                             where picture.Type == TagLib.PictureType.FrontCover
                             select picture;
            this.HasFrontCover = frontAlbum.Any();
            if(this.HasFrontCover)
            {
                using (var image = Image.FromStream(new MemoryStream(frontAlbum.First().Data.Data)))
                {
                    this.FrontCoverHeight = image.Height;
                    this.FrontCoverWidth = image.Width;
                }
            }
        }
    }
}
