// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Tools.TeamMate.Foundation.Native
{
    /// <summary>
    /// Exposes PInvoke method wrappers for functions in OleAut32.dll.
    /// </summary>
    public static partial class NativeMethods
    {
        [DllImport("OleAut32.dll", PreserveSig = true)] // psa is actually returned, not hresult
        public extern static IntPtr SafeArrayCreateVector(ushort vt, int lowerBound, uint cElems);

        [DllImport("OleAut32.dll", PreserveSig = false)] // returns hresult
        public extern static IntPtr SafeArrayAccessData(IntPtr psa);

        [DllImport("OleAut32.dll", PreserveSig = false)] // returns hresult
        public extern static void SafeArrayUnaccessData(IntPtr psa);

        [DllImport("OleAut32.dll", PreserveSig = true)] // retuns uint32
        public extern static uint SafeArrayGetDim(IntPtr psa);

        [DllImport("OleAut32.dll", PreserveSig = false)] // returns hresult
        public extern static int SafeArrayGetLBound(IntPtr psa, uint nDim);

        [DllImport("OleAut32.dll", PreserveSig = false)] // returns hresult
        public extern static int SafeArrayGetUBound(IntPtr psa, uint nDim);

        // This decl for SafeArrayGetElement is only valid for cDims==1!
        [DllImport("OleAut32.dll", PreserveSig = false)] // returns hresult
        [return: MarshalAs(UnmanagedType.IUnknown)]
        public extern static object SafeArrayGetElement(IntPtr psa, ref int rgIndices);
    }
}
