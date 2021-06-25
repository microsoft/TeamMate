using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Media;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Controls
{
    /// <summary>
    /// Provides utility methods and WPF attached properties for performing metro animations.
    /// </summary>
    public static class MetroAnimations
    {
        private static readonly TimeSpan IntervalBetweenChildAnimations = TimeSpan.FromSeconds(0.1);

        /// <summary>
        /// The slide right on load property
        /// </summary>
        public static readonly DependencyProperty SlideRightOnLoadProperty = DependencyProperty.RegisterAttached(
            "SlideRightOnLoad", typeof(bool), typeof(MetroAnimations), new PropertyMetadata(OnSlideRightOnLoadChanged)
        );

        /// <summary>
        /// Sets a value to slide the control from left to right on load.
        /// </summary>
        public static void SetSlideRightOnLoad(DependencyObject element, bool value)
        {
            element.SetValue(SlideRightOnLoadProperty, value);
        }

        /// <summary>
        /// Gets a value that determines whether the control slides from left to right on load.
        /// </summary>
        public static bool GetSlideRightOnLoad(DependencyObject element)
        {
            return (bool)element.GetValue(SlideRightOnLoadProperty);
        }

        /// <summary>
        /// Called when when the slide right on load property value changed.
        /// </summary>
        private static void OnSlideRightOnLoadChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            FrameworkElement element = (FrameworkElement)source;
            if (element.IsInDesignMode())
            {
                return;
            }

            if ((bool)args.NewValue == true)
            {
                element.Loaded += HandleElementWithSlideOnRightLoaded;
            }
            else
            {
                element.Loaded -= HandleElementWithSlideOnRightLoaded;
            }
        }

        /// <summary>
        /// Handles the element with slide on right loaded.
        /// </summary>
        public static void HandleElementWithSlideOnRightLoaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            SlideRight(element);
        }

        /// <summary>
        /// Begins the slide in of a framework element.
        /// </summary>
        /// <param name="element">The element.</param>
        public static void SlideRight(FrameworkElement element)
        {
            SlideRight(element, TimeSpan.Zero);
        }

        /// <summary>
        /// Begins the slide in of a of a framework elemenet at the given being time.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="beginTime">The begin time.</param>
        public static void SlideRight(FrameworkElement element, TimeSpan beginTime)
        {
            element.RenderTransform = new TranslateTransform();

            Storyboard ns = ControlResources.Animations.FindResource<Storyboard>("SlideRightStoryboard").Clone();
            ParallelTimeline fadeInTimeline = ns.Children.OfType<ParallelTimeline>().Single();
            fadeInTimeline.BeginTime = beginTime;
            ns.Begin(element);
        }

        public static void SlideUp(FrameworkElement element)
        {
            element.RenderTransform = new TranslateTransform();

            Storyboard ns = ControlResources.Animations.FindResource<Storyboard>("SlideUpStoryboard").Clone();
            ns.Begin(element);
        }

        /// <summary>
        /// The animate children property
        /// </summary>
        public static readonly DependencyProperty AnimateChildrenProperty = DependencyProperty.RegisterAttached(
            "AnimateChildren", typeof(bool), typeof(MetroAnimations), new PropertyMetadata(OnAnimateChildrenChanged)
        );

        /// <summary>
        /// Sets the animate children property.
        /// </summary>
        public static void SetAnimateChildren(DependencyObject element, bool value)
        {
            element.SetValue(AnimateChildrenProperty, value);
        }

        /// <summary>
        /// Gets the animate children property.
        /// </summary>
        public static bool GetAnimateChildren(DependencyObject element)
        {
            return (bool)element.GetValue(AnimateChildrenProperty);
        }

        /// <summary>
        /// Called when animate children property changed.
        /// </summary>
        private static void OnAnimateChildrenChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            FrameworkElement element = (FrameworkElement)source;
            if (element.IsInDesignMode())
            {
                return;
            }

            if ((bool)args.NewValue == true)
            {
                element.Loaded += HandleElementWithAnimatedChildrenLoaded;
                element.IsVisibleChanged += HandleElementWithAnimatedChildrenVisibilityChanged;
            }
            else
            {
                element.Loaded -= HandleElementWithAnimatedChildrenLoaded;
                element.IsVisibleChanged -= HandleElementWithAnimatedChildrenVisibilityChanged;
            }
        }

        /// <summary>
        /// Handles the visibility change of an elemement with animated children.
        /// </summary>
        private static void HandleElementWithAnimatedChildrenVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            AnimateChildren(element);
        }

        /// <summary>
        /// Handles the load event of an elemement with animated children.
        /// </summary>
        private static void HandleElementWithAnimatedChildrenLoaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            AnimateChildren(element);
        }

        /// <summary>
        /// Animates the children of an element.
        /// </summary>
        /// <param name="element">The element.</param>
        public static void AnimateChildren(FrameworkElement element)
        {
            if (element.IsLoaded && element.IsVisible)
            {
                var children = VisualTreeUtilities.GetChildren(element).OfType<FrameworkElement>();

                int index = 0;
                foreach (var child in children)
                {
                    TimeSpan beginTime = TimeSpan.FromSeconds(IntervalBetweenChildAnimations.TotalSeconds * index++);
                    MetroAnimations.SlideRight(child, beginTime);
                }
            }
        }
    }
}
