using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Internal.Tools.TeamMate.ViewModels;
using System.Windows.Controls;

namespace Microsoft.Internal.Tools.TeamMate.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    [View(typeof(SettingsPageViewModel))]
    public partial class SettingsPage : UserControl
    {
        public SettingsPage()
        {
            InitializeComponent();
            View.Initialize(this);
        }
    }
}
