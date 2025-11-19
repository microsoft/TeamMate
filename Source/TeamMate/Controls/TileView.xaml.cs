using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Controls
{
    /// <summary>
    /// Interaction logic for TileView.xaml
    /// </summary>
    [SupportedOSPlatform("windows10.0.19041.0")]
    public partial class TileView : UserControl
    {
        private bool buttonPressed;

        public TileView()
        {
            InitializeComponent();
            View.Initialize(this);

            this.MouseLeftButtonDown += HandleMouseLeftButtonDown;
            this.MouseLeftButtonUp += HandleMouseLeftButtonUp;
            this.KeyDown += HandleKeyDown;
        }

        public TileViewModel ViewModel
        {
            get { return this.DataContext as TileViewModel; }
        }

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            // TODO: Not working, not being called...
            if (e.Key == Key.Enter)
            {
                Activate();
                e.Handled = true;
            }
        }

        private void HandleMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                this.buttonPressed = true;
            }
        }

        private void HandleMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.buttonPressed)
            {
                this.buttonPressed = false;

                Activate();
                e.Handled = true;
            }
        }

        private void Activate()
        {
            if (ViewModel != null)
            {
                ViewModel.Activate();
            }
        }
    }
}
