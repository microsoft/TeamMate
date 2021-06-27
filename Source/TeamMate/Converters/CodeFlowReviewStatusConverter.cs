// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Windows;
using Microsoft.Tools.TeamMate.Platform.CodeFlow.Dashboard;
using Microsoft.Tools.TeamMate.Resources;
using Microsoft.Tools.TeamMate.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Microsoft.Tools.TeamMate.Converters
{
    public class CodeFlowReviewStatusConverter : OneWayConverterBase
    {
        private Brush defaultColor = new SolidColorBrush(Colors.Gray);

        private static readonly ReviewerStatus[] DisplayStatusOrder = new ReviewerStatus[] {
            ReviewerStatus.Waiting, ReviewerStatus.SignedOff, ReviewerStatus.Reviewing, ReviewerStatus.Started, ReviewerStatus.Declined
        };

        private static readonly Dictionary<ReviewerStatus, string> TextMap = new Dictionary<ReviewerStatus, string>()
        {
            { ReviewerStatus.SignedOff, "Signed Off" },
        };

        private static readonly Dictionary<ReviewerStatus, FontWeight> FontWeightMap = new Dictionary<ReviewerStatus, FontWeight>()
        {
            // { ReviewerStatus.Waiting, FontWeights.Bold },
            // { ReviewerStatus.SignedOff, FontWeights.Bold },
        };

        private static readonly Dictionary<ReviewerStatus, string> IconMap = new Dictionary<ReviewerStatus, string>()
        {
            { ReviewerStatus.Waiting, "SmallWaitingIcon" },
            { ReviewerStatus.SignedOff, "SmallSignedOffIcon" },
            { ReviewerStatus.Started, "SmallReviewingIcon" },
            { ReviewerStatus.Reviewing, "SmallReviewingIcon" },
            { ReviewerStatus.Declined, "SmallDeclinedIcon" },
        };

        private readonly Dictionary<ReviewerStatus, Brush> BrushMap = new Dictionary<ReviewerStatus, Brush>()
        {
            { ReviewerStatus.Waiting, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CA4A2B")) },
            { ReviewerStatus.SignedOff, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#58947E")) },
            { ReviewerStatus.Reviewing, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#727272")) },
            { ReviewerStatus.Started, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#727272")) },
            { ReviewerStatus.Declined, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#777777")) },
        };

        public CodeFlowReviewStatusConverterMode Mode { get; set; }

        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            CodeFlowReviewViewModel info = value as CodeFlowReviewViewModel;
            if (info != null)
            {
                switch (Mode)
                {
                    case CodeFlowReviewStatusConverterMode.StatusText:
                        return GetReviewStatusText(info);

                    case CodeFlowReviewStatusConverterMode.StatusImage:
                        return GetReviewStatusImage(info);

                    case CodeFlowReviewStatusConverterMode.ReviewerStatus:
                        return GetReviewerStatus(info);

                    case CodeFlowReviewStatusConverterMode.IterationCount:
                        if (info.IterationCount > 1)
                        {
                            return " - Iteration " + info.IterationCount;
                        }

                        break;
                }
            }

            return null;
        }

        private Span GetReviewerStatus(CodeFlowReviewViewModel info)
        {
            Span span = new Span();

            var summary = info.Summary;

            if(summary != null)
            {
                var reviewersByStatus = summary.Reviewers.Where(r => r.HasLastUpdatedOn()).GroupBy((r) => r.Status).ToDictionary((i) => i.Key);
                foreach (var status in DisplayStatusOrder)
                {
                    IGrouping<ReviewerStatus, Reviewer> group;
                    if (reviewersByStatus.TryGetValue(status, out group))
                    {
                        var orderedReviewers = group.OrderBy(r => r.LastUpdatedOn);

                        string statusText = GetMapValue(TextMap, status, (s) => s.ToString());
                        String text = String.Format("{1}", statusText, String.Join(", ", orderedReviewers.Select(r => r.DisplayName)));

                        string iconName = GetMapValue(IconMap, status, (string)null);
                        ImageSource source = (iconName != null) ? FindImageResource(iconName) : null;

                        if (source != null)
                        {
                            Image image = new Image();
                            image.Source = source;
                            image.Width = 12;
                            image.Margin = new Thickness(12, 3, 3, 0);
                            image.ToolTip = statusText;

                            var iuic = new InlineUIContainer(image);
                            iuic.BaselineAlignment = BaselineAlignment.Center;

                            span.Inlines.Add(iuic);
                        }

                        Run run = new Run(text);
                        run.Foreground = GetMapValue(BrushMap, status, defaultColor);
                        run.FontWeight = GetMapValue(FontWeightMap, status, FontWeights.Normal);
                        run.ToolTip = statusText;

                        span.Inlines.Add(run);
                    }
                }
            }

            return span;
        }

        private static ImageSource FindImageResource(string iconName)
        {
            return TeamMateResources.FindResource<ImageSource>(iconName);
        }

        private string GetReviewStatusText(CodeFlowReviewViewModel info)
        {
            if (info.IsWaiting)
            {
                return "Mmmm... Might need some changes.";
            }
            else if (info.IsSignedOff)
            {
                return "Ship it!";
            }
            else if (info.IsPending)
            {
                return "Waiting for feedback";
            }

            return null;
        }

        private ImageSource GetReviewStatusImage(CodeFlowReviewViewModel info)
        {
            if (info.IsWaiting)
            {
                return FindImageResource("SmallWaitingIcon");
            }
            else if (info.IsSignedOff)
            {
                return FindImageResource("SmallSignedOffIcon");
            }
            else if (info.IsPending)
            {
                return FindImageResource("SmallReviewingIcon");
            }

            // E.g. a completed review with no feedback? TODO: follow up and see what we want to do with these completed ones, or if we use another flag
            return FindImageResource("SmallReviewingIcon");
        }

        private static U GetMapValue<T, U>(IDictionary<T, U> map, T key, U defaultValue)
        {
            U result;
            if (!map.TryGetValue(key, out result))
            {
                result = defaultValue;
            }

            return result;
        }

        private static U GetMapValue<T, U>(IDictionary<T, U> map, T key, Func<T, U> defaultValue)
        {
            U result;
            if (!map.TryGetValue(key, out result))
            {
                result = defaultValue(key);
            }

            return result;
        }
    }

    public enum CodeFlowReviewStatusConverterMode
    {
        StatusText,
        StatusImage,
        ReviewerStatus,
        IterationCount
    }
}
