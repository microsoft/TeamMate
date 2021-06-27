using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.ViewModels;
using System.Windows.Controls;

namespace Microsoft.Tools.TeamMate.Pages
{
    /// <summary>
    /// Interaction logic for DeveloperOptionsPage.xaml
    /// </summary>
    [View(typeof(DeveloperOptionsPageViewModel))]
    public partial class DeveloperOptionsPage : UserControl
    {
        public DeveloperOptionsPage()
        {
            InitializeComponent();
            View.Initialize(this);
        }
    }
}
