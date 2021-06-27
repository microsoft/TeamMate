namespace Microsoft.Tools.TeamMate.Model.Settings
{
    public class DeveloperSettings : SettingsBase
    {
        private bool debugAllNotifications;

        public bool DebugAllNotifications
        {
            get { return this.debugAllNotifications; }
            set { SetProperty(ref this.debugAllNotifications, value); }
        }

        private bool forceLegacyNotifications;

        public bool ForceLegacyNotifications
        {
            get { return this.forceLegacyNotifications; }
            set { SetProperty(ref this.forceLegacyNotifications, value); }
        }
    }
}
