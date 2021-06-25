using Microsoft.Internal.Tools.TeamMate.Foundation.Threading;
using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Internal.Tools.TeamMate.Model;
using Microsoft.Internal.Tools.TeamMate.Services;
using System;
using System.Threading.Tasks;

namespace Microsoft.Internal.Tools.TeamMate.ViewModels
{
    public abstract class QueryViewModelBase : ViewModelBase
    {
        private string name;
        private bool showNotifications;
        private DateTime? lastUpdated;
        private bool isHot;
        private int itemCount;
        private ConditionalFormattingRule formattingRule;

        public event EventHandler QueryExecuting;
        public event EventHandler QueryExecuted;

        private TaskContext progressContext;

        public TaskContext ProgressContext
        {
            get { return this.progressContext; }
            set { SetProperty(ref this.progressContext, value); }
        }

        public string Name
        {
            get { return this.name; }
            set { SetProperty(ref this.name, value); }
        }

        public bool ShowNotifications
        {
            get { return this.showNotifications; }
            set { SetProperty(ref this.showNotifications, value); }
        }

        public virtual Task RefreshAsync(NotificationScope notificationScope = null)
        {
            return TaskUtilities.NullTask;
        }

        protected void FireQueryExecuting()
        {
            QueryExecuting?.Invoke(this, EventArgs.Empty);
        }

        protected void FireQueryExecuted()
        {
            QueryExecuted?.Invoke(this, EventArgs.Empty);
        }

        public DateTime? LastUpdated
        {
            get { return this.lastUpdated; }
            set { SetProperty(ref this.lastUpdated, value); }
        }

        public bool IsHot
        {
            get { return this.isHot; }
            set { SetProperty(ref this.isHot, value); }
        }

        public int ItemCount
        {
            get { return this.itemCount; }
            set
            {
                if (SetProperty(ref this.itemCount, value))
                {
                    InvalidateIsHot();
                }
            }
        }

        private int unreadItemCount;

        public int UnreadItemCount
        {
            get { return this.unreadItemCount; }
            set { SetProperty(ref this.unreadItemCount, value); }
        }

        private bool includeInItemCountSummary;

        public bool IncludeInItemCountSummary
        {
            get { return this.includeInItemCountSummary; }
            set { SetProperty(ref this.includeInItemCountSummary, value); }
        }

        public ConditionalFormattingRule FormattingRule
        {
            get { return this.formattingRule; }
            set
            {
                ConditionalFormattingRule oldValue = this.formattingRule;
                if (SetProperty(ref this.formattingRule, value))
                {
                    if (oldValue != null)
                    {
                        oldValue.Changed -= HandleFormattingRuleChanged;
                    }

                    if (this.formattingRule != null)
                    {
                        this.formattingRule.Changed += HandleFormattingRuleChanged;
                    }

                    InvalidateIsHot();

                    QueryExecuting?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void HandleFormattingRuleChanged(object sender, EventArgs e)
        {
            InvalidateIsHot();
        }

        private void InvalidateIsHot()
        {
            IsHot = (formattingRule != null && formattingRule.Evaluate(ItemCount));
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
