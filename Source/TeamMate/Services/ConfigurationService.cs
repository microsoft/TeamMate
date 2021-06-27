using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Shell;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Model.Settings;
using Microsoft.Win32;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Tools.TeamMate.Services
{
    public class ConfigurationService
    {
        public void Initialize()
        {
            Settings.LaunchOnStartupChanged += HandleLaunchOnStartupChanged;
        }

        public void Configure(bool force = false)
        {
            if (TeamMateApplicationInfo.IsFirstRun || TeamMateApplicationInfo.IsUpgrade || force)
            {
                InstallInBackground();
            }
        }

        private async void InstallInBackground()
        {
            try
            {
                await Task.Run(() => Install());
            }
            catch (Exception e)
            {
                Log.ErrorAndBreak(e, "Error installing application");
            }
        }

        [Import]
        public SettingsService SettingsService { get; set; }

        private ApplicationSettings Settings
        {
            get { return this.SettingsService.Settings; }
        }

        public void RegisterApplication()
        {
            ApplicationRegistrationServices.RegisterApplication(
                TeamMateApplicationInfo.ApplicationName, TeamMateApplicationInfo.BaseSoftwareRegistryPath,
                TeamMateApplicationInfo.ExePath, TeamMateApplicationInfo.ApplicationDescription,
                new FileTypeRegistration(TeamMateApplicationInfo.TeamMateFileExtension, TeamMateApplicationInfo.TeamMateFileDescription)
            );
        }

        public void UnregisterApplication()
        {
            ApplicationRegistrationServices.UnegisterApplication(TeamMateApplicationInfo.ApplicationName, TeamMateApplicationInfo.BaseSoftwareRegistryPath,
                TeamMateApplicationInfo.TeamMateFileExtension);
        }

        public void SetLaunchOnStartup(bool launchOnStartup)
        {
            string commandLine = String.Format("\"{0}\" {1}", TeamMateApplicationInfo.ExePath, CommandLineService.GetArgsForAutostart());
            ApplicationRegistrationServices.SetRunOnStartup(TeamMateApplicationInfo.ApplicationName, commandLine, launchOnStartup);
        }
        public void Install()
        {
            try
            {
                RegisterApplication();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            var settings = this.Settings;

            try
            {
                if (settings.LaunchOnStartup)
                {
                    SetLaunchOnStartup(settings.LaunchOnStartup);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            try
            {
                UpdateUninstallIcon();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public void UpdateUninstallIcon()
        {
            // http://social.msdn.microsoft.com/Forums/en-US/winformssetup/thread/db1d57ee-7743-4409-8072-f0e84ab5330a

            string iconPath = TeamMateApplicationInfo.ApplicationIconPath;
            if (File.Exists(iconPath))
            {
                using (RegistryKey uninstallKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall"))
                {
                    if (uninstallKey != null)
                    {
                        foreach (string subKeyName in uninstallKey.GetSubKeyNames())
                        {
                            using (RegistryKey subKey = uninstallKey.OpenSubKey(subKeyName, true))
                            {
                                object displayName = subKey.GetValue("DisplayName") as string;
                                if (String.Equals(displayName, TeamMateApplicationInfo.ApplicationName))
                                {
                                    subKey.SetValue("DisplayIcon", iconPath);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Uninstall()
        {
            UnregisterApplication();
            SetLaunchOnStartup(false);
        }

        private void HandleLaunchOnStartupChanged(object sender, EventArgs e)
        {
            SetLaunchOnStartup(Settings.LaunchOnStartup);
        }
    }
}
