using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Internal.Tools.TeamMate.ViewModels;
using System.Windows.Controls;

namespace Microsoft.Internal.Tools.TeamMate.Pages
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
