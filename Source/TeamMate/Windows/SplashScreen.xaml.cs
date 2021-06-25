using System.Windows;
using System.Windows.Input;

namespace Microsoft.Internal.Tools.TeamMate.Windows
{
    /// <summary>
    /// Interaction logic for xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();

            this.CloseImage.MouseLeftButtonDown += HandleCloseImageMouseLeftButtonDown;
            this.MinimizeImage.MouseLeftButtonDown += HandleMinimizeImageMouseLeftButtonDown;
        }

        private void HandleMinimizeImageMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Hide();
        }

        private void HandleCloseImageMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
