using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Internal.Tools.TeamMate.ViewModels;
using System.Windows.Controls;

namespace Microsoft.Internal.Tools.TeamMate.Pages
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    [View(typeof(HomePageViewModel))]
    public partial class HomePage : UserControl
    {
        public HomePage()
        {
            InitializeComponent();
            View.Initialize(this);
        }
    }
}