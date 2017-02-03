using Katatsuki.API;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using System.IO;

namespace Katatsuki
{
    public class KatatsukiContext
    {
        public ReadOnlyObservableCollection<Track> Tracks { get; }
        public string Query { get; set; }
        public IEnumerator<Track> SelectedTracks { get; }
        private ObservableCollection<Track> tracks;
        private SynchronizationContext uiContext;
        private LiteCollection<Track> tracksCache;
        public TrackboxListener Watcher { get; }
        public Library TrackLibrary { get; }

        public KatatsukiContext()
        {
            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"katatsuki")))
                    Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "katatsuki"));
            this.tracksCache = new LiteDatabase(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                "katatsuki", "tracks.db")).GetCollection<Track>("tracks");
            this.tracksCache.EnsureIndex(x => x.FilePath);

            this.tracks = new ObservableCollection<Track>(tracksCache.FindAll());
            this.Tracks = new ReadOnlyObservableCollection<Track>(this.tracks);
            this.uiContext = SynchronizationContext.Current;

            this.TrackLibrary = new Library(@"G:\Katatsuki\");
            this.Watcher = new TrackboxListener(@"G:\\Automatically Add to My Library\");

            if (!Directory.Exists(Path.Combine(this.Watcher.TrackboxPath.FullName, ".notadded")))
                Directory.CreateDirectory(Path.Combine(this.Watcher.TrackboxPath.FullName, ".notadded"));
            this.Watcher.InitAsync();

        }

        public void Add(Track t)
        {
            this.tracksCache.Insert(t);
            this.uiContext.Post(x => this.tracks.Add(t), null);
        }
    }
}
