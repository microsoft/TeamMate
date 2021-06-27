// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Win32;
using System;

namespace Microsoft.Tools.TeamMate.Foundation.Shell
{
    /// <summary>
    /// Helper class for dealing with Windows Application Registration
    /// </summary>
    public static class ApplicationRegistrationServices
    {
        /// <summary>
        /// Register the specified application in the registry
        /// </summary>
        /// <param name="appName">Application name</param>
        /// <param name="registryPath">Registry path (relative to HKLM\Software)</param>
        /// <param name="appExeLocation">Application exe location</param>
        /// <param name="description">Application description</param>
        /// <param name="fileTypes">File extensions to register for this app (e.g. ".txt")</param>
        public static void RegisterApplication(string appName, string registryPath, string appExeLocation, string description, params FileTypeRegistration[] fileTypes)
        {
            Assert.ParamIsNotNullOrEmpty(appName, "appName");
            Assert.ParamIsNotNullOrEmpty(registryPath, "registryPath");
            Assert.ParamIsNotNullOrEmpty(appExeLocation, "appExeLocation");

            RegistryKey currentUser = Registry.CurrentUser;

            // Register with Default Programs
            string capabilitiesRegPath = string.Format(@"SOFTWARE\{0}\Capabilities", registryPath);
            using (RegistryKey capabilities = currentUser.CreateSubKey(capabilitiesRegPath))
            {
                capabilities.SetValue("ApplicationName", appName);
                capabilities.SetValue("ApplicationDescription", description);

                if (fileTypes.Length > 0)
                {
                    using (RegistryKey fileAssociationsKey = capabilities.CreateSubKey("FileAssociations"))
                    {
                        foreach (FileTypeRegistration fileType in fileTypes)
                        {
                            string progId = CreateProgId(appName, fileType);
                            fileAssociationsKey.SetValue(fileType.Extension, progId);
                        }
                    }
                }
            }

            using (RegistryKey registeredApplicationsKey = currentUser.OpenSubKey(@"SOFTWARE\RegisteredApplications", true))
            {
                registeredApplicationsKey.SetValue(appName, capabilitiesRegPath);
            }

            // Register ProgIDs
            foreach (FileTypeRegistration fileType in fileTypes)
            {
                string progId = CreateProgId(appName, fileType);
                string regPath = string.Format(@"SOFTWARE\Classes\{0}", progId);
                using (RegistryKey extKey = currentUser.CreateSubKey(regPath))
                {
                    if (!String.IsNullOrEmpty(fileType.Description))
                    {
                        extKey.SetValue(null, fileType.Description);
                    }

                    using (RegistryKey iconKey = extKey.CreateSubKey("DefaultIcon"))
                    {
                        iconKey.SetValue(null, appExeLocation);
                    }

                    using (RegistryKey commandKey = extKey.CreateSubKey(@"shell\open\command"))
                    {
                        commandKey.SetValue(null, string.Format("\"{0}\" \"%1\"", appExeLocation));
                    }
                }

                string fileExtensionKeyPath = string.Format(@"SOFTWARE\Classes\{0}\OpenWithProgids", fileType.Extension);
                using (RegistryKey fileExtensionKey = currentUser.CreateSubKey(fileExtensionKeyPath))
                {
                    fileExtensionKey.SetValue(progId, new byte[0], RegistryValueKind.Binary);
                }
            }
        }

        /// <summary>
        /// Creates the ProgId for a given application and file type..
        /// </summary>
        /// <param name="appName">Name of the application.</param>
        /// <param name="fileType">Type of the file.</param>
        /// <returns>A prog id.</returns>
        private static string CreateProgId(string appName, FileTypeRegistration fileType)
        {
            string progId = String.Format("{0}{1}", appName, fileType.Extension);
            return progId;
        }

        /// <summary>
        /// Unegisters an application from the registry.
        /// </summary>
        /// <param name="appName">Name of the application.</param>
        /// <param name="registryPath">The registry path.</param>
        /// <param name="fileExtensions">The file extensions associated with that application.</param>
        public static void UnegisterApplication(string appName, string registryPath, params string[] fileExtensions)
        {
            Assert.ParamIsNotNull(appName, "appName");
            Assert.ParamIsNotNull(registryPath, "registryPath");

            RegistryKey currentUser = Registry.CurrentUser;
            string capabilitiesRegPath = string.Format(@"SOFTWARE\{0}\Capabilities", registryPath);
            currentUser.DeleteSubKeyTree(capabilitiesRegPath, false);

            using (RegistryKey registeredApplicationsKey = currentUser.OpenSubKey(@"SOFTWARE\RegisteredApplications", true))
            {
                registeredApplicationsKey.DeleteValue(appName, false);
            }

            foreach (string fileExtension in fileExtensions)
            {
                string progId = String.Format("{0}{1}", appName, fileExtension);
                string regPath = string.Format(@"SOFTWARE\Classes\{0}", progId);
                currentUser.DeleteSubKeyTree(regPath, false);

                string fileExtensionKeyPath = string.Format(@"SOFTWARE\Classes\{0}", fileExtension);
                currentUser.DeleteSubKeyTree(fileExtensionKeyPath, false);
            }
        }

        /// <summary>
        /// Sets whether a given application should run on startup.
        /// </summary>
        /// <param name="appName">Name of the application.</param>
        /// <param name="commandLine">The command line.</param>
        /// <param name="runOnStartup">if set to <c>true</c>, configures the application to run on startup. Otherwise, removes it from running on startup..</param>
        public static void SetRunOnStartup(string appName, string commandLine, bool runOnStartup = true)
        {
            Assert.ParamIsNotNull(appName, "appName");
            Assert.ParamIsNotNull(commandLine, "commandLine");

            RegistryKey currentUser = Registry.CurrentUser;
            if (runOnStartup)
            {
                using (RegistryKey runKey = currentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run"))
                {
                    runKey.SetValue(appName, commandLine);
                }
            }
            else
            {
                using (RegistryKey runKey = currentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (runKey != null)
                    {
                        runKey.DeleteValue(appName, false);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Provides the registration information for a file type.
    /// </summary>
    public class FileTypeRegistration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileTypeRegistration"/> class.
        /// </summary>
        /// <param name="extension">The file extension.</param>
        /// <param name="description">An optional description.</param>
        public FileTypeRegistration(string extension, string description = null)
        {
            Assert.ParamIsNotNullOrEmpty(extension, "extension");

            this.Extension = extension;
            this.Description = description;
        }

        /// <summary>
        /// Gets the file extension.
        /// </summary>
        public string Extension { get; private set; }

        /// <summary>
        /// Gets an optional description.
        /// </summary>
        public string Description { get; private set; }
    }
}
