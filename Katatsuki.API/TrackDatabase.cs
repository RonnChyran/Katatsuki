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
        public TrackDatabase(string databasePath): this(new SqliteDatabase(databasePath))
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
                "MusixBrainzTrackId TEXT",
                "HasFrontCover TEXT",
                "FrontCoverWidth INTEGER",
                "FrontCoverHeight INTEGER");
        }
        public IEnumerable<Track> GetAllTracks()
        {
            
            const string sql = @"SELECT * from tracks";
            return this.backingDatabase.Query<IEnumerable<Track>>(dbConn =>
            {
                using (var query = dbConn.QueryMultiple(sql))
                {
                    try
                    {
                        var tracks = query.Read<Track>();
                        throw new NotImplementedException();
                    }
                    catch
                    {
                        throw new NotImplementedException();
                    }
                }
            });
        }

        public IEnumerable<Track> GetAlbum()
        {
            throw new NotImplementedException();

        }

        public IEnumerable<Track> GetByArtist()
        {
            throw new NotImplementedException();

        }

        public void AddTrack(Track track)
        {

        }
    }
}
