// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.Windows;

namespace Microsoft.Tools.TeamMate.Foundation.Windows
{
    /// <summary>
    /// Provides utility methods for the Clipboard.
    /// </summary>
    public static class ClipboardUtilities
    {
        /// <summary>
        /// Tries the get data object from the clipboard, and never fails with a thrown exception.
        /// </summary>
        /// <returns>The data object, or <c>null</c> if one couldn't be retrieved.</returns>
        public static IDataObject TryGetDataObject()
        {
            try
            {
                return Clipboard.GetDataObject();
            }
            catch(Exception e)
            {
                // System.Runtime.InteropServices.SEHException (0x80004005): External component has thrown an exception.
                //   at System.Windows.Clipboard.GetDataObjectInternal()

                // System.Runtime.InteropServices.COMException (0x800401D0): OpenClipboard Failed (Exception from HRESULT: 0x800401D0 (CLIPBRD_E_CANT_OPEN))
                // at System.Runtime.InteropServices.Marshal.ThrowExceptionForHRInternal(Int32 errorCode, IntPtr errorInfo)
                // at System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(Int32 errorCode)
                // at System.Windows.Clipboard.GetDataObjectInternal()

                Log.WarnAndBreak(e);
            }

            return null;
        }
    }
}
