using Microsoft.Tools.TeamMate.ViewModels;
using System.Linq;
using System.Windows.Controls;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Controls
{
    /// <summary>
    /// Interaction logic for WorkItemRowView.xaml
    /// </summary>
    [SupportedOSPlatform("windows10.0.19041.0")]
    public partial class WorkItemRowView : UserControl
    {
        private const int MaxTagsToDisplay = 3;

        public WorkItemRowView()
        {
            InitializeComponent();
            this.DataContextChanged += HandleDataContextChanges;
        }

        private void HandleDataContextChanges(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            WorkItemRowViewModel oldModel = e.OldValue as WorkItemRowViewModel;
            WorkItemRowViewModel newModel = e.NewValue as WorkItemRowViewModel;

            if (oldModel != null)
            {
                oldModel.PropertyChanged += OnViewModelPropertyChanged;
            }

            if (newModel != null)
            {
                newModel.PropertyChanged += OnViewModelPropertyChanged;
            }

            InvalidateTags();
        }

        private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Tags")
            {
                InvalidateTags();
            }
        }

        private void InvalidateTags()
        {
            // Remove existing tags
            var oldTags = this.tagContainer.Children.OfType<WorkItemTag>().ToArray();
            foreach (var oldTag in oldTags)
            {
                this.tagContainer.Children.Remove(oldTag);
            }

            // Add new tags
            WorkItemRowViewModel newModel = this.DataContext as WorkItemRowViewModel;
            string[] tags = (newModel != null) ? newModel.Tags : null;
            if (tags != null && tags.Length > 0)
            {
                foreach (var tag in tags.Take(MaxTagsToDisplay))
                {
                    AddTag(tag);
                }

                if (tags.Length > MaxTagsToDisplay)
                {
                    string remainingTags = string.Join(", ", tags.Skip(MaxTagsToDisplay));
                    var tag = AddTag("...");
                    tag.ToolTip = remainingTags;
                }
            }
        }

        private WorkItemTag AddTag(string tag)
        {
            WorkItemTag label = new WorkItemTag();
            label.tagContent.Text = tag;
            label.tagContent.Margin = new System.Windows.Thickness(3);
            label.tagContent.Focusable = false;
            label.tagContent.IsReadOnly = true;
            this.tagContainer.Children.Add(label);
            return label;
        }
    }
}
