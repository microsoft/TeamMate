using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Management;

namespace Microsoft.Tools.TeamMate.Foundation.Diagnostics
{
    /// <summary>
    /// Provides extension methods for the Process class.
    /// </summary>
    public static class ProcessExtensions
    {
        /// <summary>
        /// Kills the process tree.
        /// </summary>
        /// <param name="process">The process.</param>
        public static void KillTree(this Process process)
        {
            if (!process.HasExited)
            {
                ICollection<Process> childProcesses = GetChildren(process);
                foreach (var child in childProcesses)
                {
                    KillTree(child);
                }

                if (!process.HasExited)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Win32Exception)
                    {
                        // Ignore, process might have exited
                    }
                }
            }
        }

        /// <summary>
        /// Gets the child processes.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <returns>A collection of child processes.</returns>
        public static ICollection<Process> GetChildren(this Process process)
        {
            ICollection<Process> childProcesses = new List<Process>();

            try
            {
                if (!process.HasExited)
                {
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                        "Select * From Win32_Process Where ParentProcessID = " + process.Id.ToString(CultureInfo.InvariantCulture)
                    );

                    foreach (ManagementObject item in searcher.Get())
                    {
                        try
                        {
                            var childPid = Convert.ToInt32(item["ProcessID"]);
                            childProcesses.Add(Process.GetProcessById(childPid));
                        }
                        catch (Exception)
                        {
                            // Ignore errors
                        }
                    }
                }
            }
            catch (Win32Exception)
            {
                // Ignore errors
            }

            return childProcesses;
        }
    }
}
