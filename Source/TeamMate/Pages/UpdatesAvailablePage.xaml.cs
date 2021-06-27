using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.ViewModels;
using System.Windows.Controls;

namespace Microsoft.Tools.TeamMate.Pages
{
    /// <summary>
    /// Interaction logic for UpdatesAvailablePage.xaml
    /// </summary>
    [View(typeof(UpdatesAvailablePageViewModel))]
    public partial class UpdatesAvailablePage : UserControl
    {
        public UpdatesAvailablePage()
        {
            InitializeComponent();
            View.Initialize(this);
        }
    }
}
