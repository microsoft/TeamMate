using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Model;
using System;
using System.ComponentModel.Composition;
using System.Windows;

namespace Microsoft.Tools.TeamMate.Services
{
    public class UpgradeService
    {
        [Import]
        public MessageBoxService MessageBoxService { get; set; }

        public void UpgradeIfNeeded()
        {
            Version lastVersion = TeamMateApplicationInfo.LastVersion;

            if (lastVersion != null)
            {
                Version currentVersion = TeamMateApplicationInfo.Version;
                if (lastVersion < currentVersion)
                {
                    Upgrade(lastVersion, currentVersion);
                }
            }

            // IMPORTANT: Settings, data directory and related files can only be read starting at this point!
            TeamMateApplicationInfo.IsDataDirectoryAccessAllowed = true;
        }

        private void Upgrade(Version fromVersion, Version toVersion)
        {
            Log.Info("Executing upgrade from {0} to {1}", fromVersion, toVersion);

            Version versionWhereLegacyTfsSupportWasDropped = new Version("2.1.21006.2");
            if (fromVersion < versionWhereLegacyTfsSupportWasDropped && toVersion >= versionWhereLegacyTfsSupportWasDropped)
            {
                this.ShouldDisplayLegacyTfsSupportDroppedBanner = true;
            }
        }

        public void DowngradeIfNeeded()
        {
            if (TeamMateApplicationInfo.IsDowngrade)
            {
                // WARNING: Downgrade! A previously higher version of the app was installed, so there are files in the
                // data directory we do not understand. Hence, we will clear the data directory and mark this as a first run.

                var result = this.MessageBoxService.Show(
                    "Warning! A higher version of TeamMate was previously installed. In order to continue safely, "
                    + "we need to reset the data directory and clear all past data. This will delete previously "
                    + "pinned queries, project information, etc.\n\n"
                    + "Do you want to clear any data that still exists in the data directory?",
                    "Important", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);

                if (result == MessageBoxResult.Yes)
                {
                    TeamMateApplicationInfo.ResetDataDirectory();

                    // IMPORTANT!!! Do this after reset, otherwise the log file might be held open and trying to delete the
                    // data directory will fail
                    Log.Warn("Executing downgrade from {0} to {1}", TeamMateApplicationInfo.LastVersion, TeamMateApplicationInfo.Version);
                }
            }
        }

        public bool ShouldDisplayLegacyTfsSupportDroppedBanner { get; private set; }
    }
}
