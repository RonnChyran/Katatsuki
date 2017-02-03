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
            this.context = new KatatsukiContext();
            new LibraryListener(context);
            this.queryProcessor = new TrackQueryProcessor();
            this.viewSource = (CollectionViewSource)(this.FindResource("TracksViewSource"));
            this.DataContext = context;
            this.viewSource.Filter += TracksViewSource_Filter;

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
    }
   

}
