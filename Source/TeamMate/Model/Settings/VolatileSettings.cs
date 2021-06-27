using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Windows;
using System.Collections.Generic;
using System.Windows;

namespace Microsoft.Tools.TeamMate.Model.Settings
{
    public class VolatileSettings : SettingsBase
    {
        private ProjectInfo lastUsedProject;
        private bool isWorkItemRibbonMinimized;
        private bool showRecordingBallonTip;

        private Dictionary<string, WindowStateInfo> windowStates = new Dictionary<string, WindowStateInfo>();

        public VolatileSettings()
        {
            // Defaults
            ShowRecordingBallonTip = true;
        }

        public ProjectInfo LastUsedProject
        {
            get { return this.lastUsedProject; }
            set { SetProperty(ref this.lastUsedProject, value); }
        }

        public bool IsWorkItemRibbonMinimized
        {
            get { return this.isWorkItemRibbonMinimized; }
            set { SetProperty(ref this.isWorkItemRibbonMinimized, value); }
        }

        public bool ShowRecordingBallonTip
        {
            get { return this.showRecordingBallonTip; }
            set { SetProperty(ref this.showRecordingBallonTip, value); }
        }

        public ICollection<string> LastKnownStateKeys
        {
            get { return windowStates.Keys; }
        }

        public WindowStateInfo GetLastKnownState(Window window)
        {
            Assert.ParamIsNotNull(window, "window");

            return GetLastKnownState(GetKey(window));
        }

        public WindowStateInfo GetLastKnownState(string key)
        {
            Assert.ParamIsNotNull(key, "key");

            WindowStateInfo result;
            windowStates.TryGetValue(key, out result);
            return result;
        }

        public void SetLastKnownState(Window window, WindowStateInfo state)
        {
            Assert.ParamIsNotNull(window, "window");

            SetLastKnownState(GetKey(window), state);
            OnPropertyChanged("WindowStates");
        }

        public void SetLastKnownState(string key, WindowStateInfo state)
        {
            Assert.ParamIsNotNull(key, "key");

            if (state != null)
            {
                windowStates[key] = state;
            }
            else
            {
                windowStates.Remove(key);
            }
        }

        private static string GetKey(Window window)
        {
            return window.GetType().FullName;
        }

        private bool trayIconReminderWasShown;

        public bool TrayIconReminderWasShown
        {
            get { return this.trayIconReminderWasShown; }
            set { SetProperty(ref this.trayIconReminderWasShown, value); }
        }

        private bool overviewWindowWasHiddenBefore;

        public bool OverviewWindowWasHiddenBefore
        {
            get { return this.overviewWindowWasHiddenBefore; }
            set { SetProperty(ref this.overviewWindowWasHiddenBefore, value); }
        }

        private bool displayLegacyTfsSupportDroppedBanner;

        public bool DisplayLegacyTfsSupportDroppedBanner
        {
            get { return this.displayLegacyTfsSupportDroppedBanner; }
            set { this.SetProperty(ref this.displayLegacyTfsSupportDroppedBanner, value); }
        }
    }
}
