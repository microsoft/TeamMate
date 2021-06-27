using Microsoft.Win32;
using System;
using System.Collections.Generic;

namespace Microsoft.Tools.TeamMate.Foundation.Win32
{
    /// <summary>
    /// Provides utility methods for manipulating the registry in 32 and 64-bit environments.
    /// </summary>
    public static class RegistryViewUtilities
    {
        /// <summary>
        /// Returns a set of all of the available Classes Root registry keys (32-bit and 64-bit if appropriate).
        /// </summary>
        public static IEnumerable<RegistryKey> OpenAllClassesRootKeys()
        {
            return OpenBaseKeys(RegistryHive.ClassesRoot);
        }

        /// <summary>
        /// Returns a set of all of the available Current User registry keys (32-bit and 64-bit if appropriate).
        /// </summary>
        public static IEnumerable<RegistryKey> OpenAllCurrentUserKeys()
        {
            return OpenBaseKeys(RegistryHive.CurrentUser);
        }

        /// <summary>
        /// Returns a set of all of the available Local Machine registry keys (32-bit and 64-bit if appropriate).
        /// </summary>
        public static IEnumerable<RegistryKey> OpenAllLocalMachineKeys()
        {
            return OpenBaseKeys(RegistryHive.LocalMachine);
        }

        /// <summary>
        /// Returns a set of all of the available registry keys (32-bit and 64-bit if appropriate) for a given hive.
        /// </summary>
        /// <param name="hive">The hive.</param>
        private static IEnumerable<RegistryKey> OpenBaseKeys(RegistryHive hive)
        {
            if (Environment.Is64BitProcess)
            {
                yield return RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            }
            else if (Environment.Is64BitOperatingSystem)
            {
                yield return RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            }

            yield return RegistryKey.OpenBaseKey(hive, RegistryView.Registry32);
        }
    }
}
