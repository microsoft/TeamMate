using System;
using System.Collections.Generic;

namespace Microsoft.Tools.TeamMate.Foundation.Diagnostics.Reports
{
    /// <summary>
    /// Base class for diagnostic reports from end users.
    /// </summary>
    public abstract class UserReportBase
    {
        /// <summary>
        /// Gets or sets the unique id for this report.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the date of the report.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the system information.
        /// </summary>
        public SystemInfo SystemInfo { get; set; }

        /// <summary>
        /// Gets or sets the email address of the person reporting the issue.
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the User display name.
        /// </summary>
        public string UserDisplayName { get; set; }

        /// <summary>
        /// Gets the collection of attachments associated with the report.
        /// </summary>
        public ICollection<Attachment> Attachments { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserReportBase"/> class.
        /// </summary>
        protected UserReportBase()
        {
            this.Attachments = new List<Attachment>();
        }

        /// <summary>
        /// Initializes this instance with a unique id and timestamp.
        /// </summary>
        protected void Initialize()
        {
            this.Id = Guid.NewGuid();
            this.Date = DateTime.Now;
        }
    }
}
