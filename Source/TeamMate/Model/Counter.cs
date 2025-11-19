using Microsoft.Tools.TeamMate.Foundation.ComponentModel;
using System;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Model
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class Counter : ObservableObjectBase
    {
        private int count;
        private bool hasCount;

        public event EventHandler Changed;

        public int Count
        {
            get { return this.count; }
            set { SetProperty(ref this.count, value); }
        }

        public bool HasCount
        {
            get { return this.hasCount; }
            set { SetProperty(ref this.hasCount, value); }
        }

        private bool hasCountGreaterThanZero;

        public bool HasCountGreaterThanZero
        {
            get { return this.hasCountGreaterThanZero; }
            set { SetProperty(ref this.hasCountGreaterThanZero, value); }
        }

        private bool isRead;

        public bool IsRead
        {
            get { return this.isRead; }
            set { SetProperty(ref this.isRead, value); }
        }

        public void Reset()
        {
            this.HasCount = false;
            this.IsRead = false;
            this.Count = 0;
            this.HasCountGreaterThanZero = false;

            Changed?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateCount(int count, bool isRead)
        {
            this.Count = count;
            this.HasCount = true;
            this.IsRead = isRead;
            this.HasCountGreaterThanZero = (count > 0);

            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
