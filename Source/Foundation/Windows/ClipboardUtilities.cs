using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.Text;
using System.Windows;

namespace Microsoft.Tools.TeamMate.Foundation.Windows
{
    /// <summary>
    /// Provides utility methods for the Clipboard.
    /// </summary>
    public static class ClipboardUtilities
    {
        private const string Header = @"Version:0.9      
StartHTML:{0}      
EndHTML:{1}      
StartFragment:{2}      
EndFragment:{3}      
StartSelection:{2}      
EndSelection:{3}";

        private const string StartFragment = "<!--StartFragment-->";

        private const string EndFragment = @"<!--EndFragment-->";

        private static readonly char[] byteCount = new char[1];

        /// <summary>
        /// Tries the get data object from the clipboard, and never fails with a thrown exception.
        /// </summary>
        /// <returns>The data object, or <c>null</c> if one couldn't be retrieved.</returns>
        public static IDataObject TryGetDataObject()
        {
            try
            {
                return Clipboard.GetDataObject();
            }
            catch(Exception e)
            {
                // System.Runtime.InteropServices.SEHException (0x80004005): External component has thrown an exception.
                //   at System.Windows.Clipboard.GetDataObjectInternal()

                // System.Runtime.InteropServices.COMException (0x800401D0): OpenClipboard Failed (Exception from HRESULT: 0x800401D0 (CLIPBRD_E_CANT_OPEN))
                // at System.Runtime.InteropServices.Marshal.ThrowExceptionForHRInternal(Int32 errorCode, IntPtr errorInfo)
                // at System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(Int32 errorCode)
                // at System.Windows.Clipboard.GetDataObjectInternal()

                Log.WarnAndBreak(e);
            }

            return null;
        }

        private static DataObject CreateDataObject(string html, string text)
        {
            html = html ?? String.Empty;
            var htmlFragment = GetHtmlDataString(html);

            // re-encode the string so it will work  correctly (fixed in CLR 4.0)      
            if (Environment.Version.Major < 4 && html.Length != Encoding.UTF8.GetByteCount(html))
                htmlFragment = Encoding.Default.GetString(Encoding.UTF8.GetBytes(htmlFragment));

            var dataObject = new DataObject();
            dataObject.SetData(DataFormats.Html, htmlFragment);
            dataObject.SetData(DataFormats.Text, text);
            dataObject.SetData(DataFormats.UnicodeText, text);
            return dataObject;
        }

        public static void CopyToClipboard(string html, string text)
        {
            var dataObject = CreateDataObject(html, text);
            Clipboard.SetDataObject(dataObject, true);
        }

        private static string GetHtmlDataString(string html)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(Header);
            stringBuilder.AppendLine(@"<!DOCTYPE HTML  PUBLIC ""-//W3C//DTD HTML 4.0  Transitional//EN"">");

            var indexHtmlOpenStart = html.IndexOf("<html", StringComparison.OrdinalIgnoreCase);
            var indexHtmlOpenEnd = html.IndexOf('>', indexHtmlOpenStart) + 1;
            var indexHtmlCloseStart = html.LastIndexOf("</html", StringComparison.OrdinalIgnoreCase);

            int indexHtmlBodyStart = html.IndexOf("<body", StringComparison.OrdinalIgnoreCase);
            int indexHtmlBodyEnd = html.IndexOf('>', indexHtmlBodyStart) + 1;

            int indexHtmlBodyEndStart = html.LastIndexOf("</body", StringComparison.OrdinalIgnoreCase);
            stringBuilder.Append(html, 0, indexHtmlOpenEnd);
            stringBuilder.Append(html, indexHtmlOpenEnd, indexHtmlBodyEnd - indexHtmlOpenEnd);

            stringBuilder.Append(StartFragment);
            int fragmentStart = GetByteCount(stringBuilder);

            stringBuilder.Append(html, indexHtmlBodyEnd, indexHtmlBodyEndStart - indexHtmlBodyEnd);

            int fragmentEnd = GetByteCount(stringBuilder);
            stringBuilder.Append(EndFragment);
            stringBuilder.Append(html, indexHtmlBodyEndStart, html.Length - indexHtmlBodyEndStart);

            stringBuilder.Replace("{0}", Header.Length.ToString("D9"), 0, Header.Length);
            stringBuilder.Replace("{1}", GetByteCount(stringBuilder).ToString("D9"), 0, Header.Length);
            stringBuilder.Replace("{2}", fragmentStart.ToString("D9"), 0, Header.Length);
            stringBuilder.Replace("{3}", fragmentEnd.ToString("D9"), 0, Header.Length);

            return stringBuilder.ToString();
        }

        private static int GetByteCount(StringBuilder sb)
        {
            int count = 0;
            for (int i = 0; i < sb.Length; i++)
            {
                byteCount[0] = sb[i];
                count += Encoding.UTF8.GetByteCount(byteCount);
            }

            return count;
        }
    }
}
