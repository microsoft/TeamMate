using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Controls
{
    /// <summary>
    /// Interaction logic for VideoPlayer.xaml
    /// </summary>
    public partial class VideoPlayer : UserControl
    {
        // Useful info from: http://blogs.microsoft.co.il/blogs/davids/archive/2009/05/17/mediaelement-and-more-with-wpf.aspx

        public static readonly TimeSpan FadeControlPaneInterval = TimeSpan.FromSeconds(1);

        private TimeDisplayOptions timeDisplay;
        private Window fullScreenContainer;
        private bool changingPosition;
        private DispatcherTimer timer;
        private bool isFullScreenToggleReopen;
        private TimeSpan? positionToRestoreOnFullScreenToggle;
        private DispatcherTimer hideControlPaneTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoPlayer"/> class.
        /// </summary>
        public VideoPlayer()
        {
            InitializeComponent();

            RegisterBindings(this.dettachableRoot.CommandBindings);

            this.dettachableRoot.MouseLeftButtonDown += HandleDoubleClick;
            this.dettachableRoot.MouseEnter += HandleMouseEnter;
            this.dettachableRoot.MouseLeave += HandleMouseLeave;
            this.dettachableRoot.PreviewMouseMove += HandlePreviewMouseMove;
            this.dettachableRoot.PreviewKeyDown += HandlePreviewKeyDown;

            mediaElement.MediaOpened += HandleMediaOpened;
            mediaElement.MediaFailed += HandleMediaFailed;
            mediaElement.MediaEnded += HandleMediaEnded;
            mediaElement.BufferingStarted += HandleBufferingStarted;
            mediaElement.BufferingEnded += HandleBufferingEnded;

            this.Unloaded += HandleUnloaded;

            positionSlider.IsMoveToPointEnabled = true;

            positionSlider.ValueChanged += HandleSliderValueChanged;
            positionSlider.AddHandler(Thumb.DragStartedEvent, new DragStartedEventHandler(HandleSliderDragStarted));
            positionSlider.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler(HandleSliderDragCompleted));
        }

        /// <summary>
        /// Shows the control pane.
        /// </summary>
        private void ShowControlPane()
        {
            IsControlPaneVisible = true;
            StartOrResetControlPaneTimer();
        }

        /// <summary>
        /// Hides the control pane.
        /// </summary>
        private void HideControlPane()
        {
            IsControlPaneVisible = false;
            StopControlPaneTimer();
        }

        /// <summary>
        /// Starts the or reset control pane timer.
        /// </summary>
        private void StartOrResetControlPaneTimer()
        {
            if (hideControlPaneTimer != null)
            {
                hideControlPaneTimer.Stop();
                hideControlPaneTimer.Start();
            }
            else
            {
                hideControlPaneTimer = new DispatcherTimer();
                hideControlPaneTimer.ExecuteOnce(FadeControlPaneInterval, HideControlPane);
            }
        }

        /// <summary>
        /// Stops the control pane timer.
        /// </summary>
        private void StopControlPaneTimer()
        {
            if (hideControlPaneTimer != null)
            {
                hideControlPaneTimer.Stop();
                hideControlPaneTimer = null;
            }
        }

        /// <summary>
        /// Registers command bindings.
        /// </summary>
        /// <param name="bindings">The bindings.</param>
        private void RegisterBindings(CommandBindingCollection bindings)
        {
            bindings.Add(MediaCommands.Stop, Stop, () => IsPlaying);
            bindings.Add(MediaCommands.Rewind, RewindStep, () => IsPlaying);
            bindings.Add(MediaCommands.TogglePlayPause, TogglePlayPause, () => Source != null);
            bindings.Add(MediaCommands.FastForward, FastForwardStep, () => IsPlaying);
            bindings.Add(MediaCommands.MuteVolume, ToggleMute);
        }

        /// <summary>
        /// Gets the media element.
        /// </summary>
        public MediaElement MediaElement
        {
            get { return this.mediaElement; }
        }

        /// <summary>
        /// The is control pane visible property
        /// </summary>
        public static readonly DependencyProperty IsControlPaneVisibleProperty = DependencyProperty.Register(
            "IsControlPaneVisible", typeof(bool), typeof(VideoPlayer)
        );

        /// <summary>
        /// Gets or sets a value indicating whether [is control pane visible].
        /// </summary>
        /// <value>
        /// <c>true</c> if [is control pane visible]; otherwise, <c>false</c>.
        /// </value>
        public bool IsControlPaneVisible
        {
            get { return (bool)GetValue(IsControlPaneVisibleProperty); }
            set { SetValue(IsControlPaneVisibleProperty, value); }
        }

        /// <summary>
        /// The is playing property
        /// </summary>
        public static readonly DependencyProperty IsPlayingProperty = DependencyProperty.Register(
            "IsPlaying", typeof(bool), typeof(VideoPlayer), new PropertyMetadata(OnIsPlayingChanged)
        );

        /// <summary>
        /// Gets or sets a value indicating whether [is playing].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is playing]; otherwise, <c>false</c>.
        /// </value>
        public bool IsPlaying
        {
            get { return (bool)GetValue(IsPlayingProperty); }
            set { SetValue(IsPlayingProperty, value); }
        }

        /// <summary>
        /// Called when is playing changed.
        /// </summary>
        private static void OnIsPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VideoPlayer player = (VideoPlayer)d;
            player.InvalidateIsPlaying();
        }

        /// <summary>
        /// The is dragging thumb property
        /// </summary>
        public static readonly DependencyProperty IsDraggingThumbProperty = DependencyProperty.Register(
            "IsDraggingThumb", typeof(bool), typeof(VideoPlayer)
        );

        /// <summary>
        /// Gets or sets a value indicating whether [is dragging thumb].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is dragging thumb]; otherwise, <c>false</c>.
        /// </value>
        public bool IsDraggingThumb
        {
            get { return (bool)GetValue(IsDraggingThumbProperty); }
            set { SetValue(IsDraggingThumbProperty, value); }
        }

        /// <summary>
        /// The source property
        /// </summary>
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof(Uri), typeof(VideoPlayer), new PropertyMetadata(OnSourceChanged)
        );

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        public Uri Source
        {
            get { return (Uri)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>
        /// Called when the source changed.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VideoPlayer player = (VideoPlayer)d;
            player.AutoPlay();
        }

        /// <summary>
        /// Begins auto play for the loaded video.
        /// </summary>
        private void AutoPlay()
        {
            if (!this.IsInDesignMode() && Source != null)
            {
                Play();
            }
        }

        /// <summary>
        /// The is full screen property
        /// </summary>
        public static readonly DependencyProperty IsFullScreenProperty = DependencyProperty.Register(
            "IsFullScreen", typeof(bool), typeof(VideoPlayer), new PropertyMetadata(OnIsFullScreenChanged)
        );

        /// <summary>
        /// Gets or sets a value indicating whether [is full screen].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is full screen]; otherwise, <c>false</c>.
        /// </value>
        public bool IsFullScreen
        {
            get { return (bool)GetValue(IsFullScreenProperty); }
            set { SetValue(IsFullScreenProperty, value); }
        }

        /// <summary>
        /// Called when is full screen changed.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnIsFullScreenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((VideoPlayer)d).InvalidateFullScreen();
        }

        /// <summary>
        /// Plays the loaded video.
        /// </summary>
        public void Play()
        {
            if (!IsPlaying)
            {
                if (Duration != null && Position == Duration.Value)
                {
                    Position = TimeSpan.Zero;
                }

                mediaElement.Play();
                IsPlaying = true;
            }
        }

        /// <summary>
        /// Pauses the video.
        /// </summary>
        public void Pause()
        {
            if (mediaElement.CanPause)
            {
                mediaElement.Pause();
                IsPlaying = false;
            }
        }

        /// <summary>
        /// Stops the vvideo.
        /// </summary>
        public void Stop()
        {
            mediaElement.Stop();
            IsPlaying = false;
        }

        /// <summary>
        /// Toggles between play and pause.
        /// </summary>
        public void TogglePlayPause()
        {
            if (!IsPlaying)
            {
                Play();
            }
            else
            {
                Pause();
            }
        }

        /// <summary>
        /// Rewinds a step.
        /// </summary>
        public void RewindStep()
        {
            TimeSpan nextPosition = Position.Subtract(TimeSpan.FromSeconds(20));
            this.Position = nextPosition;
        }

        /// <summary>
        /// Fasts forward a step.
        /// </summary>
        public void FastForwardStep()
        {
            TimeSpan nextPosition = Position.Add(TimeSpan.FromSeconds(10));
            this.Position = nextPosition;
        }

        /// <summary>
        /// Invalidates the is playing.
        /// </summary>
        private void InvalidateIsPlaying()
        {
            playPauseButton.IsChecked = !IsPlaying;
            InvalidatePositionUpdateTimer();
        }

        /// <summary>
        /// Invalidates the position update timer.
        /// </summary>
        private void InvalidatePositionUpdateTimer()
        {
            if (IsPlaying)
            {
                if (timer == null)
                {
                    timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(0.5);
                    timer.Tick += HandleUpdatePositionTick;
                    timer.Start();
                }
            }
            else
            {
                if (timer != null)
                {
                    timer.Stop();
                    timer = null;
                }

                InvalidatePositionIfNotDragging();
            }
        }

        /// <summary>
        /// Invalidates the position.
        /// </summary>
        private void InvalidatePosition()
        {
            InvalidatePositionTextBlock();
            InvalidateSlider();
        }

        /// <summary>
        /// Invalidates the position if not dragging.
        /// </summary>
        private void InvalidatePositionIfNotDragging()
        {
            if (!IsDraggingThumb)
            {
                InvalidatePosition();
            }
        }

        /// <summary>
        /// Invalidates the slider.
        /// </summary>
        private void InvalidateSlider()
        {
            changingPosition = true;
            positionSlider.Value = Position.TotalSeconds;
            changingPosition = false;
        }

        /// <summary>
        /// Invalidates the position text block.
        /// </summary>
        private void InvalidatePositionTextBlock()
        {
            TimeSpan dragPosition = (IsDraggingThumb) ? PositionSliderPosition() : Position;
            positionTextBlock.Text = dragPosition.ToShortestSecondsString();
        }

        /// <summary>
        /// Gets or sets the position in the video.
        /// </summary>
        private TimeSpan Position
        {
            get
            {
                return mediaElement.Position;
            }

            set
            {
                TimeSpan? duration = Duration;
                TimeSpan nextPosition = value;
                if (duration != null && nextPosition > duration.Value)
                {
                    nextPosition = duration.Value;
                }
                else if (nextPosition < TimeSpan.Zero)
                {
                    nextPosition = TimeSpan.Zero;
                }

                mediaElement.Position = nextPosition;
                InvalidatePosition();
            }
        }

        /// <summary>
        /// Gets the duration of the video, if available.
        /// </summary>
        private TimeSpan? Duration
        {
            get
            {
                return (mediaElement.NaturalDuration.HasTimeSpan) ? (TimeSpan?)mediaElement.NaturalDuration.TimeSpan : null;
            }
        }

        /// <summary>
        /// Toggles the mute.
        /// </summary>
        public void ToggleMute()
        {
            mediaElement.IsMuted = !mediaElement.IsMuted;
        }

        /// <summary>
        /// Invalidates the full screen.
        /// </summary>
        private void InvalidateFullScreen()
        {
            bool wasPlaying = IsPlaying;

            if (IsFullScreen)
            {
                isFullScreenToggleReopen = true;
                positionToRestoreOnFullScreenToggle = this.Position;
                // Go to full screen
                var root = this.dettachableRoot;
                this.Content = null;
                Window window = new Window();
                window.PreviewKeyDown += delegate(object sender, KeyEventArgs e)
                {
                    if (e.Key == Key.Escape)
                    {
                        IsFullScreen = false;
                        e.Handled = true;
                    }
                };

                window.Content = root;

                Rect screenBounds = WindowUtilities.GetBoundsForScreenContaining(Window.GetWindow(this));
                WindowUtilities.ToFullScreen(window, screenBounds);

                using (ExtendedSystemParameters.DisableSystemAnimations())
                {
                    window.Show();
                }

                this.fullScreenContainer = window;
            }
            else
            {
                StopFullScreen();
            }

            // KLUDGE: When we toggle full screen mode, and we were not playing (e.g. at the end of the video), the media player
            // restarts itself (probably because we unloaded and reloaded it). This is needed to avoid that.
            if (!wasPlaying)
            {
                mediaElement.Pause();
            }

            this.positionSlider.Focus();
        }

        /// <summary>
        /// Stops the full screen.
        /// </summary>
        private void StopFullScreen()
        {
            if (this.fullScreenContainer != null)
            {
                isFullScreenToggleReopen = true;
                positionToRestoreOnFullScreenToggle = this.Position;

                FrameworkElement root = (FrameworkElement)this.fullScreenContainer.Content;
                this.fullScreenContainer.Content = null;
                this.fullScreenContainer.Close();
                this.fullScreenContainer = null;

                this.Content = root;
                this.mediaElement.Focus();
            }
        }

        /// <summary>
        /// Gets the current position based on the slider.
        /// </summary>
        private TimeSpan PositionSliderPosition()
        {
            return TimeSpan.FromSeconds(positionSlider.Value);
        }

        /// <summary>
        /// Rotates the position display.
        /// </summary>
        private void TogglePositionDisplayOption()
        {
            TimeDisplayOptions rotatedOption = timeDisplay + 1;
            if (rotatedOption == TimeDisplayOptions.Max)
            {
                rotatedOption = (TimeDisplayOptions)0;
            }

            timeDisplay = rotatedOption;

            InvalidatePositionTextBlock();
        }

        /// <summary>
        /// Closes a full screenn video.
        /// </summary>
        private void Close()
        {
            IsFullScreen = false;
            Stop();
        }

        /// <summary>
        /// Invalidates the duration.
        /// </summary>
        private void InvalidateDuration()
        {
            TimeSpan? duration = Duration;
            string durationText = (duration != null) ? duration.Value.ToShortestSecondsString() : null;
            this.durationTextBlock.Text = durationText;
        }

        /// <summary>
        /// Handles the update position tick.
        /// </summary>
        private void HandleUpdatePositionTick(object sender, EventArgs e)
        {
            InvalidatePositionIfNotDragging();
        }

        /// <summary>
        /// Handles the preview key down.
        /// </summary>
        private void HandlePreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.System && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt && e.SystemKey == Key.Enter)
            {
                IsFullScreen = !IsFullScreen;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles the unloaded event.
        /// </summary>
        private void HandleUnloaded(object sender, RoutedEventArgs e)
        {
            StopFullScreen();
        }

        /// <summary>
        /// Handles the double click event.
        /// </summary>
        private void HandleDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                IsFullScreen = !IsFullScreen;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles the slider value changed.
        /// </summary>
        private void HandleSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!changingPosition)
            {
                if (IsDraggingThumb)
                {
                    InvalidatePositionTextBlock();
                }
                else
                {
                    // Clicked on a specific position
                    this.Position = PositionSliderPosition();
                }
            }
        }

        /// <summary>
        /// Handles the slider drag started.
        /// </summary>
        private void HandleSliderDragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            IsDraggingThumb = true;
        }

        /// <summary>
        /// Handles the slider drag completed.
        /// </summary>
        private void HandleSliderDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            IsDraggingThumb = false;
            this.Position = PositionSliderPosition();
        }

        /// <summary>
        /// Handles the buffering started.
        /// </summary>
        private void HandleBufferingStarted(object sender, RoutedEventArgs e)
        {
            statusTextBlock.Text = "Buffering...";
        }

        /// <summary>
        /// Handles the buffering ended.
        /// </summary>
        private void HandleBufferingEnded(object sender, RoutedEventArgs e)
        {
            statusTextBlock.Text = null;
            InvalidateDuration();
        }

        /// <summary>
        /// Handles the media opened.
        /// </summary>
        private void HandleMediaOpened(object sender, RoutedEventArgs e)
        {
            if (!isFullScreenToggleReopen)
            {
                TimeSpan? duration = Duration;
                if (duration != null)
                {
                    positionSlider.Maximum = duration.Value.TotalSeconds;
                }

                InvalidatePosition();
            }
            else
            {
                isFullScreenToggleReopen = false;

                if (positionToRestoreOnFullScreenToggle != null)
                {
                    if (IsPlaying)
                    {
                        // KLUDGE: Even if we reset the position to the exact same location, the media player will find 
                        // the next "key frame" I think to set that position. From that perspective, let's remove some time
                        // otherwise, sometimes toggling full screen mode makes you lose precious seconds of video watching.
                        this.Position = positionToRestoreOnFullScreenToggle.Value.Subtract(TimeSpan.FromSeconds(5));
                    }

                    positionToRestoreOnFullScreenToggle = null;
                }
            }

            InvalidateDuration();
        }

        /// <summary>
        /// Handles the media ended.
        /// </summary>
        private void HandleMediaEnded(object sender, RoutedEventArgs e)
        {
            IsPlaying = false;
        }

        /// <summary>
        /// Handles the media failed.
        /// </summary>
        private void HandleMediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Log.Error(e.ErrorException, "VideoPlayer Media Failed.");
            IsPlaying = false;
            InvalidateDuration();
        }

        /// <summary>
        /// Handles the mouse enter.
        /// </summary>
        private void HandleMouseEnter(object sender, MouseEventArgs e)
        {
            ShowControlPane();
        }

        /// <summary>
        /// Handles the preview mouse move.
        /// </summary>
        private void HandlePreviewMouseMove(object sender, MouseEventArgs e)
        {
            ShowControlPane();
        }

        /// <summary>
        /// Handles the mouse leave.
        /// </summary>
        private void HandleMouseLeave(object sender, MouseEventArgs e)
        {
            HideControlPane();
        }

        /// <summary>
        /// The time display options on the video player control panel.
        /// </summary>
        public enum TimeDisplayOptions
        {
            Elapsed,
            ElapsedAndTotal,
            Remaining,
            Max
        }
    }
}
