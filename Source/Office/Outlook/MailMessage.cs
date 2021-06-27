using System;
using System.Collections.Generic;

namespace Microsoft.Tools.TeamMate.Office.Outlook
{
    public class MailMessage
    {
        private const string DefaultMailFont = "Calibri";
        private const string DefaultMailFontSize = "11pt";

        private ICollection<string> toList = new List<string>();
        private ICollection<string> ccList = new List<string>();
        private ICollection<string> attachments = new List<string>();

        public MailMessage()
        {
            this.Importance = MessageImportance.Normal;
        }

        public ICollection<string> To
        {
            get { return this.toList; }
        }

        public ICollection<string> CC
        {
            get { return this.ccList; }
        }

        public ICollection<string> Attachments
        {
            get { return this.attachments; }
        }

        /// <summary>
        /// The subject of the email message.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// The body of the email message.
        /// </summary>
        public string HtmlBody { get; set; }

        /// <summary>
        /// An optional reminder date for receivers.
        /// </summary>
        public DateTime? ReminderDate { get; set; }

        /// <summary>
        /// Gets or sets the importance of the message.
        /// </summary>
        public MessageImportance Importance { get; set; }

        public static string WrapHtmlInDefaultFont(string htmlBody)
        {
            if (htmlBody != null)
            {
                htmlBody = String.Format("<div style='font-family: {0}, sans-serif; font-size: {1}'>{2}</div>",
                                         DefaultMailFont, DefaultMailFontSize, htmlBody);
            }

            return htmlBody;
        }
    }

    public enum MessageImportance
    {
        Low = Microsoft.Office.Interop.Outlook.OlImportance.olImportanceLow,
        Normal = Microsoft.Office.Interop.Outlook.OlImportance.olImportanceNormal,
        High = Microsoft.Office.Interop.Outlook.OlImportance.olImportanceHigh
    }
}
