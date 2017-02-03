using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace Katatsuki.API
{
    public class Library
    {
        public string LibraryPath { get; }
        IList<Track> LibraryTracks { get; }

        public Library(string libaryPath)
        {
            this.LibraryTracks = new List<Track>();
            this.LibraryPath = Path.GetDirectoryName(libaryPath);
            this.EnsureDirectory(this.LibraryPath);
        }
        public Track Add(Track t)
        {
            string trackDir = this.GetTrackDirectory(t);
            string trackFileName = this.GetSongFilename(t, trackDir);
            File.Move(t.FilePath, trackFileName);
            var track = new Track(trackFileName);
            this.LibraryTracks.Add(track);
            return track;
        }

        public string EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }


        public string SanitiseFileName(string path)
        {
            var invalids = Path.GetInvalidFileNameChars();
            return String.Join("_", path.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }
        public string GetTrackDirectory(Track t)
        {
            return this.EnsureDirectory(
                Path.Combine(this.LibraryPath, this.SanitiseFileName(t.AlbumArtists.Count > 0 ? String.Join(", ", t.AlbumArtists) 
                : (String.IsNullOrWhiteSpace(t.Artist) ? "Unknown Artist" : t.Artist)), this.SanitiseFileName(t.Album)));
        }

        public string GetSongFilename(Track t, string destination, int iterations = 0)
        {
            string extension = Path.GetExtension(t.FilePath);
            string newFilename = this.SanitiseFileName($"{t.TrackNumber:00} {t.Title}{(iterations > 0 ? $" ({iterations})" :"")}{extension}");
            if (File.Exists(Path.Combine(destination, newFilename))) return this.GetSongFilename(t, destination, ++iterations);
            return Path.Combine(destination, newFilename);

        }
    }
}
