using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Internal.Tools.TeamMate.Foundation.DirectoryServices;
using Microsoft.Internal.Tools.TeamMate.Foundation.IO;
using Microsoft.Internal.Tools.TeamMate.Foundation.Reflection;
using Microsoft.Internal.Tools.TeamMate.Foundation.Xml;
using System;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Microsoft.Internal.Tools.TeamMate.Model
{
    public static class TeamMateApplicationInfo
    {
        public const string TeamMateFileExtension = ".tmx";
        public const string TeamMateFileDescription = "TeamMate File";

        private static Lazy<bool> isNetworkDeployed = new Lazy<bool>(() => ApplicationDeployment.IsNetworkDeployed);
        private static Version version;

        private static bool historyLoaded;
        private static ApplicationHistory history;
        private static SystemInfo systemInfo;

        public const string HockeyAppId = "5facbd5f1a6441ae87a28da827a9338f";

        static TeamMateApplicationInfo()
        {
            ApplicationName = "TeamMate";
            ApplicationDescription = "An internal tool for managing work items at Microsoft";
            VersionQualifier = null; // e.g. was "Preview"
            Author = "Ben Amodio";
            FeedbackEmail = "teammate@microsoft.com";

            // KLUDGE: An AppUserModelId is needed to display Toast Notifications from a Windows Desktop app
            // See https://msdn.microsoft.com/en-us/library/windows/desktop/dd378459(v=vs.85).aspx
            // 1. When running from a developer (non-ClickOnce) build, OS toasts only work if we've created a shortcut
            //    to the desktop app in the start menu. We do this smartly on app initialize.
            // 2. When deployed through ClickOnce, ClickOnce manages and applies an automatic AppUserModelId.
            //    However, we don't know what it is. Furthermore, unfortunately, in ClickOnce "mode", whatever user id
            //    we use is shown as the application name in the Action Center. Hence, choosing the application name
            //    as the default AppUserModelId no matter what. It works for both scenarios.
            AppUserModelId = ApplicationName;

            // URLs
            ToolboxHomeUrl = "http://toolbox/TeamMate";
            RatingUrl = ToolboxHomeUrl;
            IssueListUrl = "http://aka.ms/teammatevote";
            ReportBugUrl = "http://aka.ms/teammatebug";
            SuggestFeatureUrl = "http://aka.ms/teammatefeature";
            YammerUrl = "https://aka.ms/teammateyammer";
            BlogUrl = "http://aka.ms/teammateblog";

            // TODO: Update Help Url
            HelpUrl = ToolboxHomeUrl;
            PrivacyStatementUrl = "http://aka.ms/teammateprivacy";
            LegacyTfsSupportDroppedUrl = "http://aka.ms/teammateoptimizedforvsts";
        }

        private static void TryEnsureDirectoryExists(string baseFileShare)
        {
            try
            {
                PathUtilities.EnsureDirectoryExists(baseFileShare);
            }
            catch (Exception e)
            {
                Log.WarnAndBreak(e);
            }
        }

        public static async Task<SystemInfo> GetSystemInfoAsync()
        {
            if (systemInfo == null)
            {
                systemInfo = await Task.Run(() => SystemInfo.Capture());
            }

            return systemInfo;
        }

        public static bool IsDataDirectoryAccessAllowed { get; set; }

        public static void AssertDataDirectoryAccessIsAllowed()
        {
            if (!IsDataDirectoryAccessAllowed)
            {
                Log.WarnAndBreak(new Exception(), "Code is trying to access the data directory before it has been upgraded.");
            }
        }

        public static string VersionQualifier { get; private set; }

        public static ApplicationHistory History
        {
            get
            {
                if (!historyLoaded)
                {
                    historyLoaded = true;
                    string applicationFile = ApplicationHistoryFile;
                    if (File.Exists(applicationFile))
                    {
                        history = ApplicationHistory.Load(applicationFile);
                    }
                }

                return history;
            }
        }

        public static UserEntry UserInfo { get; private set; }

        private static string ApplicationHistoryFile
        {
            get { return Path.Combine(TeamMateApplicationInfo.DataDirectory, "History.xml"); }
        }

        private static string VersionFile
        {
            get { return Path.Combine(TeamMateApplicationInfo.DataDirectory, "Version.xml"); }
        }

        public static void Initialize()
        {
            if (LastVersion == null)
            {
                // First run ever, no knowledge of a previous version being installed
                IsFirstRun = true;
            }
            else
            {
                IsUpgrade = (LastVersion != null && LastVersion < Version);
                IsDowngrade = (LastVersion != null && LastVersion > Version);

                if (IsDowngrade)
                {
                    // WARNING: Downgrade! A previously higher version of the app was installed, so there are files in the
                    // data directory we do not understand. Hence, we will mark this as a first run.
                    IsFirstRun = true;
                }
            }

            BeingLoadUserInfo();
        }

        private static async void BeingLoadUserInfo()
        {
            try
            {
                DirectoryBrowser browser = new DirectoryBrowser(DirectoryBrowserMode.Light);
                if (browser.IsInDomain)
                {
                    UserInfo = await Task.Run(() => browser.FindMe());
                }
            }
            catch (Exception e)
            {
                Log.Warn(e);
            }
        }

        public static bool IsDebugBuild
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        public static bool IsFirstRun { get; private set; }

        public static bool IsUpgrade { get; private set; }

        public static bool IsDowngrade { get; private set; }

        public static void ResetDataDirectory()
        {
            if (Directory.Exists(TeamMateApplicationInfo.DataDirectory))
            {
                // Delete contents and mark as first run to perform install again
                PathUtilities.DeleteContents(TeamMateApplicationInfo.DataDirectory, DeleteMode.Force);
            }
        }

        public static Uri ActivationUri
        {
            get
            {
                return (IsNetworkDeployed) ? CurrentDeployment.ActivationUri : null;
            }
        }

        public static string FullVersion
        {
            get
            {
                return (!String.IsNullOrEmpty(VersionQualifier)) ? String.Format("{0} {1}", Version, VersionQualifier) : Version.ToString();
            }
        }

        private static bool lastVersionLoaded;
        private static Version lastVersion;

        public static Version LastVersion
        {
            get
            {
                if (!lastVersionLoaded)
                {
                    lastVersionLoaded = true;

                    if (File.Exists(VersionFile))
                    {
                        XDocument doc = XDocument.Load(VersionFile);
                        XElement root = doc.Root;
                        lastVersion = root.GetValue<Version>();
                    }
                }

                return lastVersion;
            }

            set
            {
                if (!Object.Equals(LastVersion, value))
                {
                    lastVersion = value;
                    if (lastVersion != null)
                    {
                        XDocument doc = new XDocument(
                            new XElement("Version", XmlExtensions.ToXmlString(lastVersion))
                        );

                        PathUtilities.EnsureParentDirectoryExists(VersionFile);
                        doc.Save(VersionFile);
                    }
                    else
                    {
                        if (File.Exists(VersionFile))
                        {
                            File.Delete(VersionFile);
                        }
                    }
                }
            }
        }

        public static Version Version
        {
            get
            {
                if (version == null)
                {
                    try
                    {
                        if (IsNetworkDeployed)
                        {
                            // If it is a ClickOnce application, return that version
                            version = CurrentDeployment.CurrentVersion;
                        }
                        else
                        {
                            // Otherwise return the Assembly File Version
                            Assembly assembly = typeof(TeamMateApplicationInfo).Assembly;
                            Version fileVersion = assembly.GetFileVersion();
                            if (fileVersion == null)
                            {
                                // Worst case scenario, return assembly version
                                fileVersion = assembly.GetVersion();
                            }

                            version = fileVersion;
                        }
                    }
                    catch
                    {
                        version = new Version();
                    }
                }

                return version;
            }
        }

        public static string ApplicationName { get; private set; }

        public static string AppUserModelId { get; private set; }

        public static string ApplicationDescription { get; private set; }

        public static string BaseSoftwareRegistryPath
        {
            get
            {
                return String.Format(@"Microsoft\{0}", TeamMateApplicationInfo.ApplicationName);
            }
        }

        public static string FullApplicationName
        {
            get
            {
                return String.Format("{0} ({1})", ApplicationName, Version);
            }
        }

        public static string FeedbackEmail { get; private set; }

        public static string FeedbackFileShare { get; private set; }

        public static string TelemetryFileShare { get; private set; }

        public static ApplicationDeployment CurrentDeployment
        {
            get
            {
                return (IsNetworkDeployed) ? ApplicationDeployment.CurrentDeployment : null;
            }
        }

        public static bool IsNetworkDeployed
        {
            get
            {
                bool result = isNetworkDeployed.Value;
                return result;
            }
        }

        public static string DataDirectory
        {
            get
            {
                /*
                 * We settled on always using the LocalApplicationData folder, even if installing from ClickOnce
                if (IsNetworkDeployed)
                {
                    return CurrentDeployment.DataDirectory;
                }
                 */

                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(localAppData, ApplicationName);
            }
        }

        public static string LogsFolder
        {
            get
            {
                return Path.Combine(TeamMateApplicationInfo.DataDirectory, "Logs");
            }
        }

        public static string HelpUrl { get; private set; }

        public static string ToolboxHomeUrl { get; private set; }

        public static string RatingUrl { get; private set; }

        public static string IssueListUrl { get; private set; }

        public static string ReportBugUrl { get; private set; }

        public static string SuggestFeatureUrl { get; private set; }

        public static string PrivacyStatementUrl { get; private set; }

        public static string YammerUrl { get; private set; }

        public static string BlogUrl { get; private set; }

        public static string Author { get; private set; }

        public static string InstallPath
        {
            get
            {
                return Path.GetDirectoryName(ExePath);
            }
        }

        public static string ExePath
        {
            get
            {
                string exePath = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
                return exePath;
            }
        }

        private static bool LaunchedWithTeamMateExe
        {
            get
            {
                string entryExe = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
                string teamMateExe = Path.GetFileName(ExePath);
                return String.Equals(entryExe, teamMateExe, StringComparison.OrdinalIgnoreCase);
            }
        }

        public static string[] CommandLineArgs
        {
            get
            {
                string[] args = Environment.GetCommandLineArgs();

                // Skip the exe name in command line args
                string appName = args[0];
                string[] launchArgs = args.Skip(1).ToArray();

                // When the app checks for updates, and restarts, it is launched through the applaunch.exe bootstraper with the following command line:
                // "C:\Windows\Microsoft.NET\Framework\v4.0.30319\applaunch.exe" /activate 
                // "file://office/TeamMatePublish/Debug/TeamMate.application#TeamMate.application, Version=1.0.51221.9, Culture=en-US, PublicKeyToken=08d949f891c61941, processorArchitecture=x86/TeamMate.exe, Version=1.0.51221.9, Culture=en-US, PublicKeyToken=08d949f891c61941, processorArchitecture=x86, type=win32" 
                //
                // DO NOT consume input arguments if the exe that was launched was not ours
                if (!LaunchedWithTeamMateExe)
                {
                    launchArgs = new string[0];
                }

                return launchArgs;
            }
        }

        public static string ApplicationIconPath
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(ExePath), "TeamMate.ico");
            }
        }

        public static Uri ApplicationImageUri
        {
            get
            {
                string filePath = Path.Combine(Path.GetDirectoryName(ExePath), "TeamMate.png");
                return new Uri(filePath);
            }
        }

        public static string LegacyTfsSupportDroppedUrl { get; private set; }

        private static Guid GetAnonymousUserId()
        {
            string username = String.Format("{0}\\{1}", Environment.UserDomainName, Environment.UserName);
            username = username.ToLowerInvariant();
            HashAlgorithm hashAlgorithm = new SHA256Managed();
            byte[] hash = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(username));
            byte[] first16 = new byte[16];
            Array.Copy(hash, first16, first16.Length);
            Guid guid = new Guid(first16);
            return guid;
        }
    }
}
