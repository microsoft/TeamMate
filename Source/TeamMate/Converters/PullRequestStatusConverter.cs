using Microsoft.Tools.TeamMate.Foundation.Windows;
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
    public class PullRequestStatusConverter : OneWayConverterBase
    {
        public enum PullRequestStatus
        {
            Waiting,
            SignedOff,
            Reviewing,
            Started,
            Declined
        }

        private Brush defaultColor = new SolidColorBrush(Colors.Gray);

        // Summary:
        //     Vote on a pull request:
        //     10 - approved 5 - approved with suggestions 0 - no vote -5 - waiting for author
        //     -10 - rejected

        private static readonly PullRequestStatus[] DisplayStatusOrder = new PullRequestStatus[] {
            PullRequestStatus.Waiting, PullRequestStatus.SignedOff, PullRequestStatus.Reviewing, PullRequestStatus.Started, PullRequestStatus.Declined
        };

        private static readonly Dictionary<PullRequestStatus, string> TextMap = new Dictionary<PullRequestStatus, string>()
        {
            { PullRequestStatus.SignedOff, "Signed Off" },
        };

        private static readonly Dictionary<PullRequestStatus, FontWeight> FontWeightMap = new Dictionary<PullRequestStatus, FontWeight>()
        {
            // { PullRequestStatus.Waiting, FontWeights.Bold },
            // { PullRequestStatus.SignedOff, FontWeights.Bold },
        };

        private static readonly Dictionary<PullRequestStatus, string> IconMap = new Dictionary<PullRequestStatus, string>()
        {
            { PullRequestStatus.Waiting, "SmallWaitingIcon" },
            { PullRequestStatus.SignedOff, "SmallSignedOffIcon" },
            { PullRequestStatus.Started, "SmallReviewingIcon" },
            { PullRequestStatus.Reviewing, "SmallReviewingIcon" },
            { PullRequestStatus.Declined, "SmallDeclinedIcon" },
        };

        private readonly Dictionary<PullRequestStatus, Brush> BrushMap = new Dictionary<PullRequestStatus, Brush>()
        {
            { PullRequestStatus.Waiting, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CA4A2B")) },
            { PullRequestStatus.SignedOff, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#58947E")) },
            { PullRequestStatus.Reviewing, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#727272")) },
            { PullRequestStatus.Started, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#727272")) },
            { PullRequestStatus.Declined, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#777777")) },
        };

        public PullRequestStatusConverterMode Mode { get; set; }

        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PullRequestRowViewModel info = value as PullRequestRowViewModel;
            if (info != null)
            {
                switch (Mode)
                {
                    case PullRequestStatusConverterMode.StatusText:
                        return GetReviewStatusText(info);

                    case PullRequestStatusConverterMode.StatusImage:
                        return GetReviewStatusImage(info);

                    case PullRequestStatusConverterMode.ReviewerStatus:
                        return GetReviewerStatus(info);

                    case PullRequestStatusConverterMode.IterationCount:
                        if (info.IterationCount > 1)
                        {
                            return " - Iteration " + info.IterationCount;
                        }

                        break;
                }
            }

            return null;
        }

        private Span GetReviewerStatus(PullRequestRowViewModel info)
        {
            Span span = new Span();

            var reference = info.Reference;

            if(reference != null)
            {
                foreach (var status in DisplayStatusOrder)
                {
                    foreach (var review in reference.Reviewers)
                    {
                        string statusText = GetMapValue(TextMap, status, (s) => s.ToString());
                        String text = String.Format("{1}", statusText, String.Join(", ", reference.Reviewers.Select(r => r.DisplayName)));

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

        private string GetReviewStatusText(PullRequestRowViewModel info)
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

        private ImageSource GetReviewStatusImage(PullRequestRowViewModel info)
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

    public enum PullRequestStatusConverterMode
    {
        StatusText,
        StatusImage,
        ReviewerStatus,
        IterationCount
    }
}
