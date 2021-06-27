using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Documents
{
    /// <summary>
    /// A helper class to highlight text in documents and text elements.
    /// </summary>
    public class Highlighter
    {
        private static Color DefaultHighlightColor = (Color)ColorConverter.ConvertFromString("#FFEE80");    // Yello highlighter

        public static Lazy<Brush> DefaultHighlightBrush = new Lazy<Brush>(() => new SolidColorBrush(DefaultHighlightColor));

        /// <summary>
        /// The default highlight operation to color a range.
        /// </summary>
        /// <param name="range">The range.</param>
        private static void DefaultHighlightDelegate(TextRange range)
        {
            range.ApplyPropertyValue(TextElement.BackgroundProperty, DefaultHighlightBrush.Value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Highlighter"/> class with a default highlight delegate.
        /// </summary>
        public Highlighter()
            : this(DefaultHighlightDelegate)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Highlighter"/> class.
        /// </summary>
        /// <param name="highlightDelegate">The highlight delegate.</param>
        public Highlighter(HighlightDelegate highlightDelegate)
        {
            Assert.ParamIsNotNull(highlightDelegate, "highlightDelegate");

            this.HighlightDelegate = highlightDelegate;
        }

        /// <summary>
        /// Gets the highlight delegate.
        /// </summary>
        /// <value>
        /// The highlight delegate.
        /// </value>
        public HighlightDelegate HighlightDelegate { get; private set; }

        /// <summary>
        /// Highlights the items in the specified document that match the given regular expression.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="regex">The regular expression.</param>
        public void Highlight(FlowDocument document, Regex regex)
        {
            Assert.ParamIsNotNull(document, "document");
            Assert.ParamIsNotNull(regex, "regex");

            Highlight(new TextRange(document.ContentStart, document.ContentEnd), regex);
        }

        /// <summary>
        /// Highlights the items in the specified text block that match the given regular expression.
        /// </summary>
        /// <param name="textBlock">The text block.</param>
        /// <param name="regex">The regular expression.</param>
        public void Highlight(TextBlock textBlock, Regex regex)
        {
            Assert.ParamIsNotNull(textBlock, "textBlock");
            Assert.ParamIsNotNull(regex, "regex");

            Highlight(new TextRange(textBlock.ContentStart, textBlock.ContentEnd), regex);
        }

        /// <summary>
        /// Highlights the items in the specified text element that match the given regular expression.
        /// </summary>
        /// <param name="element">The text element.</param>
        /// <param name="regex">The regular expression.</param>
        public void Highlight(TextElement element, Regex regex)
        {
            Assert.ParamIsNotNull(element, "element");
            Assert.ParamIsNotNull(regex, "regex");

            Highlight(new TextRange(element.ContentStart, element.ContentEnd), regex);
        }

        /// <summary>
        /// Highlights the items in the specified text range that match the given regular expression.
        /// </summary>
        /// <param name="range">The text range.</param>
        /// <param name="regex">The regex.</param>
        public void Highlight(TextRange range, Regex regex)
        {
            Assert.ParamIsNotNull(range, "range");
            Assert.ParamIsNotNull(regex, "regex");

            Highlight(range, GetMatchingFragments(range, regex));
        }

        /// <summary>
        /// Highlights a fragment in specified document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="fragment">The fragment.</param>
        public void Highlight(FlowDocument document, TextFragment fragment)
        {
            Assert.ParamIsNotNull(document, "document");
            Assert.ParamIsNotNull(fragment, "fragment");

            Highlight(new TextRange(document.ContentStart, document.ContentEnd), fragment);
        }

        /// <summary>
        /// Highlights a fragment in specified text block.
        /// </summary>
        /// <param name="textBlock">The text block.</param>
        /// <param name="fragment">The fragment.</param>
        public void Highlight(TextBlock textBlock, TextFragment fragment)
        {
            Assert.ParamIsNotNull(textBlock, "textBlock");
            Assert.ParamIsNotNull(fragment, "fragment");

            Highlight(new TextRange(textBlock.ContentStart, textBlock.ContentEnd), fragment);
        }

        /// <summary>
        /// Highlights a fragment in specified text element.
        /// </summary>
        /// <param name="element">The text element.</param>
        /// <param name="fragment">The fragment.</param>
        public void Highlight(TextElement element, TextFragment fragment)
        {
            Assert.ParamIsNotNull(element, "element");
            Assert.ParamIsNotNull(fragment, "fragment");

            Highlight(new TextRange(element.ContentStart, element.ContentEnd), fragment);
        }

        /// <summary>
        /// Highlights a fragment in specified text range.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <param name="fragment">The fragment.</param>
        public void Highlight(TextRange range, TextFragment fragment)
        {
            Assert.ParamIsNotNull(range, "range");
            Assert.ParamIsNotNull(fragment, "fragment");

            Highlight(range, new TextFragment[] { fragment });
        }

        /// <summary>
        /// Highlights multiple fragments in specified document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="fragments">The fragments.</param>
        public void Highlight(FlowDocument document, IEnumerable<TextFragment> fragments)
        {
            Assert.ParamIsNotNull(document, "document");
            Assert.ParamIsNotNull(fragments, "fragments");

            Highlight(new TextRange(document.ContentStart, document.ContentEnd), fragments);
        }

        /// <summary>
        /// Highlights multiple fragments in specified text block.
        /// </summary>
        /// <param name="textBlock">The text block.</param>
        /// <param name="fragments">The fragments.</param>
        public void Highlight(TextBlock textBlock, IEnumerable<TextFragment> fragments)
        {
            Assert.ParamIsNotNull(textBlock, "textBlock");
            Assert.ParamIsNotNull(fragments, "fragments");

            Highlight(new TextRange(textBlock.ContentStart, textBlock.ContentEnd), fragments);
        }

        /// <summary>
        /// Highlights multiple fragments in specified text element.
        /// </summary>
        /// <param name="element">The text element.</param>
        /// <param name="fragments">The fragments.</param>
        public void Highlight(TextElement element, IEnumerable<TextFragment> fragments)
        {
            Assert.ParamIsNotNull(element, "element");
            Assert.ParamIsNotNull(fragments, "fragments");

            Highlight(new TextRange(element.ContentStart, element.ContentEnd), fragments);
        }

        /// <summary>
        /// Highlights multiple fragments in specified text range.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <param name="fragments">The fragments.</param>
        public void Highlight(TextRange range, IEnumerable<TextFragment> fragments)
        {
            Assert.ParamIsNotNull(range, "range");
            Assert.ParamIsNotNull(fragments, "fragments");

            foreach (var childRange in TextUtilities.GetTextRanges(range, fragments))
            {
                HighlightDelegate(childRange);
            }
        }

        /// <summary>
        /// Gets the fragments in the specified text range that match a regular expression.
        /// </summary>
        /// <param name="range">The text range.</param>
        /// <param name="regex">The regular expression.</param>
        /// <returns>The matching text fragments.</returns>
        private static IEnumerable<TextFragment> GetMatchingFragments(TextRange range, Regex regex)
        {
            return regex.Matches(range.Text).OfType<Match>().Select(match => new TextFragment(match.Index, match.Length));
        }
    }

    /// <summary>
    /// A delegate used to highlight a given range.
    /// </summary>
    /// <param name="range">The range.</param>
    public delegate void HighlightDelegate(TextRange range);
}
