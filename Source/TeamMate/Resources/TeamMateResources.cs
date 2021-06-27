// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Windows;
using Microsoft.Tools.TeamMate.Foundation.Windows.Interop;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Icon = System.Drawing.Icon;

namespace Microsoft.Tools.TeamMate.Resources
{
    public static class TeamMateResources
    {
        private static readonly Uri NotificationSoundUri = new Uri("pack://application:,,,/Resources/Audio/Notification.wav");

        public static Stream NotificationSoundStream
        {
            get
            {
                return Application.GetResourceStream(NotificationSoundUri).Stream;
            }
        }

        public static Icon RecordingTrayIcon
        {
            get
            {
                return InteropUtilities.IconFromBitmapSource(FindResource<BitmapSource>("RecordingTrayIcon"));
            }
        }

        public static Icon TrayIcon
        {
            get
            {
                return InteropUtilities.IconFromBitmapSource(FindResource<BitmapSource>("TrayIcon"));
            }
        }

        public static ImageSource ErrorOverlayIcon
        {
            get
            {
                return FindResource<ImageSource>("ErrorOverlayIcon");
            }
        }

        public static ImageSource ToastIcon
        {
            get
            {
                return FindResource<ImageSource>("ToastIcon");
            }
        }

        public static ImageSource LargeContactPhoto
        {
            get
            {
                return FindResource<ImageSource>("ContactPhotoLargeIcon");
            }
        }

        public static Color ApplicationColor
        {
            get
            {
                return FindResource<Color>("ApplicationColor");
            }
        }

        public static T FindResource<T>(object resourceKey)
        {
            var application = Application.Current;
            return (application != null) ? application.FindResource<T>(resourceKey) : default(T);
        }
    }
}
