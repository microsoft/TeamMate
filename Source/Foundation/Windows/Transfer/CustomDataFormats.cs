using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Transfer
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public static class CustomDataFormats
    {
        public const string FileGroupDescriptorW = "FileGroupDescriptorW";
        public const string FileContents = "FileContents";
        public const string PreferredDropEffect = "Preferred DropEffect";
        public const string PerformedDropEffect = "Performed DropEffect";

        public const string UniformResourceLocator = "UniformResourceLocator";
        public const string UniformResourceLocatorW = "UniformResourceLocatorW";
    }
}
