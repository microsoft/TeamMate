using System;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Microsoft.Tools.TeamMate.WindowsRuntime.UI.Notifications
{
    public class ToastNotificationManager
    {
        private string applicationId;
        private ToastNotifier toastNotifier;

        public event EventHandler<ToastNotificationManagerEventArgs> ToastOccurred;

        public ToastNotificationManager(string applicationId)
        {
            this.applicationId = applicationId;
        }

        public void Show(string toastXml)
        {
            XmlDocument toastContent = new XmlDocument();
            toastContent.LoadXml(toastXml);

            ToastNotification toast = new ToastNotification(toastContent);
            toast.Activated += Toast_Activated;
            toast.Dismissed += Toast_Dismissed;
            toast.Failed += Toast_Failed;

            this.GetNotifier().Show(toast);
        }

        private ToastNotifier GetNotifier()
        {
            if (this.toastNotifier == null)
            {
                this.toastNotifier = global::Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier(this.applicationId);
            }

            return this.toastNotifier;
        }

        private void Toast_Failed(ToastNotification sender, ToastFailedEventArgs args)
        {
            this.ToastOccurred?.Invoke(this, new ToastNotificationManagerEventArgs
            {
                EventType = ToastNotificationEventType.Failed,
                FailedErrorCode = args.ErrorCode
            });
        }

        private void Toast_Dismissed(ToastNotification sender, ToastDismissedEventArgs args)
        {
            this.ToastOccurred?.Invoke(this, new ToastNotificationManagerEventArgs
            {
                EventType = ToastNotificationEventType.Dismissed,
                DismissReason = (ToastDismissalReason)args.Reason
            });
        }

        private void Toast_Activated(ToastNotification sender, object args)
        {
            if (this.ToastOccurred != null)
            {
                ToastActivatedEventArgs activatedEventArgs = args as ToastActivatedEventArgs;
                string activatedArguments = (activatedEventArgs != null) ? activatedEventArgs.Arguments : null;

                this.ToastOccurred(this, new ToastNotificationManagerEventArgs
                {
                    EventType = ToastNotificationEventType.Activated,
                    ActivatedArguments = activatedArguments
                });
            }
        }
    }

    public class ToastNotificationManagerEventArgs : EventArgs
    {
        public string ActivatedArguments { get; internal set; }

        public ToastDismissalReason DismissReason { get; internal set; }

        public ToastNotificationEventType EventType { get; internal set; }

        public Exception FailedErrorCode { get; internal set; }
    }

    public enum ToastNotificationEventType
    {
        Activated,
        Dismissed,
        Failed
    }

    /// <summary>
    /// Mirrors Windows.UI.Notifications.ToastDismissalReason values.
    /// These values are hardcoded to match the Windows API as of Nov 2025.
    /// If the Windows API changes, update these values accordingly.
    /// Consider adding runtime validation to ensure values stay in sync.
    /// </summary>
    public enum ToastDismissalReason
    {
        UserCanceled = 0,
        ApplicationHidden = 1,
        TimedOut = 2
    }
}