// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Windows;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using System;
using System.Windows;

namespace Microsoft.Tools.TeamMate.Services
{
    public class MessageBoxService
    {
        public MessageBoxResult Show(string messageBoxText,
            string caption = null,
            MessageBoxButton button = MessageBoxButton.OK,
            MessageBoxImage icon = MessageBoxImage.None,
            MessageBoxResult defaultResult = MessageBoxResult.None,
            MessageBoxOptions options = MessageBoxOptions.None)
        {
            return Show(null, messageBoxText, caption, button, icon, defaultResult, options);
        }

        public MessageBoxResult Show(ViewModelBase ownerViewModel,
            string messageBoxText,
            string caption = null,
            MessageBoxButton button = MessageBoxButton.OK,
            MessageBoxImage icon = MessageBoxImage.None,
            MessageBoxResult defaultResult = MessageBoxResult.None,
            MessageBoxOptions options = MessageBoxOptions.None)
        {
            Window owner = View.GetWindow(ownerViewModel);
            if (owner != null)
            {
                return MessageBox.Show(owner, messageBoxText, caption, button, icon, defaultResult, options);
            }
            else
            {
                return MessageBox.Show(messageBoxText, caption, button, icon, defaultResult, options);
            }
        }

        public void ShowError(Exception e)
        {
            UserFeedback.ShowError(e);
        }

        public void ShowError(ViewModelBase ownerViewModel, Exception e)
        {
            Window owner = View.GetWindow(ownerViewModel);
            UserFeedback.ShowError(owner, e);
        }

        public void ShowError(ViewModelBase ownerViewModel, string message, Exception e)
        {
            Window owner = View.GetWindow(ownerViewModel);
            UserFeedback.ShowError(owner, message, e);
        }

        public void ShowError(string message)
        {
            UserFeedback.ShowError(message);
        }

        public void ShowError(ViewModelBase ownerViewModel, string message)
        {
            Window owner = View.GetWindow(ownerViewModel);
            UserFeedback.ShowError(owner, message);
        }
    }
}
