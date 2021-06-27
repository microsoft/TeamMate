// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Windows;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.Resources;
using System.Collections.Generic;
using System.Windows.Input;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    public class NavigationViewModel : ViewModelBase, ICommandProvider
    {
        private IList<object> navigationStack = new List<object>();
        private int navigationIndex;
        private object page;

        public NavigationViewModel()
        {
            ClearHistory();
        }

        public void RegisterBindings(CommandBindingCollection bindings)
        {
            bindings.Add(TeamMateCommands.BrowseBack, GoBack, () => CanGoBack);
            bindings.Add(NavigationCommands.BrowseBack, GoBack, () => CanGoBack);
            bindings.Add(NavigationCommands.BrowseForward, GoForward, () => CanGoForward);
        }

        public object Page
        {
            get { return this.page; }
            private set
            {
                var currentPage = this.Page;

                if (value != currentPage)
                {
                    var fromPageViewModel = currentPage as PageViewModelBase;
                    var toPageViewModel = value as PageViewModelBase;

                    if(fromPageViewModel != null)
                    {
                        fromPageViewModel.OnNavigatingFrom();
                    }

                    if (toPageViewModel != null)
                    {
                        toPageViewModel.OnNavigatingTo();
                    }

                    SetProperty(ref this.page, value);
                }
            }
        }

        private void AddToNavigationStack()
        {
            if (this.Page != null)
            {
                while (this.navigationStack.Count > this.navigationIndex + 1)
                {
                    this.navigationStack.RemoveAt(this.navigationIndex + 1);
                }

                this.navigationStack.Add(this.Page);
                navigationIndex++;
                this.InvalidateState();
            }
        }

        private void InvalidateState()
        {
            this.CanGoBack = (navigationIndex >= 1);
            this.CanGoForward = (navigationIndex < navigationStack.Count - 1);
        }

        private bool canGoBack;

        public bool CanGoBack
        {
            get { return this.canGoBack; }
            private set { SetProperty(ref this.canGoBack, value); }
        }

        public void GoBack()
        {
            if (CanGoBack)
            {
                this.Page = navigationStack[--navigationIndex];
                this.InvalidateState();
            }
        }

        private bool canGoForward;

        public bool CanGoForward
        {
            get { return this.canGoForward; }
            set { SetProperty(ref this.canGoForward, value); }
        }

        public void GoForward()
        {
            if (CanGoForward)
            {
                this.Page = navigationStack[++navigationIndex];
                this.InvalidateState();
            }
        }

        public void NavigateToPage(object page)
        {
            Assert.ParamIsNotNull(page, "page");

            // If we are already on that page, do not navigate to it
            if (page != this.Page)
            {
                this.Page = page;

                AddToNavigationStack();
            }
        }

        public void ClearHistory()
        {
            navigationStack.Clear();
            navigationIndex = -1;
            this.InvalidateState();

            AddToNavigationStack();
        }
    }
}
