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
