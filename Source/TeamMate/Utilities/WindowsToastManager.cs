using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Internal.Tools.TeamMate.Foundation.Shell;
using Microsoft.Internal.Tools.TeamMate.Model;
using Microsoft.Internal.Tools.TeamMate.WindowsRuntime.UI.Notifications;
using System;
using System.IO;
using System.Xml.Linq;

namespace Microsoft.Internal.Tools.TeamMate.Utilities
{
    public class WindowsToastManager : IToastManager
    {
        public event EventHandler<ToastActivatedEventArgs> ToastActivated;

        private ToastNotificationManager toastNotificationManager;

        public WindowsToastManager()
        {
            try
            {
                var toastNotificationManager = new ToastNotificationManager(TeamMateApplicationInfo.AppUserModelId);
                toastNotificationManager.ToastOccurred += ToastManager_ToastOccurred;

                this.toastNotificationManager = toastNotificationManager;

                this.CreateDesktopShortcutIfNeeded();
            }
            catch (Exception ex)
            {
                Log.ErrorAndBreak("Error initializing built-in toast notification service", ex);
            }
        }

        public void Show(ToastInfo toast)
        {
            if (this.toastNotificationManager == null)
            {
                // Toast notification service was not initialized successfully, ignore any future calls
                return;
            }

            string appLogoImageUri = TeamMateApplicationInfo.ApplicationImageUri.ToString();
            string xmlContent = CreateToastXml(appLogoImageUri, toast.Arguments, toast.Title, toast.Description);
            this.toastNotificationManager.Show(xmlContent);
        }

        private static string CreateToastXml(string appLogImageUri, string launchArgs, string lineOneText, string lineTwoText)
        {
            // See toast schema @ https://msdn.microsoft.com/en-us/library/windows/apps/br230849.aspx

            // IMPORTANT: When running in ClickOnce mode, the toasts are not automatically picking up the app logo icon.
            // To make sure toast show the proper icon, we force override the app logo here with our own deployed copy of
            // a PNG icon.

            XDocument doc = new XDocument(
                new XElement("toast", (!String.IsNullOrEmpty(launchArgs)) ? new XAttribute("launch", launchArgs) : null,
                    new XElement("visual",
                        new XElement("binding", new XAttribute("template", "ToastGeneric"),
                            new XElement("text", lineOneText),
                            new XElement("text", lineTwoText),
                            new XElement("image", new XAttribute("placement", "AppLogoOverride"),
                                                  new XAttribute("src", appLogImageUri)
            )))));

            string xmlContent = doc.ToString(SaveOptions.DisableFormatting);
            return xmlContent;
        }


        public void Dispose()
        {
        }

        private void ToastManager_ToastOccurred(object sender, ToastNotificationManagerEventArgs e)
        {
            if (e.EventType == ToastNotificationEventType.Activated && e.ActivatedArguments != null)
            {
                this.ToastActivated?.Invoke(this, new ToastActivatedEventArgs(e.ActivatedArguments));
            }
        }

        private void CreateDesktopShortcutIfNeeded()
        {
            // A shortcut on the start menu, with the appropriate AppUserModel id is required for desktop apps to
            // use the Windows Notification system in WinRT.
            // ClickOnce installs registers a shortcut and manages the AppUserModel id, but developer builds/xcopies do not, so create one on demand
            // See https://msdn.microsoft.com/en-us/library/windows/desktop/hh802762(v=vs.85).aspx
            // And https://code.msdn.microsoft.com/windowsdesktop/Sending-toast-notifications-71e230a2
            if (!TeamMateApplicationInfo.IsNetworkDeployed)
            {
                string shortcut = ShortcutUtilities.GetPathToShortcut(TeamMateApplicationInfo.ApplicationName);
                if (!File.Exists(shortcut))
                {
                    ShortcutUtilities.CreateShortcutWithAppUserModelId(shortcut, TeamMateApplicationInfo.ExePath, TeamMateApplicationInfo.AppUserModelId);
                }
            }
        }
    }
}
