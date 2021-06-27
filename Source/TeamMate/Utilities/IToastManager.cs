// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Tools.TeamMate.Utilities
{
    public interface IToastManager : IDisposable
    {
        event EventHandler<ToastActivatedEventArgs> ToastActivated;

        void Show(ToastInfo toast);
    }

    public class ToastActivatedEventArgs : EventArgs
    {
        public ToastActivatedEventArgs(string arguments)
        {
            this.Arguments = arguments;
        }

        public string Arguments { get; private set; }
    }

    public class ToastInfo
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string Arguments { get; set; }
    }
}
