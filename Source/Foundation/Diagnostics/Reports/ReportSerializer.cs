using Microsoft.Internal.Tools.TeamMate.Foundation.IO.Packaging;
using Microsoft.Internal.Tools.TeamMate.Foundation.Net.Mime;
using Microsoft.Internal.Tools.TeamMate.Foundation.Xml;
using System;
using System.IO;
using System.IO.Packaging;
using System.Net.Mime;
using System.Xml.Linq;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics.Reports
{
    /// <summary>
    /// Serializes and deserializes user reports from files.
    /// </summary>
    internal class ReportSerializer
    {
        private DiagnosticsSerializer diagnosticsSerializer = new DiagnosticsSerializer();

        /// <summary>
        /// Reads a feedback report from a file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>The read report.</returns>
        public FeedbackReport ReadFeedbackReport(string filename)
        {
            Assert.ParamIsNotNull(filename, "filename");

            FeedbackReport report = new FeedbackReport();

            using (Package package = Package.Open(filename, FileMode.Open))
            {
                var feedbackPart = package.GetSingleRelatedPart(Schema.Relationships.Feedback);
                if (feedbackPart == null)
                {
                    throw new Exception("Invalid package");
                }

                ReadUserReport(report, package);
                ReadFeedbackReport(report, feedbackPart);
            }

            return report;
        }

        /// <summary>
        /// Reads the base information from a user report.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="package">The package containing the information.</param>
        private void ReadUserReport(UserReportBase report, Package package)
        {
            ReadReportInfo(report, package);
            report.SystemInfo = ReadSystemInfo(package);
            ReadAttachments(report, package);
        }

        /// <summary>
        /// Reads the system information from a package.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <returns>The system information.</returns>
        private SystemInfo ReadSystemInfo(Package package)
        {
            SystemInfo result = null;

            var systemInfoPart = package.GetSingleRelatedPart(Schema.Relationships.SystemInfo);
            if (systemInfoPart != null)
            {
                using (Stream stream = systemInfoPart.OpenRead())
                {
                    result = diagnosticsSerializer.DeserializeSystemInfo(stream);
                }
            }

            return result;
        }

        /// <summary>
        /// Reads the attachments contained in a package into a user report.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="package">The package.</param>
        private void ReadAttachments(UserReportBase report, Package package)
        {
            foreach (var relationship in package.GetRelationshipsByType(Schema.Relationships.Attachment))
            {
                PackagePart part = relationship.TryGetInternalPart();
                if (part != null)
                {
                    string name = Path.GetFileName(part.Uri.OriginalString);
                    byte[] content;

                    using (var inputStream = part.OpenRead())
                    using (var outputStream = new MemoryStream())
                    {
                        inputStream.CopyTo(outputStream);
                        content = outputStream.GetBuffer();
                    }

                    Attachment attachment = new Attachment(name, content);
                    report.Attachments.Add(attachment);
                }
            }
        }

        /// <summary>
        /// Reads the feedback report information from a package part.
        /// </summary>
        /// <param name="feedback">The feedback.</param>
        /// <param name="part">The part.</param>
        private void ReadFeedbackReport(FeedbackReport feedback, PackagePart part)
        {
            XDocument doc;
            using (var stream = part.GetStream())
            {
                doc = XDocument.Load(stream);
            }

            var feedbackElement = doc.Element(Schema.FeedbackXml.Feedback);
            feedback.Type = feedbackElement.GetElementValue<FeedbackType>(Schema.FeedbackXml.Type);
            feedback.Text = feedbackElement.GetElementValue<string>(Schema.FeedbackXml.Text);
        }

        /// <summary>
        /// Reads the core report information from a package.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="package">The package.</param>
        private void ReadReportInfo(UserReportBase report, Package package)
        {
            var part = package.GetSingleRelatedPart(Schema.Relationships.ReportInfo);
            if (part != null)
            {
                XDocument doc;
                using (var stream = part.GetStream())
                {
                    doc = XDocument.Load(stream);
                }

                var feedbackElement = doc.Element(Schema.ReportInfoXml.ReportInfo);
                report.Id = feedbackElement.GetElementValue<Guid>(Schema.ReportInfoXml.Id);
                report.Date = feedbackElement.GetElementValue<DateTime>(Schema.ReportInfoXml.Date);
                report.EmailAddress = feedbackElement.GetElementValue<string>(Schema.ReportInfoXml.EmailAddress);
                report.UserDisplayName = feedbackElement.GetElementValue<string>(Schema.ReportInfoXml.UserDisplayName);
            }
        }

        /// <summary>
        /// Writes a feedback report to a file.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="filename">The filename.</param>
        public void WriteFeedbackReport(FeedbackReport report, string filename)
        {
            Assert.ParamIsNotNull(report, "report");
            Assert.ParamIsNotNull(filename, "filename");

            using (Package package = Package.Open(filename, FileMode.Create))
            {
                var feedbackPart = package.CreateRelatedPart(Schema.Parts.Feedback, MediaTypeNames.Text.Xml, Schema.Relationships.Feedback);

                WriteUserReport(report, package);
                WriteFeedback(report, feedbackPart);
            }
        }

        /// <summary>
        /// Writes the system information to a package.
        /// </summary>
        /// <param name="systemInfo">The system information.</param>
        /// <param name="package">The package.</param>
        private void WriteSystemInfo(SystemInfo systemInfo, Package package)
        {
            // For API usability, allowing parameter to be null
            if (systemInfo != null)
            {
                var systemInfoPart = package.CreateRelatedPart(Schema.Parts.SystemInfo, MediaTypeNames.Text.Xml, Schema.Relationships.SystemInfo);
                using (Stream stream = systemInfoPart.OpenWrite())
                {
                    diagnosticsSerializer.Serialize(systemInfo, stream);
                }
            }
        }

        /// <summary>
        /// Writes the attachments to a package.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="package">The package.</param>
        private void WriteAttachments(UserReportBase report, Package package)
        {
            foreach (var attachment in report.Attachments)
            {
                Uri partUri = new Uri(Schema.Parts.AttachmentsBaseUri + attachment.Name, UriKind.Relative);
                string mimeType = MimeTypes.GetMimeType(Path.GetExtension(attachment.Name));

                var attachmentPart = package.CreateRelatedPart(partUri, mimeType, Schema.Relationships.Attachment);
                using (var stream = attachmentPart.OpenWrite())
                {
                    stream.Write(attachment.Content, 0, attachment.Content.Length);
                }
            }
        }

        /// <summary>
        /// Writes the feedback report to an output package part.
        /// </summary>
        /// <param name="feedback">The feedback.</param>
        /// <param name="part">The part.</param>
        private void WriteFeedback(FeedbackReport feedback, PackagePart part)
        {
            var element = new XElement(Schema.FeedbackXml.Feedback);
            element.SetElementValue<FeedbackType>(Schema.FeedbackXml.Type, feedback.Type);
            element.SetElementValue<string>(Schema.FeedbackXml.Text, feedback.Text);

            XDocument doc = new XDocument(element);
            using (var stream = part.OpenWrite())
            {
                doc.Save(stream);
            }
        }

        /// <summary>
        /// Writes the report information to a package.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="package">The package.</param>
        private void WriteReportInfo(UserReportBase report, Package package)
        {
            var element = new XElement(Schema.ReportInfoXml.ReportInfo);
            element.SetElementValue<Guid>(Schema.ReportInfoXml.Id, report.Id);
            element.SetElementValue<DateTime>(Schema.ReportInfoXml.Date, report.Date);
            element.SetElementValue<string>(Schema.ReportInfoXml.EmailAddress, report.EmailAddress);
            element.SetElementValue<string>(Schema.ReportInfoXml.UserDisplayName, report.UserDisplayName);

            XDocument doc = new XDocument(element);

            var part = package.CreateRelatedPart(Schema.Parts.ReportInfo, MediaTypeNames.Text.Xml, Schema.Relationships.ReportInfo);
            using (Stream stream = part.OpenWrite())
            {
                doc.Save(stream);
            }
        }

        /// <summary>
        /// Reads an error report from a file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>The read error report.</returns>
        public ErrorReport ReadErrorReport(string filename)
        {
            Assert.ParamIsNotNull(filename, "filename");

            ErrorReport errorReport = new ErrorReport();

            using (Package package = Package.Open(filename, FileMode.Open))
            {
                ReadUserReport(errorReport, package);
                errorReport.Exception = ReadExceptionInfo(package);
            }

            return errorReport;
        }

        /// <summary>
        /// Reads the exception information from a package..
        /// </summary>
        /// <param name="package">The package.</param>
        /// <returns>The exception information or <c>null if none was contained</c>.</returns>
        private ExceptionInfo ReadExceptionInfo(Package package)
        {
            ExceptionInfo exceptionInfo = null;

            var part = package.GetSingleRelatedPart(Schema.Relationships.Exception);
            if (part != null)
            {
                using (Stream stream = part.OpenRead())
                {
                    exceptionInfo = diagnosticsSerializer.DeserializeExceptionInfo(stream);
                }
            }

            return exceptionInfo;
        }

        /// <summary>
        /// Writes an error report to a file.
        /// </summary>
        /// <param name="errorReport">The error report.</param>
        /// <param name="filename">The filename.</param>
        public void Write(ErrorReport errorReport, string filename)
        {
            Assert.ParamIsNotNull(errorReport, "errorReport");
            Assert.ParamIsNotNull(filename, "filename");

            using (Package package = Package.Open(filename, FileMode.Create))
            {
                WriteUserReport(errorReport, package);
                WriteExceptionInfo(errorReport.Exception, package);
            }
        }

        /// <summary>
        /// Writes the common user report information to a package.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="package">The package.</param>
        private void WriteUserReport(UserReportBase report, Package package)
        {
            WriteReportInfo(report, package);
            WriteSystemInfo(report.SystemInfo, package);
            WriteAttachments(report, package);
        }

        /// <summary>
        /// Writes the exception information to a package.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="package">The package.</param>
        private void WriteExceptionInfo(ExceptionInfo exception, Package package)
        {
            // For API usability, allowing parameter to be null
            if (exception != null)
            {
                var part = package.CreateRelatedPart(Schema.Parts.Exception, MediaTypeNames.Text.Xml, Schema.Relationships.Exception);
                using (Stream stream = part.OpenWrite())
                {
                    diagnosticsSerializer.Serialize(exception, stream);
                }
            }
        }

        /// <summary>
        /// The XML schema for user reports.
        /// </summary>
        internal static class Schema
        {
            public const string Id = "http://schemas.microsoft.com/TeamMate/UserReports/2014";

            public static class Parts
            {
                public static readonly Uri Feedback = new Uri("feedback.xml", UriKind.Relative);
                public static readonly Uri Exception = new Uri("exception.xml", UriKind.Relative);
                public static readonly Uri SystemInfo = new Uri("systemInfo.xml", UriKind.Relative);
                public static readonly Uri ApplicationInfo = new Uri("applicationInfo.xml", UriKind.Relative);
                public static readonly Uri ReportInfo = new Uri("reportInfo.xml", UriKind.Relative);

                public const string AttachmentsBaseUri = "attachments/";
            }

            public static class Relationships
            {
                // Follow a convention similar to the OpenXML format, e.g. URL: http://schemas.openxmlformats.org/officeDocument/2006/relationships/attachedTemplate
                private const string RelationshipBaseUri = Schema.Id + "/relationships/";

                public const string Feedback = RelationshipBaseUri + "feedback";
                public const string Exception = RelationshipBaseUri + "exception";
                public const string SystemInfo = RelationshipBaseUri + "systemInfo";
                public const string ApplicationInfo = RelationshipBaseUri + "applicationInfo";
                public const string ReportInfo = RelationshipBaseUri + "reportInfo";
                public const string Attachment = RelationshipBaseUri + "attachment";
            }

            public static class ReportInfoXml
            {
                public static readonly XNamespace Namespace = Schema.Id + "/report";

                public static readonly XName ReportInfo = Namespace + "ReportInfo";
                public static readonly XName Id = Namespace + "Id";
                public static readonly XName Date = Namespace + "Date";
                public static readonly XName EmailAddress = Namespace + "EmailAddress";
                public static readonly XName UserDisplayName = Namespace + "UserDisplayName";
            }

            public static class FeedbackXml
            {
                public static readonly XNamespace Namespace = Schema.Id + "/feedback";

                public static readonly XName Feedback = Namespace + "Feedback";
                public static readonly XName Type = Namespace + "Type";
                public static readonly XName Text = Namespace + "Text";
            }
        }
    }
}
