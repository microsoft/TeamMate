using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Tools.TeamMate.Utilities
{
    public static class FileUtilities
    {
        private static string GetTimeBasedFilename()
        {
            return DateTime.UtcNow.ToString("yyyy_MM_dd_HH_mm_ss");
        }

        public static ICollection<string> GetStaleFiles(string directory, int fileLimit)
        {
            if (Directory.Exists(directory))
            {
                var allFiles = Directory.GetFiles(directory).Select(f => new FileInfo(f));

                // Skip the n freshest files, then reverse and return
                return allFiles.OrderByDescending(f => f.LastWriteTime).Skip(fileLimit).Reverse().Select(f => f.FullName).ToArray();
            }

            return new string[0];
        }

        public static string GetTimeBasedFilePath(string parentFolder, string extension)
        {
            string filename = FileUtilities.GetTimeBasedFilename() + extension;

            string path = Path.Combine(parentFolder, filename);
            path = PathUtilities.EnsureFilenameIsUnique(path);

            // Ensure that the parent folder exists, it gets created on demand
            PathUtilities.EnsureParentDirectoryExists(path);

            return path;
        }

        public static void CleanStaleFiles(string folder, int fileLimit)
        {
            try
            {
                var staleFiles = GetStaleFiles(folder, fileLimit);
                foreach (var file in staleFiles)
                {
                    PathUtilities.TryDelete(file, DeleteMode.Force);
                }
            }
            catch (Exception e)
            {
                Log.WarnAndBreak("An error occurred attempting to delete stale log files from " + folder, e);
            }
        }
    }
}
