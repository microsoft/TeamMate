using Microsoft.Internal.Tools.TeamMate.Foundation.ComponentModel;
using Microsoft.Internal.Tools.TeamMate.Foundation.Xml;
using System;
using System.Xml.Linq;

namespace Microsoft.Internal.Tools.TeamMate.Model
{
    public class ApplicationHistory : ObservableObjectBase
    {
        public static readonly TimeSpan IntervalBetweenFeedbackChecks = TimeSpan.FromDays(30);
        public static readonly TimeSpan IntervalBetweenFeedbackPrompts = TimeSpan.FromDays(180);

        private DateTime? lastRun;
        private int launches;
        private TimeSpan uptime;
        private DateTime? firstRun;
        private int workItemsCreated;
        private int workItemViews;

        public DateTime? LastRun
        {
            get { return this.lastRun; }
            set { SetProperty(ref this.lastRun, value); }
        }

        public int Launches
        {
            get { return this.launches; }
            set { SetProperty(ref this.launches, value); }
        }

        public TimeSpan Uptime
        {
            get { return this.uptime; }
            set { SetProperty(ref this.uptime, value); }
        }

        public DateTime? FirstRun
        {
            get { return this.firstRun; }
            set { SetProperty(ref this.firstRun, value); }
        }

        public int WorkItemsCreated
        {
            get { return this.workItemsCreated; }
            set { SetProperty(ref this.workItemsCreated, value); }
        }

        public int WorkItemViews
        {
            get { return this.workItemViews; }
            set { SetProperty(ref this.workItemViews, value); }
        }

        private int feedbacksReported;

        public int FeedbacksReported
        {
            get { return this.feedbacksReported; }
            set { SetProperty(ref this.feedbacksReported, value); }
        }

        private int errorsReported;

        public int ErrorsReported
        {
            get { return this.errorsReported; }
            set { SetProperty(ref this.errorsReported, value); }
        }

        private DateTime? lastFeedbackProvided;

        public DateTime? LastFeedbackProvidedOrPrompted
        {
            get { return this.lastFeedbackProvided; }
            set { SetProperty(ref this.lastFeedbackProvided, value); }
        }

        private DateTime nextTimeToRequestFeedback = DateTime.Now.AddDays(7);

        public DateTime NextTimeToRequestFeedback
        {
            get { return this.nextTimeToRequestFeedback; }
            set { SetProperty(ref this.nextTimeToRequestFeedback, value); }
        }

        private DateTime? lastRated;

        public DateTime? LastRated
        {
            get { return this.lastRated; }
            set { SetProperty(ref this.lastRated, value); }
        }

        public bool RatingEverProvided
        {
            get { return LastRated != null; }
        }

        private bool notInterestedInRating;

        public bool NotInterestedInRating
        {
            get { return this.notInterestedInRating; }
            set { SetProperty(ref this.notInterestedInRating, value); }
        }

        public bool HasBeenUsedEnough
        {
            get
            {
                return Launches > 10 || Uptime > TimeSpan.FromDays(7);
            }
        }

        private bool notInterestedInFeedback;

        public bool NotInterestedInFeedback
        {
            get { return this.notInterestedInFeedback; }
            set { SetProperty(ref this.notInterestedInFeedback, value); }
        }

        public XDocument Write()
        {
            XElement element = new XElement(Schema.History);
            element.SetAttribute(Schema.Version, Schema.CurrentVersion);

            element.SetElementValue<DateTime?>(Schema.LastRun, LastRun);
            element.SetElementValue<int>(Schema.Launches, Launches);
            element.SetElementValue<TimeSpan>(Schema.Uptime, Uptime);
            element.SetElementValue<DateTime?>(Schema.FirstRun, FirstRun);
            element.SetElementValue<int>(Schema.WorkItemsCreated, WorkItemsCreated);
            element.SetElementValue<int>(Schema.WorkItemViews, WorkItemViews);
            element.SetElementValue<int>(Schema.ErrorsReported, ErrorsReported);
            element.SetElementValue<int>(Schema.FeedbacksReported, FeedbacksReported);
            element.SetElementValue<DateTime?>(Schema.LastFeedbackProvidedOrPrompted, LastFeedbackProvidedOrPrompted);
            element.SetElementValue<DateTime?>(Schema.NextTimeToRequestFeedback, NextTimeToRequestFeedback);
            element.SetElementValue<DateTime?>(Schema.LastRated, LastRated);
            element.SetElementValue<bool>(Schema.NotInterestedInRating, NotInterestedInRating);
            element.SetElementValue<bool>(Schema.NotInterestedInFeedback, NotInterestedInFeedback);

            return new XDocument(element);
        }

        public static ApplicationHistory Load(string filename)
        {
            XDocument doc = XDocument.Load(filename);
            XElement root = doc.Root;

            ApplicationHistory history = new ApplicationHistory();

            history.LastRun = root.GetElementValue<DateTime?>(Schema.LastRun);
            history.Launches = root.GetElementValue<int>(Schema.Launches);
            history.Uptime = root.GetElementValue<TimeSpan>(Schema.Uptime);
            history.FirstRun = root.GetElementValue<DateTime?>(Schema.FirstRun);
            history.WorkItemsCreated = root.GetElementValue<int>(Schema.WorkItemsCreated);
            history.WorkItemViews = root.GetElementValue<int>(Schema.WorkItemViews);
            history.ErrorsReported = root.GetElementValue<int>(Schema.ErrorsReported);
            history.FeedbacksReported = root.GetElementValue<int>(Schema.FeedbacksReported);
            history.LastFeedbackProvidedOrPrompted = root.GetElementValue<DateTime?>(Schema.LastFeedbackProvidedOrPrompted);
            history.LastRated = root.GetElementValue<DateTime?>(Schema.LastRated);
            history.NotInterestedInRating = root.GetElementValue<bool>(Schema.NotInterestedInRating);
            history.NotInterestedInFeedback = root.GetElementValue<bool>(Schema.NotInterestedInFeedback);

            var nextTimeToRequestFeedback = root.GetElementValue<DateTime?>(Schema.NextTimeToRequestFeedback);
            if (nextTimeToRequestFeedback != null)
            {
                history.NextTimeToRequestFeedback = nextTimeToRequestFeedback.Value;
            }

            return history;
        }

        private static class Schema
        {
            public static readonly Version CurrentVersion = new Version("1.0");

            public static readonly XName History = "History";
            public static readonly string Version = "Version";

            public static readonly XName LastRun = "LastRun";
            public static readonly XName Launches = "Launches";
            public static readonly XName Uptime = "Uptime";
            public static readonly XName FirstRun = "FirstRun";
            public static readonly XName WorkItemsCreated = "WorkItemsCreated";
            public static readonly XName WorkItemViews = "WorkItemViews";
            public static readonly XName ErrorsReported = "ErrorsReported";
            public static readonly XName FeedbacksReported = "FeedbacksReported";
            public static readonly XName LastFeedbackProvidedOrPrompted = "LastFeedbackProvidedOrPrompted";
            public static readonly XName NextTimeToRequestFeedback = "NextTimeToRequestFeedback";
            public static readonly XName LastRated = "LastRated";
            public static readonly XName NotInterestedInRating = "NotInterestedInRating";
            public static readonly XName NotInterestedInFeedback = "NotInterestedInFeedback";
        }
    }
}
