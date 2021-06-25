using Microsoft.Internal.Tools.TeamMate.Foundation.Native;
using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.DragAndDrop;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Controls
{
    /// <summary>
    /// Represents a metro-styled window, without chrome.
    /// </summary>
    public class RibbonWindow : Window
    {
        public const uint TPM_RETURNCMD = 0x0100;
        public const uint TPM_LEFTBUTTON = 0x0;

        private MouseButtonEventArgs mouseDownEvent;
        private Point initialPosition;
        private DateTime? previousHeaderClickTime;

        static RibbonWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonWindow), new FrameworkPropertyMetadata(typeof(RibbonWindow)));
        }

        /// <summary>
        /// The background image property
        /// </summary>
        public static readonly DependencyProperty BackgroundImageProperty = DependencyProperty.Register(
            "BackgroundImage", typeof(ImageSource), typeof(RibbonWindow)
        );

        /// <summary>
        /// Gets or sets the background image.
        /// </summary>
        public ImageSource BackgroundImage
        {
            get { return (ImageSource)GetValue(BackgroundImageProperty); }
            set { SetValue(BackgroundImageProperty, value); }
        }

        /// <summary>
        /// The header left content property
        /// </summary>
        public static readonly DependencyProperty HeaderLeftContentProperty = DependencyProperty.Register(
            "HeaderLeftContent", typeof(object), typeof(RibbonWindow)
        );

        /// <summary>
        /// Gets or sets the content of the header left.
        /// </summary>
        /// <value>
        /// The content of the header left.
        /// </value>
        public object HeaderLeftContent
        {
            get { return (object)GetValue(HeaderLeftContentProperty); }
            set { SetValue(HeaderLeftContentProperty, value); }
        }

        /// <summary>
        /// Overridden to capture template parts.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ButtonBase minimizeButton = GetTemplateChild("PART_MinimizeButton") as ButtonBase;
            ButtonBase maximizeButton = GetTemplateChild("PART_MaximizeButton") as ButtonBase;
            ButtonBase restoreButton = GetTemplateChild("PART_RestoreButton") as ButtonBase;
            ButtonBase closeButton = GetTemplateChild("PART_CloseButton") as ButtonBase;
            UIElement header = GetTemplateChild("PART_Header") as UIElement;
            UIElement windowIconImage = GetTemplateChild("PART_WindowIconImage") as UIElement;

            if (minimizeButton != null)
            {
                minimizeButton.Click += ((s, e) => WindowState = WindowState.Minimized);
            }

            if (maximizeButton != null)
            {
                maximizeButton.Click += ((s, e) => WindowState = WindowState.Maximized);
            }

            if (restoreButton != null)
            {
                restoreButton.Click += ((s, e) => WindowState = WindowState.Normal);
            }

            if (closeButton != null)
            {
                closeButton.Click += ((s, e) => Close());
            }

            if (header != null)
            {
                header.MouseLeftButtonDown += HandleHeaderMouseLeftButtonDown;
                header.MouseMove += HandleHeaderMouseMove;
                header.MouseLeftButtonUp += HandleHeaderMouseLeftButonUp;
                header.MouseRightButtonUp += HandleHeaderMouseRightButtonUp;
            }

            if (windowIconImage != null)
            {
                windowIconImage.MouseLeftButtonDown += HandleWindowIconMouseLeftButtonDown;
            }
        }

        /// <summary>
        /// Begins the drag operation of the window header.
        /// </summary>
        /// <param name="header">The header.</param>
        private void BeginDrag(FrameworkElement header)
        {
            // Make sure we are not maximized if we wan't to start moving the window
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;

                // Make sure the mouse is positioned over the 
                Point cursor = WindowUtilities.ScreenCursorPosition;
                Point headerCursor = header.PointFromScreen(cursor);
                if (headerCursor.X < 0 || headerCursor.Y < 0 || headerCursor.X > header.ActualWidth || headerCursor.Y > header.ActualHeight)
                {
                    // After restoring the state, the cursor is no longer on top of the header, we need to reposition window so that it is on top of the
                    var bounds = WindowUtilities.GetBoundsForScreenContaining(this);

                    this.Top = bounds.Top;
                    this.Left = cursor.X - (this.ActualWidth / 2);

                    WindowUtilities.EnsureWithinBounds(this, bounds);
                }
            }

            DragMove();
        }

        /// <summary>
        /// Shows the system menu at the current cursor position.
        /// </summary>
        private void ShowSystemMenu()
        {
            POINT p;
            NativeMethods.GetPhysicalCursorPos(out p);

            var hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd == IntPtr.Zero || !NativeMethods.IsWindow(hwnd))
            {
                return;
            }

            var hmenu = NativeMethods.GetSystemMenu(hwnd, false);
            var cmd = NativeMethods.TrackPopupMenuEx(hmenu, TPM_LEFTBUTTON | TPM_RETURNCMD, p.X, p.Y, hwnd, IntPtr.Zero);
            if (cmd != 0)
            {
                NativeMethods.PostMessage(hwnd, (uint)WindowsMessage.WM_SYSCOMMAND, new IntPtr((int)cmd), IntPtr.Zero);
            }
        }
        /// <summary>
        /// Determines whether the mouse event is on a disabled UI element.
        /// </summary>
        private bool IsOnDisabledUiElement(MouseButtonEventArgs e)
        {
            var hitTestResult = VisualTreeHelper.HitTest(this, e.GetPosition(this));
            UIElement uiElement = hitTestResult.VisualHit as UIElement;
            return (uiElement != null && !uiElement.IsEnabled);
        }

        /// <summary>
        /// Determines whether if a click event counts as a double click event.
        /// </summary>
        /// <param name="previousClickTime">The previous click time.</param>
        private static bool IsDoubleClick(DateTime? previousClickTime)
        {
            return previousClickTime != null && DateTime.Now.Subtract(previousClickTime.Value).TotalMilliseconds <= System.Windows.Forms.SystemInformation.DoubleClickTime;
        }

        /// <summary>
        /// Handles the window icon mouse left button down.
        /// </summary>
        private void HandleWindowIconMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowSystemMenu();
            e.Handled = true;
        }

        /// <summary>
        /// Handles the header mouse right button up.
        /// </summary>
        private void HandleHeaderMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowSystemMenu();
            e.Handled = true;
        }

        /// <summary>
        /// Handles the header mouse left button down.
        /// </summary>
        private void HandleHeaderMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsOnDisabledUiElement(e))
            {
                // Handle the scenario where we are clicking on a disabled e.g. quick toolbar button in the header
                // Simply ignore the event
                this.previousHeaderClickTime = null;
                this.mouseDownEvent = null;
                e.Handled = true;
            }

            if (IsDoubleClick(this.previousHeaderClickTime))
            {
                WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
                this.previousHeaderClickTime = null;
                this.mouseDownEvent = null;
                e.Handled = true;
            }
            else
            {
                this.previousHeaderClickTime = DateTime.Now;
                this.mouseDownEvent = e;
                this.initialPosition = e.GetPosition((IInputElement)e.Source);
            }
        }


        /// <summary>
        /// Handles the header mouse move.
        /// </summary>
        private void HandleHeaderMouseMove(object sender, MouseEventArgs e)
        {
            if (this.mouseDownEvent != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition((IInputElement)e.Source);
                if (DragDropUtilities.ShouldBeginDrag(this.initialPosition, currentPosition))
                {
                    this.mouseDownEvent = null;
                    BeginDrag((FrameworkElement)e.Source);
                }
            }
        }

        /// <summary>
        /// Handles the header mouse left buton up.
        /// </summary>
        private void HandleHeaderMouseLeftButonUp(object sender, MouseButtonEventArgs e)
        {
            this.mouseDownEvent = null;
        }
    }
}
