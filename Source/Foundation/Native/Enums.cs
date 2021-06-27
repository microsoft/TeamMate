// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Tools.TeamMate.Foundation.Native
{
    // Defines enum mappings for PInvoke functions.

    public enum SystemMetric
    {
        /// <summary>
        /// Number of pixels per logical inch along the screen width. In a system with multiple display monitors, this value is the same for all monitors.
        /// </summary>
        SM_LOGPIXELSX = 88,

        /// <summary>
        /// Number of pixels per logical inch along the screen height. In a system with multiple display monitors, this value is the same for all monitors.
        /// </summary>
        SM_LOGPIXELSY = 90,

        /// <summary>
        /// The return value is a bitmask that specifies the type of digitizer input supported by the device.
        /// </summary>
        SM_DIGITIZER = 94,
        
        /// <summary>
        /// The aggregate maximum of the maximum number of contacts supported by every digitizer in the system.
        /// </summary>
        SM_MAXIMUMTOUCHES = 95,
    }

    public static class NativeConstants
    {
        public const int LWA_COLORKEY = 0x1;
        public const int LWA_ALPHA = 0x2;

        public const int GCL_HICONSM = -34;
        public const int GCL_HICON = -14;

        public const int ICON_SMALL = 0;
        public const int ICON_BIG = 1;
        public const int ICON_SMALL2 = 2;

        public const int WS_ACTIVECAPTION = 0x0001;
    }

    [Flags]
    public enum SPIF
    {
        NONE = 0x00,
        SPIF_UPDATEINIFILE = 0x01,  // Writes the new system-wide parameter setting to the user profile.
        SPIF_SENDCHANGE = 0x02,  // Broadcasts the WM_SETTINGCHANGE message after updating the user profile.
        SPIF_SENDWININICHANGE = 0x02   // Same as SPIF_SENDCHANGE.
    }

    public enum RasterOperations : uint
    {
        SRCCOPY = 0x00CC0020, /* dest = source*/
        SRCPAINT = 0x00EE0086, /* dest = source OR dest*/
        SRCAND = 0x008800C6, /* dest = source AND dest*/
        SRCINVERT = 0x00660046, /* dest = source XOR dest*/
        SRCERASE = 0x00440328, /* dest = source AND (NOT dest )*/
        NOTSRCCOPY = 0x00330008, /* dest = (NOT source)*/
        NOTSRCERASE = 0x001100A6, /* dest = (NOT src) AND (NOT dest) */
        MERGECOPY = 0x00C000CA, /* dest = (source AND pattern)*/
        MERGEPAINT = 0x00BB0226, /* dest = (NOT source) OR dest*/
        PATCOPY = 0x00F00021, /* dest = pattern*/
        PATPAINT = 0x00FB0A09, /* dest = DPSnoo*/
        PATINVERT = 0x005A0049, /* dest = pattern XOR dest*/
        DSTINVERT = 0x00550009, /* dest = (NOT dest)*/
        BLACKNESS = 0x00000042, /* dest = BLACK*/
        WHITENESS = 0x00FF0062, /* dest = WHITE*/
    };

    public enum GetWindowLong : int
    {
        GWL_WNDPROC = (-4),
        GWL_HINSTANCE = (-6),
        GWL_HWNDPARENT = (-8),
        GWL_STYLE = (-16),
        GWL_EXSTYLE = (-20),
        GWL_USERDATA = (-21),
        GWL_ID = (-12)
    }

    public enum ShowWindow : int
    {
        /// <summary>
        /// Hides the window and activates another window.
        /// </summary>
        SW_HIDE = 0,
        /// <summary>
        /// Activates and displays a window. If the window is minimized or 
        /// maximized, the system restores it to its original size and position.
        /// An application should specify this flag when displaying the window 
        /// for the first time.
        /// </summary>
        SW_SHOWNORMAL = 1,

        /// <summary>
        /// Activates the window and displays it as a minimized window.
        /// </summary>
        SW_SHOWMINIMIZED = 2,
        /// <summary>
        /// Maximizes the specified window.
        /// </summary>
        SW_SHOWMAXIMIZED = 3, // is this the right value?
        /// <summary>
        /// Displays a window in its most recent size and position. This value 
        /// is similar to <see cref="Win32.ShowWindowCommand.Normal"/>, except 
        /// the window is not actived.
        /// </summary>
        SW_SHOWNOACTIVATE = 4,
        /// <summary>
        /// Activates the window and displays it in its current size and position. 
        /// </summary>
        SW_SHOW = 5,
        /// <summary>
        /// Minimizes the specified window and activates the next top-level 
        /// window in the Z order.
        /// </summary>
        SW_MINIMIZE = 6,
        /// <summary>
        /// Displays the window as a minimized window. This value is similar to
        /// <see cref="Win32.ShowWindowCommand.ShowMinimized"/>, except the 
        /// window is not activated.
        /// </summary>
        SW_SHOWMINNOACTIVE = 7,
        /// <summary>
        /// Displays the window in its current size and position. This value is 
        /// similar to <see cref="Win32.ShowWindowCommand.Show"/>, except the 
        /// window is not activated.
        /// </summary>
        SW_SHOWNA = 8,
        /// <summary>
        /// Activates and displays the window. If the window is minimized or 
        /// maximized, the system restores it to its original size and position. 
        /// An application should specify this flag when restoring a minimized window.
        /// </summary>
        SW_RESTORE = 9,
        /// <summary>
        /// Sets the show state based on the SW_* value specified in the 
        /// STARTUPINFO structure passed to the CreateProcess function by the 
        /// program that started the application.
        /// </summary>
        SW_SHOWDEFAULT = 10,
        /// <summary>
        ///  <b>Windows 2000/XP:</b> Minimizes a window, even if the thread 
        /// that owns the window is not responding. This flag should only be 
        /// used when minimizing windows from a different thread.
        /// </summary>
        SW_FORCEMINIMIZE = 11
    }

    /// <summary>
    /// Window Styles.
    /// The following styles can be specified wherever a window style is required. After the control has been created, these styles cannot be modified, except as noted.
    /// </summary>
    [Flags]
    public enum WindowStyles : uint
    {
        /// <summary>
        /// Creates an overlapped window. An overlapped window usually has a caption and a border.
        /// </summary>
        WS_OVERLAPPED = 0x00000000,

        /// <summary>
        /// Creates a pop-up window. Cannot be used with the <see cref="WS_CHILD"/> style.
        /// </summary>
        WS_POPUP = 0x80000000,

        /// <summary>
        /// Creates a child window. Cannot be used with the <see cref="WS_POPUP"/> style.
        /// </summary>
        WS_CHILD = 0x40000000,

        /// <summary>
        /// Creates a window that is initially minimized. For use with the <see cref="WS_OVERLAPPED"/> style only.
        /// </summary>
        WS_MINIMIZE = 0x20000000,

        /// <summary>
        /// Creates a window that is initially visible.
        /// </summary>
        WS_VISIBLE = 0x10000000,

        /// <summary>
        /// Creates a window that is initially disabled.
        /// </summary>
        WS_DISABLED = 0x08000000,

        /// <summary>
        /// Clips child windows relative to each other; that is, when a particular child window receives a paint message, the WS_CLIPSIBLINGS style clips all other overlapped child windows out of the region of the child window to be updated. (If WS_CLIPSIBLINGS is not given and child windows overlap, when you draw within the client area of a child window, it is possible to draw within the client area of a neighboring child window.) For use with the <see cref="WS_CHILD"/> style only.
        /// </summary>
        WS_CLIPSIBLINGS = 0x04000000,

        /// <summary>
        /// Excludes the area occupied by child windows when you draw within the parent window.
        /// Used when you create the parent window. 
        /// </summary>
        WS_CLIPCHILDREN = 0x02000000,

        /// <summary>
        /// Creates a window of maximum size.
        /// </summary>
        WS_MAXIMIZE = 0x01000000,

        /// <summary>
        /// Creates a window that has a border.
        /// </summary>
        WS_BORDER = 0x00800000,

        /// <summary>
        /// Creates a window with a double border but no title.
        /// </summary>
        WS_DLGFRAME = 0x00400000,

        /// <summary>
        /// Creates a window that has a vertical scroll bar.
        /// </summary>
        WS_VSCROLL = 0x00200000,

        /// <summary>
        /// Creates a window that has a horizontal scroll bar.
        /// </summary>
        WS_HSCROLL = 0x00100000,

        /// <summary>
        /// Creates a window that has a Control-menu box in its title bar. Used only for windows with title bars.
        /// </summary>
        WS_SYSMENU = 0x00080000,

        /// <summary>
        /// Creates a window with a thick frame that can be used to size the window.
        /// </summary>
        WS_THICKFRAME = 0x00040000,

        /// <summary>
        /// Specifies the first control of a group of controls in which the user can move from one control to the next with the arrow keys. All controls defined with the WS_GROUP style FALSE after the first control belong to the same group. The next control with the WS_GROUP style starts the next group (that is, one group ends where the next begins).
        /// </summary>
        WS_GROUP = 0x00020000,

        /// <summary>
        /// Specifies one of any number of controls through which the user can move by using the TAB key. The TAB key moves the user to the next control specified by the WS_TABSTOP style.
        /// </summary>
        WS_TABSTOP = 0x00010000,

        /// <summary>
        /// Creates a window that has a Minimize button.
        /// </summary>
        WS_MINIMIZEBOX = 0x00020000,

        /// <summary>
        /// Creates a window that has a Maximize button.
        /// </summary>
        WS_MAXIMIZEBOX = 0x00010000,

        /// <summary>
        /// Creates a window that has a title bar (implies the <see cref="WS_BORDER"/> style).
        /// Cannot be used with the <see cref="WS_DLGFRAME"/> style.
        /// </summary>
        WS_CAPTION = WS_BORDER | WS_DLGFRAME,

        /// <summary>
        /// Creates an overlapped window. An overlapped window has a title bar and a border. Same as the <see cref="WS_OVERLAPPED"/> style.
        /// </summary>
        WS_TILED = WS_OVERLAPPED,

        /// <summary>
        /// Creates a window that is initially minimized. Same as the <see cref="WS_MINIMIZE"/> style. 
        /// </summary>
        WS_ICONIC = WS_MINIMIZE,

        /// <summary>
        /// Creates a window that has a sizing border. Same as the <see cref="WS_THICKFRAME"/> style.
        /// </summary>
        WS_SIZEBOX = WS_THICKFRAME,

        /// <summary>
        /// Creates an overlapped window with the <see cref="WS_OVERLAPPED"/>, <see cref="WS_CAPTION"/>, <see cref="WS_SYSMENU"/>, <see cref="WS_THICKFRAME"/>, <see cref="WS_MINIMIZEBOX"/>, and <see cref="WS_MAXIMIZEBOX"/> styles. Same as the <see cref="WS_OVERLAPPEDWINDOW"/> style.
        /// </summary>
        WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW,

        /// <summary>
        /// Creates an overlapped window with the <see cref="WS_OVERLAPPED"/>, <see cref="WS_CAPTION"/>, <see cref="WS_SYSMENU"/>, <see cref="WS_THICKFRAME"/>, <see cref="WS_MINIMIZEBOX"/>, and <see cref="WS_MAXIMIZEBOX"/> styles. 
        /// </summary>
        WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,

        /// <summary>
        /// Creates a pop-up window with the <see cref="WS_BORDER"/>, <see cref="WS_POPUP"/>, and <see cref="WS_SYSMENU"/> styles. The WS_CAPTION style must be combined with the <see cref="WS_POPUPWINDOW"/> style to make the Control menu visible.
        /// </summary>
        WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,

        /// <summary>
        /// Same as the <see cref="WS_CHILD"/> style.
        /// </summary>
        WS_CHILDWINDOW = WS_CHILD,

        //Extended Window Styles

        /// <summary>
        /// Designates a window with a double border that may (optionally) be created with a title bar when you specify the <see cref="WS_CAPTION"/> style flag in the dwStyle parameter.
        /// </summary>
        WS_EX_DLGMODALFRAME = 0x00000001,

        /// <summary>
        /// Specifies that a child window created with this style will not send the <see cref="WM_PARENTNOTIFY"/> message to its parent window when the child window is created or destroyed.
        /// </summary>
        WS_EX_NOPARENTNOTIFY = 0x00000004,

        /// <summary>
        /// Specifies that a window created with this style should be placed above all nontopmost windows and stay above them even when the window is deactivated. An application can use the <see cref="SetWindowPos"/> member function to add or remove this attribute.
        /// </summary>
        WS_EX_TOPMOST = 0x00000008,

        /// <summary>
        /// Specifies that a window created with this style accepts drag-and-drop files.
        /// </summary>
        WS_EX_ACCEPTFILES = 0x00000010,

        /// <summary>
        /// Specifies that a window created with this style is to be transparent. That is, any windows that are beneath the window are not obscured by the window. A window created with this style receives <see cref="WM_PAINT"/> messages only after all sibling windows beneath it have been updated.
        /// </summary>
        WS_EX_TRANSPARENT = 0x00000020,

        //#if(WINVER >= 0x0400)

        /// <summary>
        /// Creates an MDI child window.
        /// </summary>
        WS_EX_MDICHILD = 0x00000040,

        /// <summary>
        /// Creates a tool window, which is a window intended to be used as a floating toolbar. A tool window has a title bar that is shorter than a normal title bar, and the window title is drawn using a smaller font. A tool window does not appear in the task bar or in the window that appears when the user presses ALT+TAB.
        /// </summary>
        WS_EX_TOOLWINDOW = 0x00000080,

        /// <summary>
        /// Specifies that a window has a border with a raised edge.
        /// </summary>
        WS_EX_WINDOWEDGE = 0x00000100,

        /// <summary>
        /// Specifies that a window has a 3D look — that is, a border with a sunken edge.
        /// </summary>
        WS_EX_CLIENTEDGE = 0x00000200,

        /// <summary>
        /// Includes a question mark in the title bar of the window. When the user clicks the question mark, the cursor changes to a question mark with a pointer. If the user then clicks a child window, the child receives a <see cref="WM_HELP"/> message.
        /// </summary>
        WS_EX_CONTEXTHELP = 0x00000400,

        /// <summary>
        /// Gives a window generic right-aligned properties. This depends on the window class.
        /// </summary>
        WS_EX_RIGHT = 0x00001000,

        /// <summary>
        /// Gives window generic left-aligned properties. This is the default.
        /// </summary>
        WS_EX_LEFT = 0x00000000,

        /// <summary>
        /// Displays the window text using right-to-left reading order properties.
        /// </summary>
        WS_EX_RTLREADING = 0x00002000,

        /// <summary>
        /// Displays the window text using left-to-right reading order properties. This is the default.
        /// </summary>
        WS_EX_LTRREADING = 0x00000000,

        /// <summary>
        /// Places a vertical scroll bar to the left of the client area.
        /// </summary>
        WS_EX_LEFTSCROLLBAR = 0x00004000,

        /// <summary>
        /// Places a vertical scroll bar (if present) to the right of the client area. This is the default.
        /// </summary>
        WS_EX_RIGHTSCROLLBAR = 0x00000000,

        /// <summary>
        /// Allows the user to navigate among the child windows of the window by using the TAB key.
        /// </summary>
        WS_EX_CONTROLPARENT = 0x00010000,

        /// <summary>
        /// Creates a window with a three-dimensional border style intended to be used for items that do not accept user input.
        /// </summary>
        WS_EX_STATICEDGE = 0x00020000,

        /// <summary>
        /// Forces a top-level window onto the taskbar when the window is visible.
        /// </summary>
        WS_EX_APPWINDOW = 0x00040000,

        /// <summary>
        /// Combines the <see cref="WS_EX_CLIENTEDGE"/> and <see cref="WS_EX_WINDOWEDGE"/> styles.
        /// </summary>
        WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE),

        /// <summary>
        /// Combines the <see cref="WS_EX_WINDOWEDGE"/> and <see cref="WS_EX_TOPMOST"/> styles.
        /// </summary>
        WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST),
        //#endif /* WINVER >= 0x0400 */

        //#if(_WIN32_WINNT >= 0x0500)
        /// <summary>
        /// Windows 2000/XP: Creates a layered window. Note that this cannot be used for child windows. Also, this cannot be used if the window has a class style of either <see cref="CS_OWNDC"/> or <see cref="CS_CLASSDC"/>.
        /// </summary>
        WS_EX_LAYERED = 0x00080000,
        //#endif /* _WIN32_WINNT >= 0x0500 */

        //#if(WINVER >= 0x0500)
        /// <summary>
        /// Windows 2000/XP: A window created with this style does not pass its window layout to its child windows.
        /// </summary>
        WS_EX_NOINHERITLAYOUT = 0x00100000,

        /// <summary>
        /// Arabic and Hebrew versions of Windows 98/Me, Windows 2000/XP: Creates a window whose horizontal origin is on the right edge. Increasing horizontal values advance to the left.
        /// </summary>
        WS_EX_LAYOUTRTL = 0x00400000,
        //#endif /* WINVER >= 0x0500 */

        //#if(_WIN32_WINNT >= 0x0500)
        /// <summary>
        /// Windows XP: Paints all descendants of a window in bottom-to-top painting order using double-buffering. For more information, see Remarks. This cannot be used if the window has a class style of either <see cref="CS_OWNDC"/> or <see cref="CS_CLASSDC"/>.
        /// </summary>
        WS_EX_COMPOSITED = 0x02000000,

        /// <summary>
        /// Windows 2000/XP: A top-level window created with this style does not become the foreground window when the user clicks it. The system does not bring this window to the foreground when the user minimizes or closes the foreground window.
        /// To activate the window, use the <see cref="SetActiveWindow"/> or <see cref="SetForegroundWindow"/> function.
        /// The window does not appear on the taskbar by default. To force the window to appear on the taskbar, use the <see cref="WS_EX_APPWINDOW"/> style.
        /// </summary>
        WS_EX_NOACTIVATE = 0x08000000
        //#endif /* _WIN32_WINNT >= 0x0500 */
    }

    /// <summary>
    /// Windows Messages
    /// Defined in winuser.h from Windows SDK v6.1
    /// Documentation pulled from MSDN.
    /// </summary>
    public enum WindowsMessage : uint
    {
        /// <summary>
        /// The WM_NULL message performs no operation. An application sends the WM_NULL message if it wants to post a message that the recipient window will ignore.
        /// </summary>
        WM_NULL = 0x0000,
        /// <summary>
        /// The WM_CREATE message is sent when an application requests that a window be created by calling the CreateWindowEx or CreateWindow function. (The message is sent before the function returns.) The window procedure of the new window receives this message after the window is created, but before the window becomes visible.
        /// </summary>
        WM_CREATE = 0x0001,
        /// <summary>
        /// The WM_DESTROY message is sent when a window is being destroyed. It is sent to the window procedure of the window being destroyed after the window is removed from the screen. 
        /// This message is sent first to the window being destroyed and then to the child windows (if any) as they are destroyed. During the processing of the message, it can be assumed that all child windows still exist.
        /// /// </summary>
        WM_DESTROY = 0x0002,
        /// <summary>
        /// The WM_MOVE message is sent after a window has been moved. 
        /// </summary>
        WM_MOVE = 0x0003,
        /// <summary>
        /// The WM_SIZE message is sent to a window after its size has changed.
        /// </summary>
        WM_SIZE = 0x0005,
        /// <summary>
        /// The WM_ACTIVATE message is sent to both the window being activated and the window being deactivated. If the windows use the same input queue, the message is sent synchronously, first to the window procedure of the top-level window being deactivated, then to the window procedure of the top-level window being activated. If the windows use different input queues, the message is sent asynchronously, so the window is activated immediately. 
        /// </summary>
        WM_ACTIVATE = 0x0006,
        /// <summary>
        /// The WM_SETFOCUS message is sent to a window after it has gained the keyboard focus. 
        /// </summary>
        WM_SETFOCUS = 0x0007,
        /// <summary>
        /// The WM_KILLFOCUS message is sent to a window immediately before it loses the keyboard focus. 
        /// </summary>
        WM_KILLFOCUS = 0x0008,
        /// <summary>
        /// The WM_ENABLE message is sent when an application changes the enabled state of a window. It is sent to the window whose enabled state is changing. This message is sent before the EnableWindow function returns, but after the enabled state (WS_DISABLED style bit) of the window has changed. 
        /// </summary>
        WM_ENABLE = 0x000A,
        /// <summary>
        /// An application sends the WM_SETREDRAW message to a window to allow changes in that window to be redrawn or to prevent changes in that window from being redrawn. 
        /// </summary>
        WM_SETREDRAW = 0x000B,
        /// <summary>
        /// An application sends a WM_SETTEXT message to set the text of a window. 
        /// </summary>
        WM_SETTEXT = 0x000C,
        /// <summary>
        /// An application sends a WM_GETTEXT message to copy the text that corresponds to a window into a buffer provided by the caller. 
        /// </summary>
        WM_GETTEXT = 0x000D,
        /// <summary>
        /// An application sends a WM_GETTEXTLENGTH message to determine the length, in characters, of the text associated with a window. 
        /// </summary>
        WM_GETTEXTLENGTH = 0x000E,
        /// <summary>
        /// The WM_PAINT message is sent when the system or another application makes a request to paint a portion of an application's window. The message is sent when the UpdateWindow or RedrawWindow function is called, or by the DispatchMessage function when the application obtains a WM_PAINT message by using the GetMessage or PeekMessage function. 
        /// </summary>
        WM_PAINT = 0x000F,
        /// <summary>
        /// The WM_CLOSE message is sent as a signal that a window or an application should terminate.
        /// </summary>
        WM_CLOSE = 0x0010,
        /// <summary>
        /// The WM_QUERYENDSESSION message is sent when the user chooses to end the session or when an application calls one of the system shutdown functions. If any application returns zero, the session is not ended. The system stops sending WM_QUERYENDSESSION messages as soon as one application returns zero.
        /// After processing this message, the system sends the WM_ENDSESSION message with the wParam parameter set to the results of the WM_QUERYENDSESSION message.
        /// </summary>
        WM_QUERYENDSESSION = 0x0011,
        /// <summary>
        /// The WM_QUERYOPEN message is sent to an icon when the user requests that the window be restored to its previous size and position.
        /// </summary>
        WM_QUERYOPEN = 0x0013,
        /// <summary>
        /// The WM_ENDSESSION message is sent to an application after the system processes the results of the WM_QUERYENDSESSION message. The WM_ENDSESSION message informs the application whether the session is ending.
        /// </summary>
        WM_ENDSESSION = 0x0016,
        /// <summary>
        /// The WM_QUIT message indicates a request to terminate an application and is generated when the application calls the PostQuitMessage function. It causes the GetMessage function to return zero.
        /// </summary>
        WM_QUIT = 0x0012,
        /// <summary>
        /// The WM_ERASEBKGND message is sent when the window background must be erased (for example, when a window is resized). The message is sent to prepare an invalidated portion of a window for painting. 
        /// </summary>
        WM_ERASEBKGND = 0x0014,
        /// <summary>
        /// This message is sent to all top-level windows when a change is made to a system color setting. 
        /// </summary>
        WM_SYSCOLORCHANGE = 0x0015,
        /// <summary>
        /// The WM_SHOWWINDOW message is sent to a window when the window is about to be hidden or shown.
        /// </summary>
        WM_SHOWWINDOW = 0x0018,
        /// <summary>
        /// An application sends the WM_WININICHANGE message to all top-level windows after making a change to the WIN.INI file. The SystemParametersInfo function sends this message after an application uses the function to change a setting in WIN.INI.
        /// Note  The WM_WININICHANGE message is provided only for compatibility with earlier versions of the system. Applications should use the WM_SETTINGCHANGE message.
        /// </summary>
        WM_WININICHANGE = 0x001A,
        /// <summary>
        /// An application sends the WM_WININICHANGE message to all top-level windows after making a change to the WIN.INI file. The SystemParametersInfo function sends this message after an application uses the function to change a setting in WIN.INI.
        /// Note  The WM_WININICHANGE message is provided only for compatibility with earlier versions of the system. Applications should use the WM_SETTINGCHANGE message.
        /// </summary>
        WM_SETTINGCHANGE = WM_WININICHANGE,
        /// <summary>
        /// The WM_DEVMODECHANGE message is sent to all top-level windows whenever the user changes device-mode settings. 
        /// </summary>
        WM_DEVMODECHANGE = 0x001B,
        /// <summary>
        /// The WM_ACTIVATEAPP message is sent when a window belonging to a different application than the active window is about to be activated. The message is sent to the application whose window is being activated and to the application whose window is being deactivated.
        /// </summary>
        WM_ACTIVATEAPP = 0x001C,
        /// <summary>
        /// An application sends the WM_FONTCHANGE message to all top-level windows in the system after changing the pool of font resources. 
        /// </summary>
        WM_FONTCHANGE = 0x001D,
        /// <summary>
        /// A message that is sent whenever there is a change in the system time.
        /// </summary>
        WM_TIMECHANGE = 0x001E,
        /// <summary>
        /// The WM_CANCELMODE message is sent to cancel certain modes, such as mouse capture. For example, the system sends this message to the active window when a dialog box or message box is displayed. Certain functions also send this message explicitly to the specified window regardless of whether it is the active window. For example, the EnableWindow function sends this message when disabling the specified window.
        /// </summary>
        WM_CANCELMODE = 0x001F,
        /// <summary>
        /// The WM_SETCURSOR message is sent to a window if the mouse causes the cursor to move within a window and mouse input is not captured. 
        /// </summary>
        WM_SETCURSOR = 0x0020,
        /// <summary>
        /// The WM_MOUSEACTIVATE message is sent when the cursor is in an inactive window and the user presses a mouse button. The parent window receives this message only if the child window passes it to the DefWindowProc function.
        /// </summary>
        WM_MOUSEACTIVATE = 0x0021,
        /// <summary>
        /// The WM_CHILDACTIVATE message is sent to a child window when the user clicks the window's title bar or when the window is activated, moved, or sized.
        /// </summary>
        WM_CHILDACTIVATE = 0x0022,
        /// <summary>
        /// The WM_QUEUESYNC message is sent by a computer-based training (CBT) application to separate user-input messages from other messages sent through the WH_JOURNALPLAYBACK Hook procedure. 
        /// </summary>
        WM_QUEUESYNC = 0x0023,
        /// <summary>
        /// The WM_GETMINMAXINFO message is sent to a window when the size or position of the window is about to change. An application can use this message to override the window's default maximized size and position, or its default minimum or maximum tracking size. 
        /// </summary>
        WM_GETMINMAXINFO = 0x0024,
        /// <summary>
        /// Windows NT 3.51 and earlier: The WM_PAINTICON message is sent to a minimized window when the icon is to be painted. This message is not sent by newer versions of Microsoft Windows, except in unusual circumstances explained in the Remarks.
        /// </summary>
        WM_PAINTICON = 0x0026,
        /// <summary>
        /// Windows NT 3.51 and earlier: The WM_ICONERASEBKGND message is sent to a minimized window when the background of the icon must be filled before painting the icon. A window receives this message only if a class icon is defined for the window; otherwise, WM_ERASEBKGND is sent. This message is not sent by newer versions of Windows.
        /// </summary>
        WM_ICONERASEBKGND = 0x0027,
        /// <summary>
        /// The WM_NEXTDLGCTL message is sent to a dialog box procedure to set the keyboard focus to a different control in the dialog box. 
        /// </summary>
        WM_NEXTDLGCTL = 0x0028,
        /// <summary>
        /// The WM_SPOOLERSTATUS message is sent from Print Manager whenever a job is added to or removed from the Print Manager queue. 
        /// </summary>
        WM_SPOOLERSTATUS = 0x002A,
        /// <summary>
        /// The WM_DRAWITEM message is sent to the parent window of an owner-drawn button, combo box, list box, or menu when a visual aspect of the button, combo box, list box, or menu has changed.
        /// </summary>
        WM_DRAWITEM = 0x002B,
        /// <summary>
        /// The WM_MEASUREITEM message is sent to the owner window of a combo box, list box, list view control, or menu item when the control or menu is created.
        /// </summary>
        WM_MEASUREITEM = 0x002C,
        /// <summary>
        /// Sent to the owner of a list box or combo box when the list box or combo box is destroyed or when items are removed by the LB_DELETESTRING, LB_RESETCONTENT, CB_DELETESTRING, or CB_RESETCONTENT message. The system sends a WM_DELETEITEM message for each deleted item. The system sends the WM_DELETEITEM message for any deleted list box or combo box item with nonzero item data.
        /// </summary>
        WM_DELETEITEM = 0x002D,
        /// <summary>
        /// Sent by a list box with the LBS_WANTKEYBOARDINPUT style to its owner in response to a WM_KEYDOWN message. 
        /// </summary>
        WM_VKEYTOITEM = 0x002E,
        /// <summary>
        /// Sent by a list box with the LBS_WANTKEYBOARDINPUT style to its owner in response to a WM_CHAR message. 
        /// </summary>
        WM_CHARTOITEM = 0x002F,
        /// <summary>
        /// An application sends a WM_SETFONT message to specify the font that a control is to use when drawing text. 
        /// </summary>
        WM_SETFONT = 0x0030,
        /// <summary>
        /// An application sends a WM_GETFONT message to a control to retrieve the font with which the control is currently drawing its text. 
        /// </summary>
        WM_GETFONT = 0x0031,
        /// <summary>
        /// An application sends a WM_SETHOTKEY message to a window to associate a hot key with the window. When the user presses the hot key, the system activates the window. 
        /// </summary>
        WM_SETHOTKEY = 0x0032,
        /// <summary>
        /// An application sends a WM_GETHOTKEY message to determine the hot key associated with a window. 
        /// </summary>
        WM_GETHOTKEY = 0x0033,
        /// <summary>
        /// The WM_QUERYDRAGICON message is sent to a minimized (iconic) window. The window is about to be dragged by the user but does not have an icon defined for its class. An application can return a handle to an icon or cursor. The system displays this cursor or icon while the user drags the icon.
        /// </summary>
        WM_QUERYDRAGICON = 0x0037,
        /// <summary>
        /// The system sends the WM_COMPAREITEM message to determine the relative position of a new item in the sorted list of an owner-drawn combo box or list box. Whenever the application adds a new item, the system sends this message to the owner of a combo box or list box created with the CBS_SORT or LBS_SORT style. 
        /// </summary>
        WM_COMPAREITEM = 0x0039,
        /// <summary>
        /// Active Accessibility sends the WM_GETOBJECT message to obtain information about an accessible object contained in a server application. 
        /// Applications never send this message directly. It is sent only by Active Accessibility in response to calls to AccessibleObjectFromPoint, AccessibleObjectFromEvent, or AccessibleObjectFromWindow. However, server applications handle this message. 
        /// </summary>
        WM_GETOBJECT = 0x003D,
        /// <summary>
        /// The WM_COMPACTING message is sent to all top-level windows when the system detects more than 12.5 percent of system time over a 30- to 60-second interval is being spent compacting memory. This indicates that system memory is low.
        /// </summary>
        WM_COMPACTING = 0x0041,
        /// <summary>
        /// WM_COMMNOTIFY is Obsolete for Win32-Based Applications
        /// </summary>
        [Obsolete]
        WM_COMMNOTIFY = 0x0044,
        /// <summary>
        /// The WM_WINDOWPOSCHANGING message is sent to a window whose size, position, or place in the Z order is about to change as a result of a call to the SetWindowPos function or another window-management function.
        /// </summary>
        WM_WINDOWPOSCHANGING = 0x0046,
        /// <summary>
        /// The WM_WINDOWPOSCHANGED message is sent to a window whose size, position, or place in the Z order has changed as a result of a call to the SetWindowPos function or another window-management function.
        /// </summary>
        WM_WINDOWPOSCHANGED = 0x0047,
        /// <summary>
        /// Notifies applications that the system, typically a battery-powered personal computer, is about to enter a suspended mode.
        /// Use: POWERBROADCAST
        /// </summary>
        [Obsolete]
        WM_POWER = 0x0048,
        /// <summary>
        /// An application sends the WM_COPYDATA message to pass data to another application. 
        /// </summary>
        WM_COPYDATA = 0x004A,
        /// <summary>
        /// The WM_CANCELJOURNAL message is posted to an application when a user cancels the application's journaling activities. The message is posted with a NULL window handle. 
        /// </summary>
        WM_CANCELJOURNAL = 0x004B,
        /// <summary>
        /// Sent by a common control to its parent window when an event has occurred or the control requires some information. 
        /// </summary>
        WM_NOTIFY = 0x004E,
        /// <summary>
        /// The WM_INPUTLANGCHANGEREQUEST message is posted to the window with the focus when the user chooses a new input language, either with the hotkey (specified in the Keyboard control panel application) or from the indicator on the system taskbar. An application can accept the change by passing the message to the DefWindowProc function or reject the change (and prevent it from taking place) by returning immediately. 
        /// </summary>
        WM_INPUTLANGCHANGEREQUEST = 0x0050,
        /// <summary>
        /// The WM_INPUTLANGCHANGE message is sent to the topmost affected window after an application's input language has been changed. You should make any application-specific settings and pass the message to the DefWindowProc function, which passes the message to all first-level child windows. These child windows can pass the message to DefWindowProc to have it pass the message to their child windows, and so on. 
        /// </summary>
        WM_INPUTLANGCHANGE = 0x0051,
        /// <summary>
        /// Sent to an application that has initiated a training card with Microsoft Windows Help. The message informs the application when the user clicks an authorable button. An application initiates a training card by specifying the HELP_TCARD command in a call to the WinHelp function.
        /// </summary>
        WM_TCARD = 0x0052,
        /// <summary>
        /// Indicates that the user pressed the F1 key. If a menu is active when F1 is pressed, WM_HELP is sent to the window associated with the menu; otherwise, WM_HELP is sent to the window that has the keyboard focus. If no window has the keyboard focus, WM_HELP is sent to the currently active window. 
        /// </summary>
        WM_HELP = 0x0053,
        /// <summary>
        /// The WM_USERCHANGED message is sent to all windows after the user has logged on or off. When the user logs on or off, the system updates the user-specific settings. The system sends this message immediately after updating the settings.
        /// </summary>
        WM_USERCHANGED = 0x0054,
        /// <summary>
        /// Determines if a window accepts ANSI or Unicode structures in the WM_NOTIFY notification message. WM_NOTIFYFORMAT messages are sent from a common control to its parent window and from the parent window to the common control.
        /// </summary>
        WM_NOTIFYFORMAT = 0x0055,
        /// <summary>
        /// The WM_CONTEXTMENU message notifies a window that the user clicked the right mouse button (right-clicked) in the window.
        /// </summary>
        WM_CONTEXTMENU = 0x007B,
        /// <summary>
        /// The WM_STYLECHANGING message is sent to a window when the SetWindowLong function is about to change one or more of the window's styles.
        /// </summary>
        WM_STYLECHANGING = 0x007C,
        /// <summary>
        /// The WM_STYLECHANGED message is sent to a window after the SetWindowLong function has changed one or more of the window's styles
        /// </summary>
        WM_STYLECHANGED = 0x007D,
        /// <summary>
        /// The WM_DISPLAYCHANGE message is sent to all windows when the display resolution has changed.
        /// </summary>
        WM_DISPLAYCHANGE = 0x007E,
        /// <summary>
        /// The WM_GETICON message is sent to a window to retrieve a handle to the large or small icon associated with a window. The system displays the large icon in the ALT+TAB dialog, and the small icon in the window caption. 
        /// </summary>
        WM_GETICON = 0x007F,
        /// <summary>
        /// An application sends the WM_SETICON message to associate a new large or small icon with a window. The system displays the large icon in the ALT+TAB dialog box, and the small icon in the window caption. 
        /// </summary>
        WM_SETICON = 0x0080,
        /// <summary>
        /// The WM_NCCREATE message is sent prior to the WM_CREATE message when a window is first created.
        /// </summary>
        WM_NCCREATE = 0x0081,
        /// <summary>
        /// The WM_NCDESTROY message informs a window that its nonclient area is being destroyed. The DestroyWindow function sends the WM_NCDESTROY message to the window following the WM_DESTROY message. WM_DESTROY is used to free the allocated memory object associated with the window. 
        /// The WM_NCDESTROY message is sent after the child windows have been destroyed. In contrast, WM_DESTROY is sent before the child windows are destroyed.
        /// </summary>
        WM_NCDESTROY = 0x0082,
        /// <summary>
        /// The WM_NCCALCSIZE message is sent when the size and position of a window's client area must be calculated. By processing this message, an application can control the content of the window's client area when the size or position of the window changes.
        /// </summary>
        WM_NCCALCSIZE = 0x0083,
        /// <summary>
        /// The WM_NCHITTEST message is sent to a window when the cursor moves, or when a mouse button is pressed or released. If the mouse is not captured, the message is sent to the window beneath the cursor. Otherwise, the message is sent to the window that has captured the mouse.
        /// </summary>
        WM_NCHITTEST = 0x0084,
        /// <summary>
        /// The WM_NCPAINT message is sent to a window when its frame must be painted. 
        /// </summary>
        WM_NCPAINT = 0x0085,
        /// <summary>
        /// The WM_NCACTIVATE message is sent to a window when its nonclient area needs to be changed to indicate an active or inactive state.
        /// </summary>
        WM_NCACTIVATE = 0x0086,
        /// <summary>
        /// The WM_GETDLGCODE message is sent to the window procedure associated with a control. By default, the system handles all keyboard input to the control; the system interprets certain types of keyboard input as dialog box navigation keys. To override this default behavior, the control can respond to the WM_GETDLGCODE message to indicate the types of input it wants to process itself.
        /// </summary>
        WM_GETDLGCODE = 0x0087,
        /// <summary>
        /// The WM_SYNCPAINT message is used to synchronize painting while avoiding linking independent GUI threads.
        /// </summary>
        WM_SYNCPAINT = 0x0088,
        /// <summary>
        /// The WM_NCMOUSEMOVE message is posted to a window when the cursor is moved within the nonclient area of the window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
        /// </summary>
        WM_NCMOUSEMOVE = 0x00A0,
        /// <summary>
        /// The WM_NCLBUTTONDOWN message is posted when the user presses the left mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
        /// </summary>
        WM_NCLBUTTONDOWN = 0x00A1,
        /// <summary>
        /// The WM_NCLBUTTONUP message is posted when the user releases the left mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
        /// </summary>
        WM_NCLBUTTONUP = 0x00A2,
        /// <summary>
        /// The WM_NCLBUTTONDBLCLK message is posted when the user double-clicks the left mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
        /// </summary>
        WM_NCLBUTTONDBLCLK = 0x00A3,
        /// <summary>
        /// The WM_NCRBUTTONDOWN message is posted when the user presses the right mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
        /// </summary>
        WM_NCRBUTTONDOWN = 0x00A4,
        /// <summary>
        /// The WM_NCRBUTTONUP message is posted when the user releases the right mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
        /// </summary>
        WM_NCRBUTTONUP = 0x00A5,
        /// <summary>
        /// The WM_NCRBUTTONDBLCLK message is posted when the user double-clicks the right mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
        /// </summary>
        WM_NCRBUTTONDBLCLK = 0x00A6,
        /// <summary>
        /// The WM_NCMBUTTONDOWN message is posted when the user presses the middle mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
        /// </summary>
        WM_NCMBUTTONDOWN = 0x00A7,
        /// <summary>
        /// The WM_NCMBUTTONUP message is posted when the user releases the middle mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
        /// </summary>
        WM_NCMBUTTONUP = 0x00A8,
        /// <summary>
        /// The WM_NCMBUTTONDBLCLK message is posted when the user double-clicks the middle mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
        /// </summary>
        WM_NCMBUTTONDBLCLK = 0x00A9,
        /// <summary>
        /// The WM_NCXBUTTONDOWN message is posted when the user presses the first or second X button while the cursor is in the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
        /// </summary>
        WM_NCXBUTTONDOWN = 0x00AB,
        /// <summary>
        /// The WM_NCXBUTTONUP message is posted when the user releases the first or second X button while the cursor is in the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
        /// </summary>
        WM_NCXBUTTONUP = 0x00AC,
        /// <summary>
        /// The WM_NCXBUTTONDBLCLK message is posted when the user double-clicks the first or second X button while the cursor is in the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
        /// </summary>
        WM_NCXBUTTONDBLCLK = 0x00AD,
        /// <summary>
        /// The WM_INPUT_DEVICE_CHANGE message is sent to the window that registered to receive raw input. A window receives this message through its WindowProc function.
        /// </summary>
        WM_INPUT_DEVICE_CHANGE = 0x00FE,
        /// <summary>
        /// The WM_INPUT message is sent to the window that is getting raw input. 
        /// </summary>
        WM_INPUT = 0x00FF,
        /// <summary>
        /// This message filters for keyboard messages.
        /// </summary>
        WM_KEYFIRST = 0x0100,
        /// <summary>
        /// The WM_KEYDOWN message is posted to the window with the keyboard focus when a nonsystem key is pressed. A nonsystem key is a key that is pressed when the ALT key is not pressed. 
        /// </summary>
        WM_KEYDOWN = 0x0100,
        /// <summary>
        /// The WM_KEYUP message is posted to the window with the keyboard focus when a nonsystem key is released. A nonsystem key is a key that is pressed when the ALT key is not pressed, or a keyboard key that is pressed when a window has the keyboard focus. 
        /// </summary>
        WM_KEYUP = 0x0101,
        /// <summary>
        /// The WM_CHAR message is posted to the window with the keyboard focus when a WM_KEYDOWN message is translated by the TranslateMessage function. The WM_CHAR message contains the character code of the key that was pressed. 
        /// </summary>
        WM_CHAR = 0x0102,
        /// <summary>
        /// The WM_DEADCHAR message is posted to the window with the keyboard focus when a WM_KEYUP message is translated by the TranslateMessage function. WM_DEADCHAR specifies a character code generated by a dead key. A dead key is a key that generates a character, such as the umlaut (double-dot), that is combined with another character to form a composite character. For example, the umlaut-O character (Ö) is generated by typing the dead key for the umlaut character, and then typing the O key. 
        /// </summary>
        WM_DEADCHAR = 0x0103,
        /// <summary>
        /// The WM_SYSKEYDOWN message is posted to the window with the keyboard focus when the user presses the F10 key (which activates the menu bar) or holds down the ALT key and then presses another key. It also occurs when no window currently has the keyboard focus; in this case, the WM_SYSKEYDOWN message is sent to the active window. The window that receives the message can distinguish between these two contexts by checking the context code in the lParam parameter. 
        /// </summary>
        WM_SYSKEYDOWN = 0x0104,
        /// <summary>
        /// The WM_SYSKEYUP message is posted to the window with the keyboard focus when the user releases a key that was pressed while the ALT key was held down. It also occurs when no window currently has the keyboard focus; in this case, the WM_SYSKEYUP message is sent to the active window. The window that receives the message can distinguish between these two contexts by checking the context code in the lParam parameter. 
        /// </summary>
        WM_SYSKEYUP = 0x0105,
        /// <summary>
        /// The WM_SYSCHAR message is posted to the window with the keyboard focus when a WM_SYSKEYDOWN message is translated by the TranslateMessage function. It specifies the character code of a system character key — that is, a character key that is pressed while the ALT key is down. 
        /// </summary>
        WM_SYSCHAR = 0x0106,
        /// <summary>
        /// The WM_SYSDEADCHAR message is sent to the window with the keyboard focus when a WM_SYSKEYDOWN message is translated by the TranslateMessage function. WM_SYSDEADCHAR specifies the character code of a system dead key — that is, a dead key that is pressed while holding down the ALT key. 
        /// </summary>
        WM_SYSDEADCHAR = 0x0107,
        /// <summary>
        /// The WM_UNICHAR message is posted to the window with the keyboard focus when a WM_KEYDOWN message is translated by the TranslateMessage function. The WM_UNICHAR message contains the character code of the key that was pressed. 
        /// The WM_UNICHAR message is equivalent to WM_CHAR, but it uses Unicode Transformation Format (UTF)-32, whereas WM_CHAR uses UTF-16. It is designed to send or post Unicode characters to ANSI windows and it can can handle Unicode Supplementary Plane characters.
        /// </summary>
        WM_UNICHAR = 0x0109,
        /// <summary>
        /// This message filters for keyboard messages.
        /// </summary>
        WM_KEYLAST = 0x0109,
        /// <summary>
        /// Sent immediately before the IME generates the composition string as a result of a keystroke. A window receives this message through its WindowProc function. 
        /// </summary>
        WM_IME_STARTCOMPOSITION = 0x010D,
        /// <summary>
        /// Sent to an application when the IME ends composition. A window receives this message through its WindowProc function. 
        /// </summary>
        WM_IME_ENDCOMPOSITION = 0x010E,
        /// <summary>
        /// Sent to an application when the IME changes composition status as a result of a keystroke. A window receives this message through its WindowProc function. 
        /// </summary>
        WM_IME_COMPOSITION = 0x010F,
        WM_IME_KEYLAST = 0x010F,
        /// <summary>
        /// The WM_INITDIALOG message is sent to the dialog box procedure immediately before a dialog box is displayed. Dialog box procedures typically use this message to initialize controls and carry out any other initialization tasks that affect the appearance of the dialog box. 
        /// </summary>
        WM_INITDIALOG = 0x0110,
        /// <summary>
        /// The WM_COMMAND message is sent when the user selects a command item from a menu, when a control sends a notification message to its parent window, or when an accelerator keystroke is translated. 
        /// </summary>
        WM_COMMAND = 0x0111,
        /// <summary>
        /// A window receives this message when the user chooses a command from the Window menu (formerly known as the system or control menu) or when the user chooses the maximize button, minimize button, restore button, or close button.
        /// </summary>
        WM_SYSCOMMAND = 0x0112,
        /// <summary>
        /// The WM_TIMER message is posted to the installing thread's message queue when a timer expires. The message is posted by the GetMessage or PeekMessage function. 
        /// </summary>
        WM_TIMER = 0x0113,
        /// <summary>
        /// The WM_HSCROLL message is sent to a window when a scroll event occurs in the window's standard horizontal scroll bar. This message is also sent to the owner of a horizontal scroll bar control when a scroll event occurs in the control. 
        /// </summary>
        WM_HSCROLL = 0x0114,
        /// <summary>
        /// The WM_VSCROLL message is sent to a window when a scroll event occurs in the window's standard vertical scroll bar. This message is also sent to the owner of a vertical scroll bar control when a scroll event occurs in the control. 
        /// </summary>
        WM_VSCROLL = 0x0115,
        /// <summary>
        /// The WM_INITMENU message is sent when a menu is about to become active. It occurs when the user clicks an item on the menu bar or presses a menu key. This allows the application to modify the menu before it is displayed. 
        /// </summary>
        WM_INITMENU = 0x0116,
        /// <summary>
        /// The WM_INITMENUPOPUP message is sent when a drop-down menu or submenu is about to become active. This allows an application to modify the menu before it is displayed, without changing the entire menu. 
        /// </summary>
        WM_INITMENUPOPUP = 0x0117,
        /// <summary>
        /// The WM_MENUSELECT message is sent to a menu's owner window when the user selects a menu item. 
        /// </summary>
        WM_MENUSELECT = 0x011F,
        /// <summary>
        /// The WM_MENUCHAR message is sent when a menu is active and the user presses a key that does not correspond to any mnemonic or accelerator key. This message is sent to the window that owns the menu. 
        /// </summary>
        WM_MENUCHAR = 0x0120,
        /// <summary>
        /// The WM_ENTERIDLE message is sent to the owner window of a modal dialog box or menu that is entering an idle state. A modal dialog box or menu enters an idle state when no messages are waiting in its queue after it has processed one or more previous messages. 
        /// </summary>
        WM_ENTERIDLE = 0x0121,
        /// <summary>
        /// The WM_MENURBUTTONUP message is sent when the user releases the right mouse button while the cursor is on a menu item. 
        /// </summary>
        WM_MENURBUTTONUP = 0x0122,
        /// <summary>
        /// The WM_MENUDRAG message is sent to the owner of a drag-and-drop menu when the user drags a menu item. 
        /// </summary>
        WM_MENUDRAG = 0x0123,
        /// <summary>
        /// The WM_MENUGETOBJECT message is sent to the owner of a drag-and-drop menu when the mouse cursor enters a menu item or moves from the center of the item to the top or bottom of the item. 
        /// </summary>
        WM_MENUGETOBJECT = 0x0124,
        /// <summary>
        /// The WM_UNINITMENUPOPUP message is sent when a drop-down menu or submenu has been destroyed. 
        /// </summary>
        WM_UNINITMENUPOPUP = 0x0125,
        /// <summary>
        /// The WM_MENUCOMMAND message is sent when the user makes a selection from a menu. 
        /// </summary>
        WM_MENUCOMMAND = 0x0126,
        /// <summary>
        /// An application sends the WM_CHANGEUISTATE message to indicate that the user interface (UI) state should be changed.
        /// </summary>
        WM_CHANGEUISTATE = 0x0127,
        /// <summary>
        /// An application sends the WM_UPDATEUISTATE message to change the user interface (UI) state for the specified window and all its child windows.
        /// </summary>
        WM_UPDATEUISTATE = 0x0128,
        /// <summary>
        /// An application sends the WM_QUERYUISTATE message to retrieve the user interface (UI) state for a window.
        /// </summary>
        WM_QUERYUISTATE = 0x0129,
        /// <summary>
        /// The WM_CTLCOLORMSGBOX message is sent to the owner window of a message box before Windows draws the message box. By responding to this message, the owner window can set the text and background colors of the message box by using the given display device context handle. 
        /// </summary>
        WM_CTLCOLORMSGBOX = 0x0132,
        /// <summary>
        /// An edit control that is not read-only or disabled sends the WM_CTLCOLOREDIT message to its parent window when the control is about to be drawn. By responding to this message, the parent window can use the specified device context handle to set the text and background colors of the edit control. 
        /// </summary>
        WM_CTLCOLOREDIT = 0x0133,
        /// <summary>
        /// Sent to the parent window of a list box before the system draws the list box. By responding to this message, the parent window can set the text and background colors of the list box by using the specified display device context handle. 
        /// </summary>
        WM_CTLCOLORLISTBOX = 0x0134,
        /// <summary>
        /// The WM_CTLCOLORBTN message is sent to the parent window of a button before drawing the button. The parent window can change the button's text and background colors. However, only owner-drawn buttons respond to the parent window processing this message. 
        /// </summary>
        WM_CTLCOLORBTN = 0x0135,
        /// <summary>
        /// The WM_CTLCOLORDLG message is sent to a dialog box before the system draws the dialog box. By responding to this message, the dialog box can set its text and background colors using the specified display device context handle. 
        /// </summary>
        WM_CTLCOLORDLG = 0x0136,
        /// <summary>
        /// The WM_CTLCOLORSCROLLBAR message is sent to the parent window of a scroll bar control when the control is about to be drawn. By responding to this message, the parent window can use the display context handle to set the background color of the scroll bar control. 
        /// </summary>
        WM_CTLCOLORSCROLLBAR = 0x0137,
        /// <summary>
        /// A static control, or an edit control that is read-only or disabled, sends the WM_CTLCOLORSTATIC message to its parent window when the control is about to be drawn. By responding to this message, the parent window can use the specified device context handle to set the text and background colors of the static control. 
        /// </summary>
        WM_CTLCOLORSTATIC = 0x0138,
        /// <summary>
        /// Use WM_MOUSEFIRST to specify the first mouse message. Use the PeekMessage() Function.
        /// </summary>
        WM_MOUSEFIRST = 0x0200,
        /// <summary>
        /// The WM_MOUSEMOVE message is posted to a window when the cursor moves. If the mouse is not captured, the message is posted to the window that contains the cursor. Otherwise, the message is posted to the window that has captured the mouse.
        /// </summary>
        WM_MOUSEMOVE = 0x0200,
        /// <summary>
        /// The WM_LBUTTONDOWN message is posted when the user presses the left mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
        /// </summary>
        WM_LBUTTONDOWN = 0x0201,
        /// <summary>
        /// The WM_LBUTTONUP message is posted when the user releases the left mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
        /// </summary>
        WM_LBUTTONUP = 0x0202,
        /// <summary>
        /// The WM_LBUTTONDBLCLK message is posted when the user double-clicks the left mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
        /// </summary>
        WM_LBUTTONDBLCLK = 0x0203,
        /// <summary>
        /// The WM_RBUTTONDOWN message is posted when the user presses the right mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
        /// </summary>
        WM_RBUTTONDOWN = 0x0204,
        /// <summary>
        /// The WM_RBUTTONUP message is posted when the user releases the right mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
        /// </summary>
        WM_RBUTTONUP = 0x0205,
        /// <summary>
        /// The WM_RBUTTONDBLCLK message is posted when the user double-clicks the right mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
        /// </summary>
        WM_RBUTTONDBLCLK = 0x0206,
        /// <summary>
        /// The WM_MBUTTONDOWN message is posted when the user presses the middle mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
        /// </summary>
        WM_MBUTTONDOWN = 0x0207,
        /// <summary>
        /// The WM_MBUTTONUP message is posted when the user releases the middle mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
        /// </summary>
        WM_MBUTTONUP = 0x0208,
        /// <summary>
        /// The WM_MBUTTONDBLCLK message is posted when the user double-clicks the middle mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
        /// </summary>
        WM_MBUTTONDBLCLK = 0x0209,
        /// <summary>
        /// The WM_MOUSEWHEEL message is sent to the focus window when the mouse wheel is rotated. The DefWindowProc function propagates the message to the window's parent. There should be no internal forwarding of the message, since DefWindowProc propagates it up the parent chain until it finds a window that processes it.
        /// </summary>
        WM_MOUSEWHEEL = 0x020A,
        /// <summary>
        /// The WM_XBUTTONDOWN message is posted when the user presses the first or second X button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse. 
        /// </summary>
        WM_XBUTTONDOWN = 0x020B,
        /// <summary>
        /// The WM_XBUTTONUP message is posted when the user releases the first or second X button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
        /// </summary>
        WM_XBUTTONUP = 0x020C,
        /// <summary>
        /// The WM_XBUTTONDBLCLK message is posted when the user double-clicks the first or second X button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
        /// </summary>
        WM_XBUTTONDBLCLK = 0x020D,
        /// <summary>
        /// The WM_MOUSEHWHEEL message is sent to the focus window when the mouse's horizontal scroll wheel is tilted or rotated. The DefWindowProc function propagates the message to the window's parent. There should be no internal forwarding of the message, since DefWindowProc propagates it up the parent chain until it finds a window that processes it.
        /// </summary>
        WM_MOUSEHWHEEL = 0x020E,
        /// <summary>
        /// Use WM_MOUSELAST to specify the last mouse message. Used with PeekMessage() Function.
        /// </summary>
        WM_MOUSELAST = 0x020E,
        /// <summary>
        /// The WM_PARENTNOTIFY message is sent to the parent of a child window when the child window is created or destroyed, or when the user clicks a mouse button while the cursor is over the child window. When the child window is being created, the system sends WM_PARENTNOTIFY just before the CreateWindow or CreateWindowEx function that creates the window returns. When the child window is being destroyed, the system sends the message before any processing to destroy the window takes place.
        /// </summary>
        WM_PARENTNOTIFY = 0x0210,
        /// <summary>
        /// The WM_ENTERMENULOOP message informs an application's main window procedure that a menu modal loop has been entered. 
        /// </summary>
        WM_ENTERMENULOOP = 0x0211,
        /// <summary>
        /// The WM_EXITMENULOOP message informs an application's main window procedure that a menu modal loop has been exited. 
        /// </summary>
        WM_EXITMENULOOP = 0x0212,
        /// <summary>
        /// The WM_NEXTMENU message is sent to an application when the right or left arrow key is used to switch between the menu bar and the system menu. 
        /// </summary>
        WM_NEXTMENU = 0x0213,
        /// <summary>
        /// The WM_SIZING message is sent to a window that the user is resizing. By processing this message, an application can monitor the size and position of the drag rectangle and, if needed, change its size or position. 
        /// </summary>
        WM_SIZING = 0x0214,
        /// <summary>
        /// The WM_CAPTURECHANGED message is sent to the window that is losing the mouse capture.
        /// </summary>
        WM_CAPTURECHANGED = 0x0215,
        /// <summary>
        /// The WM_MOVING message is sent to a window that the user is moving. By processing this message, an application can monitor the position of the drag rectangle and, if needed, change its position.
        /// </summary>
        WM_MOVING = 0x0216,
        /// <summary>
        /// Notifies applications that a power-management event has occurred.
        /// </summary>
        WM_POWERBROADCAST = 0x0218,
        /// <summary>
        /// Notifies an application of a change to the hardware configuration of a device or the computer.
        /// </summary>
        WM_DEVICECHANGE = 0x0219,
        /// <summary>
        /// An application sends the WM_MDICREATE message to a multiple-document interface (MDI) client window to create an MDI child window. 
        /// </summary>
        WM_MDICREATE = 0x0220,
        /// <summary>
        /// An application sends the WM_MDIDESTROY message to a multiple-document interface (MDI) client window to close an MDI child window. 
        /// </summary>
        WM_MDIDESTROY = 0x0221,
        /// <summary>
        /// An application sends the WM_MDIACTIVATE message to a multiple-document interface (MDI) client window to instruct the client window to activate a different MDI child window. 
        /// </summary>
        WM_MDIACTIVATE = 0x0222,
        /// <summary>
        /// An application sends the WM_MDIRESTORE message to a multiple-document interface (MDI) client window to restore an MDI child window from maximized or minimized size. 
        /// </summary>
        WM_MDIRESTORE = 0x0223,
        /// <summary>
        /// An application sends the WM_MDINEXT message to a multiple-document interface (MDI) client window to activate the next or previous child window. 
        /// </summary>
        WM_MDINEXT = 0x0224,
        /// <summary>
        /// An application sends the WM_MDIMAXIMIZE message to a multiple-document interface (MDI) client window to maximize an MDI child window. The system resizes the child window to make its client area fill the client window. The system places the child window's window menu icon in the rightmost position of the frame window's menu bar, and places the child window's restore icon in the leftmost position. The system also appends the title bar text of the child window to that of the frame window. 
        /// </summary>
        WM_MDIMAXIMIZE = 0x0225,
        /// <summary>
        /// An application sends the WM_MDITILE message to a multiple-document interface (MDI) client window to arrange all of its MDI child windows in a tile format. 
        /// </summary>
        WM_MDITILE = 0x0226,
        /// <summary>
        /// An application sends the WM_MDICASCADE message to a multiple-document interface (MDI) client window to arrange all its child windows in a cascade format. 
        /// </summary>
        WM_MDICASCADE = 0x0227,
        /// <summary>
        /// An application sends the WM_MDIICONARRANGE message to a multiple-document interface (MDI) client window to arrange all minimized MDI child windows. It does not affect child windows that are not minimized. 
        /// </summary>
        WM_MDIICONARRANGE = 0x0228,
        /// <summary>
        /// An application sends the WM_MDIGETACTIVE message to a multiple-document interface (MDI) client window to retrieve the handle to the active MDI child window. 
        /// </summary>
        WM_MDIGETACTIVE = 0x0229,
        /// <summary>
        /// An application sends the WM_MDISETMENU message to a multiple-document interface (MDI) client window to replace the entire menu of an MDI frame window, to replace the window menu of the frame window, or both. 
        /// </summary>
        WM_MDISETMENU = 0x0230,
        /// <summary>
        /// The WM_ENTERSIZEMOVE message is sent one time to a window after it enters the moving or sizing modal loop. The window enters the moving or sizing modal loop when the user clicks the window's title bar or sizing border, or when the window passes the WM_SYSCOMMAND message to the DefWindowProc function and the wParam parameter of the message specifies the SC_MOVE or SC_SIZE value. The operation is complete when DefWindowProc returns. 
        /// The system sends the WM_ENTERSIZEMOVE message regardless of whether the dragging of full windows is enabled.
        /// </summary>
        WM_ENTERSIZEMOVE = 0x0231,
        /// <summary>
        /// The WM_EXITSIZEMOVE message is sent one time to a window, after it has exited the moving or sizing modal loop. The window enters the moving or sizing modal loop when the user clicks the window's title bar or sizing border, or when the window passes the WM_SYSCOMMAND message to the DefWindowProc function and the wParam parameter of the message specifies the SC_MOVE or SC_SIZE value. The operation is complete when DefWindowProc returns. 
        /// </summary>
        WM_EXITSIZEMOVE = 0x0232,
        /// <summary>
        /// Sent when the user drops a file on the window of an application that has registered itself as a recipient of dropped files.
        /// </summary>
        WM_DROPFILES = 0x0233,
        /// <summary>
        /// An application sends the WM_MDIREFRESHMENU message to a multiple-document interface (MDI) client window to refresh the window menu of the MDI frame window. 
        /// </summary>
        WM_MDIREFRESHMENU = 0x0234,
        /// <summary>
        /// Sent to an application when a window is activated. A window receives this message through its WindowProc function. 
        /// </summary>
        WM_IME_SETCONTEXT = 0x0281,
        /// <summary>
        /// Sent to an application to notify it of changes to the IME window. A window receives this message through its WindowProc function. 
        /// </summary>
        WM_IME_NOTIFY = 0x0282,
        /// <summary>
        /// Sent by an application to direct the IME window to carry out the requested command. The application uses this message to control the IME window that it has created. To send this message, the application calls the SendMessage function with the following parameters.
        /// </summary>
        WM_IME_CONTROL = 0x0283,
        /// <summary>
        /// Sent to an application when the IME window finds no space to extend the area for the composition window. A window receives this message through its WindowProc function. 
        /// </summary>
        WM_IME_COMPOSITIONFULL = 0x0284,
        /// <summary>
        /// Sent to an application when the operating system is about to change the current IME. A window receives this message through its WindowProc function. 
        /// </summary>
        WM_IME_SELECT = 0x0285,
        /// <summary>
        /// Sent to an application when the IME gets a character of the conversion result. A window receives this message through its WindowProc function. 
        /// </summary>
        WM_IME_CHAR = 0x0286,
        /// <summary>
        /// Sent to an application to provide commands and request information. A window receives this message through its WindowProc function. 
        /// </summary>
        WM_IME_REQUEST = 0x0288,
        /// <summary>
        /// Sent to an application by the IME to notify the application of a key press and to keep message order. A window receives this message through its WindowProc function. 
        /// </summary>
        WM_IME_KEYDOWN = 0x0290,
        /// <summary>
        /// Sent to an application by the IME to notify the application of a key release and to keep message order. A window receives this message through its WindowProc function. 
        /// </summary>
        WM_IME_KEYUP = 0x0291,
        /// <summary>
        /// The WM_MOUSEHOVER message is posted to a window when the cursor hovers over the client area of the window for the period of time specified in a prior call to TrackMouseEvent.
        /// </summary>
        WM_MOUSEHOVER = 0x02A1,
        /// <summary>
        /// The WM_MOUSELEAVE message is posted to a window when the cursor leaves the client area of the window specified in a prior call to TrackMouseEvent.
        /// </summary>
        WM_MOUSELEAVE = 0x02A3,
        /// <summary>
        /// The WM_NCMOUSEHOVER message is posted to a window when the cursor hovers over the nonclient area of the window for the period of time specified in a prior call to TrackMouseEvent.
        /// </summary>
        WM_NCMOUSEHOVER = 0x02A0,
        /// <summary>
        /// The WM_NCMOUSELEAVE message is posted to a window when the cursor leaves the nonclient area of the window specified in a prior call to TrackMouseEvent.
        /// </summary>
        WM_NCMOUSELEAVE = 0x02A2,
        /// <summary>
        /// The WM_WTSSESSION_CHANGE message notifies applications of changes in session state.
        /// </summary>
        WM_WTSSESSION_CHANGE = 0x02B1,
        WM_TABLET_FIRST = 0x02c0,
        WM_TABLET_LAST = 0x02df,
        /// <summary>
        /// An application sends a WM_CUT message to an edit control or combo box to delete (cut) the current selection, if any, in the edit control and copy the deleted text to the clipboard in CF_TEXT format. 
        /// </summary>
        WM_CUT = 0x0300,
        /// <summary>
        /// An application sends the WM_COPY message to an edit control or combo box to copy the current selection to the clipboard in CF_TEXT format. 
        /// </summary>
        WM_COPY = 0x0301,
        /// <summary>
        /// An application sends a WM_PASTE message to an edit control or combo box to copy the current content of the clipboard to the edit control at the current caret position. Data is inserted only if the clipboard contains data in CF_TEXT format. 
        /// </summary>
        WM_PASTE = 0x0302,
        /// <summary>
        /// An application sends a WM_CLEAR message to an edit control or combo box to delete (clear) the current selection, if any, from the edit control. 
        /// </summary>
        WM_CLEAR = 0x0303,
        /// <summary>
        /// An application sends a WM_UNDO message to an edit control to undo the last operation. When this message is sent to an edit control, the previously deleted text is restored or the previously added text is deleted.
        /// </summary>
        WM_UNDO = 0x0304,
        /// <summary>
        /// The WM_RENDERFORMAT message is sent to the clipboard owner if it has delayed rendering a specific clipboard format and if an application has requested data in that format. The clipboard owner must render data in the specified format and place it on the clipboard by calling the SetClipboardData function. 
        /// </summary>
        WM_RENDERFORMAT = 0x0305,
        /// <summary>
        /// The WM_RENDERALLFORMATS message is sent to the clipboard owner before it is destroyed, if the clipboard owner has delayed rendering one or more clipboard formats. For the content of the clipboard to remain available to other applications, the clipboard owner must render data in all the formats it is capable of generating, and place the data on the clipboard by calling the SetClipboardData function. 
        /// </summary>
        WM_RENDERALLFORMATS = 0x0306,
        /// <summary>
        /// The WM_DESTROYCLIPBOARD message is sent to the clipboard owner when a call to the EmptyClipboard function empties the clipboard. 
        /// </summary>
        WM_DESTROYCLIPBOARD = 0x0307,
        /// <summary>
        /// The WM_DRAWCLIPBOARD message is sent to the first window in the clipboard viewer chain when the content of the clipboard changes. This enables a clipboard viewer window to display the new content of the clipboard. 
        /// </summary>
        WM_DRAWCLIPBOARD = 0x0308,
        /// <summary>
        /// The WM_PAINTCLIPBOARD message is sent to the clipboard owner by a clipboard viewer window when the clipboard contains data in the CF_OWNERDISPLAY format and the clipboard viewer's client area needs repainting. 
        /// </summary>
        WM_PAINTCLIPBOARD = 0x0309,
        /// <summary>
        /// The WM_VSCROLLCLIPBOARD message is sent to the clipboard owner by a clipboard viewer window when the clipboard contains data in the CF_OWNERDISPLAY format and an event occurs in the clipboard viewer's vertical scroll bar. The owner should scroll the clipboard image and update the scroll bar values. 
        /// </summary>
        WM_VSCROLLCLIPBOARD = 0x030A,
        /// <summary>
        /// The WM_SIZECLIPBOARD message is sent to the clipboard owner by a clipboard viewer window when the clipboard contains data in the CF_OWNERDISPLAY format and the clipboard viewer's client area has changed size. 
        /// </summary>
        WM_SIZECLIPBOARD = 0x030B,
        /// <summary>
        /// The WM_ASKCBFORMATNAME message is sent to the clipboard owner by a clipboard viewer window to request the name of a CF_OWNERDISPLAY clipboard format.
        /// </summary>
        WM_ASKCBFORMATNAME = 0x030C,
        /// <summary>
        /// The WM_CHANGECBCHAIN message is sent to the first window in the clipboard viewer chain when a window is being removed from the chain. 
        /// </summary>
        WM_CHANGECBCHAIN = 0x030D,
        /// <summary>
        /// The WM_HSCROLLCLIPBOARD message is sent to the clipboard owner by a clipboard viewer window. This occurs when the clipboard contains data in the CF_OWNERDISPLAY format and an event occurs in the clipboard viewer's horizontal scroll bar. The owner should scroll the clipboard image and update the scroll bar values. 
        /// </summary>
        WM_HSCROLLCLIPBOARD = 0x030E,
        /// <summary>
        /// This message informs a window that it is about to receive the keyboard focus, giving the window the opportunity to realize its logical palette when it receives the focus. 
        /// </summary>
        WM_QUERYNEWPALETTE = 0x030F,
        /// <summary>
        /// The WM_PALETTEISCHANGING message informs applications that an application is going to realize its logical palette. 
        /// </summary>
        WM_PALETTEISCHANGING = 0x0310,
        /// <summary>
        /// This message is sent by the OS to all top-level and overlapped windows after the window with the keyboard focus realizes its logical palette. 
        /// This message enables windows that do not have the keyboard focus to realize their logical palettes and update their client areas.
        /// </summary>
        WM_PALETTECHANGED = 0x0311,
        /// <summary>
        /// The WM_HOTKEY message is posted when the user presses a hot key registered by the RegisterHotKey function. The message is placed at the top of the message queue associated with the thread that registered the hot key. 
        /// </summary>
        WM_HOTKEY = 0x0312,
        /// <summary>
        /// The WM_PRINT message is sent to a window to request that it draw itself in the specified device context, most commonly in a printer device context.
        /// </summary>
        WM_PRINT = 0x0317,
        /// <summary>
        /// The WM_PRINTCLIENT message is sent to a window to request that it draw its client area in the specified device context, most commonly in a printer device context.
        /// </summary>
        WM_PRINTCLIENT = 0x0318,
        /// <summary>
        /// The WM_APPCOMMAND message notifies a window that the user generated an application command event, for example, by clicking an application command button using the mouse or typing an application command key on the keyboard.
        /// </summary>
        WM_APPCOMMAND = 0x0319,
        /// <summary>
        /// The WM_THEMECHANGED message is broadcast to every window following a theme change event. Examples of theme change events are the activation of a theme, the deactivation of a theme, or a transition from one theme to another.
        /// </summary>
        WM_THEMECHANGED = 0x031A,
        /// <summary>
        /// Sent when the contents of the clipboard have changed.
        /// </summary>
        WM_CLIPBOARDUPDATE = 0x031D,
        /// <summary>
        /// The system will send a window the WM_DWMCOMPOSITIONCHANGED message to indicate that the availability of desktop composition has changed.
        /// </summary>
        WM_DWMCOMPOSITIONCHANGED = 0x031E,
        /// <summary>
        /// WM_DWMNCRENDERINGCHANGED is called when the non-client area rendering status of a window has changed. Only windows that have set the flag DWM_BLURBEHIND.fTransitionOnMaximized to true will get this message. 
        /// </summary>
        WM_DWMNCRENDERINGCHANGED = 0x031F,
        /// <summary>
        /// Sent to all top-level windows when the colorization color has changed. 
        /// </summary>
        WM_DWMCOLORIZATIONCOLORCHANGED = 0x0320,
        /// <summary>
        /// WM_DWMWINDOWMAXIMIZEDCHANGE will let you know when a DWM composed window is maximized. You also have to register for this message as well. You'd have other windowd go opaque when this message is sent.
        /// </summary>
        WM_DWMWINDOWMAXIMIZEDCHANGE = 0x0321,
        /// <summary>
        /// Sent to request extended title bar information. A window receives this message through its WindowProc function.
        /// </summary>
        WM_GETTITLEBARINFOEX = 0x033F,
        WM_HANDHELDFIRST = 0x0358,
        WM_HANDHELDLAST = 0x035F,
        WM_AFXFIRST = 0x0360,
        WM_AFXLAST = 0x037F,
        WM_PENWINFIRST = 0x0380,
        WM_PENWINLAST = 0x038F,
        /// <summary>
        /// The WM_APP constant is used by applications to help define private messages, usually of the form WM_APP+X, where X is an integer value. 
        /// </summary>
        WM_APP = 0x8000,
        /// <summary>
        /// The WM_USER constant is used by applications to help define private messages for use by private window classes, usually of the form WM_USER+X, where X is an integer value. 
        /// </summary>
        WM_USER = 0x0400,

        /// <summary>
        /// An application sends the WM_CPL_LAUNCH message to Windows Control Panel to request that a Control Panel application be started. 
        /// </summary>
        WM_CPL_LAUNCH = WM_USER + 0x1000,
        /// <summary>
        /// The WM_CPL_LAUNCHED message is sent when a Control Panel application, started by the WM_CPL_LAUNCH message, has closed. The WM_CPL_LAUNCHED message is sent to the window identified by the wParam parameter of the WM_CPL_LAUNCH message that started the application. 
        /// </summary>
        WM_CPL_LAUNCHED = WM_USER + 0x1001,
        /// <summary>
        /// WM_SYSTIMER is a well-known yet still undocumented message. Windows uses WM_SYSTIMER for internal actions like scrolling.
        /// </summary>
        WM_SYSTIMER = 0x118
    }

    /// <summary>
    /// SPI_ System-wide parameter - Used in SystemParametersInfo function 
    /// </summary>
    public enum SPI : uint
    {
        /// <summary>
        /// Determines whether the warning beeper is on. 
        /// The pvParam parameter must point to a BOOL variable that receives TRUE if the beeper is on, or FALSE if it is off.
        /// </summary>
        SPI_GETBEEP = 0x0001,

        /// <summary>
        /// Turns the warning beeper on or off. The uiParam parameter specifies TRUE for on, or FALSE for off.
        /// </summary>
        SPI_SETBEEP = 0x0002,

        /// <summary>
        /// Retrieves the two mouse threshold values and the mouse speed.
        /// </summary>
        SPI_GETMOUSE = 0x0003,

        /// <summary>
        /// Sets the two mouse threshold values and the mouse speed.
        /// </summary>
        SPI_SETMOUSE = 0x0004,

        /// <summary>
        /// Retrieves the border multiplier factor that determines the width of a window's sizing border. 
        /// The pvParam parameter must point to an integer variable that receives this value.
        /// </summary>
        SPI_GETBORDER = 0x0005,

        /// <summary>
        /// Sets the border multiplier factor that determines the width of a window's sizing border. 
        /// The uiParam parameter specifies the new value.
        /// </summary>
        SPI_SETBORDER = 0x0006,

        /// <summary>
        /// Retrieves the keyboard repeat-speed setting, which is a value in the range from 0 (approximately 2.5 repetitions per second) 
        /// through 31 (approximately 30 repetitions per second). The actual repeat rates are hardware-dependent and may vary from 
        /// a linear scale by as much as 20%. The pvParam parameter must point to a DWORD variable that receives the setting
        /// </summary>
        SPI_GETKEYBOARDSPEED = 0x000A,

        /// <summary>
        /// Sets the keyboard repeat-speed setting. The uiParam parameter must specify a value in the range from 0 
        /// (approximately 2.5 repetitions per second) through 31 (approximately 30 repetitions per second). 
        /// The actual repeat rates are hardware-dependent and may vary from a linear scale by as much as 20%. 
        /// If uiParam is greater than 31, the parameter is set to 31.
        /// </summary>
        SPI_SETKEYBOARDSPEED = 0x000B,

        /// <summary>
        /// Not implemented.
        /// </summary>
        SPI_LANGDRIVER = 0x000C,

        /// <summary>
        /// Sets or retrieves the width, in pixels, of an icon cell. The system uses this rectangle to arrange icons in large icon view. 
        /// To set this value, set uiParam to the new value and set pvParam to null. You cannot set this value to less than SM_CXICON.
        /// To retrieve this value, pvParam must point to an integer that receives the current value.
        /// </summary>
        SPI_ICONHORIZONTALSPACING = 0x000D,

        /// <summary>
        /// Retrieves the screen saver time-out value, in seconds. The pvParam parameter must point to an integer variable that receives the value.
        /// </summary>
        SPI_GETSCREENSAVETIMEOUT = 0x000E,

        /// <summary>
        /// Sets the screen saver time-out value to the value of the uiParam parameter. This value is the amount of time, in seconds, 
        /// that the system must be idle before the screen saver activates.
        /// </summary>
        SPI_SETSCREENSAVETIMEOUT = 0x000F,

        /// <summary>
        /// Determines whether screen saving is enabled. The pvParam parameter must point to a bool variable that receives TRUE 
        /// if screen saving is enabled, or FALSE otherwise.
        /// </summary>
        SPI_GETSCREENSAVEACTIVE = 0x0010,

        /// <summary>
        /// Sets the state of the screen saver. The uiParam parameter specifies TRUE to activate screen saving, or FALSE to deactivate it.
        /// </summary>
        SPI_SETSCREENSAVEACTIVE = 0x0011,

        /// <summary>
        /// Retrieves the current granularity value of the desktop sizing grid. The pvParam parameter must point to an integer variable 
        /// that receives the granularity.
        /// </summary>
        SPI_GETGRIDGRANULARITY = 0x0012,

        /// <summary>
        /// Sets the granularity of the desktop sizing grid to the value of the uiParam parameter.
        /// </summary>
        SPI_SETGRIDGRANULARITY = 0x0013,

        /// <summary>
        /// Sets the desktop wallpaper. The value of the pvParam parameter determines the new wallpaper. To specify a wallpaper bitmap, 
        /// set pvParam to point to a null-terminated string containing the name of a bitmap file. Setting pvParam to "" removes the wallpaper. 
        /// Setting pvParam to SETWALLPAPER_DEFAULT or null reverts to the default wallpaper.
        /// </summary>
        SPI_SETDESKWALLPAPER = 0x0014,

        /// <summary>
        /// Sets the current desktop pattern by causing Windows to read the Pattern= setting from the WIN.INI file.
        /// </summary>
        SPI_SETDESKPATTERN = 0x0015,

        /// <summary>
        /// Retrieves the keyboard repeat-delay setting, which is a value in the range from 0 (approximately 250 ms delay) through 3 
        /// (approximately 1 second delay). The actual delay associated with each value may vary depending on the hardware. The pvParam parameter must point to an integer variable that receives the setting.
        /// </summary>
        SPI_GETKEYBOARDDELAY = 0x0016,

        /// <summary>
        /// Sets the keyboard repeat-delay setting. The uiParam parameter must specify 0, 1, 2, or 3, where zero sets the shortest delay 
        /// (approximately 250 ms) and 3 sets the longest delay (approximately 1 second). The actual delay associated with each value may 
        /// vary depending on the hardware.
        /// </summary>
        SPI_SETKEYBOARDDELAY = 0x0017,

        /// <summary>
        /// Sets or retrieves the height, in pixels, of an icon cell. 
        /// To set this value, set uiParam to the new value and set pvParam to null. You cannot set this value to less than SM_CYICON.
        /// To retrieve this value, pvParam must point to an integer that receives the current value.
        /// </summary>
        SPI_ICONVERTICALSPACING = 0x0018,

        /// <summary>
        /// Determines whether icon-title wrapping is enabled. The pvParam parameter must point to a bool variable that receives TRUE 
        /// if enabled, or FALSE otherwise.
        /// </summary>
        SPI_GETICONTITLEWRAP = 0x0019,

        /// <summary>
        /// Turns icon-title wrapping on or off. The uiParam parameter specifies TRUE for on, or FALSE for off.
        /// </summary>
        SPI_SETICONTITLEWRAP = 0x001A,

        /// <summary>
        /// Determines whether pop-up menus are left-aligned or right-aligned, relative to the corresponding menu-bar item. 
        /// The pvParam parameter must point to a bool variable that receives TRUE if left-aligned, or FALSE otherwise.
        /// </summary>
        SPI_GETMENUDROPALIGNMENT = 0x001B,

        /// <summary>
        /// Sets the alignment value of pop-up menus. The uiParam parameter specifies TRUE for right alignment, or FALSE for left alignment.
        /// </summary>
        SPI_SETMENUDROPALIGNMENT = 0x001C,

        /// <summary>
        /// Sets the width of the double-click rectangle to the value of the uiParam parameter. 
        /// The double-click rectangle is the rectangle within which the second click of a double-click must fall for it to be registered 
        /// as a double-click.
        /// To retrieve the width of the double-click rectangle, call GetSystemMetrics with the SM_CXDOUBLECLK flag.
        /// </summary>
        SPI_SETDOUBLECLKWIDTH = 0x001D,

        /// <summary>
        /// Sets the height of the double-click rectangle to the value of the uiParam parameter. 
        /// The double-click rectangle is the rectangle within which the second click of a double-click must fall for it to be registered 
        /// as a double-click.
        /// To retrieve the height of the double-click rectangle, call GetSystemMetrics with the SM_CYDOUBLECLK flag.
        /// </summary>
        SPI_SETDOUBLECLKHEIGHT = 0x001E,

        /// <summary>
        /// Retrieves the logical font information for the current icon-title font. The uiParam parameter specifies the size of a LOGFONT structure, 
        /// and the pvParam parameter must point to the LOGFONT structure to fill in.
        /// </summary>
        SPI_GETICONTITLELOGFONT = 0x001F,

        /// <summary>
        /// Sets the double-click time for the mouse to the value of the uiParam parameter. The double-click time is the maximum number 
        /// of milliseconds that can occur between the first and second clicks of a double-click. You can also call the SetDoubleClickTime 
        /// function to set the double-click time. To get the current double-click time, call the GetDoubleClickTime function.
        /// </summary>
        SPI_SETDOUBLECLICKTIME = 0x0020,

        /// <summary>
        /// Swaps or restores the meaning of the left and right mouse buttons. The uiParam parameter specifies TRUE to swap the meanings 
        /// of the buttons, or FALSE to restore their original meanings.
        /// </summary>
        SPI_SETMOUSEBUTTONSWAP = 0x0021,

        /// <summary>
        /// Sets the font that is used for icon titles. The uiParam parameter specifies the size of a LOGFONT structure, 
        /// and the pvParam parameter must point to a LOGFONT structure.
        /// </summary>
        SPI_SETICONTITLELOGFONT = 0x0022,

        /// <summary>
        /// This flag is obsolete. Previous versions of the system use this flag to determine whether ALT+TAB fast task switching is enabled. 
        /// For Windows 95, Windows 98, and Windows NT version 4.0 and later, fast task switching is always enabled.
        /// </summary>
        SPI_GETFASTTASKSWITCH = 0x0023,

        /// <summary>
        /// This flag is obsolete. Previous versions of the system use this flag to enable or disable ALT+TAB fast task switching. 
        /// For Windows 95, Windows 98, and Windows NT version 4.0 and later, fast task switching is always enabled.
        /// </summary>
        SPI_SETFASTTASKSWITCH = 0x0024,

        //#if(WINVER >= 0x0400)
        /// <summary>
        /// Sets dragging of full windows either on or off. The uiParam parameter specifies TRUE for on, or FALSE for off. 
        /// Windows 95:  This flag is supported only if Windows Plus! is installed. See SPI_GETWINDOWSEXTENSION.
        /// </summary>
        SPI_SETDRAGFULLWINDOWS = 0x0025,

        /// <summary>
        /// Determines whether dragging of full windows is enabled. The pvParam parameter must point to a BOOL variable that receives TRUE 
        /// if enabled, or FALSE otherwise. 
        /// Windows 95:  This flag is supported only if Windows Plus! is installed. See SPI_GETWINDOWSEXTENSION.
        /// </summary>
        SPI_GETDRAGFULLWINDOWS = 0x0026,

        /// <summary>
        /// Retrieves the metrics associated with the nonclient area of nonminimized windows. The pvParam parameter must point 
        /// to a NONCLIENTMETRICS structure that receives the information. Set the cbSize member of this structure and the uiParam parameter 
        /// to sizeof(NONCLIENTMETRICS).
        /// </summary>
        SPI_GETNONCLIENTMETRICS = 0x0029,

        /// <summary>
        /// Sets the metrics associated with the nonclient area of nonminimized windows. The pvParam parameter must point 
        /// to a NONCLIENTMETRICS structure that contains the new parameters. Set the cbSize member of this structure 
        /// and the uiParam parameter to sizeof(NONCLIENTMETRICS). Also, the lfHeight member of the LOGFONT structure must be a negative value.
        /// </summary>
        SPI_SETNONCLIENTMETRICS = 0x002A,

        /// <summary>
        /// Retrieves the metrics associated with minimized windows. The pvParam parameter must point to a MINIMIZEDMETRICS structure 
        /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(MINIMIZEDMETRICS).
        /// </summary>
        SPI_GETMINIMIZEDMETRICS = 0x002B,

        /// <summary>
        /// Sets the metrics associated with minimized windows. The pvParam parameter must point to a MINIMIZEDMETRICS structure 
        /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(MINIMIZEDMETRICS).
        /// </summary>
        SPI_SETMINIMIZEDMETRICS = 0x002C,

        /// <summary>
        /// Retrieves the metrics associated with icons. The pvParam parameter must point to an ICONMETRICS structure that receives 
        /// the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(ICONMETRICS).
        /// </summary>
        SPI_GETICONMETRICS = 0x002D,

        /// <summary>
        /// Sets the metrics associated with icons. The pvParam parameter must point to an ICONMETRICS structure that contains 
        /// the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(ICONMETRICS).
        /// </summary>
        SPI_SETICONMETRICS = 0x002E,

        /// <summary>
        /// Sets the size of the work area. The work area is the portion of the screen not obscured by the system taskbar 
        /// or by application desktop toolbars. The pvParam parameter is a pointer to a RECT structure that specifies the new work area rectangle, 
        /// expressed in virtual screen coordinates. In a system with multiple display monitors, the function sets the work area 
        /// of the monitor that contains the specified rectangle.
        /// </summary>
        SPI_SETWORKAREA = 0x002F,

        /// <summary>
        /// Retrieves the size of the work area on the primary display monitor. The work area is the portion of the screen not obscured 
        /// by the system taskbar or by application desktop toolbars. The pvParam parameter must point to a RECT structure that receives 
        /// the coordinates of the work area, expressed in virtual screen coordinates. 
        /// To get the work area of a monitor other than the primary display monitor, call the GetMonitorInfo function.
        /// </summary>
        SPI_GETWORKAREA = 0x0030,

        /// <summary>
        /// Windows Me/98/95:  Pen windows is being loaded or unloaded. The uiParam parameter is TRUE when loading and FALSE 
        /// when unloading pen windows. The pvParam parameter is null.
        /// </summary>
        SPI_SETPENWINDOWS = 0x0031,

        /// <summary>
        /// Retrieves information about the HighContrast accessibility feature. The pvParam parameter must point to a HIGHCONTRAST structure 
        /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(HIGHCONTRAST). 
        /// For a general discussion, see remarks.
        /// Windows NT:  This value is not supported.
        /// </summary>
        /// <remarks>
        /// There is a difference between the High Contrast color scheme and the High Contrast Mode. The High Contrast color scheme changes 
        /// the system colors to colors that have obvious contrast; you switch to this color scheme by using the Display Options in the control panel. 
        /// The High Contrast Mode, which uses SPI_GETHIGHCONTRAST and SPI_SETHIGHCONTRAST, advises applications to modify their appearance 
        /// for visually-impaired users. It involves such things as audible warning to users and customized color scheme 
        /// (using the Accessibility Options in the control panel). For more information, see HIGHCONTRAST on MSDN.
        /// For more information on general accessibility features, see Accessibility on MSDN.
        /// </remarks>
        SPI_GETHIGHCONTRAST = 0x0042,

        /// <summary>
        /// Sets the parameters of the HighContrast accessibility feature. The pvParam parameter must point to a HIGHCONTRAST structure 
        /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(HIGHCONTRAST).
        /// Windows NT:  This value is not supported.
        /// </summary>
        SPI_SETHIGHCONTRAST = 0x0043,

        /// <summary>
        /// Determines whether the user relies on the keyboard instead of the mouse, and wants applications to display keyboard interfaces 
        /// that would otherwise be hidden. The pvParam parameter must point to a BOOL variable that receives TRUE 
        /// if the user relies on the keyboard; or FALSE otherwise.
        /// Windows NT:  This value is not supported.
        /// </summary>
        SPI_GETKEYBOARDPREF = 0x0044,

        /// <summary>
        /// Sets the keyboard preference. The uiParam parameter specifies TRUE if the user relies on the keyboard instead of the mouse, 
        /// and wants applications to display keyboard interfaces that would otherwise be hidden; uiParam is FALSE otherwise.
        /// Windows NT:  This value is not supported.
        /// </summary>
        SPI_SETKEYBOARDPREF = 0x0045,

        /// <summary>
        /// Determines whether a screen reviewer utility is running. A screen reviewer utility directs textual information to an output device, 
        /// such as a speech synthesizer or Braille display. When this flag is set, an application should provide textual information 
        /// in situations where it would otherwise present the information graphically.
        /// The pvParam parameter is a pointer to a BOOL variable that receives TRUE if a screen reviewer utility is running, or FALSE otherwise.
        /// Windows NT:  This value is not supported.
        /// </summary>
        SPI_GETSCREENREADER = 0x0046,

        /// <summary>
        /// Determines whether a screen review utility is running. The uiParam parameter specifies TRUE for on, or FALSE for off.
        /// Windows NT:  This value is not supported.
        /// </summary>
        SPI_SETSCREENREADER = 0x0047,

        /// <summary>
        /// Retrieves the animation effects associated with user actions. The pvParam parameter must point to an ANIMATIONINFO structure 
        /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(ANIMATIONINFO).
        /// </summary>
        SPI_GETANIMATION = 0x0048,

        /// <summary>
        /// Sets the animation effects associated with user actions. The pvParam parameter must point to an ANIMATIONINFO structure 
        /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(ANIMATIONINFO).
        /// </summary>
        SPI_SETANIMATION = 0x0049,

        /// <summary>
        /// Determines whether the font smoothing feature is enabled. This feature uses font antialiasing to make font curves appear smoother 
        /// by painting pixels at different gray levels. 
        /// The pvParam parameter must point to a BOOL variable that receives TRUE if the feature is enabled, or FALSE if it is not.
        /// Windows 95:  This flag is supported only if Windows Plus! is installed. See SPI_GETWINDOWSEXTENSION.
        /// </summary>
        SPI_GETFONTSMOOTHING = 0x004A,

        /// <summary>
        /// Enables or disables the font smoothing feature, which uses font antialiasing to make font curves appear smoother 
        /// by painting pixels at different gray levels. 
        /// To enable the feature, set the uiParam parameter to TRUE. To disable the feature, set uiParam to FALSE.
        /// Windows 95:  This flag is supported only if Windows Plus! is installed. See SPI_GETWINDOWSEXTENSION.
        /// </summary>
        SPI_SETFONTSMOOTHING = 0x004B,

        /// <summary>
        /// Sets the width, in pixels, of the rectangle used to detect the start of a drag operation. Set uiParam to the new value. 
        /// To retrieve the drag width, call GetSystemMetrics with the SM_CXDRAG flag.
        /// </summary>
        SPI_SETDRAGWIDTH = 0x004C,

        /// <summary>
        /// Sets the height, in pixels, of the rectangle used to detect the start of a drag operation. Set uiParam to the new value. 
        /// To retrieve the drag height, call GetSystemMetrics with the SM_CYDRAG flag.
        /// </summary>
        SPI_SETDRAGHEIGHT = 0x004D,

        /// <summary>
        /// Used internally; applications should not use this value.
        /// </summary>
        SPI_SETHANDHELD = 0x004E,

        /// <summary>
        /// Retrieves the time-out value for the low-power phase of screen saving. The pvParam parameter must point to an integer variable 
        /// that receives the value. This flag is supported for 32-bit applications only.
        /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
        /// Windows 95:  This flag is supported for 16-bit applications only.
        /// </summary>
        SPI_GETLOWPOWERTIMEOUT = 0x004F,

        /// <summary>
        /// Retrieves the time-out value for the power-off phase of screen saving. The pvParam parameter must point to an integer variable 
        /// that receives the value. This flag is supported for 32-bit applications only.
        /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
        /// Windows 95:  This flag is supported for 16-bit applications only.
        /// </summary>
        SPI_GETPOWEROFFTIMEOUT = 0x0050,

        /// <summary>
        /// Sets the time-out value, in seconds, for the low-power phase of screen saving. The uiParam parameter specifies the new value. 
        /// The pvParam parameter must be null. This flag is supported for 32-bit applications only.
        /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
        /// Windows 95:  This flag is supported for 16-bit applications only.
        /// </summary>
        SPI_SETLOWPOWERTIMEOUT = 0x0051,

        /// <summary>
        /// Sets the time-out value, in seconds, for the power-off phase of screen saving. The uiParam parameter specifies the new value. 
        /// The pvParam parameter must be null. This flag is supported for 32-bit applications only.
        /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
        /// Windows 95:  This flag is supported for 16-bit applications only.
        /// </summary>
        SPI_SETPOWEROFFTIMEOUT = 0x0052,

        /// <summary>
        /// Determines whether the low-power phase of screen saving is enabled. The pvParam parameter must point to a BOOL variable 
        /// that receives TRUE if enabled, or FALSE if disabled. This flag is supported for 32-bit applications only.
        /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
        /// Windows 95:  This flag is supported for 16-bit applications only.
        /// </summary>
        SPI_GETLOWPOWERACTIVE = 0x0053,

        /// <summary>
        /// Determines whether the power-off phase of screen saving is enabled. The pvParam parameter must point to a BOOL variable 
        /// that receives TRUE if enabled, or FALSE if disabled. This flag is supported for 32-bit applications only.
        /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
        /// Windows 95:  This flag is supported for 16-bit applications only.
        /// </summary>
        SPI_GETPOWEROFFACTIVE = 0x0054,

        /// <summary>
        /// Activates or deactivates the low-power phase of screen saving. Set uiParam to 1 to activate, or zero to deactivate. 
        /// The pvParam parameter must be null. This flag is supported for 32-bit applications only.
        /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
        /// Windows 95:  This flag is supported for 16-bit applications only.
        /// </summary>
        SPI_SETLOWPOWERACTIVE = 0x0055,

        /// <summary>
        /// Activates or deactivates the power-off phase of screen saving. Set uiParam to 1 to activate, or zero to deactivate. 
        /// The pvParam parameter must be null. This flag is supported for 32-bit applications only.
        /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
        /// Windows 95:  This flag is supported for 16-bit applications only.
        /// </summary>
        SPI_SETPOWEROFFACTIVE = 0x0056,

        /// <summary>
        /// Reloads the system cursors. Set the uiParam parameter to zero and the pvParam parameter to null.
        /// </summary>
        SPI_SETCURSORS = 0x0057,

        /// <summary>
        /// Reloads the system icons. Set the uiParam parameter to zero and the pvParam parameter to null.
        /// </summary>
        SPI_SETICONS = 0x0058,

        /// <summary>
        /// Retrieves the input locale identifier for the system default input language. The pvParam parameter must point 
        /// to an HKL variable that receives this value. For more information, see Languages, Locales, and Keyboard Layouts on MSDN.
        /// </summary>
        SPI_GETDEFAULTINPUTLANG = 0x0059,

        /// <summary>
        /// Sets the default input language for the system shell and applications. The specified language must be displayable 
        /// using the current system character set. The pvParam parameter must point to an HKL variable that contains 
        /// the input locale identifier for the default language. For more information, see Languages, Locales, and Keyboard Layouts on MSDN.
        /// </summary>
        SPI_SETDEFAULTINPUTLANG = 0x005A,

        /// <summary>
        /// Sets the hot key set for switching between input languages. The uiParam and pvParam parameters are not used. 
        /// The value sets the shortcut keys in the keyboard property sheets by reading the registry again. The registry must be set before this flag is used. the path in the registry is \HKEY_CURRENT_USER\keyboard layout\toggle. Valid values are "1" = ALT+SHIFT, "2" = CTRL+SHIFT, and "3" = none.
        /// </summary>
        SPI_SETLANGTOGGLE = 0x005B,

        /// <summary>
        /// Windows 95:  Determines whether the Windows extension, Windows Plus!, is installed. Set the uiParam parameter to 1. 
        /// The pvParam parameter is not used. The function returns TRUE if the extension is installed, or FALSE if it is not.
        /// </summary>
        SPI_GETWINDOWSEXTENSION = 0x005C,

        /// <summary>
        /// Enables or disables the Mouse Trails feature, which improves the visibility of mouse cursor movements by briefly showing 
        /// a trail of cursors and quickly erasing them. 
        /// To disable the feature, set the uiParam parameter to zero or 1. To enable the feature, set uiParam to a value greater than 1 
        /// to indicate the number of cursors drawn in the trail.
        /// Windows 2000/NT:  This value is not supported.
        /// </summary>
        SPI_SETMOUSETRAILS = 0x005D,

        /// <summary>
        /// Determines whether the Mouse Trails feature is enabled. This feature improves the visibility of mouse cursor movements 
        /// by briefly showing a trail of cursors and quickly erasing them. 
        /// The pvParam parameter must point to an integer variable that receives a value. If the value is zero or 1, the feature is disabled. 
        /// If the value is greater than 1, the feature is enabled and the value indicates the number of cursors drawn in the trail. 
        /// The uiParam parameter is not used.
        /// Windows 2000/NT:  This value is not supported.
        /// </summary>
        SPI_GETMOUSETRAILS = 0x005E,

        /// <summary>
        /// Windows Me/98:  Used internally; applications should not use this flag.
        /// </summary>
        SPI_SETSCREENSAVERRUNNING = 0x0061,

        /// <summary>
        /// Same as SPI_SETSCREENSAVERRUNNING.
        /// </summary>
        SPI_SCREENSAVERRUNNING = SPI_SETSCREENSAVERRUNNING,
        //#endif /* WINVER >= 0x0400 */

        /// <summary>
        /// Retrieves information about the FilterKeys accessibility feature. The pvParam parameter must point to a FILTERKEYS structure 
        /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(FILTERKEYS).
        /// </summary>
        SPI_GETFILTERKEYS = 0x0032,

        /// <summary>
        /// Sets the parameters of the FilterKeys accessibility feature. The pvParam parameter must point to a FILTERKEYS structure 
        /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(FILTERKEYS).
        /// </summary>
        SPI_SETFILTERKEYS = 0x0033,

        /// <summary>
        /// Retrieves information about the ToggleKeys accessibility feature. The pvParam parameter must point to a TOGGLEKEYS structure 
        /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(TOGGLEKEYS).
        /// </summary>
        SPI_GETTOGGLEKEYS = 0x0034,

        /// <summary>
        /// Sets the parameters of the ToggleKeys accessibility feature. The pvParam parameter must point to a TOGGLEKEYS structure 
        /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(TOGGLEKEYS).
        /// </summary>
        SPI_SETTOGGLEKEYS = 0x0035,

        /// <summary>
        /// Retrieves information about the MouseKeys accessibility feature. The pvParam parameter must point to a MOUSEKEYS structure 
        /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(MOUSEKEYS).
        /// </summary>
        SPI_GETMOUSEKEYS = 0x0036,

        /// <summary>
        /// Sets the parameters of the MouseKeys accessibility feature. The pvParam parameter must point to a MOUSEKEYS structure 
        /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(MOUSEKEYS).
        /// </summary>
        SPI_SETMOUSEKEYS = 0x0037,

        /// <summary>
        /// Determines whether the Show Sounds accessibility flag is on or off. If it is on, the user requires an application 
        /// to present information visually in situations where it would otherwise present the information only in audible form. 
        /// The pvParam parameter must point to a BOOL variable that receives TRUE if the feature is on, or FALSE if it is off. 
        /// Using this value is equivalent to calling GetSystemMetrics (SM_SHOWSOUNDS). That is the recommended call.
        /// </summary>
        SPI_GETSHOWSOUNDS = 0x0038,

        /// <summary>
        /// Sets the parameters of the SoundSentry accessibility feature. The pvParam parameter must point to a SOUNDSENTRY structure 
        /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(SOUNDSENTRY).
        /// </summary>
        SPI_SETSHOWSOUNDS = 0x0039,

        /// <summary>
        /// Retrieves information about the StickyKeys accessibility feature. The pvParam parameter must point to a STICKYKEYS structure 
        /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(STICKYKEYS).
        /// </summary>
        SPI_GETSTICKYKEYS = 0x003A,

        /// <summary>
        /// Sets the parameters of the StickyKeys accessibility feature. The pvParam parameter must point to a STICKYKEYS structure 
        /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(STICKYKEYS).
        /// </summary>
        SPI_SETSTICKYKEYS = 0x003B,

        /// <summary>
        /// Retrieves information about the time-out period associated with the accessibility features. The pvParam parameter must point 
        /// to an ACCESSTIMEOUT structure that receives the information. Set the cbSize member of this structure and the uiParam parameter 
        /// to sizeof(ACCESSTIMEOUT).
        /// </summary>
        SPI_GETACCESSTIMEOUT = 0x003C,

        /// <summary>
        /// Sets the time-out period associated with the accessibility features. The pvParam parameter must point to an ACCESSTIMEOUT 
        /// structure that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(ACCESSTIMEOUT).
        /// </summary>
        SPI_SETACCESSTIMEOUT = 0x003D,

        //#if(WINVER >= 0x0400)
        /// <summary>
        /// Windows Me/98/95:  Retrieves information about the SerialKeys accessibility feature. The pvParam parameter must point 
        /// to a SERIALKEYS structure that receives the information. Set the cbSize member of this structure and the uiParam parameter 
        /// to sizeof(SERIALKEYS).
        /// Windows Server 2003, Windows XP/2000/NT:  Not supported. The user controls this feature through the control panel.
        /// </summary>
        SPI_GETSERIALKEYS = 0x003E,

        /// <summary>
        /// Windows Me/98/95:  Sets the parameters of the SerialKeys accessibility feature. The pvParam parameter must point 
        /// to a SERIALKEYS structure that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter 
        /// to sizeof(SERIALKEYS). 
        /// Windows Server 2003, Windows XP/2000/NT:  Not supported. The user controls this feature through the control panel.
        /// </summary>
        SPI_SETSERIALKEYS = 0x003F,
        //#endif /* WINVER >= 0x0400 */ 

        /// <summary>
        /// Retrieves information about the SoundSentry accessibility feature. The pvParam parameter must point to a SOUNDSENTRY structure 
        /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(SOUNDSENTRY).
        /// </summary>
        SPI_GETSOUNDSENTRY = 0x0040,

        /// <summary>
        /// Sets the parameters of the SoundSentry accessibility feature. The pvParam parameter must point to a SOUNDSENTRY structure 
        /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(SOUNDSENTRY).
        /// </summary>
        SPI_SETSOUNDSENTRY = 0x0041,

        //#if(_WIN32_WINNT >= 0x0400)
        /// <summary>
        /// Determines whether the snap-to-default-button feature is enabled. If enabled, the mouse cursor automatically moves 
        /// to the default button, such as OK or Apply, of a dialog box. The pvParam parameter must point to a BOOL variable 
        /// that receives TRUE if the feature is on, or FALSE if it is off. 
        /// Windows 95:  Not supported.
        /// </summary>
        SPI_GETSNAPTODEFBUTTON = 0x005F,

        /// <summary>
        /// Enables or disables the snap-to-default-button feature. If enabled, the mouse cursor automatically moves to the default button, 
        /// such as OK or Apply, of a dialog box. Set the uiParam parameter to TRUE to enable the feature, or FALSE to disable it. 
        /// Applications should use the ShowWindow function when displaying a dialog box so the dialog manager can position the mouse cursor. 
        /// Windows 95:  Not supported.
        /// </summary>
        SPI_SETSNAPTODEFBUTTON = 0x0060,
        //#endif /* _WIN32_WINNT >= 0x0400 */

        //#if (_WIN32_WINNT >= 0x0400) || (_WIN32_WINDOWS > 0x0400)
        /// <summary>
        /// Retrieves the width, in pixels, of the rectangle within which the mouse pointer has to stay for TrackMouseEvent 
        /// to generate a WM_MOUSEHOVER message. The pvParam parameter must point to a UINT variable that receives the width. 
        /// Windows 95:  Not supported.
        /// </summary>
        SPI_GETMOUSEHOVERWIDTH = 0x0062,

        /// <summary>
        /// Retrieves the width, in pixels, of the rectangle within which the mouse pointer has to stay for TrackMouseEvent 
        /// to generate a WM_MOUSEHOVER message. The pvParam parameter must point to a UINT variable that receives the width. 
        /// Windows 95:  Not supported.
        /// </summary>
        SPI_SETMOUSEHOVERWIDTH = 0x0063,

        /// <summary>
        /// Retrieves the height, in pixels, of the rectangle within which the mouse pointer has to stay for TrackMouseEvent 
        /// to generate a WM_MOUSEHOVER message. The pvParam parameter must point to a UINT variable that receives the height. 
        /// Windows 95:  Not supported.
        /// </summary>
        SPI_GETMOUSEHOVERHEIGHT = 0x0064,

        /// <summary>
        /// Sets the height, in pixels, of the rectangle within which the mouse pointer has to stay for TrackMouseEvent 
        /// to generate a WM_MOUSEHOVER message. Set the uiParam parameter to the new height.
        /// Windows 95:  Not supported.
        /// </summary>
        SPI_SETMOUSEHOVERHEIGHT = 0x0065,

        /// <summary>
        /// Retrieves the time, in milliseconds, that the mouse pointer has to stay in the hover rectangle for TrackMouseEvent 
        /// to generate a WM_MOUSEHOVER message. The pvParam parameter must point to a UINT variable that receives the time. 
        /// Windows 95:  Not supported.
        /// </summary>
        SPI_GETMOUSEHOVERTIME = 0x0066,

        /// <summary>
        /// Sets the time, in milliseconds, that the mouse pointer has to stay in the hover rectangle for TrackMouseEvent 
        /// to generate a WM_MOUSEHOVER message. This is used only if you pass HOVER_DEFAULT in the dwHoverTime parameter in the call to TrackMouseEvent. Set the uiParam parameter to the new time. 
        /// Windows 95:  Not supported.
        /// </summary>
        SPI_SETMOUSEHOVERTIME = 0x0067,

        /// <summary>
        /// Retrieves the number of lines to scroll when the mouse wheel is rotated. The pvParam parameter must point 
        /// to a UINT variable that receives the number of lines. The default value is 3. 
        /// Windows 95:  Not supported.
        /// </summary>
        SPI_GETWHEELSCROLLLINES = 0x0068,

        /// <summary>
        /// Sets the number of lines to scroll when the mouse wheel is rotated. The number of lines is set from the uiParam parameter. 
        /// The number of lines is the suggested number of lines to scroll when the mouse wheel is rolled without using modifier keys. 
        /// If the number is 0, then no scrolling should occur. If the number of lines to scroll is greater than the number of lines viewable, 
        /// and in particular if it is WHEEL_PAGESCROLL (#defined as UINT_MAX), the scroll operation should be interpreted 
        /// as clicking once in the page down or page up regions of the scroll bar.
        /// Windows 95:  Not supported.
        /// </summary>
        SPI_SETWHEELSCROLLLINES = 0x0069,

        /// <summary>
        /// Retrieves the time, in milliseconds, that the system waits before displaying a shortcut menu when the mouse cursor is 
        /// over a submenu item. The pvParam parameter must point to a DWORD variable that receives the time of the delay. 
        /// Windows 95:  Not supported.
        /// </summary>
        SPI_GETMENUSHOWDELAY = 0x006A,

        /// <summary>
        /// Sets uiParam to the time, in milliseconds, that the system waits before displaying a shortcut menu when the mouse cursor is 
        /// over a submenu item. 
        /// Windows 95:  Not supported.
        /// </summary>
        SPI_SETMENUSHOWDELAY = 0x006B,

        /// <summary>
        /// Determines whether the IME status window is visible (on a per-user basis). The pvParam parameter must point to a BOOL variable 
        /// that receives TRUE if the status window is visible, or FALSE if it is not.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SPI_GETSHOWIMEUI = 0x006E,

        /// <summary>
        /// Sets whether the IME status window is visible or not on a per-user basis. The uiParam parameter specifies TRUE for on or FALSE for off.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SPI_SETSHOWIMEUI = 0x006F,
        //#endif

        //#if(WINVER >= 0x0500)
        /// <summary>
        /// Retrieves the current mouse speed. The mouse speed determines how far the pointer will move based on the distance the mouse moves. 
        /// The pvParam parameter must point to an integer that receives a value which ranges between 1 (slowest) and 20 (fastest). 
        /// A value of 10 is the default. The value can be set by an end user using the mouse control panel application or 
        /// by an application using SPI_SETMOUSESPEED.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SPI_GETMOUSESPEED = 0x0070,

        /// <summary>
        /// Sets the current mouse speed. The pvParam parameter is an integer between 1 (slowest) and 20 (fastest). A value of 10 is the default. 
        /// This value is typically set using the mouse control panel application.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SPI_SETMOUSESPEED = 0x0071,

        /// <summary>
        /// Determines whether a screen saver is currently running on the window station of the calling process. 
        /// The pvParam parameter must point to a BOOL variable that receives TRUE if a screen saver is currently running, or FALSE otherwise.
        /// Note that only the interactive window station, "WinSta0", can have a screen saver running.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SPI_GETSCREENSAVERRUNNING = 0x0072,

        /// <summary>
        /// Retrieves the full path of the bitmap file for the desktop wallpaper. The pvParam parameter must point to a buffer 
        /// that receives a null-terminated path string. Set the uiParam parameter to the size, in characters, of the pvParam buffer. The returned string will not exceed MAX_PATH characters. If there is no desktop wallpaper, the returned string is empty.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_GETDESKWALLPAPER = 0x0073,
        //#endif /* WINVER >= 0x0500 */

        //#if(WINVER >= 0x0500)
        /// <summary>
        /// Determines whether active window tracking (activating the window the mouse is on) is on or off. The pvParam parameter must point 
        /// to a BOOL variable that receives TRUE for on, or FALSE for off.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SPI_GETACTIVEWINDOWTRACKING = 0x1000,

        /// <summary>
        /// Sets active window tracking (activating the window the mouse is on) either on or off. Set pvParam to TRUE for on or FALSE for off.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SPI_SETACTIVEWINDOWTRACKING = 0x1001,

        /// <summary>
        /// Determines whether the menu animation feature is enabled. This master switch must be on to enable menu animation effects. 
        /// The pvParam parameter must point to a BOOL variable that receives TRUE if animation is enabled and FALSE if it is disabled. 
        /// If animation is enabled, SPI_GETMENUFADE indicates whether menus use fade or slide animation.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SPI_GETMENUANIMATION = 0x1002,

        /// <summary>
        /// Enables or disables menu animation. This master switch must be on for any menu animation to occur. 
        /// The pvParam parameter is a BOOL variable; set pvParam to TRUE to enable animation and FALSE to disable animation.
        /// If animation is enabled, SPI_GETMENUFADE indicates whether menus use fade or slide animation.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SPI_SETMENUANIMATION = 0x1003,

        /// <summary>
        /// Determines whether the slide-open effect for combo boxes is enabled. The pvParam parameter must point to a BOOL variable 
        /// that receives TRUE for enabled, or FALSE for disabled.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SPI_GETCOMBOBOXANIMATION = 0x1004,

        /// <summary>
        /// Enables or disables the slide-open effect for combo boxes. Set the pvParam parameter to TRUE to enable the gradient effect, 
        /// or FALSE to disable it.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SPI_SETCOMBOBOXANIMATION = 0x1005,

        /// <summary>
        /// Determines whether the smooth-scrolling effect for list boxes is enabled. The pvParam parameter must point to a BOOL variable 
        /// that receives TRUE for enabled, or FALSE for disabled.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SPI_GETLISTBOXSMOOTHSCROLLING = 0x1006,

        /// <summary>
        /// Enables or disables the smooth-scrolling effect for list boxes. Set the pvParam parameter to TRUE to enable the smooth-scrolling effect,
        /// or FALSE to disable it.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SPI_SETLISTBOXSMOOTHSCROLLING = 0x1007,

        /// <summary>
        /// Determines whether the gradient effect for window title bars is enabled. The pvParam parameter must point to a BOOL variable 
        /// that receives TRUE for enabled, or FALSE for disabled. For more information about the gradient effect, see the GetSysColor function.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SPI_GETGRADIENTCAPTIONS = 0x1008,

        /// <summary>
        /// Enables or disables the gradient effect for window title bars. Set the pvParam parameter to TRUE to enable it, or FALSE to disable it. 
        /// The gradient effect is possible only if the system has a color depth of more than 256 colors. For more information about 
        /// the gradient effect, see the GetSysColor function.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SPI_SETGRADIENTCAPTIONS = 0x1009,

        /// <summary>
        /// Determines whether menu access keys are always underlined. The pvParam parameter must point to a BOOL variable that receives TRUE 
        /// if menu access keys are always underlined, and FALSE if they are underlined only when the menu is activated by the keyboard.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SPI_GETKEYBOARDCUES = 0x100A,

        /// <summary>
        /// Sets the underlining of menu access key letters. The pvParam parameter is a BOOL variable. Set pvParam to TRUE to always underline menu 
        /// access keys, or FALSE to underline menu access keys only when the menu is activated from the keyboard.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SPI_SETKEYBOARDCUES = 0x100B,

        /// <summary>
        /// Same as SPI_GETKEYBOARDCUES.
        /// </summary>
        SPI_GETMENUUNDERLINES = SPI_GETKEYBOARDCUES,

        /// <summary>
        /// Same as SPI_SETKEYBOARDCUES.
        /// </summary>
        SPI_SETMENUUNDERLINES = SPI_SETKEYBOARDCUES,

        /// <summary>
        /// Determines whether windows activated through active window tracking will be brought to the top. The pvParam parameter must point 
        /// to a BOOL variable that receives TRUE for on, or FALSE for off.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SPI_GETACTIVEWNDTRKZORDER = 0x100C,

        /// <summary>
        /// Determines whether or not windows activated through active window tracking should be brought to the top. Set pvParam to TRUE 
        /// for on or FALSE for off.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SPI_SETACTIVEWNDTRKZORDER = 0x100D,

        /// <summary>
        /// Determines whether hot tracking of user-interface elements, such as menu names on menu bars, is enabled. The pvParam parameter 
        /// must point to a BOOL variable that receives TRUE for enabled, or FALSE for disabled. 
        /// Hot tracking means that when the cursor moves over an item, it is highlighted but not selected. You can query this value to decide 
        /// whether to use hot tracking in the user interface of your application.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SPI_GETHOTTRACKING = 0x100E,

        /// <summary>
        /// Enables or disables hot tracking of user-interface elements such as menu names on menu bars. Set the pvParam parameter to TRUE 
        /// to enable it, or FALSE to disable it.
        /// Hot-tracking means that when the cursor moves over an item, it is highlighted but not selected.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SPI_SETHOTTRACKING = 0x100F,

        /// <summary>
        /// Determines whether menu fade animation is enabled. The pvParam parameter must point to a BOOL variable that receives TRUE 
        /// when fade animation is enabled and FALSE when it is disabled. If fade animation is disabled, menus use slide animation. 
        /// This flag is ignored unless menu animation is enabled, which you can do using the SPI_SETMENUANIMATION flag. 
        /// For more information, see AnimateWindow.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_GETMENUFADE = 0x1012,

        /// <summary>
        /// Enables or disables menu fade animation. Set pvParam to TRUE to enable the menu fade effect or FALSE to disable it. 
        /// If fade animation is disabled, menus use slide animation. he The menu fade effect is possible only if the system 
        /// has a color depth of more than 256 colors. This flag is ignored unless SPI_MENUANIMATION is also set. For more information, 
        /// see AnimateWindow.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_SETMENUFADE = 0x1013,

        /// <summary>
        /// Determines whether the selection fade effect is enabled. The pvParam parameter must point to a BOOL variable that receives TRUE 
        /// if enabled or FALSE if disabled. 
        /// The selection fade effect causes the menu item selected by the user to remain on the screen briefly while fading out 
        /// after the menu is dismissed.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_GETSELECTIONFADE = 0x1014,

        /// <summary>
        /// Set pvParam to TRUE to enable the selection fade effect or FALSE to disable it.
        /// The selection fade effect causes the menu item selected by the user to remain on the screen briefly while fading out 
        /// after the menu is dismissed. The selection fade effect is possible only if the system has a color depth of more than 256 colors.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_SETSELECTIONFADE = 0x1015,

        /// <summary>
        /// Determines whether ToolTip animation is enabled. The pvParam parameter must point to a BOOL variable that receives TRUE 
        /// if enabled or FALSE if disabled. If ToolTip animation is enabled, SPI_GETTOOLTIPFADE indicates whether ToolTips use fade or slide animation.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_GETTOOLTIPANIMATION = 0x1016,

        /// <summary>
        /// Set pvParam to TRUE to enable ToolTip animation or FALSE to disable it. If enabled, you can use SPI_SETTOOLTIPFADE 
        /// to specify fade or slide animation.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_SETTOOLTIPANIMATION = 0x1017,

        /// <summary>
        /// If SPI_SETTOOLTIPANIMATION is enabled, SPI_GETTOOLTIPFADE indicates whether ToolTip animation uses a fade effect or a slide effect.
        ///  The pvParam parameter must point to a BOOL variable that receives TRUE for fade animation or FALSE for slide animation. 
        ///  For more information on slide and fade effects, see AnimateWindow.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_GETTOOLTIPFADE = 0x1018,

        /// <summary>
        /// If the SPI_SETTOOLTIPANIMATION flag is enabled, use SPI_SETTOOLTIPFADE to indicate whether ToolTip animation uses a fade effect 
        /// or a slide effect. Set pvParam to TRUE for fade animation or FALSE for slide animation. The tooltip fade effect is possible only 
        /// if the system has a color depth of more than 256 colors. For more information on the slide and fade effects, 
        /// see the AnimateWindow function.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_SETTOOLTIPFADE = 0x1019,

        /// <summary>
        /// Determines whether the cursor has a shadow around it. The pvParam parameter must point to a BOOL variable that receives TRUE 
        /// if the shadow is enabled, FALSE if it is disabled. This effect appears only if the system has a color depth of more than 256 colors.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_GETCURSORSHADOW = 0x101A,

        /// <summary>
        /// Enables or disables a shadow around the cursor. The pvParam parameter is a BOOL variable. Set pvParam to TRUE to enable the shadow 
        /// or FALSE to disable the shadow. This effect appears only if the system has a color depth of more than 256 colors.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_SETCURSORSHADOW = 0x101B,

        //#if(_WIN32_WINNT >= 0x0501)
        /// <summary>
        /// Retrieves the state of the Mouse Sonar feature. The pvParam parameter must point to a BOOL variable that receives TRUE 
        /// if enabled or FALSE otherwise. For more information, see About Mouse Input on MSDN.
        /// Windows 2000/NT, Windows 98/95:  This value is not supported.
        /// </summary>
        SPI_GETMOUSESONAR = 0x101C,

        /// <summary>
        /// Turns the Sonar accessibility feature on or off. This feature briefly shows several concentric circles around the mouse pointer 
        /// when the user presses and releases the CTRL key. The pvParam parameter specifies TRUE for on and FALSE for off. The default is off. 
        /// For more information, see About Mouse Input.
        /// Windows 2000/NT, Windows 98/95:  This value is not supported.
        /// </summary>
        SPI_SETMOUSESONAR = 0x101D,

        /// <summary>
        /// Retrieves the state of the Mouse ClickLock feature. The pvParam parameter must point to a BOOL variable that receives TRUE 
        /// if enabled, or FALSE otherwise. For more information, see About Mouse Input.
        /// Windows 2000/NT, Windows 98/95:  This value is not supported.
        /// </summary>
        SPI_GETMOUSECLICKLOCK = 0x101E,

        /// <summary>
        /// Turns the Mouse ClickLock accessibility feature on or off. This feature temporarily locks down the primary mouse button 
        /// when that button is clicked and held down for the time specified by SPI_SETMOUSECLICKLOCKTIME. The uiParam parameter specifies 
        /// TRUE for on, 
        /// or FALSE for off. The default is off. For more information, see Remarks and About Mouse Input on MSDN.
        /// Windows 2000/NT, Windows 98/95:  This value is not supported.
        /// </summary>
        SPI_SETMOUSECLICKLOCK = 0x101F,

        /// <summary>
        /// Retrieves the state of the Mouse Vanish feature. The pvParam parameter must point to a BOOL variable that receives TRUE 
        /// if enabled or FALSE otherwise. For more information, see About Mouse Input on MSDN.
        /// Windows 2000/NT, Windows 98/95:  This value is not supported.
        /// </summary>
        SPI_GETMOUSEVANISH = 0x1020,

        /// <summary>
        /// Turns the Vanish feature on or off. This feature hides the mouse pointer when the user types; the pointer reappears 
        /// when the user moves the mouse. The pvParam parameter specifies TRUE for on and FALSE for off. The default is off. 
        /// For more information, see About Mouse Input on MSDN.
        /// Windows 2000/NT, Windows 98/95:  This value is not supported.
        /// </summary>
        SPI_SETMOUSEVANISH = 0x1021,

        /// <summary>
        /// Determines whether native User menus have flat menu appearance. The pvParam parameter must point to a BOOL variable 
        /// that returns TRUE if the flat menu appearance is set, or FALSE otherwise.
        /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_GETFLATMENU = 0x1022,

        /// <summary>
        /// Enables or disables flat menu appearance for native User menus. Set pvParam to TRUE to enable flat menu appearance 
        /// or FALSE to disable it. 
        /// When enabled, the menu bar uses COLOR_MENUBAR for the menubar background, COLOR_MENU for the menu-popup background, COLOR_MENUHILIGHT 
        /// for the fill of the current menu selection, and COLOR_HILIGHT for the outline of the current menu selection. 
        /// If disabled, menus are drawn using the same metrics and colors as in Windows 2000 and earlier.
        /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_SETFLATMENU = 0x1023,

        /// <summary>
        /// Determines whether the drop shadow effect is enabled. The pvParam parameter must point to a BOOL variable that returns TRUE 
        /// if enabled or FALSE if disabled.
        /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_GETDROPSHADOW = 0x1024,

        /// <summary>
        /// Enables or disables the drop shadow effect. Set pvParam to TRUE to enable the drop shadow effect or FALSE to disable it. 
        /// You must also have CS_DROPSHADOW in the window class style.
        /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_SETDROPSHADOW = 0x1025,

        /// <summary>
        /// Retrieves a BOOL indicating whether an application can reset the screensaver's timer by calling the SendInput function 
        /// to simulate keyboard or mouse input. The pvParam parameter must point to a BOOL variable that receives TRUE 
        /// if the simulated input will be blocked, or FALSE otherwise. 
        /// </summary>
        SPI_GETBLOCKSENDINPUTRESETS = 0x1026,

        /// <summary>
        /// Determines whether an application can reset the screensaver's timer by calling the SendInput function to simulate keyboard 
        /// or mouse input. The uiParam parameter specifies TRUE if the screensaver will not be deactivated by simulated input, 
        /// or FALSE if the screensaver will be deactivated by simulated input.
        /// </summary>
        SPI_SETBLOCKSENDINPUTRESETS = 0x1027,
        //#endif /* _WIN32_WINNT >= 0x0501 */

        /// <summary>
        /// Determines whether UI effects are enabled or disabled. The pvParam parameter must point to a BOOL variable that receives TRUE 
        /// if all UI effects are enabled, or FALSE if they are disabled.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_GETUIEFFECTS = 0x103E,

        /// <summary>
        /// Enables or disables UI effects. Set the pvParam parameter to TRUE to enable all UI effects or FALSE to disable all UI effects.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_SETUIEFFECTS = 0x103F,

        /// <summary>
        /// Retrieves the amount of time following user input, in milliseconds, during which the system will not allow applications 
        /// to force themselves into the foreground. The pvParam parameter must point to a DWORD variable that receives the time.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SPI_GETFOREGROUNDLOCKTIMEOUT = 0x2000,

        /// <summary>
        /// Sets the amount of time following user input, in milliseconds, during which the system does not allow applications 
        /// to force themselves into the foreground. Set pvParam to the new timeout value.
        /// The calling thread must be able to change the foreground window, otherwise the call fails.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SPI_SETFOREGROUNDLOCKTIMEOUT = 0x2001,

        /// <summary>
        /// Retrieves the active window tracking delay, in milliseconds. The pvParam parameter must point to a DWORD variable 
        /// that receives the time.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SPI_GETACTIVEWNDTRKTIMEOUT = 0x2002,

        /// <summary>
        /// Sets the active window tracking delay. Set pvParam to the number of milliseconds to delay before activating the window 
        /// under the mouse pointer.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SPI_SETACTIVEWNDTRKTIMEOUT = 0x2003,

        /// <summary>
        /// Retrieves the number of times SetForegroundWindow will flash the taskbar button when rejecting a foreground switch request. 
        /// The pvParam parameter must point to a DWORD variable that receives the value.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SPI_GETFOREGROUNDFLASHCOUNT = 0x2004,

        /// <summary>
        /// Sets the number of times SetForegroundWindow will flash the taskbar button when rejecting a foreground switch request. 
        /// Set pvParam to the number of times to flash.
        /// Windows NT, Windows 95:  This value is not supported.
        /// </summary>
        SPI_SETFOREGROUNDFLASHCOUNT = 0x2005,

        /// <summary>
        /// Retrieves the caret width in edit controls, in pixels. The pvParam parameter must point to a DWORD that receives this value.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_GETCARETWIDTH = 0x2006,

        /// <summary>
        /// Sets the caret width in edit controls. Set pvParam to the desired width, in pixels. The default and minimum value is 1.
        /// Windows NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_SETCARETWIDTH = 0x2007,

        //#if(_WIN32_WINNT >= 0x0501)
        /// <summary>
        /// Retrieves the time delay before the primary mouse button is locked. The pvParam parameter must point to DWORD that receives 
        /// the time delay. This is only enabled if SPI_SETMOUSECLICKLOCK is set to TRUE. For more information, see About Mouse Input on MSDN.
        /// Windows 2000/NT, Windows 98/95:  This value is not supported.
        /// </summary>
        SPI_GETMOUSECLICKLOCKTIME = 0x2008,

        /// <summary>
        /// Turns the Mouse ClickLock accessibility feature on or off. This feature temporarily locks down the primary mouse button 
        /// when that button is clicked and held down for the time specified by SPI_SETMOUSECLICKLOCKTIME. The uiParam parameter 
        /// specifies TRUE for on, or FALSE for off. The default is off. For more information, see Remarks and About Mouse Input on MSDN.
        /// Windows 2000/NT, Windows 98/95:  This value is not supported.
        /// </summary>
        SPI_SETMOUSECLICKLOCKTIME = 0x2009,

        /// <summary>
        /// Retrieves the type of font smoothing. The pvParam parameter must point to a UINT that receives the information.
        /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_GETFONTSMOOTHINGTYPE = 0x200A,

        /// <summary>
        /// Sets the font smoothing type. The pvParam parameter points to a UINT that contains either FE_FONTSMOOTHINGSTANDARD, 
        /// if standard anti-aliasing is used, or FE_FONTSMOOTHINGCLEARTYPE, if ClearType is used. The default is FE_FONTSMOOTHINGSTANDARD. 
        /// When using this option, the fWinIni parameter must be set to SPIF_SENDWININICHANGE | SPIF_UPDATEINIFILE; otherwise, 
        /// SystemParametersInfo fails.
        /// </summary>
        SPI_SETFONTSMOOTHINGTYPE = 0x200B,

        /// <summary>
        /// Retrieves a contrast value that is used in ClearType™ smoothing. The pvParam parameter must point to a UINT 
        /// that receives the information.
        /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_GETFONTSMOOTHINGCONTRAST = 0x200C,

        /// <summary>
        /// Sets the contrast value used in ClearType smoothing. The pvParam parameter points to a UINT that holds the contrast value. 
        /// Valid contrast values are from 1000 to 2200. The default value is 1400.
        /// When using this option, the fWinIni parameter must be set to SPIF_SENDWININICHANGE | SPIF_UPDATEINIFILE; otherwise, 
        /// SystemParametersInfo fails.
        /// SPI_SETFONTSMOOTHINGTYPE must also be set to FE_FONTSMOOTHINGCLEARTYPE.
        /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_SETFONTSMOOTHINGCONTRAST = 0x200D,

        /// <summary>
        /// Retrieves the width, in pixels, of the left and right edges of the focus rectangle drawn with DrawFocusRect. 
        /// The pvParam parameter must point to a UINT.
        /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_GETFOCUSBORDERWIDTH = 0x200E,

        /// <summary>
        /// Sets the height of the left and right edges of the focus rectangle drawn with DrawFocusRect to the value of the pvParam parameter.
        /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_SETFOCUSBORDERWIDTH = 0x200F,

        /// <summary>
        /// Retrieves the height, in pixels, of the top and bottom edges of the focus rectangle drawn with DrawFocusRect. 
        /// The pvParam parameter must point to a UINT.
        /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_GETFOCUSBORDERHEIGHT = 0x2010,

        /// <summary>
        /// Sets the height of the top and bottom edges of the focus rectangle drawn with DrawFocusRect to the value of the pvParam parameter.
        /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_SETFOCUSBORDERHEIGHT = 0x2011,

        /// <summary>
        /// Not implemented.
        /// </summary>
        SPI_GETFONTSMOOTHINGORIENTATION = 0x2012,

        /// <summary>
        /// Not implemented.
        /// </summary>
        SPI_SETFONTSMOOTHINGORIENTATION = 0x2013,
    }

    [Flags]
    public enum SHGFI : int
    {
        SHGFI_ICON = 0x000000100,               // get icon
        SHGFI_DISPLAYNAME = 0x000000200,        // get display name
        SHGFI_TYPENAME = 0x000000400,           // get type name
        SHGFI_ATTRIBUTES = 0x000000800,         // get attributes
        SHGFI_ICONLOCATION = 0x000001000,       // get icon location
        SHGFI_EXETYPE = 0x000002000,            // return exe type
        SHGFI_SYSICONINDEX = 0x000004000,       // get system icon index
        SHGFI_LINKOVERLAY = 0x000008000,        // put a link overlay on icon
        SHGFI_SELECTED = 0x000010000,           // show icon in selected state
        SHGFI_ATTR_SPECIFIED = 0x000020000,     // get only specified attributes
        SHGFI_LARGEICON = 0x000000000,          // get large icon
        SHGFI_SMALLICON = 0x000000001,          // get small icon
        SHGFI_OPENICON = 0x000000002,           // get open icon
        SHGFI_SHELLICONSIZE = 0x000000004,      // get shell size icon
        SHGFI_PIDL = 0x000000008,               // pszPath is a pidl
        SHGFI_USEFILEATTRIBUTES = 0x000000010,  // use passed dwFileAttribute
        SHGFI_ADDOVERLAYS = 0x000000020,        // apply the appropriate overlays
        SHGFI_OVERLAYINDEX = 0x000000040,       // Get the index of the overlay in the upper 8 bits of the iIcon
    }

    [Flags]
    public enum SWP : uint
    {
        SWP_NOSIZE = 0x0001,
        SWP_NOMOVE = 0x0002,
        SWP_NOZORDER = 0x0004,
        SWP_NOREDRAW = 0x0008,
        SWP_NOACTIVATE = 0x0010,
        SWP_FRAMECHANGED = 0x0020,
        SWP_SHOWWINDOW = 0x0040,
        SWP_HIDEWINDOW = 0x0080,
        SWP_NOCOPYBITS = 0x0100,
        SWP_NOOWNERZORDER = 0x0200,
        SWP_NOSENDCHANGING = 0x0400,
        SWP_DRAWFRAME = SWP_FRAMECHANGED,
        SWP_NOREPOSITION = SWP_NOOWNERZORDER,
        SWP_DEFERERASE = 0x2000,
        SWP_ASYNCWINDOWPOS = 0x4000
    }

    /// <summary>
    /// STGM values used in the storage and stream interfaces to indicate the conditions for creating and deleting the object and access modes for the object.
    /// </summary>
    [Flags]
    public enum STGM : uint
    {
        STGM_DIRECT = 0x00000000,
        STGM_TRANSACTED = 0x00010000,
        STGM_SIMPLE = 0x08000000,
        STGM_READ = 0x00000000,
        STGM_WRITE = 0x00000001,
        STGM_READWRITE = 0x00000002,
        STGM_SHARE_DENY_NONE = 0x00000040,
        STGM_SHARE_DENY_READ = 0x00000030,
        STGM_SHARE_DENY_WRITE = 0x00000020,
        STGM_SHARE_EXCLUSIVE = 0x00000010,
        STGM_PRIORITY = 0x00040000,
        STGM_DELETEONRELEASE = 0x04000000,
        STGM_CREATE = 0x00001000,
        STGM_CONVERT = 0x00020000,
        STGM_FAILIFTHERE = 0x00000000,
        STGM_NOSCRATCH = 0x00100000,
        STGM_NOSNAPSHOT = 0x00200000,
        STGM_DIRECT_SWMR = 0x00400000,
    }

    [Flags]
    public enum FILE_ATTRIBUTE : uint
    {
        FILE_ATTRIBUTE_READONLY        = 0x00000001,
        FILE_ATTRIBUTE_HIDDEN          = 0x00000002,
        FILE_ATTRIBUTE_SYSTEM          = 0x00000004,
        FILE_ATTRIBUTE_DIRECTORY       = 0x00000010,
        FILE_ATTRIBUTE_ARCHIVE         = 0x00000020,
        FILE_ATTRIBUTE_DEVICE          = 0x00000040,
        FILE_ATTRIBUTE_NORMAL          = 0x00000080,
        FILE_ATTRIBUTE_TEMPORARY       = 0x00000100,
        FILE_ATTRIBUTE_SPARSE_FILE     = 0x00000200,
        FILE_ATTRIBUTE_REPARSE_POINT   = 0x00000400,
        FILE_ATTRIBUTE_COMPRESSED      = 0x00000800,
        FILE_ATTRIBUTE_OFFLINE         = 0x00001000,
        FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x00002000,
        FILE_ATTRIBUTE_ENCRYPTED        = 0x00004000,
    }

    /// <summary>
    /// Flags controlling how the Image List item is 
    /// drawn
    /// </summary>
    [Flags]
    public enum IMAGELISTDRAWFLAGS : int
    {
        /// <summary>
        /// Draw item normally.
        /// </summary>
        ILD_NORMAL = 0x0,
        /// <summary>
        /// Draw item transparently.
        /// </summary>
        ILD_TRANSPARENT = 0x1,
        /// <summary>
        /// Draw item blended with 25% of the specified foreground colour
        /// or the Highlight colour if no foreground colour specified.
        /// </summary>
        ILD_BLEND25 = 0x2,
        /// <summary>
        /// Draw item blended with 50% of the specified foreground colour
        /// or the Highlight colour if no foreground colour specified.
        /// </summary>
        ILD_SELECTED = 0x4,
        /// <summary>
        /// Draw the icon's mask
        /// </summary>
        ILD_MASK = 0x10,
        /// <summary>
        /// Draw the icon image without using the mask
        /// </summary>
        ILD_IMAGE = 0x20,
        /// <summary>
        /// Draw the icon using the ROP specified.
        /// </summary>
        ILD_ROP = 0x40,
        /// <summary>
        /// Preserves the alpha channel in dest. XP only.
        /// </summary>
        ILD_PRESERVEALPHA = 0x1000,
        /// <summary>
        /// Scale the image to cx, cy instead of clipping it.  XP only.
        /// </summary>
        ILD_SCALE = 0x2000,
        /// <summary>
        /// Scale the image to the current DPI of the display. XP only.
        /// </summary>
        ILD_DPISCALE = 0x4000
    }

    /// <summary>
    /// Enumeration containing XP ImageList Draw State options
    /// </summary>
    [Flags]
    public enum IMAGELISTSTATEFLAGS : int
    {
        /// <summary>
        /// The image state is not modified. 
        /// </summary>
        ILS_NORMAL = (0x00000000),
        /// <summary>
        /// Adds a glow effect to the icon, which causes the icon to appear to glow 
        /// with a given color around the edges. (Note: does not appear to be
        /// implemented)
        /// </summary>
        ILS_GLOW = (0x00000001), //The color for the glow effect is passed to the IImageList::Draw method in the crEffect member of IMAGELISTDRAWPARAMS. 
        /// <summary>
        /// Adds a drop shadow effect to the icon. (Note: does not appear to be implemented)
        /// </summary>
        ILS_SHADOW = (0x00000002), //The color for the drop shadow effect is passed to the IImageList::Draw method in the crEffect member of IMAGELISTDRAWPARAMS. 
        /// <summary>
        /// Saturates the icon by increasing each color component 
        /// of the RGB triplet for each pixel in the icon. (Note: only ever appears to result in a completely unsaturated icon)
        /// </summary>
        ILS_SATURATE = (0x00000004), // The amount to increase is indicated by the frame member in the IMAGELISTDRAWPARAMS method. 
        /// <summary>
        /// Alpha blends the icon. Alpha blending controls the transparency 
        /// level of an icon, according to the value of its alpha channel. 
        /// (Note: does not appear to be implemented).
        /// </summary>
        ILS_ALPHA = (0x00000008) //The value of the alpha channel is indicated by the frame member in the IMAGELISTDRAWPARAMS method. The alpha channel can be from 0 to 255, with 0 being completely transparent, and 255 being completely opaque. 
    }

    /// <summary>
    /// Available system image list sizes
    /// </summary>
    public enum SHIL
    {
        /// <summary>
        /// The image size is normally 32x32 pixels. However, if the Use large icons option is selected from the Effects 
        /// section of the Appearance tab in Display Properties, the image is 48x48 pixels.
        /// </summary>
        SHIL_LARGE = 0x0,

        /// <summary>
        /// These images are the Shell standard small icon size of 16x16, but the size can be customized by the user.
        /// </summary>
        SHIL_SMALL = 0x1,

        /// <summary>
        /// These images are the Shell standard extra-large icon size. This is typically 48x48, but the size can be customized by the user.
        /// </summary>
        SHIL_EXTRALARGE = 0x2,

        /// <summary>
        /// These images are the size specified by GetSystemMetrics called with SM_CXSMICON and GetSystemMetrics called with SM_CYSMICON.
        /// </summary>
        SHIL_SYSSMALL = 0X3,

        /// <summary>
        /// Windows Vista and later. The image is normally 256x256 pixels.
        /// </summary>
        SHIL_JUMBO = 0x4
    }

    public enum SIGDN : uint
    {
        NORMALDISPLAY = 0,
        PARENTRELATIVEPARSING = 0x80018001,
        PARENTRELATIVEFORADDRESSBAR = 0x8001c001,
        DESKTOPABSOLUTEPARSING = 0x80028000,
        PARENTRELATIVEEDITING = 0x80031001,
        DESKTOPABSOLUTEEDITING = 0x8004c000,
        FILESYSPATH = 0x80058000,
        URL = 0x80068000
    }

    [Flags]
    public enum SIIGBF
    {
        SIIGBF_RESIZETOFIT = 0x00,
        SIIGBF_BIGGERSIZEOK = 0x01,
        SIIGBF_MEMORYONLY = 0x02,
        SIIGBF_ICONONLY = 0x04,
        SIIGBF_THUMBNAILONLY = 0x08,
        SIIGBF_INCACHEONLY = 0x10,
    }

    public enum SysCommands
    {
        SC_SIZE = 0xF000,
        SC_MOVE = 0xF010,
        SC_MINIMIZE = 0xF020,
        SC_MAXIMIZE = 0xF030,
        SC_NEXTWINDOW = 0xF040,
        SC_PREVWINDOW = 0xF050,
        SC_CLOSE = 0xF060,
        SC_VSCROLL = 0xF070,
        SC_HSCROLL = 0xF080,
        SC_MOUSEMENU = 0xF090,
        SC_KEYMENU = 0xF100,
        SC_ARRANGE = 0xF110,
        SC_RESTORE = 0xF120,
        SC_TASKLIST = 0xF130,
        SC_SCREENSAVE = 0xF140,
        SC_HOTKEY = 0xF150,
        //#if(WINVER >= 0x0400) //Win95
        SC_DEFAULT = 0xF160,
        SC_MONITORPOWER = 0xF170,
        SC_CONTEXTHELP = 0xF180,
        SC_SEPARATOR = 0xF00F,
        //#endif /* WINVER >= 0x0400 */

        //#if(WINVER >= 0x0600) //Vista
        SCF_ISSECURE = 0x00000001,
        //#endif /* WINVER >= 0x0600 */

        /*
          * Obsolete names
          */
        SC_ICON = SC_MINIMIZE,
        SC_ZOOM = SC_MAXIMIZE,
    }

    public enum MonitorOptions
    {
        MONITOR_DEFAULTTONULL = 0x00000000,
        MONITOR_DEFAULTTOPRIMARY = 0x00000001,
        MONITOR_DEFAULTTONEAREST = 0x00000002
    }

    [Flags]
    public enum FileDescriptorFlags : uint
    {
        FD_CLSID = 0x00000001,
        FD_SIZEPOINT = 0x00000002,
        FD_ATTRIBUTES = 0x00000004,
        FD_CREATETIME = 0x00000008,
        FD_ACCESSTIME = 0x00000010,
        FD_WRITESTIME = 0x00000020,
        FD_FILESIZE = 0x00000040,
        FD_PROGRESSUI = 0x00004000,
        FD_LINKUI = 0x00008000,
    }

    [Flags]
    public enum GlobalMemoryFlags : uint
    {
        GMEM_FIXED = 0x0000,
        GMEM_MOVEABLE = 0x0002,
        GMEM_ZEROINIT = 0x0040,
        GMEM_MODIFY = 0x0080,
        GMEM_VALID_FLAGS = 0x7F72,
        GMEM_INVALID_HANDLE = 0x8000,
        GHND = (GMEM_MOVEABLE | GMEM_ZEROINIT),
        GPTR = (GMEM_FIXED | GMEM_ZEROINIT),

        /*The following values are obsolete, but are provided for compatibility with 16-bit Windows. They are ignored.*/
        GMEM_DDESHARE = 0x2000,
        GMEM_DISCARDABLE = 0x0100,
        GMEM_LOWER = GMEM_NOT_BANKED,
        GMEM_NOCOMPACT = 0x0010,
        GMEM_NODISCARD = 0x0020,
        GMEM_NOT_BANKED = 0x1000,
        GMEM_NOTIFY = 0x4000,
        GMEM_SHARE = 0x2000
    }

    [Flags]
    public enum NID : uint
    {
        TABLET_CONFIG_NONE = 0x00000000,    // The input digitizer does not have touch capabilities. 
        NID_INTEGRATED_TOUCH = 0x00000001,  // An integrated touch digitizer is used for input. 
        NID_EXTERNAL_TOUCH = 0x00000002,    // An external touch digitizer is used for input. 
        NID_INTEGRATED_PEN = 0x00000004,    // An integrated pen digitizer is used for input. 
        NID_EXTERNAL_PEN = 0x00000008,      // An external pen digitizer is used for input. 
        NID_MULTI_INPUT = 0x00000040,       // An input digitizer with support for multiple inputs is used for input. 
        NID_READY = 0x00000080,             // The input digitizer is ready for input. If this value is unset, it may mean that the 
                                            // tablet service is stopped, the digitizer is not supported, or digitizer drivers have not been installed.
    }
}
