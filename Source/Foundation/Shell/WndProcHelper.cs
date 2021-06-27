using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.Windows;
using System.Windows.Interop;

namespace Microsoft.Tools.TeamMate.Foundation.Shell
{
    /// <summary>
    /// A helper class that allows to easily register for and process native WndProc events.
    /// </summary>
    public abstract class WndProcHelper : IDisposable
    {
        private Window window;
        private IntPtr hWnd;
        private HwndSource source;
        private HwndSourceHook sourceHook;
        private bool ownsWindow;

        /// <summary>
        /// Initializes a new instance of the <see cref="WndProcHelper"/> class, creating
        /// a fake invisible window as the source of window event introspection.
        /// </summary>
        protected WndProcHelper()
            : this(CreateFakeWindow())
        {
            this.ownsWindow = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WndProcHelper"/> class.
        /// </summary>
        /// <param name="window">Reuses the given window as the source of window event introspection.</param>
        protected WndProcHelper(Window window)
        {
            Assert.ParamIsNotNull(window, "window");

            this.window = window;
            this.window.Loaded += HandleWindowLoaded;
            this.window.Unloaded += HandleWindowUnloaded;

            if (window.IsLoaded)
            {
                Register();
            }
        }

        /// <summary>
        /// Gets the window.
        /// </summary>
        public Window Window
        {
            get { return this.window; }
        }

        /// <summary>
        /// Gets the window handle.
        /// </summary>
        public IntPtr Hwnd
        {
            get { return this.hWnd; }
        }

        /// <summary>
        /// Registers the appropriate hooks so that WndProc will be invoked.
        /// </summary>
        private void Register()
        {
            if (source == null)
            {
                this.hWnd = new WindowInteropHelper(window).Handle;
                this.source = HwndSource.FromHwnd(hWnd);
                this.sourceHook = new HwndSourceHook(WndProc);
                source.AddHook(sourceHook);

                OnRegistered();
            }
        }

        /// <summary>
        /// Unregisters any previously registered hooks so that WndProc will no longer be invoked.
        /// </summary>
        private void Unregister()
        {
            if (source != null)
            {
                OnUnregistered();

                this.source.RemoveHook(sourceHook);
                this.source.Dispose();
                this.source = null;

                this.sourceHook = null;
                this.hWnd = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Invoked when the source window has been loaded and any registration should occur.
        /// </summary>
        protected virtual void OnRegistered()
        {
        }

        /// <summary>
        /// Invoked when the source window has been unloaded and any unregistration should occur.
        /// </summary>
        protected virtual void OnUnregistered()
        {
        }

        /// <summary>
        /// WNDs the proc.
        /// </summary>
        /// <param name="hwnd">A handle to the window</param>
        /// <param name="msg">The message .</param>
        /// <param name="wParam">Additional message information.</param>
        /// <param name="lParam">Additional message information.</param>
        /// <param name="handled">set to <c>true</c> to indicate that a message has been handled and needs no further processing.</param>
        /// <returns>The value of the procesed message (Zero means unhandled).</returns>
        protected virtual IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            return IntPtr.Zero;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Unregister();

            if (this.window != null)
            {
                this.window.Loaded -= HandleWindowLoaded;
                this.window.Unloaded -= HandleWindowUnloaded;

                if (this.ownsWindow)
                {
                    this.window.Close();
                }

                this.window = null;
            }
        }

        /// <summary>
        /// Creates the fake invisible window to be used when no windows is passed in by default.
        /// </summary>
        /// <returns>The fake invisible window to be used.</returns>
        private static Window CreateFakeWindow()
        {
            Window fakeWindow = new Window();
            fakeWindow.ShowInTaskbar = false;
            fakeWindow.ShowActivated = false;
            fakeWindow.WindowStyle = WindowStyle.None;
            fakeWindow.Width = 0;
            fakeWindow.Height = 0;

            // Hide the window immediately when displayed, this is a fake window!
            RoutedEventHandler loaded = null;
            loaded = delegate(object sender, RoutedEventArgs e)
            {
                fakeWindow.Loaded -= loaded;
                fakeWindow.Hide();
            };

            fakeWindow.Loaded += loaded;

            fakeWindow.Show();

            return fakeWindow;
        }

        private void HandleWindowUnloaded(object sender, RoutedEventArgs e)
        {
            Unregister();
        }

        private void HandleWindowLoaded(object sender, RoutedEventArgs e)
        {
            Register();
        }
    }
}
