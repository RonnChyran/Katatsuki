using Katatsuki.API;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;

namespace Katatsuki
{
    public class LibraryListener
    {
        private KatatsukiContext context;
        private readonly NotifyIcon icon;
        private ConcurrentStack<Track> RecentTracks { get; }
        public LibraryListener(KatatsukiContext context)
        {
            this.icon = new NotifyIcon();
            this.RecentTracks = new ConcurrentStack<Track>();
            this.context = context;
            this.context.Watcher.NewTrackFound += Watcher_NewTrackFound;
            this.context.Watcher.CorruptedTrackFound += Watcher_CorruptedTrackFound;
            this.icon.Icon = new System.Drawing.Icon("test.ico");
            this.icon.Visible = true;
            var timer = new System.Timers.Timer(10000);
            timer.AutoReset = true;
            //todo use an autoreset event
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (this.RecentTracks.IsEmpty) return;
            if (this.RecentTracks.Count > 1)
            {
                this.RecentTracks.TryPop(out Track t);
                this.icon.ShowBalloonTip(1000, "New Tracks Added",
                    $"Added {t?.Artist} - {t?.Title} and {this.RecentTracks.Count} other tracks.", ToolTipIcon.Info);
                this.RecentTracks.Clear();
            }
            if (this.RecentTracks.Count == 1)
            {
                this.RecentTracks.TryPop(out Track t);
                this.icon.ShowBalloonTip(1000, "New Track Added",
                    $"Found {t?.Artist} - {t?.Title}", ToolTipIcon.Info);
            }
        }

        private void MoveToNotAdded(string path)
        {
            try
            {
                File.Move(path, ContainsFilePath(Path.Combine(this.context.Watcher
                    .TrackboxPath.FullName, ".notadded", Path.GetFileName(path))));
            }
            catch
            {
                return;
            }
        }

        private void SortTrack(Track t)
        {
            try
            {
                this.context.TrackLibrary.Add(t);
            }
            catch
            {
                try
                {
                    this.MoveToNotAdded(t.FilePath);
                }
                catch
                {
                    return;
                }
            }
        }

        private void Watcher_CorruptedTrackFound(object sender, TrackboxCorruptedEventArgs e)
        {
            this.MoveToNotAdded(e.Path);
        }

        private void Watcher_NewTrackFound(object sender, TrackboxEventArgs e)
        {
            this.SortTrack(e.Track);
            this.RecentTracks.Push(e.Track);
        }

        private static string ContainsFilePath(string path, int iterations = 0)
        {
            string extension = Path.GetExtension(path);
            string destination = Path.GetDirectoryName(path);
            string newFilename = $"{Path.GetFileNameWithoutExtension(path)}{(iterations > 0 ? $" ({iterations})" : "")}{extension}";
            if (File.Exists(Path.Combine(destination, newFilename))) return ContainsFilePath(path, ++iterations);
            return Path.Combine(destination, newFilename);

        }
    }
}
