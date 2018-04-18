using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace Katatsuki.API
{
    public class Library
    {
        public string LibraryPath { get; }
        private static readonly string[] FileMasks = new string[] { ".flac", ".mp3", ".m4a" };
        public ICollection<Track> LibraryTracks => this.libraryTracks.Values;
        IDictionary<string, Track> libraryTracks;
        public event EventHandler<Track> TrackAddedEvent;
        public event EventHandler<Track> TrackDeletedEvent;
        public Library(string libraryPath, IDictionary<string, Track> tracks)
        {
            this.libraryTracks = tracks;
            this.LibraryPath = Path.GetDirectoryName(libraryPath);
            this.EnsureDirectory(this.LibraryPath);
        }


        public Library(string libraryPath):this(libraryPath, new Dictionary<string, Track>()) { }

        public Track Add(Track t)
        {
            string trackDir = this.GetTrackDirectory(t);
            string trackFileName = this.GetSongFilename(t, trackDir);
            File.Move(t.FilePath, trackFileName);
            var track = new Track(trackFileName, t.Source);
            this.libraryTracks[t.FilePath] = track;
            this.TrackAddedEvent?.Invoke(this, track);
            return track;
        }

        public Track Refresh(Track t)
        {
            if(!File.Exists(t.FilePath))
            {
                this.libraryTracks.Remove(t.FilePath);
                this.TrackDeletedEvent?.Invoke(this, t);
                return null;
            }

            try
            {
                var track = new Track(t.FilePath, t.Source);
                this.libraryTracks[t.FilePath] = track;
                this.TrackAddedEvent?.Invoke(this, track);
                return track;
            }
            catch(IOException)
            {
                this.libraryTracks.Remove(t.FilePath);
                this.TrackDeletedEvent?.Invoke(this, t);
                return null;
            }
            catch (TagLib.CorruptFileException)
            {
                this.libraryTracks.Remove(t.FilePath);
                this.TrackDeletedEvent?.Invoke(this, t);
                return null;
            }
        }
        

        private string EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        private string SanitiseFileName(string path)
        {
            var invalids = Path.GetInvalidFileNameChars();
            return String.Join("_", path.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }

        private string GetTrackDirectory(Track t)
        {
            return this.EnsureDirectory(
                Path.Combine(this.LibraryPath, this.SanitiseFileName(t.AlbumArtists.Count > 0 ? String.Join(", ", t.AlbumArtists) 
                : (String.IsNullOrWhiteSpace(t.Artist) ? "Unknown Artist" : t.Artist)), this.SanitiseFileName(t.Album)));
        }

        private string GetSongFilename(Track t, string destination, int iterations = 0)
        {
            string extension = Path.GetExtension(t.FilePath);
            string newFilename = this.SanitiseFileName($"{t.DiscNumber:0}-{t.TrackNumber:00} {t.Title}{(iterations > 0 ? $" ({iterations})" :"")}{extension}");
            if (File.Exists(Path.Combine(destination, newFilename))) return this.GetSongFilename(t, destination, ++iterations);
            return Path.Combine(destination, newFilename);

        }
    }
}
