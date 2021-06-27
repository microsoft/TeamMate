using Microsoft.Tools.TeamMate.Controls;
using Microsoft.Tools.TeamMate.Foundation;
using Microsoft.Tools.TeamMate.Foundation.Windows;
using Microsoft.Tools.TeamMate.Foundation.Windows.Controls;
using Microsoft.Tools.TeamMate.Foundation.Windows.Input;
using Microsoft.Tools.TeamMate.Utilities;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Microsoft.Tools.TeamMate.Converters
{
    public static class TeamMateConverters
    {
        private static readonly Brush DefaultWorkItemBrush = BrushFromHex("#474747");
        private static readonly Brush DefaultStateBrush = BrushFromHex("#B1B1B1");

        private static readonly IDictionary<string, Tuple<Bowtie, Brush>> WorkItemTypes = new Dictionary<string, Tuple<Bowtie, Brush>>(StringComparer.OrdinalIgnoreCase)
        {
            { "Bug", new Tuple<Bowtie, Brush>(Bowtie.SymbolBug, BrushFromHex("#CC3340")) },
            { "Epic", new Tuple<Bowtie, Brush>(Bowtie.SymbolCrown, BrushFromHex("#FF7F01")) },
            { "Feature", new Tuple<Bowtie, Brush>(Bowtie.SymbolTrophy, BrushFromHex("#7B3B92")) },
            { "Impediment", new Tuple<Bowtie, Brush>(Bowtie.SymbolImpediment, BrushFromHex("#B6019C")) },
            { "Issue", new Tuple<Bowtie, Brush>(Bowtie.SymbolImpediment, BrushFromHex("#B6019C")) },
            { "Product Backlog Item", new Tuple<Bowtie, Brush>(Bowtie.SymbolList, BrushFromHex("#009CCC")) },
            { "Task", new Tuple<Bowtie, Brush>(Bowtie.SymbolTask, BrushFromHex("#EFCD27")) },
            { "Test Case", new Tuple<Bowtie, Brush>(Bowtie.SymbolTestCase, BrushFromHex("#014D52")) },
            { "Test Plan", new Tuple<Bowtie, Brush>(Bowtie.TestParameter, BrushFromHex("#014D52")) },
            { "Test Suite", new Tuple<Bowtie, Brush>(Bowtie.TestSuite, BrushFromHex("#014D52")) },
            { "User Story", new Tuple<Bowtie, Brush>(Bowtie.SymbolBook, BrushFromHex("#1E99CB")) },
        };

        private static readonly IDictionary<string, Brush> StatesToColors = new Dictionary<string, Brush>
        {
            { "New", BrushFromHex("#B1B1B1") },
            { "Design", BrushFromHex("#CC3340") },
            { "Active", BrushFromHex("#2676CB") },
            { "Ready", BrushFromHex("#2676CB") },
            { "In Planning", BrushFromHex("#2676CB") },
            { "In Progress", BrushFromHex("#2676CB") },
            { "Resolved", BrushFromHex("#FE9F08") },
            { "Closed", BrushFromHex("#2C9938") },
            { "Inactive", BrushFromHex("#2C9938") },
            { "Removed", BrushFromHex("#EAEFFB") }, // TODO: Removed is white with a blue border #6387E0
        };

        static TeamMateConverters()
        {
            AssignedTo = DelegateFactory.CreateValueConverter(FormatAssignedTo);
            ValueOrUndefined = DelegateFactory.CreateValueConverter(FormatUndefined);
            PriorityFormat = DelegateFactory.CreateValueConverter(FormatPriority);
            FriendlyDate = DelegateFactory.CreateValueConverter(FormatFriendlyDate);
            CreateImage = DelegateFactory.CreateValueConverter(CreateImageFromSource);
            CommandToolTip = DelegateFactory.CreateValueConverter(GetCommandToolTip);

            Duration = DelegateFactory.CreateValueConverter(FormatDuration);
            ByteSize = DelegateFactory.CreateValueConverter(FormatByteSize);

            CreateSymbolIcon = DelegateFactory.CreateValueConverter(ToSymbolIcon);

            WorkItemTypeToIcon = DelegateFactory.CreateValueConverter(GetWorkItemTypeIcon);
            WorkItemTypeToColor = DelegateFactory.CreateValueConverter(GetWorkItemTypeColor);
            WorkItemStateToColor = DelegateFactory.CreateValueConverter(GetWorkItemStateColor);
        }

        private static object ToSymbolIcon(object value)
        {
            SymbolIcon symbolIcon = new SymbolIcon();
            symbolIcon.Symbol = (Symbol)value;
            return symbolIcon;
        }

        private static object FormatDuration(object arg)
        {
            if (arg is TimeSpan)
            {
                TimeSpan duration = (TimeSpan)arg;
                return duration.ToShortestSecondsString();
            }

            return String.Empty;
        }

        private static object FormatByteSize(object arg)
        {
            if (arg is long || arg is int)
            {
                return FormatUtilities.FormatBytes((long)arg);
            }

            return String.Empty;
        }

        public static IValueConverter CreateSymbolIcon { get; private set; }

        public static IValueConverter AssignedTo { get; private set; }
        public static IValueConverter ValueOrUndefined { get; private set; }
        public static IValueConverter PriorityFormat { get; private set; }
        public static IValueConverter FriendlyDate { get; private set; }

        public static IValueConverter ByteSize { get; private set; }
        public static IValueConverter Duration { get; private set; }


        public static IValueConverter CommandToolTip { get; private set; }
        // Creates images from image sources... Used in some WPF elements that take an Image rather than an ImageSource for an icon.
        public static IValueConverter CreateImage { get; private set; }
        public static IValueConverter WorkItemTypeToIcon { get; private set; }
        public static IValueConverter WorkItemTypeToColor { get; private set; }
        public static IValueConverter WorkItemStateToColor { get; private set; }

        private static object FormatPriority(object value)
        {
            if (value is int)
            {
                return Formatter.FormatPriority((int)value);
            }

            return Formatter.Undefined;
        }

        private static object FormatFriendlyDate(object value)
        {
            if (value is DateTime)
            {
                DateTime date = (DateTime)value;
                return date.ToFriendlyElapsedTimeStringWithAgo();
            }

            return Formatter.Undefined;
        }

        private static object FormatAssignedTo(object value)
        {
            return Formatter.FormatAssignedTo(value as string);
        }

        private static object FormatUndefined(object value)
        {
            return Formatter.FormatUndefined(value);
        }

        private static object CreateImageFromSource(object value)
        {
            ImageSource source = value as ImageSource;

            if (source != null)
            {
                Image image = new Image();
                image.Source = source;
                image.Stretch = Stretch.None;
                return image;
            }

            return null;
        }

        private static object GetCommandToolTip(object value)
        {
            UICommand command = value as UICommand;
            return (command != null) ? command.DescriptionAndShortcut : null;
        }

        private static object GetWorkItemTypeIcon(object value)
        {
            string type = value as string;
            if(!string.IsNullOrEmpty(type))
            {
                Tuple<Bowtie, Brush> tuple;
                if(WorkItemTypes.TryGetValue(type, out tuple))
                {
                    return tuple.Item1;
                }
            }

            return Bowtie.SymbolTask;
        }

        private static object GetWorkItemTypeColor(object value)
        {
            string type = value as string;
            if (!string.IsNullOrEmpty(type))
            {
                Tuple<Bowtie, Brush> tuple;
                if (WorkItemTypes.TryGetValue(type, out tuple))
                {
                    return tuple.Item2;
                }
            }
            return DefaultWorkItemBrush;
        }

        private static object GetWorkItemStateColor(object value)
        {
            string type = value as string;
            if (!string.IsNullOrEmpty(type))
            {
                Brush brush;
                if (StatesToColors.TryGetValue(type, out brush))
                {
                    return brush;
                }
            }

            return DefaultStateBrush;
        }

        private static Brush BrushFromHex(string color)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        }
    }
}
