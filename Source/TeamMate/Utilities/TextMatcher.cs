// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.Tools.TeamMate.Utilities
{
    public static class TextMatcher
    {
        private static readonly Lazy<Regex> WorkSplitterLazyRegex = new Lazy<Regex>(() => new Regex(@"\b\w+\b"));

        private const RegexOptions DefaultRegexOptions = RegexOptions.IgnoreCase;

        public static Predicate<string> MatchWordStart(string word)
        {
            Regex regex = MatchWordStartRegex(word);
            return (x) => x != null && regex.IsMatch(x);
        }

        public static Regex MatchWordStartRegex(string word)
        {
            return new Regex(MatchWordStartPattern(word), DefaultRegexOptions);
        }

        public static Predicate<string> MatchAnyWordStart(IEnumerable<string> words)
        {
            Regex regex = MatchAnyWordStartRegex(words);
            return (x) => x != null && regex.IsMatch(x);
        }

        public static Regex MatchAnyWordStartRegex(IEnumerable<string> words)
        {
            return new Regex(MatchAnyWordStartPattern(words), DefaultRegexOptions);
        }

        public static Predicate<string> MatchAllWordStarts(IEnumerable<string> words)
        {
            var predicates = words.Select(w => MatchWordStart(w)).ToArray();
            return (x) => predicates.All(p => p(x));
        }

        public static Predicate<IEnumerable<string>> MatchAllWordStartsMultiText(IEnumerable<string> words)
        {
            var predicates = words.Select(w => MatchWordStart(w)).ToArray();

            // Every single predicate has to match at least one of the the input strings
            return (textInputs) => predicates.All(predicate => textInputs.Any(text => predicate(text)));
        }

        private static string MatchAnyWordStartPattern(IEnumerable<string> wordsToMatch)
        {
            return String.Join("|", wordsToMatch.Select(w => MatchWordStartPattern(w)));
        }

        private static string MatchWordStartPattern(string wordStart)
        {
            return @"\b" + Regex.Escape(wordStart);
        }

        public static string[] SplitDistinctWords(string input)
        {
            return WorkSplitterLazyRegex.Value.Matches(input).OfType<Match>().Select(m => m.Value).Distinct(StringComparer.CurrentCultureIgnoreCase).ToArray();
        }

        public static string NormalizeSearchText(string text)
        {
            return (text != null) ? text.Trim() : String.Empty;
        }
    }

    public class MultiWordMatcher
    {
        private Predicate<IEnumerable<string>> Predicate { get; set; }

        public MultiWordMatcher(Predicate<IEnumerable<string>> predicate)
        {
            this.Predicate = predicate;
        }

        public static MultiWordMatcher CreateFromSearchText(string searchText)
        {
            var words = TextMatcher.SplitDistinctWords(searchText);
            if (words.Any())
            {
                var predicate = TextMatcher.MatchAllWordStartsMultiText(words);
                return new MultiWordMatcher(predicate);
            }

            return null;
        }
        
        public bool Matches(params string[] textInputs)
        {
            return Predicate(textInputs);
        }

        public bool Matches(IEnumerable<string> textInputs)
        {
            return Predicate(textInputs);
        }
    }
}
