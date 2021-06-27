using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Documents
{
    /// <summary>
    /// Provides utility methods for manipulating text and text elements.
    /// </summary>
    public static class TextUtilities
    {
        /// <summary>
        /// Gets the text ranges corresponding to text fragments in a given text range.
        /// </summary>
        /// <param name="range">The text range.</param>
        /// <param name="fragments">The fragments.</param>
        /// <returns>A set of text ranges corresponding to each fragment.</returns>
        public static IEnumerable<TextRange> GetTextRanges(TextRange range, IEnumerable<TextFragment> fragments)
        {
            TextOffset previousOffset = new TextOffset(range.Start);

            var orderedFragments = fragments.OrderBy(f => f.StartIndex);
            foreach (var fragment in fragments)
            {
                var textRange = GetTextRange(previousOffset, range.End, fragment.StartIndex, fragment.Length, out previousOffset);
                if (textRange != null)
                {
                    yield return textRange;
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Gets the text pointer at a given offset.
        /// </summary>
        /// <param name="start">The start pointer.</param>
        /// <param name="end">The end pointer.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>The corresponding pointer.</returns>
        public static TextPointer GetTextPointerAtOffset(TextPointer start, TextPointer end, int offset)
        {
            TextOffset previousOffset;
            return GetTextPointerAtOffset(new TextOffset(start), end, offset, out previousOffset);
        }

        /// <summary>
        /// Gets the text pointer at a given offset.
        /// </summary>
        /// <param name="startOffset">The start offset.</param>
        /// <param name="end">The end pointer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="previousOffset">The previous offset.</param>
        /// <returns>The text pointer at the given offset, or <c>null</c> if not found.</returns>
        private static TextPointer GetTextPointerAtOffset(TextOffset startOffset, TextPointer end, int offset, out TextOffset previousOffset)
        {
            if (startOffset.Offset > offset)
            {
                throw new ArgumentException("Start offset is greater than textOffset");
            }

            previousOffset = null;
            TextPointer current = startOffset.Pointer;

            int currentOffset = startOffset.Offset;

            while (current != null && (end == null || current.CompareTo(end) < 0))
            {
                var context = current.GetPointerContext(LogicalDirection.Forward);
                if (context == TextPointerContext.Text)
                {
                    int runLength = current.GetTextRunLength(LogicalDirection.Forward);

                    if (currentOffset + runLength >= offset)
                    {
                        previousOffset = new TextOffset(current, currentOffset);
                        int offsetInRun = (offset - currentOffset);
                        return current.GetPositionAtOffset(offsetInRun, LogicalDirection.Forward);
                    }
                    else
                    {
                        currentOffset += runLength;
                    }
                }

                current = current.GetNextContextPosition(LogicalDirection.Forward);
            }

            return null;
        }

        /// <summary>
        /// Gets a text range from a text pointer.
        /// </summary>
        /// <param name="start">The start pointer.</param>
        /// <param name="end">The end pointer.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="length">The length.</param>
        /// <returns>The text range, or <c>null</c> if it couldn't be extracted.</returns>
        public static TextRange GetTextRange(TextPointer start, TextPointer end, int startIndex, int length)
        {
            TextOffset previousOffset;
            return GetTextRange(new TextOffset(start), end, startIndex, length, out previousOffset);
        }

        /// <summary>
        /// Gets the text range from a given offset.
        /// </summary>
        /// <param name="startOffset">The start offset.</param>
        /// <param name="end">The end pointer.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="length">The length.</param>
        /// <param name="previousOffset">The previous offset.</param>
        /// <returns>The text range, or <c>null</c> if it couldn't be extracted.</returns>
        private static TextRange GetTextRange(TextOffset startOffset, TextPointer end, int startIndex, int length, out TextOffset previousOffset)
        {
            TextPointer blockEnd = null;

            TextPointer blockStart = GetTextPointerAtOffset(startOffset, end, startIndex, out previousOffset);
            if (blockStart != null)
            {
                TextOffset discardOffset;
                blockEnd = GetTextPointerAtOffset(previousOffset, end, startIndex + length, out discardOffset);

                if (blockEnd != null)
                {
                    return new TextRange(blockStart, blockEnd);
                }
            }

            return null;
        }

        private class TextOffset
        {
            public TextOffset(TextPointer pointer)
                : this(pointer, 0)
            {
            }

            public TextOffset(TextPointer pointer, int offset)
            {
                this.Pointer = pointer;
                this.Offset = offset;
            }

            public TextPointer Pointer { get; private set; }
            public int Offset { get; private set; }
        }
    }
}
