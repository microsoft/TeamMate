using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Win32;
using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Tools.TeamMate.Foundation.Runtime.InteropServices
{
    /// <summary>
    /// Provides utility methods for runtime interop.
    /// </summary>
    public class InteropUtilities
    {
        /// <summary>
        /// Determines whether the COM class associated with a given .NET type is registered in the system or not.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the COM class is registered.</returns>
        public static bool IsClassRegistered(Type type)
        {
            Assert.ParamIsNotNull(type, "type");

            return IsClassRegistered(GetGuid(type));
        }

        /// <summary>
        /// Determines whether the COM class associated with a given GUID is registered in the system or not.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns><c>true</c> if the COM class is registered.</returns>
        public static bool IsClassRegistered(Guid guid)
        {
            return IsTypeRegistered(guid, false);
        }

        /// <summary>
        /// Determines whether the COM interface associated with a given .NET type is registered in the system or not.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the COM interface is registered.</returns>
        public static bool IsInterfaceRegistered(Type type)
        {
            Assert.ParamIsNotNull(type, "type");

            return IsInterfaceRegistered(GetGuid(type));
        }

        /// <summary>
        /// Determines whether the COM interface associated with a given GUID is registered in the system or not.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns><c>true</c> if the COM interface is registered.</returns>
        public static bool IsInterfaceRegistered(Guid guid)
        {
            return IsTypeRegistered(guid, true);
        }

        /// <summary>
        /// Determines whether the COM class or interface associated with a given GUID is registered in the system or not.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="isInterface"><c>true</c> if the GUID refers to an interface rather than a class.</param>
        /// <returns><c>true</c> if the COM class or interface is registered.</returns>
        private static bool IsTypeRegistered(Guid guid, bool isInterface)
        {
            // We need to look for Interface or CLSID definitions in both the 64-bit registry hive, or the 32-bit registry
            // hive. This for example allows us to find a 64-bit regitered Outlook add-in from a 32-bit process (by looking at the 64-bit Hive)
            foreach (RegistryKey classesRootKey in RegistryViewUtilities.OpenAllClassesRootKeys())
            {
                string keyPath = String.Format(@"{0}\{{{1}}}", (isInterface) ? "Interface" : "CLSID", guid);
                using (RegistryKey subKey = classesRootKey.OpenSubKey(keyPath))
                {
                    if (subKey != null)
                    {
                        object value = subKey.GetValue(null);
                        return (value != null);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the GUID associated with a given .NET type. (e.g. looking at its [Guid] attribute).
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The associated GUID.</returns>
        private static Guid GetGuid(Type type)
        {
            return Marshal.GenerateGuidForType(type);
        }
    }
}