namespace VideoConvertWPF.Views
{
    using ViewModels.Interfaces;

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
            IShellViewModel shellViewModel = this.DataContext as IShellViewModel;

            if (shellViewModel != null)
            {
                bool canClose = shellViewModel.CanClose();
                if (!canClose)
                {
                    e.Cancel = true;
                }
            }

            base.OnClosing(e);
        }
    }
}
