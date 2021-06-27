using Microsoft.Tools.TeamMate.Foundation.ComponentModel;
using Microsoft.Tools.TeamMate.Foundation.Xml;
using System;
using System.Xml.Linq;

namespace Microsoft.Tools.TeamMate.Model
{
    public class ApplicationHistory : ObservableObjectBase
    {
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

        public bool HasBeenUsedEnough
        {
            get
            {
                return Launches > 10 || Uptime > TimeSpan.FromDays(7);
            }
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
        }
    }
}
