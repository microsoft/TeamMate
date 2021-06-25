using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Internal.Tools.TeamMate.ViewModels;
using System.Windows.Controls;

namespace Microsoft.Internal.Tools.TeamMate.Pages
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
