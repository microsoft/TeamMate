using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.ViewModels;
using System.Windows.Controls;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Pages
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    [View(typeof(HomePageViewModel))]
    [SupportedOSPlatform("windows10.0.19041.0")]
    public partial class HomePage : UserControl
    {
        public HomePage()
        {
            InitializeComponent();
            View.Initialize(this);
        }
    }
}
