// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.InteropServices;

namespace Microsoft.Tools.TeamMate.Foundation.Native
{
    /// <summary>
    /// Exposes PInvoke method wrappers for functions in ole32.dll.
    /// </summary>
    public static partial class NativeMethods
    {
        [DllImport("ole32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        public static extern int StgCreateDocfile([MarshalAs(UnmanagedType.LPWStr)]string pwcsName, uint grfMode, uint reserved, out IStorage ppstgOpen);

        /// <summary>
        /// This function registers with OLE the instance of an EXE application's IOleMessageFilter interface, 
        /// which is to be used for handling concurrency issues. DLL object applications cannot register 
        /// a message filter.
        /// </summary>
        /// <param name="newFilter">IOleMessageFilter interface on the message filter supplied by the application. 
        /// Can be <c>null</c>, indicating that the current IOleMessageFilter registration should be revoked.</param>
        /// <param name="oldFilter">Variable that receives the previously registered message filter.</param>
        [DllImport("ole32.dll")]
        public static extern int CoRegisterMessageFilter(IOleMessageFilter newFilter, out IOleMessageFilter oldFilter);

        [DllImport("Ole32.dll", PreserveSig = false)] // returns hresult
        public extern static void PropVariantClear([In, Out] PropVariant pvar);
    }
}
