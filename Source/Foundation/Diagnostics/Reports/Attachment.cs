
namespace Microsoft.Tools.TeamMate.Foundation.Diagnostics.Reports
{
    /// <summary>
    /// An item attached to a diagnostics report.
    /// </summary>
    public class Attachment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Attachment"/> class.
        /// </summary>
        /// <param name="name">The attachment name.</param>
        /// <param name="content">The attachment content.</param>
        public Attachment(string name, byte[] content)
        {
            Assert.ParamIsNotNull(name, "name");
            Assert.ParamIsNotNull(content, "content");

            this.Name = name;
            this.Content = content;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the content.
        /// </summary>
        public byte[] Content { get; private set; }
    }
}
