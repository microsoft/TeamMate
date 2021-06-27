using System.Collections.Generic;

namespace Microsoft.Tools.TeamMate.Office.Outlook
{
    public class Appointment
    {
        private ICollection<string> recipients = new List<string>();

        public string Subject { get; set; }

        public string Body { get; set; }

        public string RtfBody { get; set; }

        public ICollection<string> RequiredAttendees
        {
            get { return this.recipients; }
        }
    }
}
