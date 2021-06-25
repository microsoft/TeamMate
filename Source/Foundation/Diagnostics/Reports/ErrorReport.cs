using System;
using System.Text;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics.Reports
{
    /// <summary>
    /// An error report that contains exception information.
    /// </summary>
    public class ErrorReport : UserReportBase
    {
        /// <summary>
        /// The default file extension for error report files.
        /// </summary>
        public const string FileExtension = ".errx";

        /// <summary>
        /// Gets or sets the exception in this report.
        /// </summary>
        public ExceptionInfo Exception { get; set; }

        /// <summary>
        /// Creates a new error report, and initializes it with a unique id and current timestamp.
        /// </summary>
        /// <param name="error">The exception.</param>
        /// <returns>The created error report.</returns>
        public static ErrorReport Create(Exception error)
        {
            Assert.ParamIsNotNull(error, "error");

            ErrorReport report = new ErrorReport();
            report.Initialize();
            report.Exception = ExceptionInfo.Create(error);
            return report;
        }

        /// <summary>
        /// Parses an error report from an error report file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>The parsed error report.</returns>
        public static ErrorReport FromFile(string filename)
        {
            Assert.ParamIsNotNull(filename, "filename");

            ReportSerializer serializer = new ReportSerializer();
            return serializer.ReadErrorReport(filename);
        }

        /// <summary>
        /// Saves the error report to the specified file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        public void Save(string filename)
        {
            Assert.ParamIsNotNull(filename, "filename");

            ReportSerializer serializer = new ReportSerializer();
            serializer.Write(this, filename);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(String.Format("Date: {0}", Date));

            if (!String.IsNullOrEmpty(EmailAddress))
            {
                sb.AppendLine(String.Format("From: {0}", EmailAddress));
            }

            sb.AppendLine("Exception:");
            sb.AppendLine(Exception.ToString());
            return sb.ToString();
        }
    }
}
