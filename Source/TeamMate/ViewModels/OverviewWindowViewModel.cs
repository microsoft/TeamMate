using Microsoft.Tools.TeamMate.Foundation.Windows.Input;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Services;
using System.ComponentModel.Composition;
using System.Windows.Input;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class OverviewWindowViewModel : ViewModelBase
    {
        private ItemCountSummary itemCountSummary;

        public ItemCountSummary ItemCountSummary
        {
            get { return this.itemCountSummary; }
            set { SetProperty(ref this.itemCountSummary, value); }
        }

        public OverviewWindowViewModel()
        {
            this.HideCommand = new RelayCommand(this.HideFloatingWindow);
        }

        public ICommand HideCommand { get; private set; }

        public void HideFloatingWindow()
        {
            this.SettingsService.Settings.ShowItemCountInOverviewWindow = false;

            var volatileSettings = this.SettingsService.VolatileSettings;
            if (!volatileSettings.OverviewWindowWasHiddenBefore)
            {
                volatileSettings.OverviewWindowWasHiddenBefore = true;

                this.MessageBoxService.Show("You've hidden the work item overview window.\n\nYou can make it visible again from the application settings page.",
                    TeamMateApplicationInfo.ApplicationName, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

        public  void ShowMainWindow()
        {
            this.WindowService.ShowHomePage();
        }

        [Import]
        public WindowService WindowService { get; set; }

        [Import]
        public SettingsService SettingsService { get; set; }

        [Import]
        public MessageBoxService MessageBoxService { get; set; }
    }
}
