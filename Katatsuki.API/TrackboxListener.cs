using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Katatsuki.API
{
    public class TrackboxListener
    {
        public DirectoryInfo TrackboxPath { get; }
        private static readonly string[] FileMasks = new string[] {".flac", ".mp3", ".m4a" };
        public event EventHandler<TrackboxEventArgs> NewTrackFound;
        private FileSystemWatcher watcher;
        const int FileCopyAttempts = 100;
        public TrackboxListener(string trackboxPath)
        {
            this.TrackboxPath = new DirectoryInfo(Path.GetFullPath(trackboxPath));
            
        }

        public async Task InitAsync()
        {
            foreach(string file in 
                from file in 
                Directory.EnumerateFiles(this.TrackboxPath.FullName,"*.*", SearchOption.AllDirectories)
                where TrackboxListener.FileMasks.Contains(Path.GetExtension(file))
                select file)
            {
                await TrackCreatedAsync(file);
            }
            this.watcher = new FileSystemWatcher(this.TrackboxPath.FullName)
            {
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.Attributes |
                            NotifyFilters.CreationTime |
                            NotifyFilters.FileName |
                            NotifyFilters.LastAccess |
                            NotifyFilters.LastWrite |
                            NotifyFilters.Size |
                            NotifyFilters.Security
            };
            this.watcher.Created += OnTrackCreatedAsync;
        }

        private async void OnTrackCreatedAsync(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath.Split(Path.DirectorySeparatorChar).Contains("Not Added")) return;
            if (!TrackboxListener.FileMasks.Contains(Path.GetExtension(e.FullPath))) return;
            await Task.Run(async () =>
            {
                await TrackCreatedAsync(e.FullPath);
            });
        }

        private async Task TrackCreatedAsync(string path)
        {
            if (await GetIdleFileAsync(path))
            {
                this.NewTrackFound?.Invoke(this,
                    new TrackboxEventArgs(new Track(path, this.GetCategory(path))));
            }
        }

        private string GetCategory(string path)
        {
            var workingDirectory = new DirectoryInfo(Path.GetDirectoryName(path));
            var root = workingDirectory.Root;
            if (workingDirectory.FullName.TrimEnd('\\') == this.TrackboxPath.FullName.TrimEnd('\\')) return "None";
            for(var currentDirectory = workingDirectory;
                currentDirectory.FullName.TrimEnd('\\') != root.FullName.TrimEnd('\\');
                currentDirectory = currentDirectory.Parent)
            {
                if (currentDirectory.Parent.FullName.TrimEnd('\\') == this.TrackboxPath.FullName.TrimEnd('\\')) return currentDirectory.Name;
            }
            return "None";
        }

        private async Task<bool> GetIdleFileAsync(string path)
        {
            bool fileIdle = false;
            int attemptsMade = 0;
            await Task.Run(async () =>
            {
                while (!fileIdle && attemptsMade <= TrackboxListener.FileCopyAttempts)
                {
                    try
                    {
                        using (File.Open(path, FileMode.Open, FileAccess.ReadWrite))
                        {
                            fileIdle = true;
                        }
                    }
                    catch
                    {
                        attemptsMade++;
                        await Task.Delay(1000);
                    }
                }
            });

            return fileIdle;
        }
    }
}
