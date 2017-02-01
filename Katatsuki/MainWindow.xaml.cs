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
        public MainWindow()
        {
            this.InitializeComponent();
            this.context = new KatatsukiContext();

            this.context.Tracks = new ObservableCollection<Track>();
            this.viewSource = (CollectionViewSource)(this.FindResource("TracksViewSource"));
         //   tracksViewSource.Source = this.Tracks;
            this.watcher = new TrackboxListener(@"D:\\Automatically Add to My Library\");
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
            }else if(query.StartsWith("!")) {
                var commands = query.Split('!'); //todo smarter split 
                foreach(string command in commands)
                {
                    var predicate = command.Split(new[] { ':' }, 2);
                    if (predicate.Length < 2) continue;
                    string cmd = predicate[0];
                    string param = predicate[1];
                    if (param == String.Empty) continue;
                    switch (cmd) {
                        case "q":
                            return QueryFullText(track, param);
                        case "Q":
                            return QueryExactText(track, param);
                        case "al":
                            return track.Album.Contains(param);
                        case "AL":
                            return track.Album.Equals(param, StringComparison.InvariantCultureIgnoreCase);
                        case "a":
                            return track.Artist.Contains(param);
                        case "A":
                            return track.Artist.Equals(param, StringComparison.InvariantCultureIgnoreCase);
                        case "ala":
                            return (from artist in track.AlbumArtists where artist.Contains(param, StringComparison.InvariantCultureIgnoreCase) select artist).Any();
                        case "ALA":
                            return (from artist in track.AlbumArtists where artist.Equals(param, StringComparison.InvariantCultureIgnoreCase) select artist).Any();
                        case "f":
                            if (param.Equals("mp3", StringComparison.InvariantCultureIgnoreCase))
                                return track.FileType == TrackFileType.MP3_CBR
                                    || track.FileType == TrackFileType.MP3_VBR;
                            if (param.Equals("flac", StringComparison.InvariantCultureIgnoreCase))
                                return track.FileType == TrackFileType.FLAC
                                    || track.FileType == TrackFileType.FLAC_4
                                    || track.FileType == TrackFileType.FLAC_8
                                    || track.FileType == TrackFileType.FLAC_16
                                    || track.FileType == TrackFileType.FLAC_24
                                    || track.FileType == TrackFileType.FLAC_32;
                            if (param.Equals("alac", StringComparison.InvariantCultureIgnoreCase))
                                return track.FileType == TrackFileType.ALAC;
                            if(param.Equals("aac", StringComparison.InvariantCultureIgnoreCase) 
                                || param.Equals("mp4", StringComparison.InvariantCultureIgnoreCase))
                                return track.FileType == TrackFileType.AAC;
                            if (param.Equals("flac4", StringComparison.InvariantCultureIgnoreCase))
                                return track.FileType == TrackFileType.FLAC_4;
                            if (param.Equals("flac8", StringComparison.InvariantCultureIgnoreCase))
                                return track.FileType == TrackFileType.FLAC_8;
                            if (param.Equals("flac16", StringComparison.InvariantCultureIgnoreCase))
                                return track.FileType == TrackFileType.FLAC_16;
                            if (param.Equals("flac24", StringComparison.InvariantCultureIgnoreCase))
                                return track.FileType == TrackFileType.FLAC_24;
                            if (param.Equals("flac32", StringComparison.InvariantCultureIgnoreCase))
                                return track.FileType == TrackFileType.FLAC_32;
                            return track.FileType == TrackFileType.UNKNOWN;
                        case "sr":
                            Int32.TryParse(param, out int sr);
                            return track.SampleRate.Equals(sr);
                        case "br":
                            Int32.TryParse(param, out int br);
                            return track.Bitrate.Equals(br);
                        case "chlt":
                            Int32.TryParse(param, out int chlt);
                            return track.FrontCoverHeight <= chlt;
                        case "chgt":
                            Int32.TryParse(param, out int chgt);
                            return track.FrontCoverHeight >= Convert.ToInt32(chgt);
                        case "cwlt":
                            Int32.TryParse(param, out int cwlt);
                            return track.FrontCoverWidth <= cwlt;
                        case "cwgt":
                            Int32.TryParse(param, out int cwgt);
                            return track.FrontCoverWidth >= cwgt;
                        case "c":
                            Boolean.TryParse(param, out bool c);
                            return track.HasFrontCover == c;
                        default:
                            return false;
                    }
                }
                return false;
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
    public static class StringExtensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }
    }

}
