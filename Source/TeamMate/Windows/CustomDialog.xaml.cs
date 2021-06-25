using Microsoft.Internal.Tools.TeamMate.Foundation.Windows;
using Microsoft.Internal.Tools.TeamMate.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Internal.Tools.TeamMate.Windows
{
    /// <summary>
    /// Interaction logic for CustomDialog.xaml
    /// </summary>
    public partial class CustomDialog : Window
    {
        public CustomDialog()
        {
            InitializeComponent();
            this.DataContextChanged += HandleDataContextChanged;

            buttonPanel.AddHandler(Button.ClickEvent, (RoutedEventHandler)HandleButtonClick);
            this.Loaded += HandleLoaded;
        }

        private void HandleLoaded(object sender, RoutedEventArgs e)
        {
            var location = (Owner != null) ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen;
            this.WindowStartupLocation = location;
        }

        private CustomDialogViewModel ViewModel
        {
            get { return this.DataContext as CustomDialogViewModel; }
        }

        private void HandleButtonClick(object sender, RoutedEventArgs e)
        {
            var viewModel = this.ViewModel;
            if (viewModel != null && e.OriginalSource is Button && ((Button)e.OriginalSource).Tag is ButtonInfo)
            {
                Button button = (Button)e.OriginalSource;
                viewModel.PressedButton = (ButtonInfo)button.Tag;
                this.DialogResult = (!viewModel.PressedButton.IsCancel);
            }
        }

        private void HandleDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            InvalidateButtons();
        }

        private void InvalidateButtons()
        {
            buttonPanel.Children.Clear();

            var viewModel = this.ViewModel;
            if (viewModel != null)
            {
                foreach (var buttonInfo in viewModel.Buttons)
                {
                    buttonPanel.Children.Add(CreateButton(buttonInfo));
                }
            }
        }

        private Button CreateButton(ButtonInfo buttonInfo)
        {
            Button button = new Button();
            button.Tag = buttonInfo;
            button.Style = this.FindResource<Style>("LyncButtonStyle");
            button.Content = buttonInfo.Text;
            button.IsDefault = buttonInfo.IsDefault;
            button.IsCancel = buttonInfo.IsCancel;
            return button;
        }
    }
}
