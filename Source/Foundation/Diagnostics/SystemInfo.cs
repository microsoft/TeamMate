using Microsoft.Internal.Tools.TeamMate.Foundation.Collections;
using Microsoft.Internal.Tools.TeamMate.Foundation.Native;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics
{
    /// <summary>
    /// Represents system information of where an application is running.
    /// </summary>
    public class SystemInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemInfo"/> class.
        /// </summary>
        public SystemInfo()
        {
            this.Processors = new List<ProcessorInfo>();
            this.Screens = new List<ScreenInfo>();
            this.VisualStudioVersions = new List<string>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether [is64 bit operating system].
        /// </summary>
        public bool Is64BitOperatingSystem { get; set; }

        /// <summary>
        /// Gets or sets the OS version.
        /// </summary>
        public string OSVersion { get; set; }

        /// <summary>
        /// Gets or sets the name of the OS.
        /// </summary>
        public string OSName { get; set; }

        /// <summary>
        /// Gets or sets the CLR version.
        /// </summary>
        public string ClrVersion { get; set; }

        /// <summary>
        /// Gets or sets the name of the system culture.
        /// </summary>
        /// </value>
        public string SystemCultureName { get; set; }

        /// <summary>
        /// Gets or sets the name of the user culture.
        /// </summary>
        public string UserCultureName { get; set; }

        /// <summary>
        /// Gets or sets the name of the time zone.
        /// </summary>
        public string TimeZoneName { get; set; }

        /// <summary>
        /// Gets or sets the offset of the user's time zone from UTC.
        /// </summary>
        public TimeSpan TimeZoneUtcOffset { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current session is running in remote desktop.
        /// </summary>
        public bool IsRemoteDesktopSession { get; set; }

        /// <summary>
        /// Gets or sets the memory information.
        /// </summary>
        public MemoryInfo Memory { get; set; }

        /// <summary>
        /// Gets or sets the processor information.
        /// </summary>
        public ICollection<ProcessorInfo> Processors { get; set; }

        /// <summary>
        /// Gets or sets the virtual screen information.
        /// </summary>
        public VirtualScreenInfo VirtualScreen { get; set; }

        /// <summary>
        /// Gets the screens in the system.
        /// </summary>
        public ICollection<ScreenInfo> Screens { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether multi touch is enabled.
        /// </summary>
        public bool IsMultiTouchEnabled { get; set; }

        /// <summary>
        /// Gets or sets the Internet Explorer version.
        /// </summary>
        public string IEVersion { get; set; }

        /// <summary>
        /// Gets or sets the collection of Visual Studio versions.
        /// </summary>
        public ICollection<string> VisualStudioVersions { get; set; }


        /// <summary>
        /// Captures a snapshot of the current system information.
        /// </summary>
        /// <returns>The captured system information.</returns>
        /// <remarks>
        /// This method can take 1+ second, so you might want to call it in an asynchronous fashion.
        /// </remarks>
        public static SystemInfo Capture()
        {
            SystemInfo systemInfo = new SystemInfo();

            // OS
            systemInfo.Is64BitOperatingSystem = Environment.Is64BitOperatingSystem;
            systemInfo.OSVersion = Environment.OSVersion.Version.ToString();
            var operatingSystem = GetSingleManagementObject("Win32_OperatingSystem");
            if (operatingSystem != null)
            {
                systemInfo.OSName = GetPropertyValue<string>(operatingSystem, "Caption");
            }

            // CLR
            systemInfo.ClrVersion = Environment.Version.ToString();

            // Cultures
            systemInfo.SystemCultureName = CultureInfo.InstalledUICulture.Name;
            systemInfo.UserCultureName = CultureInfo.CurrentCulture.Name;

            // TimeZone
            systemInfo.TimeZoneName = TimeZone.CurrentTimeZone.StandardName;
            systemInfo.TimeZoneUtcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(new DateTime());

            // Memory, Processor, Screens
            systemInfo.Memory = GetMemoryInfo();

            systemInfo.Processors.AddRange(GetProcessorInfos());
            systemInfo.Screens.AddRange(GetScreensInfo());

            systemInfo.IEVersion = GetIEVersion();
            systemInfo.VisualStudioVersions.AddRange(GetVisualStudioVersions());

            systemInfo.IsMultiTouchEnabled = GetMultiTouchEnabled();

            systemInfo.IsRemoteDesktopSession = System.Windows.Forms.SystemInformation.TerminalServerSession;

            var virtualScreen = System.Windows.Forms.SystemInformation.VirtualScreen;
            systemInfo.VirtualScreen = new VirtualScreenInfo()
            {
                Width = virtualScreen.Width,
                Height = virtualScreen.Height
            };

            return systemInfo;
        }

        /// <summary>
        /// Calculate whether multi touch is enabled or not.
        /// </summary>
        /// <returns><c>true</c> if multi touch is enabled.</returns>
        private static bool GetMultiTouchEnabled()
        {
            NID digitizer = (NID) NativeMethods.GetSystemMetrics((int)SystemMetric.SM_DIGITIZER);

            bool isMultiTouchEnabled = digitizer.HasFlag(NID.NID_READY) 
                && digitizer.HasFlag(NID.NID_MULTI_INPUT) 
                && digitizer.HasFlag(NID.NID_INTEGRATED_TOUCH); // Trying to use integrated touch to differentiate from USB pens and digitizers

            return isMultiTouchEnabled;
        }

        /// <summary>
        /// Gets the Internet Explorer version.
        /// </summary>
        private static string GetIEVersion()
        {
            string result = null;

            using (RegistryKey internetExplorerKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\MICROSOFT\Internet Explorer"))
            {
                if (internetExplorerKey != null)
                {
                    // For IE10 and above, we need to rely on svcVersion key instead of Version key
                    // to get the version info for internet explorer. So here we first try to get the 
                    // version from svcVersion key and if the key is not present then we fall back to 
                    // the Version key.
                    object internetExplorerVersionKeyValue = internetExplorerKey.GetValue("svcVersion");
                    if (internetExplorerVersionKeyValue == null)
                    {
                        // svcVersion key does not exist. Get the value from 'Version' key
                        internetExplorerVersionKeyValue = internetExplorerKey.GetValue("Version");
                    }

                    if (internetExplorerVersionKeyValue != null)
                    {
                        result = internetExplorerVersionKeyValue.ToString();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the Visual Studio versions.
        /// </summary>
        private static ICollection<string> GetVisualStudioVersions()
        {
            List<string> results = new List<string>();

            // KLUDGE: Update this with new versions that we want to recognize on demand
            string[] probingVersions = new string[] { "9.0", "10.0", "11.0", "12.0", "13.0", "14.0", "15.0" };

            foreach (string probingVersion in probingVersions)
            {
                var key = Registry.ClassesRoot.OpenSubKey("VisualStudio.DTE." + probingVersion);
                if (key != null)
                {
                    results.Add(probingVersion);
                    key.Close();
                }
            }

            return results;
        }

        /// <summary>
        /// Gets the screens information.
        /// </summary>
        private static IEnumerable<ScreenInfo> GetScreensInfo()
        {
            var allScreens = System.Windows.Forms.Screen.AllScreens;
            foreach (var screen in allScreens)
            {
                ScreenInfo screenInfo = new ScreenInfo();
                screenInfo.Width = screen.Bounds.Width;
                screenInfo.Height = screen.Bounds.Height;
                screenInfo.BitsPerPixel = screen.BitsPerPixel;
                screenInfo.IsPrimary = screen.Primary;

                yield return screenInfo;
            }
        }

        /// <summary>
        /// Gets the processor information.
        /// </summary>
        private static IEnumerable<ProcessorInfo> GetProcessorInfos()
        {
            foreach (var processor in GetManagementObjects("Win32_processor"))
            {
                ProcessorInfo processorInfo = new ProcessorInfo();
                processorInfo.Name = GetPropertyValue<string>(processor, "Name");
                processorInfo.Family = GetPropertyValue<string>(processor, "Caption");
                processorInfo.NumberOfCores = (int)GetPropertyValue<uint>(processor, "NumberOfCores");
                processorInfo.NumberOfLogicalProcessors = (int)GetPropertyValue<uint>(processor, "NumberOfLogicalProcessors");
                processorInfo.MaxClockSpeed = (int)GetPropertyValue<uint>(processor, "MaxClockSpeed");
                yield return processorInfo;
            }
        }

        /// <summary>
        /// Gets the memory information.
        /// </summary>
        private static MemoryInfo GetMemoryInfo()
        {
            MEMORYSTATUSEX memoryStatus = new MEMORYSTATUSEX();
            NativeMethods.GlobalMemoryStatusEx(memoryStatus);

            MemoryInfo memoryInfo = new MemoryInfo();
            memoryInfo.AvailablePhysicalMemory = (long)memoryStatus.ullAvailPhys;
            memoryInfo.AvailableVirtualMemory = (long)memoryStatus.ullAvailVirtual;
            memoryInfo.MemoryLoad = (int)memoryStatus.dwMemoryLoad;
            memoryInfo.TotalPhysicalMemory = (long)memoryStatus.ullTotalPhys;
            memoryInfo.TotalVirtualMemory = (long)memoryStatus.ullTotalVirtual;

            return memoryInfo;
        }

        /// <summary>
        /// Gets the a set of management objects for a given class name.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <returns>The resulting management object collection.</returns>
        private static ManagementObjectCollection GetManagementObjects(string className)
        {
            SelectQuery query = new SelectQuery(className);
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            return searcher.Get();
        }

        /// <summary>
        /// Gets a single management object for a given class name.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <returns>The single management object, or <c>null</c> if none found.</returns>
        private static ManagementBaseObject GetSingleManagementObject(string className)
        {
            return GetManagementObjects(className).OfType<ManagementBaseObject>().FirstOrDefault();
        }

        /// <summary>
        /// Gets the property value for a given management object property.
        /// </summary>
        /// <typeparam name="T">The type of the proeprty value.</typeparam>
        /// <param name="item">The management object.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The property value, or the default type value if not found or compatible.</returns>
        private static T GetPropertyValue<T>(ManagementBaseObject item, string propertyName)
        {
            var property = item.Properties[propertyName];
            return (property != null && property.Value is T) ? (T)property.Value : default(T);
        }
    }

    /// <summary>
    /// Represents processor (CPU) information.
    /// </summary>
    public class ProcessorInfo
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the family.
        /// </summary>
        public string Family { get; set; }

        /// <summary>
        /// Gets or sets the number of cores.
        /// </summary>
        public int NumberOfCores { get; set; }

        /// <summary>
        /// Gets or sets the number of logical processors.
        /// </summary>
        public int NumberOfLogicalProcessors { get; set; }

        /// <summary>
        /// Gets or sets the maximum clock speed.
        /// </summary>
        public int MaxClockSpeed { get; set; }

    }

    /// <summary>
    /// Represents the system memory information.
    /// </summary>
    public class MemoryInfo
    {
        /// <summary>
        /// Gets or sets the total physical memory.
        /// </summary>
        public long TotalPhysicalMemory { get; set; }

        /// <summary>
        /// Gets or sets the available physical memory.
        /// </summary>
        public long AvailablePhysicalMemory { get; set; }

        /// <summary>
        /// Gets or sets the memory load.
        /// </summary>
        public int MemoryLoad { get; set; }

        /// <summary>
        /// Gets or sets the total virtual memory.
        /// </summary>
        public long TotalVirtualMemory { get; set; }

        /// <summary>
        /// Gets or sets the available virtual memory.
        /// </summary>
        public long AvailableVirtualMemory { get; set; }
    }

    /// <summary>
    /// Represents screen information.
    /// </summary>
    public class ScreenInfo
    {
        /// <summary>
        /// Gets or sets the height in pixels.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the width in pixels.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the bits per pixel.
        /// </summary>
        public int BitsPerPixel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the screen is the primary screen or not.
        /// </summary>
        public bool IsPrimary { get; set; }
    }

    /// <summary>
    /// Represents the virtual screen information (for multiple screens).
    /// </summary>
    public class VirtualScreenInfo
    {
        /// <summary>
        /// Gets or sets the height in pixels.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the width in pixels.
        /// </summary>
        public int Width { get; set; }
    }
}
