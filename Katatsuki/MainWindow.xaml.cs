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
            this.context.Tracks = new ObservableCollection<Track>();
            this.viewSource = (CollectionViewSource)(this.FindResource("TracksViewSource"));
            //   tracksViewSource.Source = this.Tracks;
            //this.watcher = new TrackboxListener(@"G:\\Automatically Add to My Library\");
            this.watcher = new TrackboxListener(@"I:\\iTunes\\iTunes Media\\Music\\");
            this.watcher.NewTrackFound += (s, e) =>
            {
                this.Dispatcher.Invoke(() => this.context.Tracks.Add(e.Track));
            };
           
            this.DataContext = context;
            this.viewSource.Filter += TracksViewSource_Filter;
            this.watcher.InitAsync();

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
                    || (from artist in track.AlbumArtists where artist.Contains(query, StringComparison.InvariantCultureIgnoreCase) select artist).Any());
        }

        private bool QueryExactText(Track track, string exactQuery)
        {
            return (track.Title.Equals(exactQuery, StringComparison.InvariantCultureIgnoreCase)
                   || track.Album.Equals(exactQuery, StringComparison.InvariantCultureIgnoreCase)
                   || track.Artist.Equals(exactQuery, StringComparison.InvariantCultureIgnoreCase)
                   || (from artist in track.AlbumArtists where artist.Equals(exactQuery, StringComparison.InvariantCultureIgnoreCase) select artist).Any());
        }
    }
   

}
