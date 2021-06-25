using Microsoft.Internal.Tools.TeamMate.Foundation.ComponentModel;
using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Controls
{
    public abstract class TreeItemViewModelBase : ObservableObjectBase
    {
        private bool isExpanded;

        public event EventHandler Loaded;

        public ICollection<TreeItemViewModelBase> Children { get; } = new ObservableCollection<TreeItemViewModelBase>();

        public bool IsExpanded
        {
            get { return this.isExpanded; }
            set
            {
                if (this.isExpanded != value)
                {
                    this.SetProperty(ref this.isExpanded, value);

                    if (this.isExpanded && !this.IsLoaded)
                    {
                        BeginLoading();
                    }
                }
            }
        }

        protected void InitializeAsContainer()
        {
            this.Children.Add(PlaceholderTreeItemViewModel.CreateLoadingNode());
        }

        public TreeNodeType TreeNodeType { get; private set; }

        private bool isLoading;

        public bool IsLoading
        {
            get { return this.isLoading; }
            private set { this.SetProperty(ref this.isLoading, value); }
        }

        private bool isLoaded;

        public bool IsLoaded
        {
            get { return this.isLoaded; }
            private set { this.SetProperty(ref this.isLoaded, value); }
        }

        private Exception loadError;

        public Exception LoadError
        {
            get { return this.loadError; }
            set { this.SetProperty(ref this.loadError, value); }
        }

        public void Reload()
        {
            this.IsLoaded = false;
            this.Children.Clear();
            this.InitializeAsContainer();

            this.BeginLoading();
        }

        private async void BeginLoading()
        {
            await EnsureLoadedAsync();
        }

        public async Task EnsureLoadedAsync()
        {
            if (!this.IsLoaded)
            {
                await OnLoadChildrenAsync();
            }
        }

        private async Task OnLoadChildrenAsync()
        {
            try
            {
                this.IsLoading = true;

                IEnumerable<TreeItemViewModelBase> children = await LoadChildrenAsync();
                this.Children.Clear();

                foreach (var newChild in children)
                {
                    this.Children.Add(newChild);
                }

                this.Loaded?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading tree view items");
                this.LoadError = ex;
                this.Children.Clear();
                this.Children.Add(PlaceholderTreeItemViewModel.CreateErrorNode(ex, this));
            }
            finally
            {
                this.IsLoading = false;
                this.IsLoaded = true;
            }
        }

        protected abstract Task<IEnumerable<TreeItemViewModelBase>> LoadChildrenAsync();

        class PlaceholderTreeItemViewModel : TreeItemViewModelBase
        {
            private TreeItemViewModelBase parent;

            public static PlaceholderTreeItemViewModel CreateLoadingNode()
            {
                return new PlaceholderTreeItemViewModel
                {
                    TreeNodeType = TreeNodeType.Loading
                };
            }

            public static PlaceholderTreeItemViewModel CreateErrorNode(Exception error, TreeItemViewModelBase parent)
            {
                return new PlaceholderTreeItemViewModel
                {
                    TreeNodeType = TreeNodeType.Error,
                    LoadError = error,
                    parent = parent
                };
            }


            private ICommand retryCommand;

            public ICommand RetryCommand
            {
                get
                {
                    if (this.retryCommand == null)
                    {
                        this.retryCommand = new RelayCommand(this.Retry);
                    }

                    return this.retryCommand;
                }
            }

            private void Retry()
            {
                if (this.parent != null)
                {
                    this.parent.Reload();
                }
            }

            protected override Task<IEnumerable<TreeItemViewModelBase>> LoadChildrenAsync()
            {
                return Task.FromResult(Enumerable.Empty<TreeItemViewModelBase>());
            }
        }
    }

    public enum TreeNodeType
    {
        Normal,
        Loading,
        Error
    }
}
