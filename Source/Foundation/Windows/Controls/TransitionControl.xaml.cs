// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.ComponentModel;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Controls
{
    /// <summary>
    /// Interaction logic for MyTransitionControl.xaml
    /// </summary>
    public partial class TransitionControl : UserControl
    {
        public bool useContent1 = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransitionControl"/> class.
        /// </summary>
        public TransitionControl()
        {
            // Default value for the fade transition...
            // Transition = new FadeTransition();

            InitializeComponent();
            this.Loaded += HandleLoaded;
            this.Unloaded += HandleUnloaded;

            this.MouseEnter += HandleMouseEnter;
            this.MouseLeave += HandleMouseLeave;
        }

        /// <summary>
        /// The pause on mouse over property
        /// </summary>
        public static readonly DependencyProperty PauseOnMouseOverProperty = DependencyProperty.Register(
            "PauseOnMouseOver", typeof(bool), typeof(TransitionControl), new PropertyMetadata(false)
        );

        /// <summary>
        /// Gets or sets a value indicating whether the transition should pause when the mouse is over the control.
        /// </summary>
        public bool PauseOnMouseOver
        {
            get { return (bool)GetValue(PauseOnMouseOverProperty); }
            set { SetValue(PauseOnMouseOverProperty, value); }
        }

        /// <summary>
        /// Handles the mouse leave.
        /// </summary>
        private void HandleMouseLeave(object sender, MouseEventArgs e)
        {
            if (PauseOnMouseOver && transitionTimer != null)
            {
                transitionTimer.Start();
            }
        }

        /// <summary>
        /// Handles the mouse enter.
        /// </summary>
        private void HandleMouseEnter(object sender, MouseEventArgs e)
        {
            if (PauseOnMouseOver && transitionTimer != null)
            {
                transitionTimer.Stop();
            }
        }

        /// <summary>
        /// Handles the loaded.
        /// </summary>
        private void HandleLoaded(object sender, RoutedEventArgs e)
        {
            StartTimer();
        }

        /// <summary>
        /// Handles the unloaded.
        /// </summary>
        private void HandleUnloaded(object sender, RoutedEventArgs e)
        {
            StopTimer();
        }

        /// <summary>
        /// My content template property
        /// </summary>
        public static readonly DependencyProperty MyContentTemplateProperty = DependencyProperty.Register(
            "MyContentTemplate", typeof(DataTemplate), typeof(TransitionControl)
        );

        /// <summary>
        /// Gets or sets my content template.
        /// </summary>
        public DataTemplate MyContentTemplate
        {
            get { return (DataTemplate)GetValue(MyContentTemplateProperty); }
            set { SetValue(MyContentTemplateProperty, value); }
        }

        /// <summary>
        /// The transition property
        /// </summary>
        public static readonly DependencyProperty TransitionProperty = DependencyProperty.Register(
            "Transition", typeof(Transition), typeof(TransitionControl)
        );

        /// <summary>
        /// Gets or sets the transition.
        /// </summary>
        public Transition Transition
        {
            get { return (Transition)GetValue(TransitionProperty); }
            set { SetValue(TransitionProperty, value); }
        }

        /// <summary>
        /// My content property
        /// </summary>
        public static readonly DependencyProperty MyContentProperty = DependencyProperty.Register(
            "MyContent", typeof(object), typeof(TransitionControl), new PropertyMetadata(HandleMyContentChanged)
       );

        /// <summary>
        /// Handles my content changed.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void HandleMyContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TransitionControl)d).InvalidateContent();
        }

        /// <summary>
        /// Invalidates the content.
        /// </summary>
        private void InvalidateContent()
        {
            if (Transition != null)
            {
                Transition.Stop();
            }

            if (useContent1)
            {
                Content1 = MyContent;
                if (Transition != null)
                {
                    Transition.Begin(content1Container, content2Container, Duration);
                }
            }
            else
            {
                Content2 = MyContent;
                if (Transition != null)
                {
                    Transition.Begin(content2Container, content1Container, Duration);
                }
            }

            useContent1 = !useContent1;
        }

        /// <summary>
        /// The duration property
        /// </summary>
        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register(
            "Duration", typeof(TimeSpan), typeof(TransitionControl), new PropertyMetadata(TimeSpan.FromSeconds(1), HandleDurationChanged)
        );

        /// <summary>
        /// Handles the duration changed.
        /// </summary>
        private static void HandleDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TransitionControl)d).InvalidateTimerInterval();
        }

        /// <summary>
        /// Gets or sets the duration.
        /// </summary>
        public TimeSpan Duration
        {
            get { return (TimeSpan)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        /// <summary>
        /// Gets or sets my content.
        /// </summary>
        public object MyContent
        {
            get { return (object)GetValue(MyContentProperty); }
            set { SetValue(MyContentProperty, value); }
        }

        /// <summary>
        /// The content1 property
        /// </summary>
        public static readonly DependencyProperty Content1Property = DependencyProperty.Register(
            "Content1", typeof(object), typeof(TransitionControl)
        );

        /// <summary>
        /// Gets or sets the content1.
        /// </summary>
        public object Content1
        {
            get { return (object)GetValue(Content1Property); }
            set { SetValue(Content1Property, value); }
        }

        /// <summary>
        /// The content2 property
        /// </summary>
        public static readonly DependencyProperty Content2Property = DependencyProperty.Register(
            "Content2", typeof(object), typeof(TransitionControl)
        );

        /// <summary>
        /// Gets or sets the content2.
        /// </summary>
        public object Content2
        {
            get { return (object)GetValue(Content2Property); }
            set { SetValue(Content2Property, value); }
        }

        /// <summary>
        /// The item source property
        /// </summary>
        public static readonly DependencyProperty ItemSourceProperty = DependencyProperty.Register(
            "ItemSource", typeof(System.Collections.IEnumerable), typeof(TransitionControl), new PropertyMetadata(HandleItemSourceChanged)
        );

        /// <summary>
        /// Handles the item source changed.
        /// </summary>
        private static void HandleItemSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TransitionControl)d).InvalidteItemSource();
        }

        /// <summary>
        /// The current item property
        /// </summary>
        public static readonly DependencyProperty CurrentItemProperty = DependencyProperty.Register(
            "CurrentItem", typeof(object), typeof(TransitionControl)
        );

        /// <summary>
        /// Gets the current item.
        /// </summary>
        /// <value>
        /// The current item.
        /// </value>
        public object CurrentItem
        {
            get { return (object)GetValue(CurrentItemProperty); }
            private set { SetValue(CurrentItemProperty, value); }
        }

        private ICollectionView collection;

        /// <summary>
        /// Invalidtes the item source.
        /// </summary>
        private void InvalidteItemSource()
        {
            if (this.collection != null)
            {
                this.collection.CurrentChanged -= HandleCurrentChanged;
            }

            StopTimer();

            ICollectionView view = (this.ItemSource != null) ? CollectionViewSource.GetDefaultView(this.ItemSource) : null;
            this.collection = view;

            if (this.collection != null)
            {
                this.collection.CurrentChanged += HandleCurrentChanged;
            }

            InvalidateCurrentItem();
            StartTimer();
        }

        private void StartTimer()
        {
            if (IsLoaded && this.collection != null)
            {
                if (transitionTimer == null)
                {
                    transitionTimer = new DispatcherTimer();
                    InvalidateTimerInterval();
                    transitionTimer.Tick += HandleTransitionTimerElapsed;
                }

                transitionTimer.Start();
            }
        }

        /// <summary>
        /// Stops the transition timer.
        /// </summary>
        private void StopTimer()
        {
            if (transitionTimer != null)
            {
                transitionTimer.Stop();
            }
        }

        /// <summary>
        /// Handles the current changed.
        /// </summary>
        private void HandleCurrentChanged(object sender, EventArgs e)
        {
            InvalidateCurrentItem();
        }

        /// <summary>
        /// Invalidates the current item.
        /// </summary>
        private void InvalidateCurrentItem()
        {
            object value = (this.collection != null) ? this.collection.CurrentItem : null;
            MyContent = value;
            CurrentItem = value;
        }

        /// <summary>
        /// Handles the transition timer elapsed.
        /// </summary>
        private void HandleTransitionTimerElapsed(object sender, EventArgs e)
        {
            if (this.collection != null)
            {
                if (this.collection.IsCurrentLast())
                    this.collection.MoveCurrentToFirst();
                else
                    this.collection.MoveCurrentToNext();
            }
        }

        /// <summary>
        /// The transition interval property
        /// </summary>
        public static readonly DependencyProperty TransitionIntervalProperty = DependencyProperty.Register(
            "TransitionInterval", typeof(TimeSpan), typeof(TransitionControl), new PropertyMetadata(TimeSpan.FromSeconds(3), HandleTransitionIntervalChanged)
        );

        /// <summary>
        /// Handles the transition interval changed.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void HandleTransitionIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TransitionControl)d).InvalidateTimerInterval();
        }

        private DispatcherTimer transitionTimer;

        /// <summary>
        /// Invalidates the timer interval.
        /// </summary>
        private void InvalidateTimerInterval()
        {
            if (transitionTimer != null)
            {
                transitionTimer.Interval = TransitionInterval + Duration;
            }
        }

        /// <summary>
        /// Gets or sets the transition interval.
        /// </summary>
        public TimeSpan TransitionInterval
        {
            get { return (TimeSpan)GetValue(TransitionIntervalProperty); }
            set { SetValue(TransitionIntervalProperty, value); }
        }

        /// <summary>
        /// Gets or sets the item source.
        /// </summary>
        public System.Collections.IEnumerable ItemSource
        {
            get { return (System.Collections.IEnumerable)GetValue(ItemSourceProperty); }
            set { SetValue(ItemSourceProperty, value); }
        }
    }
}
