using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.Text;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Text
{
    /// <summary>
    /// Provides utility methods for dealing with text in RTF (Rich Text Format).
    /// </summary>
    public static class RtfUtilities
    {
        /// <summary>
        /// Escapes the specified text to be valid RTF text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>Valid RTF text.</returns>
        public static string Escape(string text)
        {
            // See http://www.biblioscape.com/rtf15_spec.htm
            // http://stackoverflow.com/questions/1368020/how-to-output-unicode-string-to-rtf-using-c
            Assert.ParamIsNotNull(text, "text");

            var sb = new StringBuilder();
            foreach (var c in text)
            {
                if (c == '\\' || c == '{' || c == '}')
                {
                    sb.Append(@"\" + c);
                }
                else if (c <= 0x7f)
                {
                    sb.Append(c);
                }
                else
                { 
                    sb.Append("\\u" + Convert.ToUInt32(c) + "?");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates an RTF hyperlink.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="text">The text.</param>
        /// <returns>The text representing the hyperlink in RTF format.</returns>
        public static string CreateRtfHyperlink(string uri, string text)
        {
            Assert.ParamIsNotNull(uri, "uri");
            Assert.ParamIsNotNull(text, "text");

            // KLUDGE: Reverse engineering from a simple hyperlink from wordpad copied to the clipboard
            string rtfText = @"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1033{\fonttbl{\f0\fswiss\fprq2\fcharset0 Calibri;}}
{\colortbl ;\red5\green99\blue193;}
\uc1
\pard\fs22 {\field{\*\fldinst{HYPERLINK ""@HYPERLINK@"" }}{\fldrslt{\ul\cf1 @TEXT@}}}}";

            rtfText = rtfText.Replace("@HYPERLINK@", Escape(uri));
            rtfText = rtfText.Replace("@TEXT@", Escape(text));

            return rtfText;
        }
    }
}
