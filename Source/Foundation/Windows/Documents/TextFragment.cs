using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Documents
{
    /// <summary>
    /// Represents a fragment of text in a text.
    /// </summary>
    public class TextFragment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextFragment"/> class.
        /// </summary>
        /// <param name="startIndex">The start index.</param>
        /// <param name="length">The length.</param>
        public TextFragment(int startIndex, int length)
        {
            Assert.ParamIsNotNegative(startIndex, "startIndex");
            Assert.ParamIsNotNegative(length, "length");

            this.StartIndex = startIndex;
            this.Length = length;
        }

        /// <summary>
        /// Gets the start index.
        /// </summary>
        public int StartIndex { get; private set; }

        /// <summary>
        /// Gets the length.
        /// </summary>
        public int Length { get; private set; }
    }
}
