using Microsoft.Tools.TeamMate.Foundation.Collections;
using Microsoft.Tools.TeamMate.Foundation.Windows.Input;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class WorkItemAddTagsDialogViewModel : ViewModelBase
    {
        private string title;
        private ObservableCollection<string> tags = new ObservableCollection<string>();
        private HashSet<string> originalTagCollection;
        private string newTagText;
        private ICollection<string> selectedItems = new List<string>();

        public WorkItemAddTagsDialogViewModel()
        {
            AddTagCommand = new RelayCommand(AddTag, CanAddTag);
            RemoveTagCommand = new RelayCommand(RemoveTag, CanRemoveTag);
        }

        public WorkItemAddTagsDialogViewModel(ICollection<string> tags) : this()
        {
            this.Tags.AddRange(tags);
        }

        private void AddTag()
        {
            if (!string.IsNullOrWhiteSpace(NewTagText))
            {
                string str = NewTagText.Trim();
                if (!tags.Contains(str, WorkItemConstants.TagComparer))
                {
                    Tags.Add(str);
                }
            }
            NewTagText = "";
        }

        private bool CanAddTag()
        {
            return !String.IsNullOrWhiteSpace(newTagText);
        }

        private void RemoveTag()
        {
            foreach (var item in selectedItems.ToList())
            {
                Tags.Remove(item);
            }
        }

        private bool CanRemoveTag()
        {
            return (selectedItems.Count > 0);
        }

        public void Initialize()
        {
            originalTagCollection = new HashSet<string>(tags);
        }

        public string Title
        {
            get { return this.title; }
            set { SetProperty(ref this.title, value); }
        }

        public string NewTagText
        {
            get { return this.newTagText; }
            set { SetProperty(ref this.newTagText, value); }
        }

        public ICollection<string> Tags
        {
            get { return this.tags; }
        }

        public ICollection<string> SelectedItems
        {
            get { return this.selectedItems; }
            set { SetProperty(ref this.selectedItems, value); }
        }

        public ICollection<string> GetTagsToAdd()
        {
            HashSet<string> tagsToAddSet = new HashSet<string>(tags);
            tagsToAddSet.ExceptWith(originalTagCollection);
            return tagsToAddSet;
        }

        public ICollection<string> GetTagsToRemove()
        {
            HashSet<string> tagsToRemoveSet = new HashSet<string>(originalTagCollection);
            tagsToRemoveSet.ExceptWith(tags);
            return tagsToRemoveSet;
        }

        public ICommand AddTagCommand { get; private set; }

        public ICommand RemoveTagCommand { get; private set; }
    }
}
