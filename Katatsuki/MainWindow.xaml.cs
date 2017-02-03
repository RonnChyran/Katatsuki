using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Katatsuki.API;
using System.IO;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace Katatsuki
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly TrackboxListener watcher;
        private readonly KatatsukiContext context;
        private readonly CollectionViewSource viewSource;
        private readonly TrackQueryProcessor query;
        public MainWindow()
        {
            this.InitializeComponent();
            this.context = new KatatsukiContext();
            this.query = new TrackQueryProcessor();
            var library = new Library(@"G:\Katatsuki\");
            this.viewSource = (CollectionViewSource)(this.FindResource("TracksViewSource"));
            //   tracksViewSource.Source = this.Tracks;
            this.watcher = new TrackboxListener(@"G:\\Automatically Add to My Library\");
            //this.watcher = new TrackboxListener(@"G:\Project Reboot\");
            //this.watcher = new TrackboxListener(@"I:\\iTunes\\iTunes Media\\Music\\");
            if (!Directory.Exists(Path.Combine(this.watcher.TrackboxPath.FullName, ".notadded")))
                Directory.CreateDirectory(Path.Combine(this.watcher.TrackboxPath.FullName, ".notadded"));

            this.watcher.NewTrackFound += (s, e) =>
            {
                //if(!this.context.Tracks.Contains(e.Track)) this.context.Add(e.Track);
                try
                {
                    library.Add(e.Track);
                }
                catch
                {
                    try
                    {
                        File.Move(e.Track.FilePath, this.ContainsFilePath(Path.Combine(this.watcher.TrackboxPath.FullName, ".notadded", Path.GetFileName(e.Track.FilePath))));
                    }
                    catch
                    {
                        return;
                    }
                }
            };

            this.watcher.CorruptedTrackFound += (s, e) =>
            {
                try
                {
                    File.Move(e.Path, this.ContainsFilePath(Path.Combine(this.watcher.TrackboxPath.FullName, ".notadded", Path.GetFileName(e.Path))));
                }
                catch
                {
                    return;
                }

            };
           
            this.DataContext = context;
            this.viewSource.Filter += TracksViewSource_Filter;
            this.watcher.InitAsync();

        }

        public string ContainsFilePath(string path, int iterations = 0)
        {
            string extension = Path.GetExtension(path);
            string destination = Path.GetDirectoryName(path);
            string newFilename = $"{Path.GetFileNameWithoutExtension(path)}{(iterations > 0 ? $" ({iterations})" : "")}{extension}";
            if (File.Exists(Path.Combine(destination, newFilename))) return this.ContainsFilePath(path, ++iterations);
            return Path.Combine(destination, newFilename);

        }
        private void TracksViewSource_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = QueryFilter((Track)e.Item);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           // var track = new Track(textBox.Text);
           
        }

        private void QueryChanged(object sender, RoutedEventArgs e)
        {
            this.viewSource.View.Refresh();
        }

        private bool QueryFilter(Track track)
        {
            string query = this.queryBox.Text;
            if (query == String.Empty)
            {
                return true;
            }
            else if(query.StartsWith("!")) {
                var commands = TrackQuery.BuildQuerySet(query);
                return this.query.ProcessQuery(track, commands);
            }
            else if(query.StartsWith(@"""") && query.EndsWith(@"""") && query.Length > 2)
            {
                string exactQuery = query.Substring(1, query.Length - 2);
                return QueryExactText(track, exactQuery);
            }
            else
            {
                return QueryFullText(track, query);
            }
        }

        private bool QueryFullText(Track track, string query)
        {
            return (track.Title.Contains(query, StringComparison.InvariantCultureIgnoreCase)
                    || track.Album.Contains(query, StringComparison.InvariantCultureIgnoreCase)
                    || track.Artist.Contains(query, StringComparison.InvariantCultureIgnoreCase)
                    || track.FilePath.Contains(query, StringComparison.InvariantCultureIgnoreCase)
                    || (from artist in track.AlbumArtists where artist.Contains(query, StringComparison.InvariantCultureIgnoreCase) select artist).Any());
        }

        private bool QueryExactText(Track track, string exactQuery)
        {
            return (track.Title.Equals(exactQuery, StringComparison.InvariantCultureIgnoreCase)
                   || track.Album.Equals(exactQuery, StringComparison.InvariantCultureIgnoreCase)
                   || track.Artist.Equals(exactQuery, StringComparison.InvariantCultureIgnoreCase)
                   || track.FilePath.Equals(exactQuery, StringComparison.InvariantCultureIgnoreCase)
                   || (from artist in track.AlbumArtists where artist.Equals(exactQuery, StringComparison.InvariantCultureIgnoreCase) select artist).Any());
        }

        private void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string path = Path.GetDirectoryName(((Track)this.dataGrid.SelectedItem)?.FilePath);
            if(path != null) Process.Start(path);
        }
    }
   

}
