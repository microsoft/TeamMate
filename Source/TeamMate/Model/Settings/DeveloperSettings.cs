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
    }
}
