// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Tools.TeamMate.Foundation.Native
{
    /// <summary>
    /// Exposes PInvoke method wrappers for functions in wtsapi32.dll.
    /// </summary>
    public static partial class NativeMethods
    {
        [DllImport("wtsapi32.dll")]
        public static extern bool WTSRegisterSessionNotification(IntPtr hWnd, int dwFlags);

        [DllImport("wtsapi32.dll")]
        public static extern bool WTSUnRegisterSessionNotification(IntPtr hWnd);
    }
}
