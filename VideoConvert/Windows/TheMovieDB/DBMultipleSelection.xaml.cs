using System.Collections.Generic;
using System.Windows;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Search;
using TvdbLib.Data;

namespace VideoConvert.Windows.TheMovieDB
{
    /// <summary>
    /// Interaktionslogik für DBMultipleSelection.xaml
    /// </summary>
    public partial class DBMultipleSelection
    {
        public SearchContainer<SearchMovie> MovieDBSearchResults { get; set; }
        public SearchMovie MovieDBSelectionResult { get; set; }

        public List<TvdbSearchResult> TvdbSearchResults { get; set; }
        public TvdbSearchResult TvdbSelectionResult { get; set; }

        public DBMultipleSelection()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (MovieDBSearchResults != null)
            {
                MovieResultList.ItemsSource = MovieDBSearchResults.Results;
                ResultsTabControl.SelectedIndex = 0;
                return;
            }
            if (TvdbSearchResults != null)
            {
                ShowResultList.ItemsSource = TvdbSearchResults;
                ResultsTabControl.SelectedIndex = 1;
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (ResultsTabControl.SelectedIndex == 0 && MovieResultList.SelectedIndex > -1)
                MovieDBSelectionResult = (SearchMovie) MovieResultList.SelectedItem;
            if (ResultsTabControl.SelectedIndex == 1 && ShowResultList.SelectedIndex > -1)
                TvdbSelectionResult = (TvdbSearchResult) ShowResultList.SelectedItem;

            DialogResult = true;
        }

        private void MovieResultList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OKButton_Click(sender, e);
        }
    }
}
