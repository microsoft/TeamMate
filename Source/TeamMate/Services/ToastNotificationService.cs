using Microsoft.Tools.TeamMate.Foundation;
using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Platform.CodeFlow.Dashboard;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking;
using Microsoft.Tools.TeamMate.Utilities;
using Microsoft.Tools.TeamMate.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using WorkItem = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem;

namespace Microsoft.Tools.TeamMate.Services
{
    public class ToastNotificationService : IDisposable
    {
        public static readonly string[] RequiredWorkItemFields = {
            WorkItemConstants.CoreFields.AssignedTo,
            WorkItemConstants.CoreFields.CreatedBy,
            WorkItemConstants.CoreFields.CreatedDate,
            WorkItemConstants.CoreFields.ChangedBy,
            WorkItemConstants.CoreFields.ChangedDate,
            WorkItemConstants.CoreFields.Title,
            WorkItemConstants.CoreFields.State,
            WorkItemConstants.CoreFields.Rev
        };

        private IToastManager toastManager;

        // Maximum number of toast to queue within a given time interval
        // Once that time interval is reached, a single "You have new items." toast will be shown.
        // That time interval will reset later.
        private const int MaxToastsToQueue = 10;
        private static readonly TimeSpan MaxToastToQueueInterval = TimeSpan.FromMinutes(1);
        private DateTime? currentToastCountStartTime;
        private int currentToastCount;
        private object toastQueueLock = new object();

        [Import]
        public SettingsService SettingsService { get; set; }

        [Import]
        public UIService UIService { get; set; }

        [Import]
        public WindowService WindowService { get; set; }


        public void Initialize()
        {
            this.SettingsService.DeveloperSettings.PropertyChanged += DeveloperSettings_PropertyChanged;
            InvalidateToastManager();
        }

        private void DeveloperSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ForceLegacyNotifications")
            {
                this.InvalidateToastManager();
            }
        }

        private void InvalidateToastManager()
        {
            if (this.toastManager != null)
            {
                this.toastManager.ToastActivated -= ToastManager_ToastActivated;
                this.toastManager = null;
            }

            if (Environment.OSVersion.IsWindows10OrGreater() && !this.SettingsService.DeveloperSettings.ForceLegacyNotifications)
            {
                this.toastManager = new WindowsToastManager();
            }
            else
            {
                var dispatcher = this.UIService.Dispatcher;
                var settingsService = this.SettingsService;
                this.toastManager = new CustomToastManager(dispatcher, settingsService);
            }

            this.toastManager.ToastActivated += ToastManager_ToastActivated;
        }

        public void QueueNotifications(IEnumerable<WorkItem> workItems, DateTime? previousUpdate, NotificationScope scope)
        {
            Assert.ParamIsNotNull(workItems, "workItems");

            try
            {
                var orderedWorkItems = workItems.OrderByDescending(wi => wi.ChangedDate().Value);
                var changed = (scope == null) ? orderedWorkItems : orderedWorkItems.Where(wi => scope.ShouldNotify(wi));
                var toasts = changed.Select(wi => CreateToast(wi, previousUpdate)).ToArray();
                this.QueueToasts(toasts);
            }
            catch (Exception e)
            {
                Log.ErrorAndBreak(e);
            }
        }

        public void QueueNotifications(IEnumerable<PullRequestViewModel> items, DateTime? previousUpdate, NotificationScope scope)
        {
            try
            {
                Assert.ParamIsNotNull(items, "items");

                var orderedCodeReviews = items.OrderByDescending(wi => wi.Reference.CodeReviewId);
                var changed = (scope == null) ? orderedCodeReviews : orderedCodeReviews.Where(wi => scope.ShouldNotify(wi));
                var toasts = changed.Select(wi => CreateToast(wi)).ToArray();
                this.QueueToasts(toasts);
            }
            catch (Exception e)
            {
                Log.ErrorAndBreak(e);
            }
        }

        private void QueueToasts(ICollection<ToastInfo> toasts)
        {
            lock (toastQueueLock)
            {
                // Rest max toast counter every time the interval is exceeded...
                if (this.currentToastCountStartTime == null || (DateTime.Now > this.currentToastCountStartTime.Value + MaxToastToQueueInterval))
                {
                    this.currentToastCountStartTime = DateTime.Now;
                    this.currentToastCount = 0;
                }

                if (this.currentToastCount < MaxToastsToQueue)
                {
                    this.currentToastCount += toasts.Count;

                    if (this.currentToastCount >= MaxToastsToQueue)
                    {
                        // We've exceeded our maximum toasts, show the you have new items toast instead...
                        this.Show(CreateTooManyNotificationsToast());
                    }
                    else
                    {
                        // We've not exceeded our maximum toasts, show all of them
                        foreach (var toast in toasts)
                        {
                            this.Show(toast);
                        }
                    }
                }
            }
        }

        private void Show(ToastInfo toast)
        {
            this.toastManager.Show(toast);
        }

        private ToastInfo CreateToast(WorkItem item, DateTime? previousUpdate)
        {
            // IMPORTANT: Any time you consume more work item fields here (for notifications), they should
            // be prefetched from the DB when running queries for best performance. If you modify this to
            // add more values, make sure the UsedWorkItemFields list in this class is up to date.
            ToastInfo toastInfo = new ToastInfo();

            /*
            var cache = IdentityCacheFactory.GetCache(item);
            Person person = cache.ResolveIdentity(item.ChangedBy);

            viewModel.Photo = GetPhoto(person);
             */


            bool isNewSinceLastCheck = item.CreatedDate().Value.IsAfter(previousUpdate);
            if (isNewSinceLastCheck)
            {
                toastInfo.Title = String.Format("{0} created {1}", WorkItemIdentity.GetDisplayName(item.CreatedBy()), item.GetShortTitle());
            }
            else
            {
                toastInfo.Title = String.Format("{0} changed {1}", WorkItemIdentity.GetDisplayName(item.ChangedBy()), item.GetShortTitle());
            }

            toastInfo.Description = item.Title();

            ToastActivationInfo activationInfo = new ToastActivationInfo
            {
                Action = ToastActivationAction.OpenWorkItem,
                WorkItem = new ToastWorkItemInfo
                {
                    ProjectCollectionUri = item.GetReference().ProjectCollectionUri,
                    Id = item.Id.Value
                }
            };

            toastInfo.Arguments = activationInfo.ToJson();

            return toastInfo;
        }

        private ToastInfo CreateToast(PullRequestViewModel pullRequest)
        {
            ToastInfo toastInfo = new ToastInfo();
            toastInfo.Title = pullRequest.Reference.Title;
            toastInfo.Description = pullRequest.Reference.Description;

            ToastActivationInfo activationInfo = new ToastActivationInfo
            {
                Action = ToastActivationAction.OpenCodeFlowReview,
                CodeFlowReview = new ToastCodeFlowReviewInfo
                {
                    LaunchClientUri = pullRequest.Url
                }
            };

            toastInfo.Arguments = activationInfo.ToJson();

            return toastInfo;
        }

        private ToastInfo CreateTooManyNotificationsToast()
        {
            ToastActivationInfo activationInfo = new ToastActivationInfo
            {
                Action = ToastActivationAction.OpenHomePage,
            };

            ToastInfo tooManyItemsToast = new ToastInfo
            {
                Title = "New notifications",
                Description = "You have several new or updated items.",
                Arguments = activationInfo.ToJson()
            };

            return tooManyItemsToast;
        }

        private void ToastManager_ToastActivated(object sender, ToastActivatedEventArgs e)
        {
            string arguments = e.Arguments;

            if (String.IsNullOrWhiteSpace(arguments))
            {
                Log.WarnAndBreak("Handled toast with no activation information.");
                return;
            }

            try
            {
                ToastActivationInfo activationInfo = ToastActivationInfo.FromJson(arguments);

                this.UIService.Dispatcher.BeginInvoke((Action)delegate ()
                {
                    this.Activate(activationInfo);
                });
            }
            catch (Exception ex)
            {
                Log.ErrorAndBreak(ex);
            }
        }

        private void Activate(ToastActivationInfo activationInfo)
        {
            switch (activationInfo.Action)
            {
                case ToastActivationAction.OpenHomePage:
                    this.WindowService.ShowHomePage();
                    break;

                case ToastActivationAction.OpenWorkItem:
                    WorkItemReference reference = new WorkItemReference(activationInfo.WorkItem.ProjectCollectionUri, activationInfo.WorkItem.Id);
                    this.WindowService.ShowWorkItemWindow(reference);
                    break;

                case ToastActivationAction.OpenCodeFlowReview:
                    // TODO: Would be cool to find the PullRequestViewModel and mark it as read here...
                    Process.Start(activationInfo.CodeFlowReview.LaunchClientUri.AbsoluteUri);
                    break;

                default:
                    Log.WarnAndBreak("Unrecognized toast action: " + activationInfo.Action);
                    break;
            }
        }

        public void Dispose()
        {
            if(this.toastManager != null)
            {
                this.toastManager.Dispose();
            }
        }

        private class ToastActivationInfo
        {
            public ToastActivationAction Action { get; set; }

            public ToastWorkItemInfo WorkItem { get; set; }

            public ToastCodeFlowReviewInfo CodeFlowReview { get; set; }

            public string ToJson()
            {
                return JsonConvert.SerializeObject(this, Formatting.None);
            }

            public static ToastActivationInfo FromJson(string json)
            {
                return JsonConvert.DeserializeObject<ToastActivationInfo>(json);
            }
        }

        private enum ToastActivationAction
        {
            None,
            OpenWorkItem,
            OpenCodeFlowReview,
            OpenHomePage
        }

        private class ToastWorkItemInfo
        {
            public Uri ProjectCollectionUri { get; set; }

            public int Id { get; set; }
        }

        private class ToastCodeFlowReviewInfo
        {
            public Uri LaunchClientUri { get; set; }
        }
    }

    public class NotificationScope
    {
        private Dictionary<WorkItemReference, DateTime> currentScope = new Dictionary<WorkItemReference, DateTime>();
        private Dictionary<int, DateTime> pullRequestCurrentScope = new Dictionary<int, DateTime>();

        public bool ShouldNotify(WorkItem item)
        {
            bool notify = false;

            DateTime lastKnownChangeDate;
            var reference = item.GetReference();
            if (!currentScope.TryGetValue(reference, out lastKnownChangeDate) || lastKnownChangeDate < item.ChangedDate().Value)
            {
                currentScope[reference] = item.ChangedDate().Value;
                notify = true;
            }

            return notify;
        }

        public bool ShouldNotify(PullRequestViewModel pullRequest)
        {
            bool notify = false;
             
            DateTime lastKnownChangeDate;
            var key = pullRequest.Reference.PullRequestId;
            if (!pullRequestCurrentScope.TryGetValue(key, out lastKnownChangeDate) || lastKnownChangeDate < pullRequest.ChangedDate)
            {
                pullRequestCurrentScope[key] = pullRequest.ChangedDate;
                notify = true;
            }

            return notify;
        }
    }
}
