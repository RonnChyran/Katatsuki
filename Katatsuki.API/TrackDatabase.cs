using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace Katatsuki.API
{
    public class TrackDatabase
    {
        private readonly SqliteDatabase backingDatabase;
        public TrackDatabase(string databasePath) : this(new SqliteDatabase(databasePath))
        {

        }

        internal TrackDatabase(SqliteDatabase database)
        {
            this.backingDatabase = database;
            this.CreateDatabase();
        }
        private void CreateDatabase()
        {
            this.backingDatabase.CreateTable("tracks",
                "FilePath TEXT PRIMARY KEY",
                "Title TEXT",
                "Artist TEXT",
                "AlbumArtists TEXT",
                "Album TEXT",
                "Year INTEGER",
                "TrackNumber INTEGER",
                "MusicBrainzTrackId TEXT",
                "HasFrontCover INTEGER",
                "FrontCoverWidth INTEGER",
                "FrontCoverHeight INTEGER",
                "Bitrate INTEGER",
                "SampleRate INTEGER",
                "Source TEXT",
                "DiscNumber INTEGER",
                "Duration INTEGER",
                "FileType INTEGER"
                );

            
        }
        public IEnumerable<Track> GetAllTracks()
        {
            const string sql = @"SELECT * from tracks";
            return this.backingDatabase.Query(dbConn =>
            {
                using (var query = dbConn.QueryMultiple(sql))
                {
                    return query.Read<TrackRecord>().Select(t => t.ToTrack());
                }
            });
        }

        

        public void Add(Track track)
        {
            const string sql = @"INSERT OR REPLACE INTO tracks(FilePath, 
                                        Title, Artist,
                                        AlbumArtists, Album, Year, TrackNumber,
                                        MusicBrainzTrackId, HasFrontCover, FrontCoverWidth, FrontCoverHeight, 
                                        Bitrate, SampleRate, Source, DiscNumber, Duration, FileType) 
                                        VALUES (@FilePath, 
                                        @Title, @Artist,
                                        @AlbumArtists, @Album, @Year, @TrackNumber,
                                        @MusicBrainzTrackId, @HasFrontCover, @FrontCoverWidth, @FrontCoverHeight, 
                                        @Bitrate, @SampleRate, @Source,  @DiscNumber, @Duration, @FileType)";
            this.backingDatabase.Execute((dbConn) => {
                dbConn.Execute(sql, new TrackRecord(track));
            });
           
        }

        public void Remove(Track t)
        {
            const string sql = @"DELETE FROM tracks WHERE FilePath = @FilePath";
            this.backingDatabase.Execute((dbConn) => {
                dbConn.Execute(sql, new { t.FilePath });
            });

        }
        private class TrackRecord
        {

            public string FilePath { get; set; }
            public string Title { get; set; }
            public string Artist { get; set; }
            public string AlbumArtists { get; set; }
            public string Album { get; set; }
            public uint Year { get; set; }
            public uint TrackNumber { get; }
            public string MusicBrainzTrackId { get; set; }
            public bool HasFrontCover { get; set; }
            public int FrontCoverHeight { get; set; }
            public int FrontCoverWidth { get; set; }
            public int Bitrate { get; set; }
            public int SampleRate { get; set; }
            public string Source { get; set; }
            public long Duration { get; set; }
            public uint DiscNumber { get; set; }
            public int FileType { get; set; }

            public TrackRecord()
            {

            }
            public TrackRecord(Track t)
            {
                this.FilePath = t.FilePath;
                this.Title = t.Title;
                this.Artist = t.Artist;
                this.AlbumArtists = String.Join(";", t.AlbumArtists);
                this.Album = t.Album;
                this.Year = t.Year;
                this.TrackNumber = t.TrackNumber;
                this.MusicBrainzTrackId = t.MusicBrainzTrackId;
                this.HasFrontCover = t.HasFrontCover;
                this.FrontCoverHeight = t.FrontCoverHeight;
                this.FrontCoverWidth = t.FrontCoverWidth;
                this.Bitrate = t.Bitrate;
                this.SampleRate = t.SampleRate;
                this.Source = t.Source;
                this.Duration = t.Duration.Ticks;
                this.FileType = (int)t.FileType;
            }

            public Track ToTrack()
            {
                return new Track(
                    this.FilePath,
                    this.Title,
                    this.Artist,
                    this.AlbumArtists.Split(':').ToList(),
                    this.Album,
                    this.Year,
                    this.TrackNumber,
                    this.MusicBrainzTrackId,
                    this.HasFrontCover,
                    this.FrontCoverHeight,
                    this.FrontCoverWidth,
                    this.Bitrate,
                    this.SampleRate,
                    this.Source,
                     this.DiscNumber,

                    new TimeSpan(this.Duration),

                    (TrackFileType)this.FileType);
            }

        }
    }

    
}
 