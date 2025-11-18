using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Model.Settings;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Xml.Linq;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Services
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class SettingsService
    {
        private const string SettingsFolder = "Settings";
        private const string SettingsFilename = "Settings.xml";
        private const string VolatileSettingsFilename = "VolatileSettings.xml";

        private object writeLock = new object();
        private string settingsFile;
        private string volatileSettingsFile;
        private ApplicationSettings settings;
        private VolatileSettings volatileSetings;

        // Developer settings are always in memory and never get flushed...
        private DeveloperSettings developerSettings = new DeveloperSettings();

        [Import]
        public AsyncWriterService AsyncWriterService { get; set; }

        public DeveloperSettings DeveloperSettings
        {
            get { return this.developerSettings; }
        }

        private string SettingsFile
        {
            get
            {
                if (this.settingsFile == null)
                {
                    this.settingsFile = Path.Combine(TeamMateApplicationInfo.DataDirectory, SettingsFolder, SettingsFilename);
                }

                return this.settingsFile;
            }
        }

        private string VolatileSettingsFile
        {
            get
            {
                if (this.volatileSettingsFile == null)
                {
                    this.volatileSettingsFile = Path.Combine(TeamMateApplicationInfo.DataDirectory, SettingsFolder, VolatileSettingsFilename);
                }

                return this.volatileSettingsFile;
            }
        }

        public ApplicationSettings Settings
        {
            get
            {
                if (this.settings == null)
                {
                    this.settings = LoadSettings();
                    this.settings.SettingsChanged += HandleSettingsChanged;
                }

                return settings;
            }
        }

        public VolatileSettings VolatileSettings
        {
            get
            {
                if(this.volatileSetings == null)
                {
                    this.volatileSetings = LoadVolatileSettings();
                    this.volatileSetings.SettingsChanged += HandleVolatileSettingsChanged;
                }

                return this.volatileSetings;

            }
        }

        private VolatileSettings LoadVolatileSettings()
        {
            TeamMateApplicationInfo.AssertDataDirectoryAccessIsAllowed();

            VolatileSettings volatileSettings = new VolatileSettings();

            try
            {
                if (File.Exists(VolatileSettingsFile))
                {
                    SettingsSerializer serializer = new SettingsSerializer();
                    volatileSettings = serializer.ReadVolatileSettings(VolatileSettingsFile);
                }
            }
            catch (Exception e)
            {
                Log.WarnAndBreak(e, "Failed to read settings from settings file {0}", VolatileSettingsFile);
            }

            // Safeguard to make sure the last used project is in sync with our current list of projects...
            var lastUsedProject = volatileSettings.LastUsedProject;
            if (lastUsedProject != null && !this.Settings.Projects.Contains(lastUsedProject))
            {
                volatileSettings.LastUsedProject = null;
            }

            return volatileSettings;
        }

        private ApplicationSettings LoadSettings()
        {
            TeamMateApplicationInfo.AssertDataDirectoryAccessIsAllowed();

            ApplicationSettings settings = new ApplicationSettings();

            try
            {
                if (File.Exists(SettingsFile))
                {
                    SettingsSerializer serializer = new SettingsSerializer();
                    settings = serializer.ReadSettings(SettingsFile);
                }
            }
            catch (Exception e)
            {
                Log.WarnAndBreak(e, "Failed to read settings from settings file {0}", SettingsFile);
            }

            return settings;
        }

        private void HandleSettingsChanged(object sender, EventArgs e)
        {
            FlushSettings();
        }

        private void HandleVolatileSettingsChanged(object sender, EventArgs e)
        {
            FlushVolatileSettings();
        }

        public void FlushSettings()
        {
            SettingsSerializer serializer = new SettingsSerializer();
            XDocument doc = serializer.WriteSettings(this.Settings);
            this.AsyncWriterService.Save(doc, SettingsFile);
        }

        private void FlushVolatileSettings()
        {
            SettingsSerializer serializer = new SettingsSerializer();
            XDocument doc = serializer.WriteVolatileSettings(this.VolatileSettings);
            this.AsyncWriterService.Save(doc, VolatileSettingsFile);
        }
    }
}
