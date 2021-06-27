using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Controls
{
    /// <summary>
    /// A control to display floating callouts.
    /// </summary>
    /// <remarks>
    /// Inspired by Outlook's floating callout in e.g. the Calendar view.
    /// </remarks>
    public class Callout : ContentControl
    {
        private const double ExtraMargin = 5;
        private const double TipSize = 20;
        private const double MinTipOffset = 10;

        private const double PlacementDistanceFromCursor = 20;
        private const double PlacementDistanceFromScreenBoundary = 5;

        static Callout()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Callout), new FrameworkPropertyMetadata(typeof(Callout)));
        }

        private static readonly TimeSpan CloseDelay = TimeSpan.FromSeconds(1);

        private DispatcherTimer openCalloutTimer;
        private DispatcherTimer closeCalloutTimer;
        private Popup calloutPopup;

        private static Callout currentlyOpenedCallout;

        /// <summary>
        /// Initializes a new instance of the <see cref="Callout"/> class.
        /// </summary>
        public Callout()
        {
            this.Loaded += HandleCalloutLoaded;
            this.MouseEnter += HandleCalloutMouseEnter;
            this.MouseLeave += HandleCalloutMouseLeave;
        }

        /// <summary>
        /// The initial show delay property
        /// </summary>
        public static readonly DependencyProperty InitialShowDelayProperty = DependencyProperty.Register(
            "InitialShowDelay", typeof(double), typeof(Callout), new PropertyMetadata(500d)
        );

        /// <summary>
        /// Gets or sets the initial show delay, in milliseconds.
        /// </summary>
        public double InitialShowDelay
        {
            get { return (double)GetValue(InitialShowDelayProperty); }
            set { SetValue(InitialShowDelayProperty, value); }
        }

        /// <summary>
        /// The placement property
        /// </summary>
        public static readonly DependencyProperty PlacementProperty = DependencyProperty.Register(
            "Placement", typeof(CalloutPlacement), typeof(Callout)
        );

        /// <summary>
        /// Gets or sets the placement.
        /// </summary>
        public CalloutPlacement Placement
        {
            get { return (CalloutPlacement)GetValue(PlacementProperty); }
            set { SetValue(PlacementProperty, value); }
        }

        /// <summary>
        /// The tip offset property
        /// </summary>
        public static readonly DependencyProperty TipOffsetProperty = DependencyProperty.Register(
            "TipOffset", typeof(double), typeof(Callout)
        );

        /// <summary>
        /// Gets or sets the tip offset.
        /// </summary>
        public double TipOffset
        {
            get { return (double)GetValue(TipOffsetProperty); }
            set { SetValue(TipOffsetProperty, value); }
        }

        /// <summary>
        /// The has drop shadow property
        /// </summary>
        public static readonly DependencyProperty HasDropShadowProperty = DependencyProperty.Register(
            "HasDropShadow", typeof(bool), typeof(Callout), new PropertyMetadata(true)
        );

        /// <summary>
        /// Gets or sets a value indicating whether [has drop shadow].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [has drop shadow]; otherwise, <c>false</c>.
        /// </value>
        public bool HasDropShadow
        {
            get { return (bool)GetValue(HasDropShadowProperty); }
            set { SetValue(HasDropShadowProperty, value); }
        }

        /// <summary>
        /// The is open property
        /// </summary>
        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
            "IsOpen", typeof(bool), typeof(Callout), new PropertyMetadata(OnIsOpenChanged)
        );

        /// <summary>
        /// Gets or sets a value indicating whether the callout is open or not.
        /// </summary>
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        /// <summary>
        /// Called when is open changed.
        /// </summary>
        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Callout)d).InvalidateIsOpen();
        }

        /// <summary>
        /// The placement target property
        /// </summary>
        public static readonly DependencyProperty PlacementTargetProperty = DependencyProperty.Register(
            "PlacementTarget", typeof(UIElement), typeof(Callout), new PropertyMetadata(OnPlacementTargetChanged)
        );

        /// <summary>
        /// Gets or sets the placement target.
        /// </summary>
        public UIElement PlacementTarget
        {
            get { return (UIElement)GetValue(PlacementTargetProperty); }
            set { SetValue(PlacementTargetProperty, value); }
        }

        /// <summary>
        /// Called when the placement target changed.
        /// </summary>
        private static void OnPlacementTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UIElement oldTarget = e.OldValue as UIElement;
            UIElement newTarget = e.NewValue as UIElement;
            Callout callout = (Callout)d;

            callout.InvalidatePlacementTarget(oldTarget, newTarget);
        }

        /// <summary>
        /// Invalidates the placement target.
        /// </summary>
        /// <param name="oldTarget">The old target.</param>
        /// <param name="newTarget">The new target.</param>
        private void InvalidatePlacementTarget(UIElement oldTarget, UIElement newTarget)
        {
            if (oldTarget != null)
            {
                FrameworkElement frameworkElement = oldTarget as FrameworkElement;
                if (frameworkElement != null)
                {
                    frameworkElement.Unloaded -= HandlePlacementTargetUnloaded;
                }

                oldTarget.MouseEnter -= HandlePlacementTargetMouseEnter;
                oldTarget.MouseLeave -= HandlePlacementTargetMouseLeave;
            }

            if (newTarget != null)
            {
                newTarget.MouseEnter += HandlePlacementTargetMouseEnter;
                newTarget.MouseLeave += HandlePlacementTargetMouseLeave;

                FrameworkElement frameworkElement = newTarget as FrameworkElement;
                if (frameworkElement != null)
                {
                    frameworkElement.Unloaded += HandlePlacementTargetUnloaded;
                }

                // TODO: Bugs? Who is unregistering all of these events? No dispose or anythign like that?
                Window window = Window.GetWindow(newTarget);
                if (window != null)
                {
                    window.Deactivated += HandleWindowDeactivated;
                }
            }
        }

        /// <summary>
        /// Closes the callout and stops monitoring for timing of gestures.
        /// </summary>
        private void CloseCalloutAndStopMonitoring()
        {
            IsOpen = false;
            StopOpenTimer();
            StopCloseTimer();
        }

        /// <summary>
        /// Gets the popup slide animation based on the placement.
        /// </summary>
        private Storyboard GetPopupSlideAnimation()
        {
            string slideAnimationName = null;

            switch (Placement)
            {
                case CalloutPlacement.Left:
                case CalloutPlacement.Right:
                    slideAnimationName = "HorizontalSlide";
                    break;

                case CalloutPlacement.Top:
                case CalloutPlacement.Bottom:
                    slideAnimationName = "VerticalSlide";
                    break;
            }

            if (slideAnimationName != null)
            {
                return (Storyboard)FindResource(slideAnimationName);
            }

            return null;
        }

        /// <summary>
        /// Starts the open timer to show the callout when the timer is done.
        /// </summary>
        private void StartOpenTimer()
        {
            if (!IsOpen)
            {
                openCalloutTimer = new DispatcherTimer();
                openCalloutTimer.ExecuteOnce(TimeSpan.FromMilliseconds(InitialShowDelay), delegate()
                {
                    StopOpenTimer();
                    IsOpen = true;
                });
            }
        }

        /// <summary>
        /// Stops the open timer if it was previously started.
        /// </summary>
        private void StopOpenTimer()
        {
            if (openCalloutTimer != null)
            {
                openCalloutTimer.Stop();
                openCalloutTimer = null;
            }
        }

        /// <summary>
        /// Starts the close timer to close the callout when the timer is done.
        /// </summary>
        private void StartCloseTimer()
        {
            if (IsOpen)
            {
                closeCalloutTimer = new DispatcherTimer();
                closeCalloutTimer.ExecuteOnce(CloseDelay, delegate()
                {
                    StopCloseTimer();
                    IsOpen = false;
                });
            }
        }

        /// <summary>
        /// Stops the close timer if it was previously started.
        /// </summary>
        private void StopCloseTimer()
        {
            if (closeCalloutTimer != null)
            {
                closeCalloutTimer.Stop();
                closeCalloutTimer = null;
            }
        }

        /// <summary>
        /// Invalidates the is open.
        /// </summary>
        private void InvalidateIsOpen()
        {
            if (IsOpen)
            {
                CalloutOpeningEventArgs openingEventArgs = new CalloutOpeningEventArgs();
                RaiseEvent(openingEventArgs);
                if (openingEventArgs.Handled)
                {
                    IsOpen = false;
                    return;
                }

                if (this.calloutPopup == null)
                {
                    this.calloutPopup = new Popup();
                    this.calloutPopup.AllowsTransparency = true;
                    this.calloutPopup.Child = this;
                    this.calloutPopup.Opened += HandlePopupOpened;
                }

                PositionPopupAtCursor();

                if (currentlyOpenedCallout != null)
                {
                    currentlyOpenedCallout.IsOpen = false;
                }

                this.calloutPopup.IsOpen = IsOpen;
                currentlyOpenedCallout = this;
            }
            else
            {
                if (this.calloutPopup != null && this.calloutPopup.IsOpen)
                {
                    this.calloutPopup.IsOpen = IsOpen;
                    if (currentlyOpenedCallout == this)
                    {
                        currentlyOpenedCallout = null;
                    }

                    StopOpenTimer();
                    StopCloseTimer();
                }
            }
        }

        /// <summary>
        /// The callout opening event
        /// </summary>
        public static readonly RoutedEvent CalloutOpeningEvent = EventManager.RegisterRoutedEvent(
            "CalloutOpeningEvent", RoutingStrategy.Bubble, typeof(CalloutOpeningEventHandler), typeof(Callout));

        /// <summary>
        /// Occurs when the callout is opening.
        /// </summary>
        public event CalloutOpeningEventHandler CalloutOpening
        {
            add { AddHandler(CalloutOpeningEvent, value); }
            remove { RemoveHandler(CalloutOpeningEvent, value); }
        }

        /// <summary>
        /// The callout opened event
        /// </summary>
        public static readonly RoutedEvent CalloutOpenedEvent = EventManager.RegisterRoutedEvent(
            "CalloutOpenedEvent", RoutingStrategy.Bubble, typeof(CalloutOpenedEventHandler), typeof(Callout));

        /// <summary>
        /// Occurs when callout is opened.
        /// </summary>
        public event CalloutOpenedEventHandler CalloutOpened
        {
            add { AddHandler(CalloutOpenedEvent, value); }
            remove { RemoveHandler(CalloutOpenedEvent, value); }
        }

        /// <summary>
        /// Positions the popup based on the current cursor position.
        /// </summary>
        private void PositionPopupAtCursor()
        {
            Point screenPoint = WindowUtilities.ScreenCursorPosition;
            CalculatePreferredPlacement(screenPoint);
            PositionPopup(screenPoint);
        }

        /// <summary>
        /// Calculates the preferred placement of the callout based on a screen point.
        /// </summary>
        /// <param name="screenPoint">The point.</param>
        private void CalculatePreferredPlacement(Point screenPoint)
        {
            CalloutPlacement[] preferredPlacements = GetPreferredPlacementOrder();

            var screenBounds = WindowUtilities.ScreenBoundsFromPoint(screenPoint);
            var screenSize = screenBounds.Size;

            foreach (var placement in preferredPlacements)
            {
                Placement = placement;

                var size = MeasureDesiredSize(screenSize);

                // TODO: Make this more sophisticated, measuring for BOTH dimensions in all cases
                if ((placement == CalloutPlacement.Right && screenPoint.X + PlacementDistanceFromCursor + size.Width < screenBounds.X + screenBounds.Width)
                    || (placement == CalloutPlacement.Left && screenPoint.X - PlacementDistanceFromCursor - size.Width >= screenBounds.X)
                    || (placement == CalloutPlacement.Bottom && screenPoint.Y + PlacementDistanceFromCursor + size.Height < screenBounds.Y + screenBounds.Height)
                    || (placement == CalloutPlacement.Top && screenPoint.Y - PlacementDistanceFromCursor - size.Height >= screenBounds.Y))
                {
                    // Found a tip placement (in order of preference) that would work...
                    return;
                }
            }

            Placement = CalloutPlacement.Center;
        }

        /// <summary>
        /// Gets a collection of placement orders, in order of preference.
        /// </summary>
        private CalloutPlacement[] GetPreferredPlacementOrder()
        {
            CalloutPlacement[] preferredPlacements;

            if (PreferredPlacement == CalloutPlacement.Left)
            {
                preferredPlacements = new CalloutPlacement[] {
                    CalloutPlacement.Left, CalloutPlacement.Right, CalloutPlacement.Bottom, CalloutPlacement.Top
                };
            }
            else if (PreferredPlacement == CalloutPlacement.Top)
            {
                preferredPlacements = new CalloutPlacement[] {
                    CalloutPlacement.Top, CalloutPlacement.Bottom, CalloutPlacement.Right, CalloutPlacement.Left
                };
            }
            else if (PreferredPlacement == CalloutPlacement.Bottom)
            {
                preferredPlacements = new CalloutPlacement[] {
                    CalloutPlacement.Bottom, CalloutPlacement.Top, CalloutPlacement.Right, CalloutPlacement.Left
                };
            }
            else
            {
                preferredPlacements = new CalloutPlacement[] {
                    CalloutPlacement.Right, CalloutPlacement.Left, CalloutPlacement.Bottom, CalloutPlacement.Top
                };
            }

            return preferredPlacements;
        }

        /// <summary>
        /// The preferred tip placement property
        /// </summary>
        public static readonly DependencyProperty PreferredPlacementProperty = DependencyProperty.Register(
            "PreferredPlacement", typeof(CalloutPlacement), typeof(Callout)
        );

        /// <summary>
        /// Gets or sets the preferred placement.
        /// </summary>
        public CalloutPlacement PreferredPlacement
        {
            get { return (CalloutPlacement)GetValue(PreferredPlacementProperty); }
            set { SetValue(PreferredPlacementProperty, value); }
        }

        /// <summary>
        /// Positions the popup a the given screen point.
        /// </summary>
        /// <param name="screenPoint">The point.</param>
        private void PositionPopup(Point screenPoint)
        {
            Popup popup = this.calloutPopup;

            if (Placement == CalloutPlacement.Center)
            {
                popup.Placement = PlacementMode.MousePoint;
                return;
            }

            popup.Placement = PlacementMode.AbsolutePoint;

            var screenBounds = WindowUtilities.ScreenBoundsFromPoint(screenPoint);
            var screenSize = screenBounds.Size;

            var size = MeasureDesiredSize(screenSize);

            if (Placement == CalloutPlacement.Right || Placement == CalloutPlacement.Left)
            {
                popup.VerticalOffset = ValueWithinBounds(screenPoint.Y - (size.Height / 2), screenBounds.Y + PlacementDistanceFromScreenBoundary,
                    screenBounds.Y + screenSize.Height - size.Height - PlacementDistanceFromScreenBoundary);

                TipOffset = ValueWithinBounds((screenPoint.Y - popup.VerticalOffset - ExtraMargin) - (TipSize / 2), MinTipOffset, size.Height - ExtraMargin - TipSize - MinTipOffset);
            }
            else if (Placement == CalloutPlacement.Bottom || Placement == CalloutPlacement.Top)
            {
                popup.HorizontalOffset = ValueWithinBounds(screenPoint.X - (size.Width / 2), screenBounds.X + PlacementDistanceFromScreenBoundary,
                    screenBounds.X + screenSize.Width - size.Width - PlacementDistanceFromScreenBoundary);

                TipOffset = ValueWithinBounds((screenPoint.X - popup.HorizontalOffset - ExtraMargin) - (TipSize / 2), MinTipOffset, size.Width - ExtraMargin - TipSize - MinTipOffset);
            }

            if (Placement == CalloutPlacement.Right)
            {
                popup.HorizontalOffset = screenPoint.X + PlacementDistanceFromCursor;
            }
            else if (Placement == CalloutPlacement.Left)
            {
                popup.HorizontalOffset = screenPoint.X - size.Width - PlacementDistanceFromCursor;
            }
            else if (Placement == CalloutPlacement.Bottom)
            {
                popup.VerticalOffset = screenPoint.Y + PlacementDistanceFromCursor;
            }
            else if (Placement == CalloutPlacement.Top)
            {
                popup.VerticalOffset = screenPoint.Y - size.Height - PlacementDistanceFromCursor;
            }
        }

        /// <summary>
        /// Measures the desired size of the callout.
        /// </summary>
        /// <param name="availableSize">The available size.</param>
        /// <returns>The desired size.</returns>
        private Size MeasureDesiredSize(Size availableSize)
        {
            Measure(availableSize);
            return DesiredSize;
        }

        /// <summary>
        /// Returns a value within a range, adjusting the value if it is falling outside the range.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        private static double ValueWithinBounds(double value, double minValue, double maxValue)
        {
            if (value < minValue)
            { 
                return minValue;
            }

            if (value > maxValue)
            { 
                return maxValue;
            }

            return value;
        }


        /// <summary>
        /// Handles the window deactivated.
        /// </summary>
        private void HandleWindowDeactivated(object sender, EventArgs e)
        {
            CloseCalloutAndStopMonitoring();
        }

        /// <summary>
        /// Handles the callout loaded.
        /// </summary>
        private void HandleCalloutLoaded(object sender, RoutedEventArgs e)
        {
            Popup parentPopup = this.Parent as Popup;

            Storyboard animateOpacity = (Storyboard)FindResource("AnimateCalloutOpacity");

            if (parentPopup != null)
            {
                Storyboard animateSlide = GetPopupSlideAnimation();
                if (animateSlide != null)
                {
                    animateSlide = animateSlide.Clone();
                    DoubleAnimation animation = (DoubleAnimation)animateSlide.Children[0];

                    if (Placement == CalloutPlacement.Right)
                    {
                        animation.From = parentPopup.HorizontalOffset - PlacementDistanceFromCursor;
                    }
                    else if (Placement == CalloutPlacement.Left)
                    {
                        animation.From = parentPopup.HorizontalOffset + PlacementDistanceFromCursor;
                    }
                    else if (Placement == CalloutPlacement.Bottom)
                    {
                        animation.From = parentPopup.VerticalOffset - PlacementDistanceFromCursor;
                    }
                    else if (Placement == CalloutPlacement.Top)
                    {
                        animation.From = parentPopup.VerticalOffset + PlacementDistanceFromCursor;
                    }

                    animateSlide.Begin(parentPopup);
                }
            }

            animateOpacity.Begin(this);
        }

        /// <summary>
        /// Handles the placement target mouse enter.
        /// </summary>
        private void HandlePlacementTargetMouseEnter(object sender, MouseEventArgs e)
        {
            StartOpenTimer();
            StopCloseTimer();
        }

        /// <summary>
        /// Handles the placement target mouse leave.
        /// </summary>
        private void HandlePlacementTargetMouseLeave(object sender, MouseEventArgs e)
        {
            StopOpenTimer();
            StartCloseTimer();
        }

        /// <summary>
        /// Handles the placement target unloaded.
        /// </summary>
        private void HandlePlacementTargetUnloaded(object sender, RoutedEventArgs e)
        {
            CloseCalloutAndStopMonitoring();
        }

        /// <summary>
        /// Handles the callout mouse enter.
        /// </summary>
        private void HandleCalloutMouseEnter(object sender, MouseEventArgs e)
        {
            StopCloseTimer();
        }

        /// <summary>
        /// Handles the callout mouse leave.
        /// </summary>
        private void HandleCalloutMouseLeave(object sender, MouseEventArgs e)
        {
            StartCloseTimer();
        }

        /// <summary>
        /// Handles the popup opened.
        /// </summary>
        private void HandlePopupOpened(object sender, EventArgs e)
        {
            RaiseEvent(new CalloutOpenedEventArgs());
        }
    }

    /// <summary>
    /// Defines the callout placement relative to the placement point.
    /// </summary>
    public enum CalloutPlacement
    {
        Center,
        Left,
        Top,
        Right,
        Bottom
    }

    /// <summary>
    /// Defines a callout service used to set callouts on UI elements.
    /// </summary>
    public static class CalloutService
    {
        /// <summary>
        /// The callout property
        /// </summary>
        public static readonly DependencyProperty CalloutProperty = DependencyProperty.RegisterAttached(
            "Callout", typeof(Callout), typeof(CalloutService), new PropertyMetadata(OnCalloutChanged)
        );

        /// <summary>
        /// Sets the callout on a UI element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="value">The value.</param>
        public static void SetCallout(DependencyObject element, Callout value)
        {
            element.SetValue(CalloutProperty, value);
        }

        /// <summary>
        /// Gets the callout for a UI element.
        /// </summary>
        /// <param name="element">The element.</param>
        public static Callout GetCallout(DependencyObject element)
        {
            return (Callout)element.GetValue(CalloutProperty);
        }

        /// <summary>
        /// Called when a callout has changed.
        /// </summary>
        private static void OnCalloutChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            UIElement element = (UIElement)source;
            Callout oldCallout = args.OldValue as Callout;
            Callout newCallout = args.NewValue as Callout;

            if (oldCallout != null)
            {
                oldCallout.PlacementTarget = null;
            }

            if (newCallout != null)
            {
                newCallout.PlacementTarget = element;
            }
        }
    }

    /// <summary>
    /// An event handler called when the callout is opening.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="CalloutOpeningEventArgs"/> instance containing the event data.</param>
    public delegate void CalloutOpeningEventHandler(object sender, CalloutOpeningEventArgs e);

    /// <summary>
    /// The event arguments of a callout opening event.
    /// </summary>
    public class CalloutOpeningEventArgs : RoutedEventArgs
    {
        public CalloutOpeningEventArgs()
            : base(Callout.CalloutOpeningEvent)
        {
        }
    }

    /// <summary>
    /// An event handler called when the callout has opened.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="CalloutOpenedEventArgs"/> instance containing the event data.</param>
    public delegate void CalloutOpenedEventHandler(object sender, CalloutOpenedEventArgs e);

    /// <summary>
    /// The event arguments of a callout opened event.
    /// </summary>
    public class CalloutOpenedEventArgs : RoutedEventArgs
    {
        public CalloutOpenedEventArgs()
            : base(Callout.CalloutOpenedEvent)
        {
        }
    }
}
