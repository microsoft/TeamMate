// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.Tools.TeamMate.Foundation.Native
{
    // Defines struct mappings for PInvoke functions.

    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWINFO
    {
        public uint cbSize;
        public RECT rcWindow;
        public RECT rcClient;
        public uint dwStyle;
        public uint dwExStyle;
        public uint dwWindowStatus;
        public uint cxWindowBorders;
        public uint cyWindowBorders;
        public ushort atomWindowType;
        public ushort wCreatorVersion;

        public static WINDOWINFO Create()
        {
            WINDOWINFO info = new WINDOWINFO();
            info.cbSize = WINDOWINFO.Size;
            return info;
        }

        public static uint Size
        {
            get
            {   
                return (uint)Marshal.SizeOf(typeof(WINDOWINFO));
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public override bool Equals(object obj)
        {
            if (obj is POINT)
            {
                POINT other = (POINT)obj;
                return (this.X == other.X && this.Y == other.Y);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.X ^ this.Y;
        }

        public override string ToString()
        {
            return ((System.Drawing.Point)this).ToString();
        }

        public static implicit operator System.Drawing.Point(POINT p)
        {
            return new System.Drawing.Point(p.X, p.Y);
        }

        public static implicit operator POINT(System.Drawing.Point p)
        {
            return new POINT((int)p.X, (int)p.Y);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int x, int y, int width, int height)
        {
            this.Left = x;
            this.Top = y;
            this.Right = x + width;
            this.Bottom = y + height;
        }

        public int Width
        {
            get
            {
                return Math.Abs((int)(this.Right - this.Left));
            }
        }

        public int Height
        {
            get
            {
                return Math.Abs((int)(this.Bottom - this.Top));
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is RECT)
            {
                RECT other = (RECT)obj;
                return (this.Left == other.Left && this.Top == other.Top && this.Right == other.Right && this.Bottom == other.Bottom);
            }

            return false;
        }

        public override string ToString()
        {
            return ((System.Windows.Int32Rect)this).ToString();
        }

        public override int GetHashCode()
        {
            return this.Left ^ this.Top ^ this.Right ^ this.Bottom;
        }

        public static implicit operator System.Drawing.Rectangle(RECT r)
        {
            return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
        }

        public static implicit operator RECT(System.Drawing.Rectangle r)
        {
            return new RECT(r.X, r.Y, r.Width, r.Height);
        }

        public static implicit operator System.Windows.Int32Rect(RECT r)
        {
            return new System.Windows.Int32Rect(r.Left, r.Top, r.Width, r.Height);
        }

        public static implicit operator RECT(System.Windows.Int32Rect r)
        {
            return new RECT(r.X, r.Y, r.Width, r.Height);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SIZE
    {
        public int Width;
        public int Height;

        public SIZE(int width, int height)
            : this()
        {
            if (width < 0)
            {
                throw new ArgumentOutOfRangeException("width");
            }

            if (height < 0)
            {
                throw new ArgumentOutOfRangeException("height");
            }

            Width = width;
            Height = height;
        }

        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "({0}, {1})", Width, Height);
        }
    }

    /// <summary>
    /// ANIMATIONINFO specifies animation effects associated with user actions. 
    /// Used with SystemParametersInfo when SPI_GETANIMATION or SPI_SETANIMATION action is specified.
    /// </summary>
    /// <remark>
    /// The uiParam value must be set to (System.UInt32)Marshal.SizeOf(typeof(ANIMATIONINFO)) when using this structure.
    /// </remark>
    [StructLayout(LayoutKind.Sequential)]
    public struct ANIMATIONINFO
    {
        /// <summary>
        /// Always must be set to (System.UInt32)Marshal.SizeOf(typeof(ANIMATIONINFO)).
        /// </summary>
        public uint cbSize;

        /// <summary>
        /// If non-zero, minimize/restore animation is enabled, otherwise disabled.
        /// </summary>
        public int iMinAnimate;

        public bool IsAnimationEnabled
        {
            get { return (this.iMinAnimate != 0); }
            set { this.iMinAnimate = (value) ? 1 : 0; }
        }

        /// <summary>
        /// Creates an AMINMATIONINFO structure.
        /// </summary>
        public static ANIMATIONINFO Create()
        {
            return Create(false);
        }

        public static ANIMATIONINFO Create(bool animationEnabled)
        {
            ANIMATIONINFO info = new ANIMATIONINFO();
            info.IsAnimationEnabled = animationEnabled;
            info.cbSize = ANIMATIONINFO.Size;
            return info;
        }

        public static uint Size
        {
            get
            {
                return (uint) Marshal.SizeOf(typeof(ANIMATIONINFO));
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;

        public static uint Size
        {
            get
            {
                return (uint)Marshal.SizeOf(typeof(SHFILEINFO));
            }
        }
    }

    [StructLayout(LayoutKind.Sequential), SuppressUnmanagedCodeSecurity]
    public class PROCESS_INFORMATION
    {
        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        public IntPtr hProcess;
        public IntPtr hThread;
        public int dwProcessId;
        public int dwThreadId;

        ~PROCESS_INFORMATION()
        {
            this.Close();
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal void Close()
        {
            if ((this.hProcess != IntPtr.Zero) && (this.hProcess != INVALID_HANDLE_VALUE))
            {
                NativeMethods.CloseHandle(new HandleRef(this, this.hProcess));
                this.hProcess = INVALID_HANDLE_VALUE;
            }

            if ((this.hThread != IntPtr.Zero) && (this.hThread != INVALID_HANDLE_VALUE))
            {
                NativeMethods.CloseHandle(new HandleRef(this, this.hThread));
                this.hThread = INVALID_HANDLE_VALUE;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGELISTDRAWPARAMS
    {
        public int cbSize;
        public IntPtr himl;
        public int i;
        public IntPtr hdcDst;
        public int x;
        public int y;
        public int cx;
        public int cy;
        public int xBitmap;        
        public int yBitmap;        
        public int rgbBk;
        public int rgbFg;
        public int fStyle;
        public int dwRop;
        public int fState;
        public int Frame;
        public int crEffect;

        public static int Size
        {
            get
            {
                return Marshal.SizeOf(typeof(IMAGELISTDRAWPARAMS));
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGEINFO
    {
        public IntPtr hbmImage;
        public IntPtr hbmMask;
        public int Unused1;
        public int Unused2;
        public RECT rcImage;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        private IntPtr hwnd;
        public int message;
        private IntPtr wParam;
        private IntPtr lParam;
        public int time;
        public int pt_x;
        public int pt_y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct MARGINS
    {
        public Int32 LeftWidth;
        public Int32 RightWidth;
        public Int32 TopHeight;
        public Int32 BottomHeight;

        public MARGINS(int size) : this(size, size, size, size)
        {
        }

        public MARGINS(int left, int top, int right, int bottom)
        {
            this.LeftWidth = left;
            this.TopHeight = top;
            this.RightWidth = right;
            this.BottomHeight = bottom;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public sealed class MONITORINFO
    {
        public Int32 cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public Int32 dwFlags;

        public MONITORINFO()
        {
            cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            rcMonitor = new RECT();
            rcWork = new RECT();
            dwFlags = 0;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct FILEDESCRIPTOR
    {
        public uint dwFlags;
        public Guid clsid;
        public System.Drawing.Size sizel;
        public System.Drawing.Point pointl;
        public uint dwFileAttributes;
        public FILETIME ftCreationTime;
        public FILETIME ftLastAccessTime;
        public FILETIME ftLastWriteTime;
        public uint nFileSizeHigh;
        public uint nFileSizeLow;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string cFileName;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ICONINFO
    {
        public bool fIcon;
        public int xHotspot;
        public int yHotspot;
        public IntPtr hbmMask;
        public IntPtr hbmColor;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;

        public MEMORYSTATUSEX()
        {
            this.dwLength = (uint)Marshal.SizeOf(this);
        }
    }

    /// <summary>
    /// The SECURITY_ATTRIBUTES structure contains the security descriptor for 
    /// an object and specifies whether the handle retrieved by specifying 
    /// this structure is inheritable. This structure provides security 
    /// settings for objects created by various functions, such as CreateFile, 
    /// CreateNamedPipe, CreateProcess, RegCreateKeyEx, or RegSaveKeyEx.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class SECURITY_ATTRIBUTES
    {
        public int nLength;
        public IntPtr lpSecurityDescriptor;
        public bool bInheritHandle;

        public SECURITY_ATTRIBUTES()
        {
            this.nLength = (int)Marshal.SizeOf(this);
        }
    }
}
