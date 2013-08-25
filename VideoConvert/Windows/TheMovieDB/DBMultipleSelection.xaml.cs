//============================================================================
// VideoConvert - Fast Video & Audio Conversion Tool
// Copyright © 2012 JT-Soft
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//=============================================================================

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
