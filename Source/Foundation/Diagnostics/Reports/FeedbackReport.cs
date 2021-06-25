
namespace Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics.Reports
{
    /// <summary>
    /// A "Send a Smile" feedback report from a user containing the type (smile or frown) and
    /// optional user text.
    /// </summary>
    public class FeedbackReport : UserReportBase
    {
        /// <summary>
        /// The default file extension for feedback report files.
        /// </summary>
        public const string FileExtension = ".fdbx";

        /// <summary>
        /// Gets or sets the feedback type.
        /// </summary>
        public FeedbackType Type { get; set; }

        /// <summary>
        /// Gets or sets the feedback report text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Creates a new instance, and initializes it with a unique id and current timestamp.
        /// </summary>
        /// <returns>The created report.</returns>
        public static FeedbackReport Create()
        {
            FeedbackReport item = new FeedbackReport();
            item.Initialize();
            return item;
        }

        /// <summary>
        /// Deserializes a feedback report from a given file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>The feedback report.</returns>
        public static FeedbackReport FromFile(string filename)
        {
            Assert.ParamIsNotNull(filename, "filename");

            ReportSerializer serializer = new ReportSerializer();
            return serializer.ReadFeedbackReport(filename);
        }

        /// <summary>
        /// Saves the feedback report to the specified filename.
        /// </summary>
        /// <param name="filename">The filename.</param>
        public void Save(string filename)
        {
            Assert.ParamIsNotNull(filename, "filename");

            ReportSerializer serializer = new ReportSerializer();
            serializer.WriteFeedbackReport(this, filename);
        }
    }

    /// <summary>
    /// Represents the type of feedback report.
    /// </summary>
    public enum FeedbackType
    {
        /// <summary>
        /// A positive report (send a smile).
        /// </summary>
        Smile,

        /// <summary>
        /// A negative report (send a frown).
        /// </summary>
        Frown
    }
}
