// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Exceptions;
using Microsoft.Tools.TeamMate.Foundation.Windows.Input;
using Microsoft.Tools.TeamMate.Model.Settings;
using Microsoft.Tools.TeamMate.Services;
using System;
using System.ComponentModel.Composition;
using System.Windows.Input;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    public class DeveloperOptionsPageViewModel : PageViewModelBase
    {
        public DeveloperOptionsPageViewModel()
        {
            this.Title = "Developer Options";
            this.ChaosMonkey = new ChaosMonkeyViewModel();

            this.RequestFeedbackCommand = new RelayCommand(RequestFeedback);
            this.RequestRatingCommand = new RelayCommand(RequestRating);
            this.RunInstallUpgradeConfigurationStepCommand = new RelayCommand(RunInstallUpgradeConfigurationStep);
            this.ShowWelcomeDialogCommand = new RelayCommand(ShowWelcomeDialog);
            this.CrashApplicationCommand = new RelayCommand(CrashApplication);
            this.ThrowUnhandledExceptionCommand = new RelayCommand(ThrowUnhandledException);
        }

        public ChaosMonkeyViewModel ChaosMonkey { get; private set; }

        public ICommand TriggerApplicationUpdateCommand { get; private set; }

        public ICommand CrashApplicationCommand { get; private set; }

        public ICommand ThrowUnhandledExceptionCommand { get; private set; }

        public void ThrowUnhandledException()
        {
            throw new Exception("This is an unhandled exception!");
        }

        public void CrashApplication()
        {
            throw new ForceCrashException("Force crashed application!");
        }

        public ICommand RequestRatingCommand { get; private set; }

        public void RequestRating()
        {
            this.WindowService.ShowRequestRatingDialog();
        }

        public ICommand RequestFeedbackCommand { get; private set; }

        [Import]
        public WindowService WindowService { get; set; }

        public void RequestFeedback()
        {
            this.WindowService.ShowRequestFeedbackDialog();
        }

        public ICommand RunInstallUpgradeConfigurationStepCommand { get; private set; }

        [Import]
        public ConfigurationService ConfigurationService { get; set; }

        public void RunInstallUpgradeConfigurationStep()
        {
            this.ConfigurationService.Configure(force: true);
        }

        public ICommand DownloadTeamFoundationClientFileGroupCommand { get; private set; }

        public DeveloperSettings DeveloperSettings
        {
            get { return this.SettingsService.DeveloperSettings; }
        }

        [Import]
        public SettingsService SettingsService { get; set; }


        public ICommand ShowWelcomeDialogCommand { get; private set; }

        public void ShowWelcomeDialog()
        {
            this.WindowService.ShowWelcomeDialog();
        }
    }
}
