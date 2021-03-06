﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using TagLib;
using TagLib.Flac;
using static TagLib.File;

namespace Katatsuki.API
{
    public class Track
    {
        public string FilePath { get; }
        public string Title { get;  }
        public string Artist { get; }
        public IList<string> AlbumArtists { get; }
        public string Album { get; }
        public uint Year { get; }
        public uint TrackNumber { get;  }
        public string MusicBrainzTrackId { get; }
        public bool HasFrontCover { get; }
        public int FrontCoverHeight { get; } = 0;
        public int FrontCoverWidth { get; } = 0;
        public int Bitrate { get; }
        public int SampleRate { get; }
        public string Source { get; } = "None";
        public uint DiscNumber { get; }
        public TimeSpan Duration { get; }
        public TrackFileType FileType { get; } 

        public Track(string filename, string source = "None")
        {
            this.FilePath = Path.GetFullPath(filename);
            this.Source = source;
            IFileAbstraction f = new TagLib.File.LocalFileAbstraction(filename);
            using (var file = TagLib.File.Create(f))
            {

                switch (file.Properties.Description)
                {
                    case "Flac Audio":
                        this.FileType = Track.GetFlacType(file.Properties.BitsPerSample);
                        break;
                    case "MPEG Version 1 Audio, Layer 3":
                    case "MPEG Version 2 Audio, Layer 3":
                    case "MPEG Version 2.5 Audio, Layer 3":
                        this.FileType = TrackFileType.MP3_CBR;
                        break;
                    case "MPEG Version 1 Audio, Layer 3 VBR":
                    case "MPEG Version 2 Audio, Layer 3 VBR":
                    case "MPEG Version 2.5 Audio, Layer 3 VBR":
                        this.FileType = TrackFileType.MP3_VBR;
                        break;
                    case "MPEG-4 Audio (alac)":
                        this.FileType = TrackFileType.ALAC;
                        break;
                    case "MPEG-4 Audio (mp4a)":
                        this.FileType = TrackFileType.AAC;
                        break;
                    default:
                        this.FileType = TrackFileType.UNKNOWN;
                        break;

                }
                this.Duration = file.Properties.Duration;
                this.SampleRate = file.Properties.AudioSampleRate;
                this.Bitrate = file.Properties.AudioBitrate;
                this.Album = file.Tag.Album;
                this.AlbumArtists = file.Tag.AlbumArtists.ToList();
                this.Artist = file.Tag.FirstPerformer;
                this.TrackNumber = file.Tag.Track;
                this.Year = file.Tag.Year;
                this.MusicBrainzTrackId = file.Tag.MusicBrainzTrackId;
                this.Title = file.Tag.Title;
                this.DiscNumber = file.Tag.Disc;
                var frontAlbum = from picture in file.Tag.Pictures
                                 where picture.Type == TagLib.PictureType.FrontCover
                                 select picture;
                this.HasFrontCover = frontAlbum.Any();
                if (this.HasFrontCover)
                {
                    using (var image = Image.FromStream(new MemoryStream(frontAlbum.First().Data.Data), false, false))
                    {
                        this.FrontCoverHeight = image.Height;
                        this.FrontCoverWidth = image.Width;
                    }
                }
            }

        }

        internal Track(string FilePath, 
            string Title,
            string Artist,
            IList<string> AlbumArtists,
            string Album, uint Year, uint TrackNumber, 
            string MusicBrainzTrackId, 
            bool HasFrontCover,
            int FrontCoverHeight,
            int FrontCoverWidth,
            int Bitrate, 
            int SampleRate,
            string Source,
            uint DiscNumber,
            TimeSpan Duration,
            TrackFileType FileType
            )
        {
            this.FilePath = FilePath;
            this.Title = Title;
            this.Artist = Artist;
            this.AlbumArtists = AlbumArtists;
            this.Album = Album;
            this.Year = Year;
            this.TrackNumber = TrackNumber;
            this.MusicBrainzTrackId = MusicBrainzTrackId;
            this.HasFrontCover = HasFrontCover;
            this.FrontCoverHeight = FrontCoverHeight;
            this.FrontCoverWidth = FrontCoverWidth;
            this.Bitrate = Bitrate;
            this.SampleRate = SampleRate;
            this.Source = Source;
            this.Duration = Duration;
            this.FileType = FileType;
            this.DiscNumber = DiscNumber;
        }

        
        public override bool Equals(object obj)
        {
            if(obj is Track t) {
                return t.FilePath.Equals(this.FilePath, StringComparison.InvariantCultureIgnoreCase);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.FilePath.GetHashCode();
        }

        private static TrackFileType GetFlacType(int bitdepth)
        {
            switch (bitdepth)
            {
                case 4:
                    return TrackFileType.FLAC_4;
                case 8:
                    return TrackFileType.FLAC_8;
                case 16:
                    return TrackFileType.FLAC_16;
                case 24:
                    return TrackFileType.FLAC_24;
                case 32:
                    return TrackFileType.FLAC_32;
                default:
                    return TrackFileType.FLAC;
            }
        }
    }

   
}
