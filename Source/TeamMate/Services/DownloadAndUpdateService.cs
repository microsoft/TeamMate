using Microsoft.Internal.Tools.TeamMate.Foundation;
using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Internal.Tools.TeamMate.Foundation.Threading;
using Microsoft.Internal.Tools.TeamMate.Model;
using Microsoft.Internal.Tools.TeamMate.Utilities;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Deployment.Application;
using System.Threading.Tasks;
using System.Windows;

namespace Microsoft.Internal.Tools.TeamMate.Services
{
    public class DownloadAndUpdateService
    {
        private static TimeSpan CheckForUpdatesInterval = TimeSpan.FromDays(7);

        public event EventHandler BackgroundUpdateOccurred;

        [Import]
        public MessageBoxService MessageBoxService { get; set; }

        [Import]
        public RestartService RestartService { get; set; }

        [Import]
        public WindowService WindowService { get; set; }

        public ScheduledAction CheckForUpdatesAction { get; private set; }

        public DownloadAndUpdateService()
        {
            CheckForUpdatesAction = new ScheduledAction();
            CheckForUpdatesAction.Interval = CheckForUpdatesInterval;
            CheckForUpdatesAction.Action = BackgroundCheckForUpdates;
        }

        private void ShowErrorAndExit(string message)
        {
            this.MessageBoxService.ShowError(message);
            Environment.Exit(1);
        }

        private ApplicationDeployment CurrentDeployment
        {
            get
            {
                return TeamMateApplicationInfo.CurrentDeployment;
            }
        }

        private bool IsNetworkDeployed
        {
            get
            {
                return TeamMateApplicationInfo.IsNetworkDeployed;
            }
        }

        public void BackgroundCheckForUpdates()
        {
            if (!IsNetworkDeployed)
            {
                return;
            }

            this.CheckForUpdatesAction.Reset();

            ApplicationDeployment deployment = null;

            try
            {
                Log.Info("Checking for application update...");
                deployment = CurrentDeployment;
                deployment.CheckForUpdateCompleted += HandleBackgroundCheckForUpdatesCompleted;
                deployment.CheckForUpdateAsync();
            }
            catch (Exception e)
            {
                if (deployment != null)
                {
                    deployment.CheckForUpdateCompleted -= HandleBackgroundCheckForUpdatesCompleted;
                }

                Log.Error(e, "An error occurred attempting to check for updates in the background");
            }
        }

        public void CheckForUpdates()
        {
            if (!IsNetworkDeployed)
            {
                string message = String.Format("Automatic software updates are only available in the ClickOnce version. Sorry.");
                string title = String.Format("{0} Updates", TeamMateApplicationInfo.ApplicationName);
                this.MessageBoxService.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            this.CheckForUpdatesAction.Reset();

            UpdateCheckInfo updateInfo = null;
            var deployment = CurrentDeployment;

            try
            {
                updateInfo = deployment.CheckForDetailedUpdate();
            }
            catch (Exception e)
            {
                this.MessageBoxService.ShowError(e);
            }

            if (updateInfo != null && updateInfo.UpdateAvailable)
            {
                PromptAndDownloadUpdate();
            }
            else
            {
                string message = String.Format("No updates are currently available. You are running the latest version ({0}).", deployment.CurrentVersion);
                string title = String.Format("{0} Updates", TeamMateApplicationInfo.ApplicationName);
                this.MessageBoxService.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void PromptAndDownloadUpdate()
        {
            try
            {
                string prompt = String.Format("A newer version of {0} is available. Do you want to download and install it?", TeamMateApplicationInfo.ApplicationName);
                string title = String.Format("{0} Updates", TeamMateApplicationInfo.ApplicationName);
                var result = this.MessageBoxService.Show(prompt, title, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK);

                if (result == MessageBoxResult.OK)
                {
                    using (TaskContext taskContext = new TaskContext())
                    {
                        this.WindowService.MonitorWithProgressDialog(taskContext);
                        await UpdateAsync(taskContext);
                    }

                    var deployment = CurrentDeployment;
                    string message = String.Format("The application has been upgraded to version {0}, and will now restart.", deployment.UpdatedVersion);
                    this.MessageBoxService.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
                    this.RestartService.Restart();
                }
            }
            catch (Exception e)
            {
                this.MessageBoxService.ShowError(e);
            }
        }

        private async void HandleBackgroundCheckForUpdatesCompleted(object sender, CheckForUpdateCompletedEventArgs e)
        {
            try
            {
                CurrentDeployment.CheckForUpdateCompleted -= HandleBackgroundCheckForUpdatesCompleted;
                if (e.UpdateAvailable)
                {
                    Telemetry.Event(TelemetryEvents.BackgroundCheckFoundUpdate);
                    await UpdateAsync(TaskContext.None);
                    TriggerBackgroundUpdateOccurred();
                }
                else
                {
                    Log.Info("Checked for updates, but found none");
                }
            }
            catch (Exception ex)
            {
                Log.Error("Background application update failed", ex);
            }
        }

        public void TriggerBackgroundUpdateOccurred()
        {
            BackgroundUpdateOccurred?.Invoke(this, EventArgs.Empty);
        }

        private Task UpdateAsync(TaskContext taskContext)
        {
            taskContext.Title = "Updating application";
            taskContext.Status = "Downloading latest version, please wait...";

            var deployment = CurrentDeployment;

            var taskCompletionSource = new TaskCompletionSource<bool>();

            DeploymentProgressChangedEventHandler progressChanged = null;
            AsyncCompletedEventHandler updateCompleted = null;

            progressChanged = delegate(object sender, DeploymentProgressChangedEventArgs e)
            {
                if (e.BytesTotal > 0)
                {
                    taskContext.Report(e.BytesCompleted, e.BytesTotal);
                    taskContext.Status = String.Format("Downloading: {0} of {1}", FormatUtilities.FormatBytes(e.BytesCompleted),
                                                                            FormatUtilities.FormatBytes(e.BytesTotal));
                }
            };

            updateCompleted = delegate(object sender, AsyncCompletedEventArgs e)
            {
                deployment.UpdateProgressChanged -= progressChanged;
                deployment.UpdateCompleted -= updateCompleted;

                if (e.Error != null)
                {
                    taskCompletionSource.TrySetException(e.Error);
                }
                else if (e.Cancelled)
                {
                    taskCompletionSource.TrySetCanceled();
                }
                else
                {
                    taskCompletionSource.TrySetResult(true);
                }
            };

            deployment.UpdateProgressChanged += progressChanged;
            deployment.UpdateCompleted += updateCompleted;

            taskContext.CancellationToken.Register(() => deployment.UpdateAsyncCancel());

            deployment.UpdateAsync();

            return taskCompletionSource.Task;
        }
    }
}
