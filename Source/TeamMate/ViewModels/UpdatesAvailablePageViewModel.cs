using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Input;
using Microsoft.Internal.Tools.TeamMate.Services;
using System.ComponentModel.Composition;
using System.Windows.Input;

namespace Microsoft.Internal.Tools.TeamMate.ViewModels
{
    public class UpdatesAvailablePageViewModel : PageViewModelBase
    {
        public UpdatesAvailablePageViewModel()
        {
            this.Title = "Updates Available";

            this.ApplyUpdatesAndRestartCommand = new RelayCommand(ApplyUpdatesAndRestart);
        }

        public ICommand ApplyUpdatesAndRestartCommand { get; set; }

        public void ApplyUpdatesAndRestart()
        {
            this.RestartService.Restart();
        }

        [Import]
        public RestartService RestartService { get; set; }
    }
}
