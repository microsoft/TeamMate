using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.Internal.Tools.TeamMate.Controls
{
    /// <summary>
    /// Interaction logic for SearchControl.xaml
    /// </summary>
    public partial class SearchBox : UserControl
    {
        public SearchBox()
        {
            InitializeComponent();
            this.textBox.KeyDown += HandleKeyDown;
            this.textBox.TextChanged += HandleTextChanged;
            this.clearButton.Click += ClearButton_Click;
            this.searchButton.Click += SearchButton_Click;
        }

        public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(
            "Mode", typeof(SearchBoxMode), typeof(SearchBox), new PropertyMetadata(OnModeChanged)
        );

        private static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SearchBox)d).InvalidateIsFiltering();
        }

        private void InvalidateIsFiltering()
        {
            this.IsFiltering = (Mode == SearchBoxMode.Filter);

            // Raise incremental search in the scenario where inputted some initial text and then chose to switch modes from
            // Search to Filter
            if (HasSearchText && IsFiltering)
            {
                RaiseIncrementalSearch();
            }
        }

        public SearchBoxMode Mode
        {
            get { return (SearchBoxMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        public static readonly DependencyProperty SupportsFilteringProperty = DependencyProperty.Register(
            "SupportsFiltering", typeof(bool), typeof(SearchBox), new PropertyMetadata(OnSupportsFilteringChanged)
        );

        private static void OnSupportsFilteringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SearchBox)d).InvalidateMode();
        }

        private void InvalidateMode()
        {
            if (!SupportsFiltering && Mode == SearchBoxMode.Filter)
            {
                Mode = SearchBoxMode.Search;
            }
        }

        public bool SupportsFiltering
        {
            get { return (bool)GetValue(SupportsFilteringProperty); }
            set { SetValue(SupportsFilteringProperty, value); }
        }

        public static readonly DependencyProperty IsFilteringProperty = DependencyProperty.Register(
            "IsFiltering", typeof(bool), typeof(SearchBox)
        );

        public bool IsFiltering
        {
            get { return (bool)GetValue(IsFilteringProperty); }
            private set { SetValue(IsFilteringProperty, value); }
        }

        public static readonly RoutedEvent SearchTriggeredEvent = EventManager.RegisterRoutedEvent(
            "SearchTriggeredEvent", RoutingStrategy.Bubble, typeof(SearchTriggeredEventHandler), typeof(SearchBox));

        public event SearchTriggeredEventHandler SearchTriggered
        {
            add { AddHandler(SearchTriggeredEvent, value); }
            remove { RemoveHandler(SearchTriggeredEvent, value); }
        }

        public static readonly RoutedEvent IncrementalSearchTriggeredEvent = EventManager.RegisterRoutedEvent(
            "IncrementalSearchTriggeredEvent", RoutingStrategy.Bubble, typeof(IncrementalSearchTriggeredEventHandler), typeof(SearchBox));

        public event IncrementalSearchTriggeredEventHandler IncrementalSearchTriggered
        {
            add { AddHandler(IncrementalSearchTriggeredEvent, value); }
            remove { RemoveHandler(IncrementalSearchTriggeredEvent, value); }
        }

        public static readonly DependencyProperty HintTextProperty = DependencyProperty.Register(
            "HintText", typeof(string), typeof(SearchBox)
        );

        public string HintText
        {
            get { return (string)GetValue(HintTextProperty); }
            set { SetValue(HintTextProperty, value); }
        }

        public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register(
            "SearchText", typeof(string), typeof(SearchBox), new PropertyMetadata(OnSearchTextChanged)
        );

        private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SearchBox)d).InvalidateHasSearchText();
        }

        private void InvalidateHasSearchText()
        {
            this.HasSearchText = !String.IsNullOrWhiteSpace(SearchText);
        }

        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }

        public static readonly DependencyProperty HasSearchTextProperty = DependencyProperty.Register(
            "HasSearchText", typeof(bool), typeof(SearchBox)
        );

        public bool HasSearchText
        {
            get { return (bool)GetValue(HasSearchTextProperty); }
            private set { SetValue(HasSearchTextProperty, value); }
        }

        public void FocusTextBox()
        {
            textBox.SelectAll();
            textBox.Focus();
        }

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Clear();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                if (HasSearchText || IsFiltering)
                {
                    // In Filter mode, when the filter is cleared, activate a search too, this will clear the filter
                    e.Handled = RaiseSearch();
                }
            }
        }

        private bool RaiseSearch()
        {
            var activatedEvent = new SearchTriggeredEventArgs();
            RaiseEvent(activatedEvent);
            bool handled = activatedEvent.Handled;
            return handled;
        }

        private void HandleTextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsFiltering)
            {
                RaiseIncrementalSearch();
            }
        }

        private void RaiseIncrementalSearch()
        {
            var searchEventArgs = new IncrementalSearchTriggeredEventArgs();
            RaiseEvent(searchEventArgs);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            // If I can, toggle the search mode between search and filter
            if (SupportsFiltering)
            {
                var toggledMode = (IsFiltering) ? SearchBoxMode.Search : SearchBoxMode.Filter;
                this.Mode = toggledMode;
                e.Handled = true;
            }
            else if(HasSearchText)
            {
                e.Handled = RaiseSearch();
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            Clear();
            e.Handled = true;
        }

        private void Clear()
        {
            if (HasSearchText)
            {
                SearchText = null;
            }
        }
    }

    public enum SearchBoxMode
    {
        Search,
        Filter
    }

    public delegate void SearchTriggeredEventHandler(object sender, SearchTriggeredEventArgs e);

    public class SearchTriggeredEventArgs : RoutedEventArgs
    {
        public SearchTriggeredEventArgs()
            : base(SearchBox.SearchTriggeredEvent)
        {
        }
    }

    public delegate void IncrementalSearchTriggeredEventHandler(object sender, IncrementalSearchTriggeredEventArgs e);

    public class IncrementalSearchTriggeredEventArgs : RoutedEventArgs
    {
        public IncrementalSearchTriggeredEventArgs()
            : base(SearchBox.IncrementalSearchTriggeredEvent)
        {
        }
    }
}
