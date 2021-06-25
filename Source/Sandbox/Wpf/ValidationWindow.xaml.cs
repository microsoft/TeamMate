using System.Windows;

namespace Microsoft.Internal.Tools.TeamMate.Sandbox.Wpf
{
    /// <summary>
    /// Interaction logic for ValidationWindow.xaml
    /// </summary>
    public partial class ValidationWindow : Window
    {
        public ValidationWindow()
        {
            InitializeComponent();
            this.DataContext = new ValidatingFormViewModel();
        }
    }
}
