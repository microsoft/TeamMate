// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Win32;
using System;
using System.IO;
using System.Security;

namespace Microsoft.Tools.TeamMate.Foundation.Win32
{
    /// <summary>
    /// Identifies whether a protocol (like http: or ftp:) has a handler installed on the system 
    /// by querying information from the system registry
    /// </summary>
    public static class ProtocolUtilities
    {
        /// <summary>
        /// Queries the system for info on whether a handler is registered for <paramref name="protocol"/>
        /// </summary>
        /// <param name="protocol">Name of the protocol like "http" or "ftp"</param>
        /// <returns>True if a handler for the protocol is registered, other False</returns>
        /// <remarks>
        ///     See <a href="https://msdn.microsoft.com/en-us/library/aa767914(v=vs.85).aspx">Registering an Application to a URI Scheme</a> 
        ///     on MSDN. 
        /// </remarks>
        public static bool HandlerExists(string protocol)
        {
            Assert.ParamIsNotNullOrEmpty(protocol, nameof(protocol));

            bool handlerExists = false;

            try
            {
                using (RegistryKey classKey = Registry.ClassesRoot.OpenSubKey(protocol))
                {
                    if (classKey != null)
                    {
                        var urlProtocolValue = classKey.GetValue("URL Protocol") as string;

                        if ((urlProtocolValue != null) && string.IsNullOrWhiteSpace(urlProtocolValue))
                        {
                            handlerExists = true; 
                        }
                    }
                }
            }
            catch (Exception e)
                when ((e is SecurityException) || (e is IOException) || (e is UnauthorizedAccessException))
            {
                // RegistryKey.OpenSubKey(string) can throw
                // SecurityException, IOException, UnauthorizedAccessException, 
                // ObjectDisposedException (ODE) or ArgumentNullException (ANE)
                // Of these, ODE and ANE should never occur in the code above in
                // the try block.
                //
                // RegistryKey.GetValue(string) can throw SecurityException, 
                // ObjectDisposedException (ODE), IOException, UnauthorizedAccessException. 
                // Of these, ODE should never occur in the code above in the try block. 
                // 
                // If any of the relevant exceptions are encountered, we conclude that 
                // the existance of codeflow:// protocol handler is indeterminate, and 
                // therefore we return false.

                // Do nothing here - handlerExists is already initialized to false. 
                // handlerExists = false;
            }

            return handlerExists;
        }
    }
}
