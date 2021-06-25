using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Interop;
using System;
using System.Windows;
using System.Windows.Media;
using LegacySystemIcons = System.Drawing.SystemIcons;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows
{
    public static class SystemIcons
    {
        private static Lazy<ImageSource> application = new Lazy<ImageSource>(() => Create(LegacySystemIcons.Application));
        private static Lazy<ImageSource> asterisk = new Lazy<ImageSource>(() => Create(LegacySystemIcons.Asterisk));
        private static Lazy<ImageSource> error = new Lazy<ImageSource>(() => Create(LegacySystemIcons.Error));
        private static Lazy<ImageSource> exclamation = new Lazy<ImageSource>(() => Create(LegacySystemIcons.Exclamation));
        private static Lazy<ImageSource> hand = new Lazy<ImageSource>(() => Create(LegacySystemIcons.Hand));
        private static Lazy<ImageSource> information = new Lazy<ImageSource>(() => Create(LegacySystemIcons.Information));
        private static Lazy<ImageSource> question = new Lazy<ImageSource>(() => Create(LegacySystemIcons.Question));
        private static Lazy<ImageSource> shield = new Lazy<ImageSource>(() => Create(LegacySystemIcons.Shield));
        private static Lazy<ImageSource> warning = new Lazy<ImageSource>(() => Create(LegacySystemIcons.Warning));
        private static Lazy<ImageSource> winLogo = new Lazy<ImageSource>(() => Create(LegacySystemIcons.WinLogo));

        public static ImageSource Application
        {
            get { return application.Value; }
        }

        public static ImageSource Asterisk
        {
            get { return asterisk.Value; }
        }

        public static ImageSource Error
        {
            get { return error.Value; }
        }

        public static ImageSource Exclamation
        {
            get { return exclamation.Value; }
        }

        public static ImageSource Hand
        {
            get { return hand.Value; }
        }

        public static ImageSource Information
        {
            get { return information.Value; }
        }

        public static ImageSource Question
        {
            get { return question.Value; }
        }

        public static ImageSource Shield
        {
            get { return shield.Value; }
        }

        public static ImageSource Warning
        {
            get { return warning.Value; }
        }

        public static ImageSource WinLogo
        {
            get { return winLogo.Value; }
        }

        public static ImageSource GetMessageBoxImage(MessageBoxImage image)
        {
            switch (image)
            {
                case MessageBoxImage.Error: return Error;               // Also: Hand, Stop
                case MessageBoxImage.Question: return Question;
                case MessageBoxImage.Warning: return Warning;           // Also: Exclamation
                case MessageBoxImage.Information: return Information;   // Also: Asterisk

                case MessageBoxImage.None:
                default:
                    return null;
            }
        }

        private static ImageSource Create(System.Drawing.Icon icon)
        {
            return InteropUtilities.CreateBitmapSourceFromIcon(icon);

        }
    }
}
