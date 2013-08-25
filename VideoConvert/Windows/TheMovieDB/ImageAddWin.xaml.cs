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

using System.Windows;

namespace VideoConvert.Windows.TheMovieDB
{
    /// <summary>
    /// Interaktionslogik für ImageAddWin.xaml
    /// </summary>
    public partial class ImageAddWin
    {

        public string ResultPreview = string.Empty;
        public string ResultImage = string.Empty;

        public ImageAddWin()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            ResultImage = ImageUrl.Text;
            ResultPreview = PreviewUrl.Text;

            DialogResult = true;
        }
    }
}
