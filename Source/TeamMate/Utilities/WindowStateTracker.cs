using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Internal.Tools.TeamMate.Foundation.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace Microsoft.Internal.Tools.TeamMate.Utilities
{
    public class WindowStateTracker
    {
        private Dictionary<Type, WindowStateInfo> states = new Dictionary<Type, WindowStateInfo>();
        private static WindowStateTracker singleton;

        public static WindowStateTracker Instance
        {
            get
            {
                if (singleton == null)
                {
                    singleton = new WindowStateTracker();
                }

                return singleton;
            }
        }

        public Func<Window, WindowStateInfo> GetStoredStateDelegate { get; set; }

        public StoreStateMethod StoreStateDelegate { get; set; }

        public void Track(Window window)
        {
            Assert.ParamIsNotNull(window, "window");

            window.WhenLoaded(delegate ()
            {
                Attach(window);

                // On load, also set initial window size...
                SetSize(window);
            });
        }

        private void Attach(Window window)
        {
            window.LocationChanged += HandleWindowBoundsChanged;
            window.SizeChanged += HandleWindowBoundsChanged;
            window.IsVisibleChanged += HandleWindowIsVisibleChanged;
            window.Closed += HandleWindowClosed;
        }

        private void Detach(Window window)
        {
            window.LocationChanged -= HandleWindowBoundsChanged;
            window.SizeChanged -= HandleWindowBoundsChanged;
            window.IsVisibleChanged -= HandleWindowIsVisibleChanged;
            window.Closed -= HandleWindowClosed;
        }

        private void SetSize(Window window)
        {
            var lastState = GetState(window);

            if (lastState != null)
            {
                lastState.Apply(window);
            }
            else
            {
                // TODO: Ideally we don't do this here globally... One problem is, however, that maximizing doesn't work well until
                // we are actually visible
                WindowUtilities.SizeToPrimaryWorkArea(window);
                window.WindowState = WindowState.Maximized;
            }
        }

        public WindowStateInfo GetState(Window window)
        {
            var lastState = GetCachedState(window);
            if (lastState == null && GetStoredStateDelegate != null)
            {
                lastState = GetStoredStateDelegate(window);
            }

            return lastState;
        }

        public void StoreCurrentState(Window window)
        {
            WindowStateInfo state = WindowStateInfo.Capture(window);
            if (state != null)
            {
                StoreState(window, state);
            }
        }

        public void StoreState(Window window, WindowStateInfo state)
        {
            if (StoreStateDelegate != null)
            {
                Debug.WriteLine(String.Format("Storing state for window of type {0}", window.GetType().FullName));
                StoreStateDelegate(window, state);
            }
        }

        private void HandleWindowIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Window window = (Window)sender;

            if (!window.IsVisible)
            {
                WindowStateInfo state = GetCachedState(window);
                if (state != null)
                {
                    StoreState(window, state);
                }
            }
        }

        private void CacheState(Window window)
        {
            states[window.GetType()] = WindowStateInfo.Capture(window);
        }

        private WindowStateInfo GetCachedState(Window window)
        {
            WindowStateInfo state;
            states.TryGetValue(window.GetType(), out state);
            return state;
        }

        private void HandleWindowClosed(object sender, EventArgs e)
        {
            Window window = (Window)sender;
            Detach(window);
        }

        private void HandleWindowBoundsChanged(object sender, EventArgs e)
        {
            Window window = (Window)sender;
            CacheState(window);
        }
    }


    public delegate void StoreStateMethod(Window window, WindowStateInfo state);
}
