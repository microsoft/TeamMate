using Microsoft.Internal.Tools.TeamMate.Foundation.Native;
using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Shell
{
    /// <summary>
    /// Provides utility methods for dealing with shell shortcut links to programs and files.
    /// </summary>
    public static class ShortcutUtilities
    {
        /// <summary>
        /// Gets the path to shortcut .lnk file in the AppData folder, given a base name.
        /// </summary>
        /// <param name="shortcutName">Name of the shortcut.</param>
        public static string GetPathToShortcut(string shortcutName)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, String.Format(@"Microsoft\Windows\Start Menu\Programs\{0}.lnk", shortcutName));
        }

        /// <summary>
        /// Creates a shortcut with a given AppUserModel ID.
        /// </summary>
        /// <param name="shortcutPath">The shortcut path.</param>
        /// <param name="exePath">The executable path.</param>
        /// <param name="appUserModelId">The application user model identifier.</param>
        public static void CreateShortcutWithAppUserModelId(string shortcutPath, string exePath, string appUserModelId)
        {
            // Create a shortcut to the exe
            IShellLinkW shellLink = new IShellLinkW();
            shellLink.SetPath(exePath);
            shellLink.SetArguments(string.Empty);

            // Open the shortcut property store, set the AppUserModelId property
            IPropertyStore newShortcutProperties = (IPropertyStore)shellLink;
            using (PropVariant appId = new PropVariant(appUserModelId))
            {
                /// Name:     System.AppUserModel.ID -- PKEY_AppUserModel_ID</para>
                /// Type:     String -- VT_LPWSTR
                /// FormatID: {9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3}, 5
                PropertyKey appUserModelIdPropertyKey = new PropertyKey(new Guid("{9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3}"), 5);
                newShortcutProperties.SetValue(appUserModelIdPropertyKey, appId);
                newShortcutProperties.Commit();
            }

            // Commit the shortcut to disk
            IPersistFile persistFile = (IPersistFile)shellLink;
            persistFile.Save(shortcutPath, true);
        }
    }
}
