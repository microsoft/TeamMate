// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Microsoft.Tools.TeamMate.Foundation.Native
{
    // Defines COM imports to expose well-known COM objects as C# classes

    [ComImport]
    [Guid("46EB5926-582E-4017-9FDF-E8998DAA0950")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IImageList
    {
        [PreserveSig]
        int Add(IntPtr hbmImage, IntPtr hbmMask, ref int pi);

        [PreserveSig]
        int ReplaceIcon(int i, IntPtr hicon, ref int pi);

        [PreserveSig]
        int SetOverlayImage(int iImage, int iOverlay);

        [PreserveSig]
        int Replace(int i, IntPtr hbmImage, IntPtr hbmMask);

        [PreserveSig]
        int AddMasked(IntPtr hbmImage, int crMask, ref int pi);

        [PreserveSig]
        int Draw(ref IMAGELISTDRAWPARAMS pimldp);

        [PreserveSig]
        int Remove(int i);

        [PreserveSig]
        int GetIcon(int i, int flags, ref IntPtr picon);

        [PreserveSig]
        int GetImageInfo(int i, ref IMAGEINFO pImageInfo);

        [PreserveSig]
        int Copy(int iDst, IImageList punkSrc, int iSrc, int uFlags);

        [PreserveSig]
        int Merge(int i1, IImageList punk2, int i2, int dx, int dy, ref Guid riid, ref IntPtr ppv);

        [PreserveSig]
        int Clone(ref Guid riid, ref IntPtr ppv);

        [PreserveSig]
        int GetImageRect(int i, ref RECT prc);

        [PreserveSig]
        int GetIconSize(ref int cx, ref int cy);

        [PreserveSig]
        int SetIconSize(int cx, int cy);

        [PreserveSig]
        int GetImageCount(ref int pi);

        [PreserveSig]
        int SetImageCount(int uNewCount);

        [PreserveSig]
        int SetBkColor(int clrBk, ref int pclr);

        [PreserveSig]
        int GetBkColor(ref int pclr);

        [PreserveSig]
        int BeginDrag(int iTrack, int dxHotspot, int dyHotspot);

        [PreserveSig]
        int EndDrag();

        [PreserveSig]
        int DragEnter(IntPtr hwndLock, int x, int y);

        [PreserveSig]
        int DragLeave(IntPtr hwndLock);

        [PreserveSig]
        int DragMove(int x, int y);

        [PreserveSig]
        int SetDragCursorImage(ref IImageList punk, int iDrag, int dxHotspot, int dyHotspot);

        [PreserveSig]
        int DragShowNolock(int fShow);

        [PreserveSig]
        int GetDragImage(ref POINT ppt, ref POINT pptHotspot, ref Guid riid, ref IntPtr ppv);

        [PreserveSig]
        int GetItemFlags(int i, ref int dwFlags);

        [PreserveSig]
        int GetOverlayImage(int iOverlay, ref int piIndex);
    };

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0000000B-0000-0000-C000-000000000046")]
    public interface IStorage
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        IStream CreateStream([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, [In, MarshalAs(UnmanagedType.U4)] int grfMode, [In, MarshalAs(UnmanagedType.U4)] int reserved1, [In, MarshalAs(UnmanagedType.U4)] int reserved2);

        [return: MarshalAs(UnmanagedType.Interface)]
        IStream OpenStream([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, IntPtr reserved1, [In, MarshalAs(UnmanagedType.U4)] int grfMode, [In, MarshalAs(UnmanagedType.U4)] int reserved2);

        [return: MarshalAs(UnmanagedType.Interface)]
        IStorage CreateStorage([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, [In, MarshalAs(UnmanagedType.U4)] int grfMode, [In, MarshalAs(UnmanagedType.U4)] int reserved1, [In, MarshalAs(UnmanagedType.U4)] int reserved2);

        [return: MarshalAs(UnmanagedType.Interface)]
        IStorage OpenStorage([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, IntPtr pstgPriority, [In, MarshalAs(UnmanagedType.U4)] int grfMode, IntPtr snbExclude, [In, MarshalAs(UnmanagedType.U4)] int reserved);

        void CopyTo(int ciidExclude, [In, MarshalAs(UnmanagedType.LPArray)] Guid[] pIIDExclude, IntPtr snbExclude, [In, MarshalAs(UnmanagedType.Interface)] IStorage stgDest);

        void MoveElementTo([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, [In, MarshalAs(UnmanagedType.Interface)] IStorage stgDest, [In, MarshalAs(UnmanagedType.BStr)] string pwcsNewName, [In, MarshalAs(UnmanagedType.U4)] int grfFlags);

        void Commit(int grfCommitFlags);

        void Revert();

        void EnumElements([In, MarshalAs(UnmanagedType.U4)] int reserved1, IntPtr reserved2, [In, MarshalAs(UnmanagedType.U4)] int reserved3, [MarshalAs(UnmanagedType.Interface)] out object ppVal);

        void DestroyElement([In, MarshalAs(UnmanagedType.BStr)] string pwcsName);

        void RenameElement([In, MarshalAs(UnmanagedType.BStr)] string pwcsOldName, [In, MarshalAs(UnmanagedType.BStr)] string pwcsNewName);

        void SetElementTimes([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, [In] System.Runtime.InteropServices.ComTypes.FILETIME pctime, [In] System.Runtime.InteropServices.ComTypes.FILETIME patime, [In] System.Runtime.InteropServices.ComTypes.FILETIME pmtime);

        void SetClass([In] ref Guid clsid);

        void SetStateBits(int grfStateBits, int grfMask);

        void Stat([Out]out System.Runtime.InteropServices.ComTypes.STATSTG pStatStg, int grfStatFlag);
    }

    [ComImport, Guid("591209c7-767b-42b2-9fba-44ee4615f2c7")]
    public class ApplicationRegistrationClass
    {
    }

    [CoClass(typeof(ApplicationRegistrationClass))]
    [ComImport, Guid("4e530b0a-e611-4c77-a3ac-9031d022281b")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IApplicationRegistration
    {
        [return: MarshalAs(UnmanagedType.LPWStr)]
        string QueryCurrentDefault(
            [MarshalAs(UnmanagedType.LPWStr)] string query,
            AssociationType queryType,
            AssociationLevel queryLevel);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool QueryAppIsDefault(
            [MarshalAs(UnmanagedType.LPWStr)] string query,
            AssociationType queryType,
            AssociationLevel queryLevel,
            [MarshalAs(UnmanagedType.LPWStr)] string appRegistryName);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool QueryAppIsDefaultAll(
            AssociationLevel queryLevel,
            [MarshalAs(UnmanagedType.LPWStr)] string appRegistryName);

        void SetAppAsDefault(
            [MarshalAs(UnmanagedType.LPWStr)] string appRegistryName,
            [MarshalAs(UnmanagedType.LPWStr)] string set,
            AssociationType setType);

        void SetAppAsDefaultAll(
            [MarshalAs(UnmanagedType.LPWStr)] string appRegistryName);

        void ClearUserAssociations();
    }

    public enum AssociationType
    {
        FileExtension,
        UrlProtocol,
        StartMenuClient,
        MimeType
    }

    public enum AssociationLevel
    {
        Machine,
        Effective,
        User
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("8895b1c6-b41f-4c1c-a562-0d564250836f")]
    public interface IPreviewHandler
    {
        void SetWindow(IntPtr hwnd, ref RECT rect);
        void SetRect(ref RECT rect);
        void DoPreview();
        void Unload();
        void SetFocus();
        void QueryFocus(out IntPtr phwnd);
        [PreserveSig]
        uint TranslateAccelerator(ref MSG pmsg);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("b824b49d-22ac-4161-ac8a-9916e8fa3f7f")]
    public interface IInitializeWithStream
    {
        void Initialize(IStream pstream, uint grfMode);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("b7d14566-0509-4cce-a71f-0a554233bd9b")]
    public interface IInitializeWithFile
    {
        void Initialize([MarshalAs(UnmanagedType.LPWStr)] string pszFilePath, uint grfMode);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("7F73BE3F-FB79-493C-A6C7-7EE14E245841")]
    public interface IInitializeWithItem
    {
        void Initialize(IShellItem psi, uint grfMode);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe")]
    public interface IShellItem
    {
        void BindToHandler(IntPtr pbc,
            [MarshalAs(UnmanagedType.LPStruct)]Guid bhid,
            [MarshalAs(UnmanagedType.LPStruct)]Guid riid,
            out IntPtr ppv);

        void GetParent(out IShellItem ppsi);

        void GetDisplayName(SIGDN sigdnName, out IntPtr ppszName);

        void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);

        void Compare(IShellItem psi, uint hint, out int piOrder);
    };

    [ComImportAttribute()]
    [GuidAttribute("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IShellItemImageFactory
    {
        void GetImage(
        [In, MarshalAs(UnmanagedType.Struct)] SIZE size,
        [In] SIIGBF flags,
        [Out] out IntPtr phbm);
    }

    /// <summary>
    /// The IOleMessageFilter interface provides COM servers and applications with the ability to selectively 
    /// handle incoming and outgoing COM messages while waiting for responses from synchronous calls. 
    /// Filtering messages helps to ensure that calls are handled in a manner that improves 
    /// performance and avoids deadlocks. COM messages can be synchronous, asynchronous, or input-synchronized; 
    /// the majority of interface calls are synchronous. 
    /// </summary>
    [ComImport, Guid("00000016-0000-0000-C000-000000000046"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOleMessageFilter
    {
        /// <summary>
        /// This method is an object-based method that provides the ability to filter or reject incoming calls 
        /// (or call backs) to an object or a process. This method is called prior to each method 
        /// invocation originating outside the current process. 
        /// </summary>
        /// <param name="dwCallType">Kind of incoming call that has been received.</param>
        /// <param name="hTaskCaller">Handle of the task calling this task.</param>
        /// <param name="dwTickCount">Elapsed tick count since the outgoing call was made if dwCallType is not CALLTYPE_TOPLEVEL.</param>
        /// <param name="lpInterfaceInfo">Pointer to an INTERFACEINFO structure, which identifies the object, the interface, and the method making the call.</param>
        /// <returns>
        /// <para>SERVERCALL_ISHANDLED: The application might be able to process the call.</para>
        /// <para>SERVERCALL_REJECTED: The application cannot handle the call due to an unforeseen problem, 
        /// such as network unavailability, or if it is in the process of terminating.</para>
        /// <para>SERVERCALL_RETRYLATER: The application cannot handle the call at this time.</para>
        /// </returns>
        [PreserveSig]
        int HandleInComingCall(int dwCallType, IntPtr hTaskCaller, int dwTickCount, IntPtr lpInterfaceInfo);

        /// <summary>
        /// This client-based method gives the application an opportunity to display a dialog box so 
        /// the user can retry or cancel the call, or switch to the task identified by hTaskCallee. 
        /// </summary>
        /// <param name="hTaskCallee">Handle of the server task that rejected the call.</param>
        /// <param name="dwTickCount">Number of elapsed ticks since the call was made.</param>
        /// <param name="dwRejectType">Specifies either SERVERCALL_REJECTED or SERVERCALL_RETRYLATER, as returned by the object application.</param>
        /// <returns>
        /// <para>-1: The call should be canceled. COM then returns RPC_E_CALL_REJECTED from the original method call.</para>
        /// <para>Value &gt;= 0 and &lt;100: The call is to be retried immediately.</para>
        /// <para>Value &gt;= 100: COM will wait for this many milliseconds and then retry the call.</para>
        /// </returns>
        [PreserveSig]
        int RetryRejectedCall(IntPtr hTaskCallee, int dwTickCount, int dwRejectType);

        /// <summary>
        /// This client-based method is called by COM when a Windows message appears in a COM application's 
        /// message queue while the application is waiting for a reply to a remote call. 
        /// </summary>
        /// <param name="hTaskCallee">Task handle of the called application that has not yet responded.</param>
        /// <param name="dwTickCount">Number of ticks since the call was made.</param>
        /// <param name="dwPendingType">Type of call made during which a message or event was received.</param>
        [PreserveSig]
        int MessagePending(IntPtr hTaskCallee, int dwTickCount, int dwPendingType);
    }


    [ComImport, Guid("7E5FE3D9-985F-4908-91F9-EE19F9FD1514")]
    public class AppVisibilityClass
    {
    }

    [CoClass(typeof(AppVisibilityClass))]
    [ComImport, Guid("2246EA2D-CAEA-4444-A3C4-6DE827E44313"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAppVisibility
    {
        MONITOR_APP_VISIBILITY GetAppVisibilityOnMonitor(IntPtr hMonitor);

        bool IsLauncherVisible();

        int Advise(IAppVisibilityEvents pCallback);

        void Unadvise([In] int dwCookie);
    }

    public enum MONITOR_APP_VISIBILITY
    {
        MAV_UNKNOWN = 0,
        MAV_NO_APP_VISIBLE = 1,
        MAV_APP_VISIBLE = 2
    }

    [ComImport, Guid("6584CE6B-7D82-49C2-89C9-C6BC02BA8C38"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAppVisibilityEvents
    {
        void AppVisibilityOnMonitorChanged(IntPtr hMonitor, MONITOR_APP_VISIBILITY previousMode, MONITOR_APP_VISIBILITY currentMode);

        void LauncherVisibilityChange(bool currentVisibleState);
    }

    /// <summary>The IShellLink interface allows Shell links to be created, modified, and resolved</summary>
    [CoClass(typeof(CShellLink))]
    [ComImport(), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214F9-0000-0000-C000-000000000046")]
    interface IShellLinkW
    {
        /// <summary>Retrieves the path and file name of a Shell link object</summary>
        void GetPath([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, IntPtr pfd, uint fFlags);
        /// <summary>Retrieves the list of item identifiers for a Shell link object</summary>
        void GetIDList(out IntPtr ppidl);
        /// <summary>Sets the pointer to an item identifier list (PIDL) for a Shell link object.</summary>
        void SetIDList(IntPtr pidl);
        /// <summary>Retrieves the description string for a Shell link object</summary>
        void GetDescription([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
        /// <summary>Sets the description for a Shell link object. The description can be any application-defined string</summary>
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        /// <summary>Retrieves the name of the working directory for a Shell link object</summary>
        void GetWorkingDirectory([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
        /// <summary>Sets the name of the working directory for a Shell link object</summary>
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        /// <summary>Retrieves the command-line arguments associated with a Shell link object</summary>
        void GetArguments([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
        /// <summary>Sets the command-line arguments for a Shell link object</summary>
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        /// <summary>Retrieves the hot key for a Shell link object</summary>
        void GetHotkey(out short pwHotkey);
        /// <summary>Sets a hot key for a Shell link object</summary>
        void SetHotkey(short wHotkey);
        /// <summary>Retrieves the show command for a Shell link object</summary>
        void GetShowCmd(out int piShowCmd);
        /// <summary>Sets the show command for a Shell link object. The show command sets the initial show state of the window.</summary>
        void SetShowCmd(int iShowCmd);
        /// <summary>Retrieves the location (path and index) of the icon for a Shell link object</summary>
        void GetIconLocation([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath,
            int cchIconPath, out int piIcon);
        /// <summary>Sets the location (path and index) of the icon for a Shell link object</summary>
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        /// <summary>Sets the relative path to the Shell link object</summary>
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
        /// <summary>Attempts to find the target of a Shell link, even if it has been moved or renamed</summary>
        void Resolve(IntPtr hwnd, uint fFlags);
        /// <summary>Sets the path and file name of a Shell link object</summary>
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }

    [ComImport, Guid("00021401-0000-0000-C000-000000000046"), ClassInterface(ClassInterfaceType.None)]
    public class CShellLink
    {
    }

    [ComImport, Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPropertyStore
    {
        UInt32 GetCount([Out] out uint propertyCount);
        UInt32 GetAt([In] uint propertyIndex, out PropertyKey key);
        UInt32 GetValue([In] ref PropertyKey key, [Out] PropVariant pv);
        UInt32 SetValue([In] ref PropertyKey key, [In] PropVariant pv);
        UInt32 Commit();
    }
}
