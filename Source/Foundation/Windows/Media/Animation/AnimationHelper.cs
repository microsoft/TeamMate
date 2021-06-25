using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using System.Windows;
using System.Windows.Media.Animation;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Media.Animation
{
    public class AnimationHelper
    {
        private bool isAnimating;
        private FrameworkElement target;
        private Storyboard storyboard;

        public AnimationHelper(FrameworkElement target, Storyboard storyboard)
        {
            Assert.ParamIsNotNull(target, "target");
            Assert.ParamIsNotNull(storyboard, "storyboard");

            this.target = target;
            this.storyboard = storyboard;

            target.Loaded += HandleLoaded;
            target.Unloaded += HandleUnloaded;
            target.IsVisibleChanged += HandleIsVisibleChanged;
        }

        public void Enable()
        {
            InvalidateAnimationState();
        }

        public void Invalidate()
        {
            Stop();
            InvalidateAnimationState();
        }

        private void InvalidateAnimationState()
        {
            if (target.IsLoaded && target.IsVisible)
            {
                Start();
            }
            else
            {
                Stop();
            }
        }

        private void Stop()
        {
            if (isAnimating)
            {
                storyboard.Stop(target);
                isAnimating = false;
            }
        }

        private void Start()
        {
            if (!isAnimating)
            {
                storyboard.Begin(target, true);
                isAnimating = true;
            }
        }

        private void HandleIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            InvalidateAnimationState();
        }

        private void HandleUnloaded(object sender, RoutedEventArgs e)
        {
            InvalidateAnimationState();
        }

        private void HandleLoaded(object sender, RoutedEventArgs e)
        {
            InvalidateAnimationState();
        }
    }
}
