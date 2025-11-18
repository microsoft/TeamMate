using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.IO;
using Microsoft.Tools.TeamMate.Foundation.Windows;
using Microsoft.Tools.TeamMate.Foundation.Windows.Media.Imaging;
using Microsoft.Tools.TeamMate.Foundation.Windows.Transfer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Microsoft.Tools.TeamMate.Utilities
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class ClipboardFileExporter
    {
        // We support files, groups of files, images, and text.
        // ORDER MATTERS, SO RESPECT IT HERE AND IN EXPORT()
        private static readonly ExportableDataFormat[] SupportedDataFormats = new ExportableDataFormat[] {
            new ExportableDataFormat(CustomDataFormats.FileGroupDescriptorW, ExportFileGroup),
            new ExportableDataFormat(DataFormats.FileDrop, ExportFileDrop),
            new ExportableDataFormat(DataFormats.Rtf, "PastedText.rtf"),
            new ExportableDataFormat(DataFormats.Html, "PastedText.html"),
            new ExportableDataFormat(DataFormats.CommaSeparatedValue, "PastedTable.csv"),
            new ExportableDataFormat("PNG", "PastedImage.png"),                 // PowerPoint exposes this, and it gives better images than Bitmap...
            new ExportableDataFormat(DataFormats.Bitmap, "PastedImage.png"),
            new ExportableDataFormat(DataFormats.UnicodeText, "PastedText.txt"),
        };

        public ClipboardFileExporter(string outputFolder)
        {
            Assert.ParamIsNotNull(outputFolder, "outputFolder");

            this.OutputFolder = outputFolder;
            this.ExportedFiles = new List<string>();
        }

        public string OutputFolder { get; private set; }

        public ICollection<string> ExportedFiles { get; private set; }

        public static bool CanExport(IDataObject data)
        {
            Assert.ParamIsNotNull(data, "data");
            return SupportedDataFormats.Any(format => data.GetDataPresent(format.DataFormat));
        }

        public static bool CanExportClipboard()
        {
            IDataObject data = ClipboardUtilities.TryGetDataObject();
            return (data != null && CanExport(data));
        }

        public void Export(IDataObject data)
        {
            Assert.ParamIsNotNull(data, "data");

            foreach (var supportedFormat in SupportedDataFormats)
            {
                if (TryExport(data, supportedFormat))
                {
                    return;
                }
            }
        }

        private bool TryExport(IDataObject dataObject, ExportableDataFormat exportableFormat)
        {
            bool success = false;
            string format = exportableFormat.DataFormat;

            try
            {
                if (dataObject.GetDataPresent(format))
                {
                    if (exportableFormat.IsMultiFileFormat)
                    {
                        int previousCount = ExportedFiles.Count;
                        exportableFormat.ExportFunction(dataObject, (filename) => CreateTempFile(filename), ExportedFiles);
                        success = (ExportedFiles.Count > previousCount);
                    }
                    else
                    {
                        string path = CreateTempFile(exportableFormat.PreferredFilename);

                        object data = dataObject.GetData(format);
                        if (data is Stream)
                        {
                            ExportStream((Stream)data, path);
                            success = true;
                        }
                        else if (data is String)
                        {
                            ExportText((string)data, format, path);
                            success = true;
                        }
                        else if (data is BitmapSource)
                        {
                            ExportImage((BitmapSource)data, path);
                            success = true;
                        }

                        if (success)
                        {
                            ExportedFiles.Add(path);
                        }
                    }
                }
            }
            catch (Exception)
            {
                Log.Info("Failed to query and extract format {0} from clipboard", format);
            }

            return success;
        }

        private string CreateTempFile(string preferredName)
        {
            return PathUtilities.GetUniqueOrRandomFilename(OutputFolder, preferredName);
        }

        private static void ExportStream(Stream data, string path)
        {
            using (data)
            using (Stream output = File.Create(path))
            {
                data.CopyTo(output);
            }
        }

        private static void ExportText(string data, string format, string path)
        {
            Encoding encoding = Encoding.Default;

            if (format == DataFormats.Html)
            {
                data = CleanWordHtml(data);
                encoding = Encoding.UTF8;
            }
            else if (format == DataFormats.Rtf)
            {
                encoding = Encoding.ASCII;
            }

            File.WriteAllText(path, data, encoding);
        }

        private static void ExportImage(BitmapSource data, string path)
        {
            BitmapUtilities.Save(data, path);
        }

        private static string CleanWordHtml(String dataString)
        {
            // The HTML that word gives us starts with this wacky format...
            if (dataString.StartsWith("Version:"))
            {
                // Smells like work HTML, cleanse it?
                int index = dataString.IndexOf('<');
                if (index > 0)
                {
                    dataString = dataString.Substring(index);
                }
            }
            return dataString;
        }

        private static void ExportFileDrop(IDataObject data, Func<string, string> createTempFileDelegate, ICollection<string> outputFiles)
        {
            string[] fileDrop = data.GetData(DataFormats.FileDrop) as string[];
            if (fileDrop != null)
            {
                foreach (string file in fileDrop)
                {
                    if (!String.IsNullOrEmpty(file))
                    {
                        outputFiles.Add(file);
                    }
                }
            }
        }

        private static void ExportFileGroup(IDataObject dataObject, Func<string, string> getOutputPath, ICollection<string> outputFiles)
        {
            FileGroup fileGroup = dataObject.GetFileGroup();
            foreach (var item in fileGroup.Items)
            {
                string filename = item.FileName;
                if (!String.IsNullOrEmpty(filename))
                {
                    try
                    {
                        string outputFilePath = getOutputPath(filename);
                        item.CopyTo(outputFilePath);
                        outputFiles.Add(outputFilePath);
                    }
                    catch (Exception e)
                    {
                        // Ignore failed issues
                        Log.WarnAndBreak(e);
                    }
                }
            }
        }

        private class ExportableDataFormat
        {
            public ExportableDataFormat(string dataFormat, ExportFilesDelegate exportFunction)
            {
                this.DataFormat = dataFormat;
                this.ExportFunction = exportFunction;
            }

            public ExportableDataFormat(string dataFormat, string preferredFilename)
            {
                this.DataFormat = dataFormat;
                this.PreferredFilename = preferredFilename;
            }

            public string DataFormat { get; private set; }
            public string PreferredFilename { get; private set; }
            public ExportFilesDelegate ExportFunction { get; private set; }
            public bool IsMultiFileFormat { get { return ExportFunction != null; } }
        }

        private delegate void ExportFilesDelegate(IDataObject dataObject, Func<string, string> createTempFileDelegate, ICollection<string> outputFiles);
    }
}
