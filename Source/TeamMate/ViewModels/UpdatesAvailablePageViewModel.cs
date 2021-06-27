using Microsoft.Tools.TeamMate.Foundation.Windows.Input;
using Microsoft.Tools.TeamMate.Services;
using System.ComponentModel.Composition;
using System.Windows.Input;

namespace Microsoft.Tools.TeamMate.ViewModels
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
