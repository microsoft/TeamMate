// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Windows.Documents;
using Microsoft.Tools.TeamMate.Foundation.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using ListView = Microsoft.Tools.TeamMate.Foundation.Windows.Controls.Data.ListView;

namespace Microsoft.Tools.TeamMate.Utilities
{
    public static class SearchTextHighlighter
    {

        public static readonly DependencyProperty IsHighlightedProperty = DependencyProperty.RegisterAttached(
            "IsHighlighted", typeof(bool), typeof(SearchTextHighlighter)
        );

        public static void SetIsHighlighted(DependencyObject element, bool value)
        {
            element.SetValue(IsHighlightedProperty, value);
        }

        public static bool GetIsHighlighted(DependencyObject element)
        {
            return (bool)element.GetValue(IsHighlightedProperty);
        }

        private static Highlighter highlighter;

        static SearchTextHighlighter()
        {
            // TODO: Choose the final highlighter to be used
            // this.highlighter = new Highlighter(CustomHighlight);
            highlighter = new Highlighter();

        }

        public static void Highlight(ListView listView, SearchExpression searchExpression)
        {
            var words = searchExpression.Tokens.Select(t => t.Value).ToArray();
            if (words.Any())
            {
                Regex regex = TextMatcher.MatchAnyWordStartRegex(words);

                var listBoxItems = GetHighlightableListBoxItems(listView);
                foreach (var listBoxItem in listBoxItems)
                {
                    var textBlocks = GetHighlightableTextBlocks(listBoxItem);
                    foreach (var textBlock in textBlocks)
                    {
                        highlighter.Highlight(textBlock, regex);
                    }

                    SetIsHighlighted(listBoxItem, true);
                }
            }
        }

        private static IEnumerable<ListBoxItem> GetHighlightableListBoxItems(ListView listView)
        {
            listView.ApplyTemplate();

            if (listView.ListBox != null)
            {
                listView.ListBox.UpdateLayout();
                return GetHighlightableListBoxItems(listView.ListBox);
            }
            else
            {
                return Enumerable.Empty<ListBoxItem>();
            }
        }

        private static IEnumerable<ListBoxItem> GetHighlightableListBoxItems(System.Windows.Controls.ListBox list)
        {
            return VisualTreeUtilities.FirstDescendantsOfType<ListBoxItem>(list).Where(item => !GetIsHighlighted(item));
        }

        private static IEnumerable<TextBlock> GetHighlightableTextBlocks(FrameworkElement container)
        {
            return VisualTreeUtilities.FirstDescendantsOfType<TextBlock>(container);
        }

        private static void CustomHighlight(TextRange range)
        {
            range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.ExtraBold);
        }
    }
}
