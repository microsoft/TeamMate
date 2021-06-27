// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.ViewModels;
using System.Linq;
using System.Windows.Controls;

namespace Microsoft.Tools.TeamMate.Controls
{
    /// <summary>
    /// Interaction logic for WorkItemRowView.xaml
    /// </summary>
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
            var oldTags = this.tagContainer.Children.OfType<TagLabel>().ToArray();
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

        private TagLabel AddTag(string tag)
        {
            TagLabel label = new TagLabel();
            label.Text = tag;
            this.tagContainer.Children.Add(label);
            return label;
        }
    }
}
