using System.Windows;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Search;

namespace VideoConvert.Windows.TheMovieDB
{
    /// <summary>
    /// Interaktionslogik für MovieDBMultipleSelection.xaml
    /// </summary>
    public partial class MovieDBMultipleSelection
    {
        public SearchContainer<SearchMovie> SearchResults { get; set; }

        public SearchMovie SelectionResult { get; set; }

        public MovieDBMultipleSelection()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (SearchResults != null)
                ResultList.ItemsSource = SearchResults.Results;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (ResultList.SelectedIndex > -1)
                SelectionResult = (SearchMovie)ResultList.SelectedItem;
            DialogResult = true;
        }
    }
}
