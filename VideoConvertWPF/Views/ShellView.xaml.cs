// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShellView.xaml.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvertWPF source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VideoConvertWPF.Views
{
    using VideoConvertWPF.ViewModels.Interfaces;

    /// <summary>
    /// Interaktionslogik für ShellView.xaml
    /// </summary>
    public partial class ShellView
    {
        public ShellView()
        {
            InitializeComponent();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            var shellViewModel = DataContext as IShellViewModel;

            if (shellViewModel != null)
            {
                var canClose = shellViewModel.CanClose();
                if (!canClose)
                {
                    e.Cancel = true;
                }
            }

            base.OnClosing(e);
        }
    }
}
