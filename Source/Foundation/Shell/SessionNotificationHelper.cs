using Microsoft.Internal.Tools.TeamMate.Foundation.Native;
using System;
using System.Windows;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Shell
{
    /// <summary>
    /// Provides access to interactive session events to indicate when a user has locked their console or unlocked it.
    /// </summary>
    public class SessionNotificationHelper : WndProcHelper
    {
        // constants that can be passed for the dwFlags parameter
        private const int NOTIFY_FOR_THIS_SESSION = 0;
        private const int NOTIFY_FOR_ALL_SESSIONS = 1;

        // message id to look for when processing the message (see sample code)
        private const int WM_WTSSESSION_CHANGE = 0x2b1;

        // From: http://msdn2.microsoft.com/en-us/library/aa383828.aspx

        // WParam values that can be received: 
        private const int WTS_CONSOLE_CONNECT = 0x1; // A session was connected to the console terminal.
        private const int WTS_CONSOLE_DISCONNECT = 0x2; // A session was disconnected from the console terminal.
        private const int WTS_REMOTE_CONNECT = 0x3; // A session was connected to the remote terminal.
        private const int WTS_REMOTE_DISCONNECT = 0x4; // A session was disconnected from the remote terminal.
        private const int WTS_SESSION_LOGON = 0x5; // A user has logged on to the session.
        private const int WTS_SESSION_LOGOFF = 0x6; // A user has logged off the session.
        private const int WTS_SESSION_LOCK = 0x7; // A session has been locked.
        private const int WTS_SESSION_UNLOCK = 0x8; // A session has been unlocked.
        private const int WTS_SESSION_REMOTE_CONTROL = 0x9; // A session has changed its remote controlled status.

        public event EventHandler SessionLocked;
        public event EventHandler SessionUnlocked;

        private bool sessionLocked;

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionNotificationHelper"/> class, creating
        /// a fake invisible window as the source of window event introspection.
        /// </summary>
        public SessionNotificationHelper()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionNotificationHelper"/> class.
        /// </summary>
        /// <param name="window">Reuses the given window as the source of window event introspection.</param>
        public SessionNotificationHelper(Window window)
            : base(window)
        {
        }

        /// <summary>
        /// Gets a value indicating whether the session is currently locked.
        /// </summary>
        public bool IsSessionLocked
        {
            get { return this.sessionLocked; }
        }

        /// <summary>
        /// Invoked when the source window has been loaded and any registration should occur.
        /// </summary>
        protected override void OnRegistered()
        {
            base.OnRegistered();

            // KLUDGE: WPF registers for this event already by default, so a second registration makes us receive the event twice.
            // Unregister any original registration and reregister just to be on the safe side.
            // http://juank.black-byte.com/csharp-notificacion-cambio-sesion-wpf/
            NativeMethods.WTSUnRegisterSessionNotification(Hwnd);
            NativeMethods.WTSRegisterSessionNotification(Hwnd, NOTIFY_FOR_THIS_SESSION);
        }

        /// <summary>
        /// Invoked when the source window has been unloaded and any unregistration should occur.
        /// </summary>
        protected override void OnUnregistered()
        {
            base.OnUnregistered();
            NativeMethods.WTSUnRegisterSessionNotification(Hwnd);
        }

        /// <summary>
        /// WNDs the proc.
        /// </summary>
        /// <param name="hwnd">A handle to the window</param>
        /// <param name="msg">The message .</param>
        /// <param name="wParam">Additional message information.</param>
        /// <param name="lParam">Additional message information.</param>
        /// <param name="handled">set to <c>true</c> to indicate that a message has been handled and needs no further processing.</param>
        /// <returns>
        /// The value of the procesed message (Zero means unhandled).
        /// </returns>
        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // check for session change notifications
            if (msg == WM_WTSSESSION_CHANGE)
            {
                int param = wParam.ToInt32();

                if (param == WTS_SESSION_LOCK)
                {
                    sessionLocked = true;

                    SessionLocked?.Invoke(this, EventArgs.Empty);

                    handled = true;
                }
                else if (param == WTS_SESSION_UNLOCK)
                {
                    sessionLocked = false;

                    SessionUnlocked?.Invoke(this, EventArgs.Empty);

                    handled = true;
                }
            }

            return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }
    }
}
