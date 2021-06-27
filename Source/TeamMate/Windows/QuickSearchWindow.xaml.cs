using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Windows;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Utilities;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Tools.TeamMate.Windows
{
    /// <summary>
    /// Interaction logic for QuickSearchWindow.xaml
    /// </summary>
    public partial class QuickSearchWindow : Window
    {
        public event QuickSearchTriggeredEventHandler SearchTriggered;

        public QuickSearchWindow()
        {
            InitializeComponent();
            InvalidateButtonStates();

            inputTextBox.TextChanged += HandleTextChanged;
            okButton.Click += HandleOkClicked;
            cancelButton.Click += HandleCancelClick;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
            this.Loaded += HandleLoaded;
        }

        public QuickSearchWindow(ProjectInfo project)
            :this()
        {
            this.Project = project;
        }

        private void HandleLoaded(object sender, RoutedEventArgs e)
        {
            WindowUtilities.CenterInPrimaryWorkArea(this);
        }

        private void HandleCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void HandleOkClicked(object sender, RoutedEventArgs e)
        {
            if (SearchTriggered != null)
            {
                string searchText = GetNormalizedSearchText();
                if(!String.IsNullOrEmpty(searchText))
                {
                    SearchTriggered(this, new QuickSearchTriggeredEventArgs(searchText));
                }
            }
        }

        private string GetNormalizedSearchText()
        {
            return TextMatcher.NormalizeSearchText(inputTextBox.Text);
        }

        private void HandleTextChanged(object sender, TextChangedEventArgs e)
        {
            InvalidateButtonStates();
        }

        private void InvalidateButtonStates()
        {
            string searchText = GetNormalizedSearchText();
            okButton.IsEnabled = !String.IsNullOrEmpty(searchText);
        }

        public static readonly DependencyProperty SelectedProjectProperty = DependencyProperty.Register(
            "Project", typeof(ProjectInfo), typeof(QuickSearchWindow), new PropertyMetadata(OnProjectChanged)
        );

        private static void OnProjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((QuickSearchWindow)d).InvalidateTitle();
        }

        private void InvalidateTitle()
        {
            string title = null;
            if (Project != null)
            {
                title = String.Format("Search {0}", Project.DisplayName);
            }

            this.dialogTitle.Text = title;
        }

        public ProjectInfo Project
        {
            get { return (ProjectInfo)GetValue(SelectedProjectProperty); }
            set { SetValue(SelectedProjectProperty, value); }
        }

        public void Reset()
        {
            string inputText = TryGetWorkItemIdFromClipboard();
            inputTextBox.Text = inputText;
            inputTextBox.SelectAll();
        }

        public static string TryGetWorkItemIdFromClipboard()
        {
            string inputText = null;

            try
            {
                string clipboardText = Clipboard.GetText();
                if (!String.IsNullOrEmpty(clipboardText))
                {
                    clipboardText = clipboardText.Trim();
                    if (IsTextWorkItemId(clipboardText))
                    {
                        inputText = clipboardText;
                    }
                }
            }
            catch (Exception e)
            {
                Log.ErrorAndBreak(e, "Failed to get clipboard text and set it in the quick search window");
            }
            return inputText;
        }

        private static bool IsTextWorkItemId(string inputText)
        {
            int id;
            return WorkItemReference.TryParseId(inputText, out id);
        }
    }

    public delegate void QuickSearchTriggeredEventHandler(object sender, QuickSearchTriggeredEventArgs e);

    public class QuickSearchTriggeredEventArgs : EventArgs
    {
        public QuickSearchTriggeredEventArgs(string searchText)
        {
            Assert.ParamIsNotNullOrEmpty(searchText, "searchText");

            this.SearchText = searchText;
        }
        
        public string SearchText { get; private set; }
    }
}
