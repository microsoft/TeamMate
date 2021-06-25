using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Internal.Tools.TeamMate.Foundation.Native;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Win32
{
    /// <summary>
    /// Provides information that describes a file type, based on file extension, according to the
    /// information in the system registry.
    /// </summary>
    public class FileTypeInfo
    {
        // See implementation information in
        // http://msdn.microsoft.com/en-us/library/windows/desktop/cc144067(v=vs.85).aspx

        private string description;
        private string defaultOpenExe;
        private bool selfOpens;
        private string fullPathToDefaultOpenExe;
        private string defaultOpenExeDescription;
        private Guid? previewHandlerId;
        private string previewHandlerDescription;
        private bool hasThumbnailProvider;
        private bool hasCustomIcon;

        private Parts loadedParts;

        private static Lazy<FileTypeInfo> lazyDefaultFile = new Lazy<FileTypeInfo>(() => FromExtension(String.Empty));

        /// <summary>
        /// Gets the default file information (no extension).
        /// </summary>
        public static FileTypeInfo DefaultFile
        {
            get { return lazyDefaultFile.Value; }
        }

        /// <summary>
        /// Creates a new instance for a given extension by querying the registry for file information,
        /// lazily loading information for the file type as needed from the registry.
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <returns>The created type.</returns>
        /// <remarks>
        /// Prefer to use a FileTypeRegistry instace which will cache information for a given extension.
        /// </remarks>
        public static FileTypeInfo FromExtension(string extension)
        {
            Assert.ParamIsNotNull(extension, "extension");

            FileTypeInfo info = new FileTypeInfo(extension);

            if (!String.IsNullOrEmpty(extension))
            {
                using (RegistryKey classKey = Registry.ClassesRoot.OpenSubKey(extension))
                {
                    if (classKey != null)
                    {
                        info.DefaultHandler = classKey.GetValue(null) as string;
                        info.PerceivedType = classKey.GetValue("PerceivedType") as string;
                    }
                }
            }

            return info;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="extension">The extension.</param>
        private FileTypeInfo(string extension)
        {
            Assert.ParamIsNotNull(extension, "extension");

            this.Extension = extension.ToLower();
        }

        /// <summary>
        /// Ensures all of the file type information has been fully loaded from the registry.
        /// </summary>
        public void EnsureFullyLoaded()
        {
            EnsureLoaded(Parts.All);
        }

        /// <summary>
        /// Gets the file extension.
        /// </summary>
        public string Extension { get; private set; }

        /// <summary>
        /// Gets the default handler string for the registered program that can open the file,
        /// or <c>null</c> if not set.
        /// </summary>
        public string DefaultHandler { get; private set; }

        /// <summary>
        /// Gets the perceived type name of this file type, or <c>null</c> if not defined.
        /// </summary>
        public string PerceivedType { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this file type is perceived as an image, based on the PerceivedType.
        /// </summary>
        public bool IsPerceivedAsImage
        {
            get { return PerceivedTypeIs("image"); }
        }

        /// <summary>
        /// Gets a value indicating whether this file type is perceived as a video file, based on the PerceivedType.
        /// </summary>
        public bool IsPerceivedAsVideo
        {
            get { return PerceivedTypeIs("video"); }
        }

        /// <summary>
        /// Checks whether the perceived type is a given value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the perceived type matches the value; otherwise, <c>false</c></returns>
        private bool PerceivedTypeIs(string value)
        {
            return String.Equals(PerceivedType, value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the description, or <c>null</c> if not available.
        /// </summary>
        public string Description
        {
            get
            {
                EnsureLoaded(Parts.Description);
                return this.description;
            }
        }

        /// <summary>
        /// Gets the path to the default executable associated to open this file. This could be a full path, 
        /// or just an exe filename to be found under the PATH environment variable. Returns <c>null</c> if no
        /// executable is associated with this file type.
        /// </summary>
        public string DefaultOpenExe
        {
            get
            {
                EnsureLoaded(Parts.DefaultOpenExe);
                return this.defaultOpenExe;
            }
        }

        /// <summary>
        /// Gets the full path to default executable associated to open this file. Returns <c>null</c> if
        /// no executable is associated with this file type, or, in the case of a relative executable name,
        /// if the executable coudl not be found in the current PATH.
        /// </summary>
        /// <value>
        /// The full path to default open executable.
        /// </value>
        public string FullPathToDefaultOpenExe
        {
            get
            {
                EnsureLoaded(Parts.FullPathToDefaultOpenExe);
                return this.fullPathToDefaultOpenExe;
            }
        }

        /// <summary>
        /// Gets the description for the default executable associated to open this file, or <c>null</c>
        /// if not available.
        /// </summary>
        public string DefaultOpenExeDescription
        {
            get
            {
                EnsureLoaded(Parts.DefaultOpenExeDescription);
                return this.defaultOpenExeDescription;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the file self opens or not (e.g. .exe or .ps1 files self open
        /// themselves).
        /// </summary>
        public bool SelfOpens
        {
            get
            {
                EnsureLoaded(Parts.DefaultOpenExe);
                return this.selfOpens;
            }
        }

        /// <summary>
        /// Gets the preview handler identifier if available, or <c>null</c> if not defined.
        /// </summary>
        public Guid? PreviewHandlerId
        {
            get
            {
                EnsureLoaded(Parts.PreviewHandler);
                return this.previewHandlerId;
            }
        }

        /// <summary>
        /// Gets the preview handler description, or <c>null</c> if not defined.
        /// </summary>
        public string PreviewHandlerDescription
        {
            get
            {
                EnsureLoaded(Parts.PreviewHandler);
                return this.previewHandlerDescription;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this file type has a preview handler.
        /// </summary>
        public bool HasPreviewHandler
        {
            get { return PreviewHandlerId != null; }
        }

        /// <summary>
        /// Gets a value indicating whether this file type has a thumbnail provider.
        /// </summary>
        public bool HasThumbnailProvider
        {
            get
            {
                EnsureLoaded(Parts.ThumbnailProvider);
                return this.hasThumbnailProvider;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this file type is potentially unsafe to open. This
        /// should be used to prompt/warn the user if attempting to open this file type.
        /// </summary>
        public bool IsPotentiallyUnsafeToOpen
        {
            get
            {
                return SelfOpens || UnsafeFileExtensions.IsUnsafeFileExtension(Extension);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this file type has a custom icon.
        /// </summary>
        public bool HasCustomIcon
        {
            get
            {
                EnsureLoaded(Parts.IconHandler);
                return this.hasCustomIcon;
            }
        }

        /// <summary>
        /// Converts this file type to a file filter expression that can be used in file open dialog
        /// properties.
        /// </summary>
        public string ToFileFilter()
        {
            if (!String.IsNullOrEmpty(Extension))
            {
                return String.Format("{0} (*{1})|*{1}", Description, Extension);
            }
            else
            {
                return "All files (*.*)|*.*";
            }
        }

        /// <summary>
        /// Ensures that one or more parts of this file type info have been loaded from the registry.
        /// </summary>
        /// <param name="parts">The parts to load.</param>
        private void EnsureLoaded(Parts parts)
        {
            Parts missingParts = (parts & ~loadedParts);

            if (missingParts == Parts.None)
            {
                return;
            }

            loadedParts |= missingParts;

            if ((missingParts & Parts.Description) == Parts.Description)
            {
                this.description = GetTypeName(Extension);
            }

            if ((missingParts & Parts.DefaultOpenExe) == Parts.DefaultOpenExe)
            {
                if (!String.IsNullOrEmpty(DefaultHandler))
                {
                    bool selfOpens;
                    this.defaultOpenExe = GetExePathFromDefaultCommand(DefaultHandler, out selfOpens);
                    this.selfOpens = selfOpens;
                }
            }

            if ((missingParts & Parts.FullPathToDefaultOpenExe) == Parts.FullPathToDefaultOpenExe)
            {
                if (!String.IsNullOrEmpty(DefaultOpenExe))
                {
                    this.fullPathToDefaultOpenExe = GetFullPathToExe(DefaultOpenExe);
                }
            }

            if ((missingParts & Parts.DefaultOpenExeDescription) == Parts.DefaultOpenExeDescription)
            {
                // Laugh, but you could register a file on another box... We won't spend time on those...
                if (!String.IsNullOrEmpty(FullPathToDefaultOpenExe) && !FullPathToDefaultOpenExe.StartsWith(@"\\"))
                {
                    this.defaultOpenExeDescription = GetExeDescription(FullPathToDefaultOpenExe);
                }
            }

            if ((missingParts & Parts.PreviewHandler) == Parts.PreviewHandler)
            {
                this.previewHandlerId = this.GetPreviewHandlerId();
                if (this.previewHandlerId != null)
                {
                    this.previewHandlerDescription = GetPreviewHandlerDescription(this.previewHandlerId.Value);
                }
            }

            if ((missingParts & Parts.ThumbnailProvider) == Parts.ThumbnailProvider)
            {
                this.hasThumbnailProvider = this.GetHasThumbnailProvider();
            }

            if ((missingParts & Parts.IconHandler) == Parts.IconHandler)
            {
                this.hasCustomIcon = this.GetHasCustomIcon();
            }
        }

        /// <summary>
        /// Gets the registered name for the given extension.
        /// </summary>
        /// <param name="extension">The extension.</param>
        private static string GetTypeName(string extension)
        {
            string result = ShGetFileInfoTypeName(extension);
            if (String.IsNullOrEmpty(result))
            {
                // Extension not registered, just return the default name...
                result = ShGetFileInfoTypeName(null);
            }

            return result;
        }

        /// <summary>
        /// Invokes the shell function to the get the registered file type name for the given extension.
        /// </summary>
        /// <param name="extension">The extension.</param>
        private static string ShGetFileInfoTypeName(string extension)
        {
            if (String.IsNullOrEmpty(extension))
            {
                // SHGetFileInfo does not like null or empty, so just use a dummy file extension instead
                extension = "DummyFileName";
            }

            SHFILEINFO info = new SHFILEINFO();
            FILE_ATTRIBUTE attr = FILE_ATTRIBUTE.FILE_ATTRIBUTE_NORMAL;
            SHGFI flags = SHGFI.SHGFI_TYPENAME | SHGFI.SHGFI_USEFILEATTRIBUTES;
            NativeMethods.SHGetFileInfo(extension, (uint)attr, ref info, SHFILEINFO.Size, (uint)flags);
            return info.szTypeName;
        }

        /// <summary>
        /// Gets the executable path (absolute or relative) for the default command associated with a file type.
        /// </summary>
        /// <param name="className">Name of the class associated as the default handler.</param>
        /// <param name="selfOpens">An output parameter that indicates if the file type self opens itself or not..</param>
        /// <returns>The executable path, or <c>null</c> if not defined.</returns>
        private static string GetExePathFromDefaultCommand(string className, out bool selfOpens)
        {
            selfOpens = false;
            string command = null;

            // http://msdn.microsoft.com/en-us/library/bb165967.aspx
            // The default verb is the action that is executed when a user double-clicks a file in Windows Explorer. 
            // The default verb is the verb specified as the default value for the HKEY_CLASSES_ROOT\progid\Shell key. 
            // If no value is specified, the default verb is the first verb specified in the HKEY_CLASSES_ROOT\progid\Shell key list.

            string shellPath = String.Format(@"{0}\Shell", className);
            using (RegistryKey shellKey = Registry.ClassesRoot.OpenSubKey(shellPath))
            {
                if (shellKey != null)
                {
                    string defaultVerb = shellKey.GetValue(null) as string;
                    if (String.IsNullOrEmpty(defaultVerb))
                    {
                        defaultVerb = shellKey.GetSubKeyNames().FirstOrDefault();
                    }

                    if (!String.IsNullOrEmpty(defaultVerb))
                    {
                        string verbPath = String.Format(@"{0}\Command", defaultVerb);
                        using (RegistryKey openCommandKey = shellKey.OpenSubKey(verbPath))
                        {
                            if (openCommandKey != null)
                            {
                                command = openCommandKey.GetValue(null) as string;
                            }
                        }
                    }
                }
            }

            return (!String.IsNullOrEmpty(command)) ? ExtractExeFromCommand(command, out selfOpens) : null;
        }

        /// <summary>
        /// Extracts the executable path from a registered verb command.
        /// </summary>
        /// <param name="command">The command value from the registry.</param>
        /// <param name="selfOpens">An output parameter that indicates if the file type self opens itself or not..</param>
        /// <returns>The executable path, or <c>null</c> if not defined.</returns>
        private static string ExtractExeFromCommand(string command, out bool selfOpens)
        {
            selfOpens = false;

            string exe = null;

            bool strippedQuotes = false;

            int indexOfQuote = command.IndexOf('"');
            if (indexOfQuote == 0)
            {
                // Somme exe files have quotes to deal with spaces in filenames
                strippedQuotes = true;
                int nextQuote = command.IndexOf('"', 1);
                if (nextQuote > 0)
                {
                    command = command.Substring(1, nextQuote - 1);
                }
                else
                {
                    // This seems like a broken entry (no ending quote?), we'll still try and make it better...
                    command = command.Substring(1).Trim();
                }
            }
            else if (indexOfQuote > 0)
            {
                // This is a case where the command didn't start with a quote, but a quote typically
                // means the start of a parameter (sometimes the first parameter).
                command = command.Substring(0, indexOfQuote).TrimEnd();
            }

            // Some files run "themselves" (e.g. .exe, .bat, etc)
            selfOpens = command.StartsWith("%1");
            if (!selfOpens)
            {
                exe = command;

                if (!strippedQuotes)
                {
                    // At this point, we have a path without quotes... However, there might still be parameters being
                    // passed to the EXE. Here, we try to figure that out.
                    int indexOfSpace = command.IndexOf(' ');
                    if (indexOfSpace > 0)
                    {
                        // Could be e.g.:
                        // C:\Program Files (x86)\QuickTime\QuickTimePlayer.exe
                        // C:\Windows\system32\NOTEPAD.EXE %1
                        // C:\Windows\system32\rasphone.exe -f
                        // C:\Windows\system32\rundll32.exe cryptext.dll,CryptExtOpenPKCS7 %1

                        // Find the first occurrence of ".exe " (with a space);
                        int firstIndexOfExeAndSpace = command.IndexOf(".exe ", StringComparison.OrdinalIgnoreCase);
                        if (firstIndexOfExeAndSpace > 0)
                        {
                            exe = command.Substring(0, firstIndexOfExeAndSpace + 4);
                        }
                        else if (command.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                        {
                            exe = command;
                        }
                        else
                        {
                            // E.g. very weird cases...
                            // C:\Windows\system32\perfmon
                            string firstPart = command.Substring(0, indexOfSpace);

                            if (String.IsNullOrEmpty(TryGetExtension(firstPart)))
                            {
                                // Some commands are naughty and do not put an extension on them, fix them up (needed to find file)
                                firstPart += ".exe";
                            }

                            exe = firstPart;
                        }
                    }
                }
            }

            return exe;
        }

        /// <summary>
        /// Tries to get the extension of what might be a file path.
        /// </summary>
        /// <param name="maybeAFile">Potentially a file path.</param>
        /// <returns>The extension, or <c>null</c> if it was not a valid file path.</returns>
        private static string TryGetExtension(string maybeAFile)
        {
            try
            {
                return Path.GetExtension(maybeAFile);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the description of an executable file.
        /// </summary>
        /// <param name="filePath">The path to the executable.</param>
        /// <returns>The description, or the file name if one was not defined.</returns>
        private static string GetExeDescription(string filePath)
        {
            string description = null;

            try
            {
                if (File.Exists(filePath))
                {
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(filePath);
                    description = fvi.FileDescription;
                }
            }
            catch (Exception e)
            {
                Log.Warn(e);
            }

            if (String.IsNullOrEmpty(description))
            {
                description = Path.GetFileName(filePath);
            }

            return description;
        }

        /// <summary>
        /// Gets the absolute full path to an executable, attempting to resolve the file path
        /// under the PATH environment variable if it was not a rooted path.
        /// </summary>
        /// <param name="filePath">The executable.</param>
        /// <returns>The full path, or <c>null</c> if it could not be resolved.</returns>
        private static string GetFullPathToExe(string filePath)
        {
            if (!Path.IsPathRooted(filePath))
            {
                foreach (var path in GetEnvironmentPath())
                {
                    string fullPath = Path.Combine(path, filePath);
                    if (Path.IsPathRooted(filePath) && File.Exists(path))
                    {
                        return path;
                    }
                }
            }
            else
            {
                return filePath;
            }

            return null;
        }

        /// <summary>
        /// Opens the preferred registry sub key, in order of precendence, for the current file extension
        /// and subkey path.
        /// </summary>
        /// <param name="keyName">The sub key name to find.</param>
        /// <returns>The first matching existing registry key, in order of precedence, or <c>null</c> if not found.</returns>
        /// <remarks>
        /// See the precedence rules in http://msdn.microsoft.com/en-us/library/windows/apps/bb776871(v=vs.85).aspx.
        /// </remarks>
        private RegistryKey OpenPreferredSubKey(string keyName)
        {
            // 
            RegistryKey subKey = null;

            if (!String.IsNullOrEmpty(Extension))
            {
                subKey = Registry.ClassesRoot.OpenSubKey(Extension + @"\" + keyName);

                if (subKey == null && DefaultHandler != null)
                {
                    subKey = Registry.ClassesRoot.OpenSubKey(DefaultHandler + @"\" + keyName);
                }

                if (subKey == null)
                {
                    subKey = Registry.ClassesRoot.OpenSubKey(@"SystemFileAssociations\" + Extension + @"\" + keyName);
                }

                if (subKey == null && !String.IsNullOrEmpty(PerceivedType))
                {
                    subKey = Registry.ClassesRoot.OpenSubKey(@"SystemFileAssociations\" + PerceivedType + @"\" + keyName);
                }
            }

            return subKey;
        }

        /// <summary>
        /// Gets the preview handler identifier, or <c>null</c> if not available.
        /// </summary>
        private Guid? GetPreviewHandlerId()
        {
            return GetShellExtensionId("{8895b1c6-b41f-4c1c-a562-0d564250836f}");
        }

        /// <summary>
        /// Gets a boolen that determines if the file type has a thumbnail provider.
        /// </summary>
        /// <returns></returns>
        private bool GetHasThumbnailProvider()
        {
            return GetShellExtensionId("{E357FCCD-A995-4576-B01F-234630154E96}") != null
                || GetShellExtensionId("{BB2E617C-0920-11d1-9A0B-00C04FC2D6C1}") != null;
        }

        /// <summary>
        /// Gets a boolean that determines if the file type has a custom icon.
        /// </summary>
        private bool GetHasCustomIcon()
        {
            bool hasCustomIcon = false;

            using (RegistryKey defaultIconKey = OpenPreferredSubKey("DefaultIcon"))
            {
                if (defaultIconKey != null)
                {
                    string value = defaultIconKey.GetValue(null) as string;
                    hasCustomIcon = (value != null && value == "%1");
                }
            }

            if (!hasCustomIcon)
            {
                hasCustomIcon = (GetShellExtensionId("IconHandler") != null);
            }

            return hasCustomIcon;
        }

        /// <summary>
        /// Gets the id of the given shell extension for the file type.
        /// </summary>
        /// <param name="extensionId">The extension identifier.</param>
        /// <returns>The corresponding guid, or <c>null</c> if none found.</returns>
        private Guid? GetShellExtensionId(string extensionId)
        {
            Guid? result = null;

            string path = @"shellex\" + extensionId;

            using (RegistryKey shellExtensionKey = OpenPreferredSubKey(path))
            {
                if (shellExtensionKey != null)
                {
                    string id = shellExtensionKey.GetValue(null) as string;
                    if (id != null)
                    {
                        Guid parsed;
                        if (Guid.TryParse(id, out parsed))
                        {
                            result = parsed;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the description for a preview handler.
        /// </summary>
        /// <param name="id">The previe handler class id.</param>
        /// <returns>The description, or <c>null</c> if not available.</returns>
        private string GetPreviewHandlerDescription(Guid id)
        {
            string displayName = null;

            string path = String.Format(@"CLSID\{{{0}}}", id);
            using (var key = Registry.ClassesRoot.OpenSubKey(path))
            {
                if (key != null)
                {
                    // Attempt to extract the display name first from a native resource
                    string displayNameValue = key.GetValue("DisplayName") as string;
                    if (displayNameValue != null)
                    {
                        displayName = ExtractResourceStringFromRegistryValue(displayNameValue);
                    }

                    // If there was no display name, fall back to the class name
                    if (displayName == null)
                    {
                        // Failed, fall back to class description
                        displayName = key.GetValue(null) as string;
                    }
                }
            }

            return displayName;
        }

        /// <summary>
        /// Extracts a resource string from registry value, resolving the resource string from a
        /// file if appropriate.
        /// </summary>
        /// <param name="resourceSpec">The resource specification. This could be a resource string reference or a plain string.</param>
        /// <returns>The resolved resource string.</returns>
        private static string ExtractResourceStringFromRegistryValue(string resourceSpec)
        {
            Assert.ParamIsNotNull(resourceSpec, "resourceSpec");

            // Spec @ http://msdn.microsoft.com/en-us/library/windows/desktop/dd374120(v=vs.85).aspx
            // E.g. @%CommonProgramFiles%\System\wab32res.dll,-4650

            string result = null;

            if (resourceSpec.StartsWith("@"))
            {
                int indexOfSemiColon = resourceSpec.IndexOf(';');
                if (indexOfSemiColon >= 0)
                {
                    // Remove comment...
                    resourceSpec = resourceSpec.Substring(0, indexOfSemiColon);
                }

                string[] split = resourceSpec.Split(',');
                if (split.Length == 2)
                {
                    string path = split[0];
                    string idString = split[1];

                    if (path.StartsWith("@") && idString.StartsWith("-"))
                    {
                        path = path.Substring(1);
                        idString = idString.Substring(1);

                        if (path.Length > 0 && idString.Length > 0)
                        {
                            int id;
                            if (Int32.TryParse(idString, out id))
                            {
                                path = Environment.ExpandEnvironmentVariables(path);
                                result = ExtractResourceString(path, id);
                            }
                        }
                    }
                }
            }
            else
            {
                // The input registry string was not a reference to a native resource, return the string as-is.
                result = resourceSpec;
            }

            return result;
        }

        /// <summary>
        /// Extracts the resource string from a given file path and resource id.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="id">The resource identifier.</param>
        /// <returns>The extracted string, or <c>null</c> if the string could not be extracted.</returns>
        private static string ExtractResourceString(string path, int id)
        {
            string result = null;

            if (File.Exists(path) && id >= 0)
            {
                IntPtr hInstance = IntPtr.Zero;

                try
                {
                    hInstance = NativeMethods.LoadLibrary(path);
                    if (hInstance != IntPtr.Zero)
                    {
                        StringBuilder sb = new StringBuilder(1024);
                        int returnValue = NativeMethods.LoadString(hInstance, (uint)id, sb, sb.Capacity);
                        if (returnValue > 0)
                        {
                            result = sb.ToString();
                        }
                    }
                }
                finally
                {
                    if (hInstance != IntPtr.Zero)
                    {
                        NativeMethods.FreeLibrary(hInstance);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Gets all of the components of the Path environment variable.
        /// </summary>
        private static string[] GetEnvironmentPath()
        {
            return Environment.GetEnvironmentVariable("Path").Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return Extension.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            FileTypeInfo other = obj as FileTypeInfo;
            if (other != null)
            {
                return StringComparer.OrdinalIgnoreCase.Equals(Extension, other.Extension);
            }

            return false;
        }

        /// <summary>
        /// An enumeration of flags that describes the parts that can be lazy loaded for the FileTypeInfo object.
        /// </summary>
        [Flags]
        private enum Parts
        {
            None = 0,
            Description = 1,
            DefaultOpenExe = 2,
            FullPathToDefaultOpenExe = 4,
            DefaultOpenExeDescription = 8,
            PreviewHandler = 16,
            ThumbnailProvider = 32,
            IconHandler = 64,

            All = Description | DefaultOpenExe | FullPathToDefaultOpenExe | DefaultOpenExeDescription | PreviewHandler | ThumbnailProvider | IconHandler
        }
    }
}
