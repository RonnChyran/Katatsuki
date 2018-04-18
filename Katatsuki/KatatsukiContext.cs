using Katatsuki.API;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Input;

namespace Katatsuki
{
    public class KatatsukiContext 
    {
        public ReadOnlyObservableCollection<Track> Tracks { get; }
        public string Query { get; set; }
        private ObservableCollection<Track> tracks;
        private SynchronizationContext uiContext;
        private TrackDatabase tracksCache;
        public TrackboxListener Watcher { get; }
        public Library TrackLibrary { get; }
        public event EventHandler<bool> VisibilityStateChanged;

        public KatatsukiContext(string path)
        {

            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"katatsuki")))
                    Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "katatsuki"));
            this.tracksCache = new TrackDatabase(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "katatsuki", "tracks.db"));
            this.uiContext = SynchronizationContext.Current;

            this.EnsureDirectory(Path.Combine(path, "Music"));
            this.EnsureDirectory(Path.Combine(path, "Automatically Add to Library"));

            this.TrackLibrary = new Library(Path.Combine(path, "Music\\"));
            this.Watcher = new TrackboxListener(Path.Combine(path, "Automatically Add to Library\\"));

            this.tracks = new ObservableCollection<Track>(tracksCache.GetAllTracks());
            this.Tracks = new ReadOnlyObservableCollection<Track>(this.tracks);
            this.EnsureDirectory(Path.Combine(this.Watcher.TrackboxPath.FullName, ".notadded"));

            this.TrackLibrary.TrackAddedEvent += TrackLibrary_TrackAddedEvent;
            this.TrackLibrary.TrackDeletedEvent += TrackLibrary_TrackDeletedEvent;
            this.Watcher.InitAsync();

        }

        private void TrackLibrary_TrackDeletedEvent(object sender, Track e)
        {
            this.uiContext.Post(x => this.tracks.Remove(e), null);
            this.tracksCache.Remove(e);
        }

        public void ForceVisible(bool state)
        {
            this.VisibilityStateChanged?.Invoke(this, state);
        }

        private void TrackLibrary_TrackAddedEvent(object sender, Track e)
        {
            if (this.tracks.Contains(e))
            {
                this.uiContext.Post(x => this.tracks.Remove(e), null);
            }
            this.uiContext.Post(x => this.tracks.Add(e), null);
            this.tracksCache.Add(e);
        }

        private string EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

       /* public void Add(Track t)
        {
            this.tracksCache.Insert(t);
            this.uiContext.Post(x => this.tracks.Add(t), null);
        }*/
    }
}
