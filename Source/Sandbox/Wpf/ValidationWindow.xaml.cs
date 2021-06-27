using System.Windows;

namespace Microsoft.Tools.TeamMate.Sandbox.Wpf
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
