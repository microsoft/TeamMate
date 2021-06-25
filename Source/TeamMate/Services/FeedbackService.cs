using Microsoft.HockeyApp;
using Microsoft.HockeyApp.Model;
using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics.Reports;
using Microsoft.Internal.Tools.TeamMate.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ApplicationInfo = Microsoft.Internal.Tools.TeamMate.Model.TeamMateApplicationInfo;

namespace Microsoft.Internal.Tools.TeamMate.Services
{
    public class FeedbackService
    {

        [Import]
        public HistoryService HistoryService { get; set; }

        public async Task SendFeedbackAsync(FeedbackReport report)
        {
            Assert.ParamIsNotNull(report, "item");

            report.SystemInfo = await ApplicationInfo.GetSystemInfoAsync();

            Telemetry.Event(TelemetryEvents.FeedbackReported);
            var history = this.HistoryService.History;

            history.FeedbacksReported++;
            history.LastFeedbackProvidedOrPrompted = DateTime.Now;

            var thread = HockeyClient.Current.CreateFeedbackThread();
            var feedbackType = (report.Type == FeedbackType.Smile) ? "Smile" : "Frown";
            var subject = GetSubject(feedbackType, report.Text);

            ICollection<IFeedbackAttachment> attachments = null;
            if (report.Attachments.Any())
            {
                attachments = new List<IFeedbackAttachment>();
                foreach (var a in report.Attachments)
                {
                    string mimeType = MimeMapping.GetMimeMapping(a.Name);
                    var item = new FeedbackAttachment(a.Name, a.Content, mimeType);
                    attachments.Add(item);
                }
            }

            await thread.PostFeedbackMessageAsync(TrimFeedbackBody(report.Text), report.EmailAddress, subject, report.UserDisplayName, attachments);
        }

        private static readonly char[] NewLineChars = "\r\n".ToArray();
        private static readonly char[] WhitespaceChars = " \t\r\n".ToArray();

        private static string TrimFeedbackBody(string body)
        {
            string text = body;
            TrimIfNeeded(ref text, 2048);
            return text;
        }

        private static string GetSubject(string type, string text)
        {
            string subject = (text ?? "").Trim();

            bool trimmed = false;

            int indexOfNewLine = subject.IndexOfAny(NewLineChars);
            if (indexOfNewLine >= 0)
            {
                subject = subject.Substring(0, indexOfNewLine);
                trimmed = true;
            }

            trimmed |= TrimIfNeeded(ref subject, 160);

            if (trimmed)
            {
                subject += "...";
            }

            return $"{type}: {subject}";
        }

        private static bool TrimIfNeeded(ref string text, int maxLength)
        {
            if (text.Length > maxLength)
            {
                int trimPoint = text.LastIndexOfAny(WhitespaceChars, maxLength);
                if(trimPoint <= 0)
                {
                    trimPoint = maxLength;
                }

                text = text.Substring(0, trimPoint);
                return true;
            }

            return false;
        }
    }
}
