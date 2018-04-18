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

        private readonly KatatsukiContext context;
        private readonly CollectionViewSource viewSource;
        private readonly TrackQueryProcessor queryProcessor;
        public MainWindow()
        {
            this.InitializeComponent();
            this.context = new KatatsukiContext(File.ReadAllText("path"));

            new LibraryManager(context);
            this.queryProcessor = new TrackQueryProcessor(t =>
            {
                return (from track in this.context.Tracks
                        where track.Artist == t.Artist && track.Title == t.Title && track.Album == t.Album
                        group track by new { track.Title, track.Album, track.Artist } into dupes
                        where dupes.Count() > 1
                        select dupes).Any();
            });
            this.viewSource = (CollectionViewSource)(this.FindResource("TracksViewSource"));
            this.DataContext = context;
            this.viewSource.Filter += TracksViewSource_Filter;
            this.context.VisibilityStateChanged += Context_VisibilityStateChanged;

        }

        private void Context_VisibilityStateChanged(object sender, bool e)
        {
            if (e)
            {
                this.Visibility = Visibility.Visible;
            }

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
                return this.queryProcessor.ProcessQuery(track, commands);
            }
            else if(query.StartsWith(@"""") && query.EndsWith(@"""") && query.Length > 2)
            {
                string exactQuery = query.Substring(1, query.Length - 2);
                return this.queryProcessor.QueryExactText(track, exactQuery);
            }
            else
            {
                return this.queryProcessor.QueryFullText(track, query);
            }
        }

        private void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string path = Path.GetDirectoryName(((Track)this.dataGrid.SelectedItem)?.FilePath);
            if(path != null) Process.Start(path);
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Visibility = Visibility.Hidden;
                this.WindowState = WindowState.Normal;
            }
        }
        
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            e.Cancel = true;
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            
            foreach(Track t in this.dataGrid.SelectedItems.Cast<Track>())
            {
                this.context.TrackLibrary.Refresh(t);
            }
        }
    }
   

}
