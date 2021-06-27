// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Chaos;
using Microsoft.Tools.TeamMate.Foundation.Threading;
using Microsoft.Tools.TeamMate.Foundation.Windows.Input;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.Services;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi;
using Microsoft.Tools.TeamMate.Utilities;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Input;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    public class ProjectPickerDialogViewModel : ViewModelBase
    {
        private CancellationTokenSource previousCancellationTokenSource = new CancellationTokenSource();
        private string previosUrl = string.Empty;
        private string urlText;

        public ProjectPickerDialogViewModel()
        {
            this.ConnectCommand = new RelayCommand(this.Connect);
        }

        [Import]
        public MessageBoxService MessageBoxService { get; set; }


        public string UrlText
        {
            get { return this.urlText; }
            set
            {
                if (this.SetProperty(ref this.urlText, value))
                {
                    this.PreviewProjectCollectionUrl = TryCreateProjectCollectionUrl(this.urlText);
                }
            }
        }

        private Uri previewProjectCollectionUrl;

        public Uri PreviewProjectCollectionUrl
        {
            get { return this.previewProjectCollectionUrl; }
            private set { this.SetProperty(ref this.previewProjectCollectionUrl, value); }
        }

        private ICollection<TeamProjectReference> projects;

        public ICollection<TeamProjectReference> Projects
        {
            get { return this.projects; }
            set { this.SetProperty(ref this.projects, value); }
        }

        private bool isConnecting;

        public bool IsConnecting
        {
            get { return this.isConnecting; }
            set { this.SetProperty(ref this.isConnecting, value); }
        }

        public ICommand ConnectCommand { get; private set; }

        public void Connect()
        {
            var url = (this.urlText ?? string.Empty).Trim();
            if (!string.Equals(url, this.previosUrl, StringComparison.OrdinalIgnoreCase))
            {
                BeginConnect(url);
            }
        }

        private TaskContext progress = TaskContext.None;

        public TaskContext Progress
        {
            get { return this.progress; }
            set { this.SetProperty(ref this.progress, value); }
        }

        private Uri selectedProjectCollectionUrl;

        public Uri SelectedProjectCollectionUrl
        {
            get { return this.selectedProjectCollectionUrl; }
            set { this.SetProperty(ref this.selectedProjectCollectionUrl, value); }
        }

        private ICollection<TeamProjectReference> selectedProjects;

        public ICollection<TeamProjectReference> SelectedProjects
        {
            get { return this.selectedProjects; }
            set { this.SetProperty(ref this.selectedProjects, value); }
        }

        public void CancelConnect()
        {
            this.previousCancellationTokenSource.Cancel();
        }

        private async void BeginConnect(string urlText)
        {
            TaskContext task = new TaskContext();

            this.CancelConnect();

            this.previosUrl = urlText;
            this.Projects = null;
            this.Progress = TaskContext.None;

            var projectCollectionUrl = this.previewProjectCollectionUrl;
            this.SelectedProjectCollectionUrl = projectCollectionUrl;

            if (projectCollectionUrl != null)
            {
                var newCancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = newCancellationTokenSource.Token;
                this.previousCancellationTokenSource = newCancellationTokenSource;

                try
                {
                    using (this.Progress = new TaskContext())
                    {
                        await ChaosMonkey.ChaosAsync(ChaosScenarios.ChooseProject);
                        VssConnection connection = new VssConnection(projectCollectionUrl, new VssClientCredentials());
                        await connection.ConnectAsync(cancellationToken);

                        ProjectHttpClient projectClient = connection.GetClient<ProjectHttpClient>();
                        ICollection<TeamProjectReference> allProjects = await projectClient.GetProjectsInBatchesAsync(ProjectState.WellFormed, cancellationToken);
                        var projects = allProjects.OrderBy(p => p.Name).ToList();

                        if (!cancellationToken.IsCancellationRequested)
                        {
                            // Finish the work here...
                            this.Projects = projects;
                        }
                    }
                }
                catch (Exception e)
                {
                    if (!cancellationToken.IsCancellationRequested && !(e is OperationCanceledException))
                    {
                        // TODO: Nice to have, inline errors at some point
                        this.MessageBoxService.ShowError(this, $"An error ocurred connecting to {this.SelectedProjectCollectionUrl}: {e.Message}", e);
                    }
                }
            }
        }

        private static Uri TryCreateProjectCollectionUrl(string urlText)
        {
            urlText = (urlText ?? string.Empty).Trim();

            if (urlText.Length > 0)
            {
                Uri uri;
                if (Uri.TryCreate(urlText, UriKind.Absolute, out uri))
                {
                    if ((uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                    {
                        // An absolute valid url was nputted, return that;
                        return uri;
                    }
                }
                else if (IsSubdomainName(urlText))
                {
                    if (Uri.TryCreate($"https://{urlText}.visualstudio.com", UriKind.Absolute, out uri))
                    {
                        return uri;
                    }
                }
            }

            return null;
        }

        private static Regex ValidSubdomainNameRegex = new Regex("^(?:[A-Za-z0-9][A-Za-z0-9\\-]{0,61}[A-Za-z0-9]|[A-Za-z0-9])$");

        private static bool IsSubdomainName(string text)
        {
            return ValidSubdomainNameRegex.IsMatch(text);
        }
    }
}
