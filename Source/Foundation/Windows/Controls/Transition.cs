using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Controls
{
    /// <summary>
    /// Defines a transition operation used to swap two UI elements.
    /// </summary>
    public abstract class Transition
    {
        /// <summary>
        /// Occurs when the transition has started.
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Occurs when transition has completed.
        /// </summary>
        public event EventHandler Completed;

        /// <summary>
        /// Gets the transitions dictionary.
        /// </summary>
        internal static ResourceDictionary TransitionsDictionary
        {
            get
            {
                return ControlResources.Transitions;
            }
        }

        /// <summary>
        /// Begins the transition.
        /// </summary>
        /// <param name="entering">The UI element entering.</param>
        /// <param name="exiting">The UI element exiting.</param>
        /// <param name="duration">The duration of the transition.</param>
        public abstract void Begin(FrameworkElement entering, FrameworkElement exiting, TimeSpan duration);

        /// <summary>
        /// Stops the transition if curently running.
        /// </summary>
        public abstract void Stop();


        /// <summary>
        /// Fires the started event.
        /// </summary>
        protected void FireStarted()
        {
            Started?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Fires the completed event.
        /// </summary>
        protected void FireCompleted()
        {
            Completed?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// A transition implementation based on a storyboard for the entering and exiting controls.
    /// </summary>
    public abstract class StoryboardBasedTransition : Transition
    {
        private Storyboard enterStoryboard;
        private Storyboard exitStoryboard;
        private Storyboard combinedStoryboard;

        private string enterStoryboardName;
        private string exitStoryboardName;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoryboardBasedTransition"/> class.
        /// </summary>
        protected StoryboardBasedTransition()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoryboardBasedTransition"/> class.
        /// </summary>
        /// <param name="enterStoryboardName">Name of the enter storyboard in the transitions dictionary.</param>
        /// <param name="exitStoryboardName">Name of the exit storyboard in the transitions dictionary.</param>
        protected StoryboardBasedTransition(string enterStoryboardName, string exitStoryboardName)
        {
            this.enterStoryboardName = enterStoryboardName;
            this.exitStoryboardName = exitStoryboardName;
        }

        /// <summary>
        /// Invalidates the storyboards.
        /// </summary>
        protected void InvalidateStoryboards()
        {
            combinedStoryboard = null;
            enterStoryboard = null;
            exitStoryboard = null;
        }

        /// <summary>
        /// Clones the storyboard with the given resource name.
        /// </summary>
        /// <param name="name">The resource name.</param>
        /// <returns>A clone of the storyboard.</returns>
        protected Storyboard CloneStoryboard(string name)
        {
            return ((Storyboard)Transition.TransitionsDictionary[name]).Clone();
        }

        /// <summary>
        /// Begins the transition.
        /// </summary>
        /// <param name="entering">The UI element entering.</param>
        /// <param name="exiting">The UI element exiting.</param>
        /// <param name="duration">The duration of the transition.</param>
        public override void Begin(FrameworkElement entering, FrameworkElement exiting, TimeSpan duration)
        {
            if (combinedStoryboard == null)
            {
                enterStoryboard = CreateEnterStoryboard();
                exitStoryboard = CreateExitStoryboard();

                combinedStoryboard = new Storyboard();
                combinedStoryboard.Children.Add(enterStoryboard);
                combinedStoryboard.Children.Add(exitStoryboard);
            }

            Prepare(entering, exiting, enterStoryboard, exitStoryboard, duration);

            Storyboard.SetTarget(enterStoryboard, entering);
            Storyboard.SetTarget(exitStoryboard, exiting);

            combinedStoryboard.Completed += HandleCompleted;

            FireStarted();
            combinedStoryboard.Begin();
        }

        /// <summary>
        /// Handles the completed event of the storyboard.
        /// </summary>
        private void HandleCompleted(object sender, EventArgs e)
        {
            ReleaseStoryboard();
            FireCompleted();
        }

        /// <summary>
        /// Releases any handles to the applied storyboard.
        /// </summary>
        private void ReleaseStoryboard()
        {
            if (combinedStoryboard != null)
            {
                combinedStoryboard.Completed -= HandleCompleted;
            }
        }

        /// <summary>
        /// Creates the enter storyboard.
        /// </summary>
        /// <returns></returns>
        protected virtual Storyboard CreateEnterStoryboard()
        {
            return (enterStoryboardName != null) ? CloneStoryboard(enterStoryboardName) : null;
        }

        /// <summary>
        /// Creates the exit storyboard.
        /// </summary>
        /// <returns></returns>
        protected virtual Storyboard CreateExitStoryboard()
        {
            return (exitStoryboardName != null) ? CloneStoryboard(exitStoryboardName) : null;
        }

        /// <summary>
        /// Prepares the storyboards and elements before beginning the transition.
        /// </summary>
        /// <param name="entering">The entering UI element.</param>
        /// <param name="exiting">The exiting UI element.</param>
        /// <param name="enterStoryboard">The enter storyboard.</param>
        /// <param name="exitStoryboard">The exit storyboard.</param>
        /// <param name="duration">The duration.</param>
        protected virtual void Prepare(FrameworkElement entering, FrameworkElement exiting, Storyboard enterStoryboard, Storyboard exitStoryboard, TimeSpan duration)
        {
            // CAUTION: THIS IS A VERY NAIVE way of setting the overall transition duration, and it assumes that enter and exit
            // storyboards are only composed of DIRECT animation (and not hierarchies)
            foreach (var item in enterStoryboard.Children)
            {
                item.Duration = duration;
            }

            foreach (var item in exitStoryboard.Children)
            {
                item.Duration = duration;
            }
        }

        /// <summary>
        /// Stops the transition if curently running.
        /// </summary>
        public override void Stop()
        {
            if (combinedStoryboard != null)
            {
                if (combinedStoryboard.GetCurrentState() == ClockState.Active)
                {
                    combinedStoryboard.Stop();
                    ReleaseStoryboard();
                }
            }
        }
    }

    /// <summary>
    /// A fade transition.
    /// </summary>
    public class FadeTransition : StoryboardBasedTransition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FadeTransition"/> class.
        /// </summary>
        public FadeTransition()
            : base("FadeIn", "FadeOut")
        {
        }
    }

    /// <summary>
    /// A slide transition.
    /// </summary>
    public class SlideTransition : StoryboardBasedTransition
    {
        private Random random;
        private SlideDirection direction;

        /// <summary>
        /// Gets or sets the direction.
        /// </summary>
        public SlideDirection Direction
        {
            get { return this.direction; }
            set
            {
                this.direction = value;
                InvalidateStoryboards();
            }
        }

        /// <summary>
        /// Gets or sets the current direction.
        /// </summary>
        private SlideDirection CurrentDirection { get; set; }

        /// <summary>
        /// Gets a value indicating whether the slide direction is random.
        /// </summary>
        private bool IsRandom
        {
            get
            {
                bool isRandom = (Direction == SlideDirection.RandomHorizontal || Direction == SlideDirection.RandomVertical || Direction == SlideDirection.Random);
                return isRandom;
            }
        }

        /// <summary>
        /// Creates the enter storyboard.
        /// </summary>
        protected override Storyboard CreateEnterStoryboard()
        {
            return CreateEnterOrExitStoryboard();
        }

        /// <summary>
        /// Creates the exit storyboard.
        /// </summary>
        protected override Storyboard CreateExitStoryboard()
        {
            return CreateEnterOrExitStoryboard();
        }

        /// <summary>
        /// Creates the enter or exit storyboard.
        /// </summary>
        private Storyboard CreateEnterOrExitStoryboard()
        {
            bool horizontal = (CurrentDirection == SlideDirection.LeftToRight || CurrentDirection == SlideDirection.RightToLeft);
            string name = (horizontal) ? "HorizontalTranslation" : "VerticalTranslation";
            return CloneStoryboard(name);
        }

        /// <summary>
        /// Begins the transition.
        /// </summary>
        /// <param name="entering">The UI element entering.</param>
        /// <param name="exiting">The UI element exiting.</param>
        /// <param name="duration">The duration of the transition.</param>
        public override void Begin(FrameworkElement entering, FrameworkElement exiting, TimeSpan duration)
        {
            CurrentDirection = (IsRandom) ? GetRandomDirection() : Direction;

            if (IsRandom)
            {
                InvalidateStoryboards();
            }

            base.Begin(entering, exiting, duration);
        }

        /// <summary>
        /// Prepares the storyboards and elements before beginning the transition.
        /// </summary>
        /// <param name="entering">The entering UI element.</param>
        /// <param name="exiting">The exiting UI element.</param>
        /// <param name="enterStoryboard">The enter storyboard.</param>
        /// <param name="exitStoryboard">The exit storyboard.</param>
        /// <param name="duration">The duration.</param>
        protected override void Prepare(FrameworkElement entering, FrameworkElement exiting, Storyboard enter, Storyboard exit, TimeSpan duration)
        {
            base.Prepare(entering, exiting, enter, exit, duration);

            double enterFrom = 0;
            double enterTo = 0;
            double exitFrom = 0;
            double exitTo = 0;

            switch (CurrentDirection)
            {
                case SlideDirection.LeftToRight:
                    enterFrom = -entering.ActualWidth;
                    exitTo = exiting.ActualWidth;
                    break;

                case SlideDirection.RightToLeft:
                    enterFrom = entering.ActualWidth;
                    exitTo = -exiting.ActualWidth;
                    break;

                case SlideDirection.TopToBottom:
                    enterFrom = -entering.ActualHeight;
                    exitTo = exiting.ActualHeight;
                    break;

                case SlideDirection.BottomToTop:
                    enterFrom = entering.ActualHeight;
                    exitTo = -exiting.ActualHeight;
                    break;
            }

            DoubleAnimation enterAnimation = (DoubleAnimation)enter.Children[0];
            DoubleAnimation exitAnimation = (DoubleAnimation)exit.Children[0];

            enterAnimation.From = enterFrom;
            enterAnimation.To = enterTo;
            exitAnimation.From = exitFrom;
            exitAnimation.To = exitTo;

            // TODO: Random rotation is broken, the fill behavior and hold end values without resetting some is causing
            // problems when directions change.

            if (!(entering.RenderTransform is TranslateTransform))
            {
                entering.RenderTransform = new TranslateTransform();
            }

            if (!(exiting.RenderTransform is TranslateTransform))
            {
                exiting.RenderTransform = new TranslateTransform();
            }
        }

        /// <summary>
        /// Gets the next random direction for the next slide.
        /// </summary>
        private SlideDirection GetRandomDirection()
        {
            if (random == null)
            {
                random = new Random();
            }

            SlideDirection directionToUse;

            switch (Direction)
            {
                case SlideDirection.RandomHorizontal:
                    directionToUse = (SlideDirection)random.Next((int)SlideDirection.LeftToRight, ((int)SlideDirection.RightToLeft) + 1);
                    break;

                case SlideDirection.RandomVertical:
                    directionToUse = (SlideDirection)random.Next((int)SlideDirection.TopToBottom, ((int)SlideDirection.BottomToTop) + 1);
                    break;

                default:
                    directionToUse = (SlideDirection)random.Next((int)SlideDirection.LeftToRight, ((int)SlideDirection.BottomToTop) + 1);
                    break;
            }

            return directionToUse;
        }
    }

    /// <summary>
    /// The slide direction for the slide transition.
    /// </summary>
    public enum SlideDirection
    {
        LeftToRight,
        RightToLeft,
        TopToBottom,
        BottomToTop,
        RandomHorizontal,
        RandomVertical,
        Random
    }

    /// <summary>
    /// The Horizontal Fade In transition.
    /// </summary>
    public class HorizontalFadeInTransition : StoryboardBasedTransition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HorizontalFadeInTransition"/> class.
        /// </summary>
        public HorizontalFadeInTransition()
            : base("HorizontalFadeIn", "HideImmediately")
        {
        }

        /// <summary>
        /// Prepares the storyboards and elements before beginning the transition.
        /// </summary>
        /// <param name="entering">The entering UI element.</param>
        /// <param name="exiting">The exiting UI element.</param>
        /// <param name="enterStoryboard">The enter storyboard.</param>
        /// <param name="exitStoryboard">The exit storyboard.</param>
        /// <param name="duration">The duration.</param>
        protected override void Prepare(FrameworkElement entering, FrameworkElement exiting, Storyboard enter, Storyboard exit, TimeSpan duration)
        {
            if (!(entering.RenderTransform is TranslateTransform))
            {
                entering.RenderTransform = new TranslateTransform();
            }
        }
    }
}
