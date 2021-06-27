using Microsoft.Tools.TeamMate.Foundation.Resources;
using Microsoft.Tools.TeamMate.Foundation.Windows.Media.Imaging;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Controls
{
    /// <summary>
    /// Provides access to static resources in this assembly.
    /// </summary>
    internal static class ControlResources
    {
        private static readonly string AssemblyName = typeof(ControlResources).Assembly.GetName().Name;

        private static Lazy<ResourceDictionary> controlsDictionary = new Lazy<ResourceDictionary>(() => LoadDictionary("Controls"));
        private static Lazy<ResourceDictionary> transitionsDictionary = new Lazy<ResourceDictionary>(() => LoadDictionary("Transitions"));
        private static Lazy<ResourceDictionary> animationsDictionary = new Lazy<ResourceDictionary>(() => LoadDictionary("Animations"));

        private static LazyWeakReference<Cursor> openHandCursor = new LazyWeakReference<Cursor>(() => LoadCursor(FoundationResources.OpenHandCursor));
        private static LazyWeakReference<Cursor> closedHandCursor = new LazyWeakReference<Cursor>(() => LoadCursor(FoundationResources.ClosedHandCursor));

        private static LazyWeakReference<ImageSource> emptyIcon = new LazyWeakReference<ImageSource>(() => BitmapUtilities.LoadImage(CreateUri("/Resources/Icons/Empty.png")));

        private static LazyWeakReference<FontFamily> segoeMdl2AssetsFontFamily = new LazyWeakReference<FontFamily>(() => LoadSegoeMdl2Assets());

        /// <summary>
        /// Gets the controls dictionary.
        /// </summary>
        public static ResourceDictionary Controls
        {
            get { return controlsDictionary.Value; }
        }

        /// <summary>
        /// Gets the transitions dictionary.
        /// </summary>
        public static ResourceDictionary Transitions
        {
            get { return transitionsDictionary.Value; }
        }

        /// <summary>
        /// Gets the animations.
        /// </summary>
        public static ResourceDictionary Animations
        {
            get { return animationsDictionary.Value; }
        }

        public static ImageSource EmptyIcon
        {
            get { return emptyIcon.Value; }
        }

        /// <summary>
        /// Gets the open hand cursor.
        /// </summary>
        public static Cursor OpenHandCursor
        {
            get { return openHandCursor.Value; }
        }

        /// <summary>
        /// Gets the closed hand cursor.
        /// </summary>
        public static Cursor ClosedHandCursor
        {
            get { return closedHandCursor.Value; }
        }

        public static FontFamily SegoeMdl2AssetsFontFamily
        {
            get { return segoeMdl2AssetsFontFamily.Value; }
        }

        private static FontFamily LoadSegoeMdl2Assets()
        {
            // If the font is installed (e.g. Windows 10), use that. Otherwise, load the embedded font,
            // e.g. on Windows 2012 Server or pre Windows 10.
            string familyName = "Segoe MDL2 Assets";
            FontFamily installedFont = new FontFamily(familyName);
            if (!installedFont.FamilyNames.Values.Contains(familyName))
            {
                FontFamily embeddedFont = Controls.FindResource<FontFamily>("SegoeMdl2AssetsFont");
                return embeddedFont;
            }

            return installedFont;
        }

        private static Cursor LoadCursor(byte[] bytes)
        {
            return new Cursor(new MemoryStream(bytes));
        }

        private static Uri CreateUri(string resourceName)
        {
            return new Uri(String.Format("pack://application:,,,/{0};component{1}", AssemblyName, resourceName), UriKind.Absolute);
        }

        private static ResourceDictionary LoadDictionary(string name)
        {
            // Must be a relative URI
            var uri = new Uri(String.Format("{0};component/Windows/Controls/Resources/{1}.xaml", AssemblyName, name), UriKind.Relative);
            return (ResourceDictionary)Application.LoadComponent(uri);
        }
    }
}
