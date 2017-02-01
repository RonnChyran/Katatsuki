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
        public MainWindow()
        {
            this.InitializeComponent();
            this.context = new KatatsukiContext();

            this.context.Tracks = new ObservableCollection<Track>();

            CollectionViewSource tracksViewSource;
            tracksViewSource = (CollectionViewSource)(this.FindResource("TracksViewSource"));
         //   tracksViewSource.Source = this.Tracks;
            this.watcher = new TrackboxListener(@"G:\\Automatically Add to My Library\");
            this.watcher.NewTrackFound += (s, e) =>
            {
                this.Dispatcher.Invoke(() => this.context.Tracks.Add(e.Track));
            };
            this.DataContext = context;
            tracksViewSource.Filter += TracksViewSource_Filter;
            this.watcher.InitAsync();

        }

        private void TracksViewSource_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           // var track = new Track(textBox.Text);
           
        }

        private void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
