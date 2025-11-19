using Microsoft.Tools.TeamMate.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Windows
{
    /// <summary>
    /// Interaction logic for LegacyToastWindow.xaml
    /// </summary>
    [SupportedOSPlatform("windows10.0.19041.0")]
    public partial class CustomToastWindow : Window
    {
        public CustomToastWindow()
        {
            InitializeComponent();

            this.closeImage.MouseLeftButtonDown += HandleCloseClick;

            Storyboard storyboard = (Storyboard)FindResource("fadeOutStoryboard");
            Storyboard fistoryboard = (Storyboard)FindResource("fadeInStoryboard");
            storyboard.Completed += HandleFadeOutCompleted;
            fistoryboard.Completed += HandleFadeOutCompleted;

            this.MouseLeftButtonDown += HandleMouseLeftButtonDown;
        }

        private void HandleMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ToastViewModel viewModel = this.DataContext as ToastViewModel;
            if (viewModel != null)
            {
                e.Handled = true;

                viewModel.Activate();

                // TODO: There might be no handlers registered with Activate, only close if handlers where there and did something?
                this.Close();
            }
        }

        private void HandleFadeOutCompleted(object sender, EventArgs e)
        {
            if (Opacity < 0.1)
            {
                Close();
            }
        }

        private void HandleCloseClick(object sender, MouseButtonEventArgs e)
        {
            Close();
            e.Handled = true;
        }
    }
}
