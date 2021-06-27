// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Transfer
{
    internal static class HtmlDataFactory
    {
        /// <summary>
        /// The string contains index references to other spots in the string, so we need placeholders so we can compute the offsets. <br/>
        /// The <![CDATA[<<<<<<<]]>_ strings are just placeholders. We'll back-patch them actual values afterwards. <br/>
        /// The string layout (<![CDATA[<<<]]>) also ensures that it can't appear in the body of the html because the <![CDATA[<]]> <br/>
        /// character must be escaped. <br/>
        /// </summary>
        private const string HtmlHeader = @"Version:0.9
StartHTML:<<<<<<<<1
EndHTML:<<<<<<<<2
StartFragment:<<<<<<<<3
EndFragment:<<<<<<<<4
StartSelection:<<<<<<<<3
EndSelection:<<<<<<<<4";

        /// <summary>
        /// html comment to point the beginning of html fragment
        /// </summary>
        private const string HtmlStartFragment = "<!--StartFragment-->";

        /// <summary>
        /// html comment to point the end of html fragment
        /// </summary>
        private const string HtmlEndFragment = @"<!--EndFragment-->";

        /// <summary>
        /// Generate HTML fragment data string with header that is required for the clipboard.
        /// </summary>
        /// <param name="html">the html to generate for</param>
        /// <returns>the resulted string</returns>
        /// <remarks>
        /// <para>
        /// Windows Clipboard works with UTF-8 Unicode encoding while .NET strings use with UTF-16 so for clipboard to correctly
        /// decode Unicode string added to it from .NET we needs to be re-encoded it using UTF-8 encoding.
        /// </para>
        /// <para>
        /// Builds the CF_HTML header correctly for all possible HTMLs<br/>
        /// If given html contains start/end fragments then it will use them in the header:
        /// <code><![CDATA[<html><body><!--StartFragment-->hello <b>world</b><!--EndFragment--></body></html>]]></code>
        /// If given html contains html/body tags then it will inject start/end fragments to exclude html/body tags:
        /// <code><![CDATA[<html><body>hello <b>world</b></body></html>]]></code>
        /// If given html doesn't contain html/body tags then it will inject the tags and start/end fragments properly:
        /// <code><![CDATA[hello <b>world</b>]]></code>
        /// In all cases creating a proper CF_HTML header:<br/>
        /// <code>
        /// <![CDATA[
        /// Version:1.0
        /// StartHTML:000000177
        /// EndHTML:000000329
        /// StartFragment:000000277
        /// EndFragment:000000295
        /// StartSelection:000000277
        /// EndSelection:000000277
        /// <!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
        /// <html><body><!--StartFragment-->hello <b>world</b><!--EndFragment--></body></html>
        /// ]]>
        /// </code>
        /// See format specification here: http://msdn.microsoft.com/library/default.asp?url=/workshop/networking/clipboard/htmlclipboard.asp
        /// </para>
        /// </remarks>
        public static string GetHtmlDataString(string html)
        {
            var sb = new StringBuilder();
            sb.AppendLine(HtmlHeader);
            sb.AppendLine(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">");

            // if given html already provided the fragments we won't add them
            int fragmentStart, fragmentEnd;
            int fragmentStartIdx = html.IndexOf(HtmlStartFragment, StringComparison.OrdinalIgnoreCase);
            int fragmentEndIdx = html.LastIndexOf(HtmlEndFragment, StringComparison.OrdinalIgnoreCase);

            // if html tag is missing add it surrounding the given html (critical)
            int htmlOpenIdx = html.IndexOf("<html", StringComparison.OrdinalIgnoreCase);
            int htmlOpenEndIdx = htmlOpenIdx > -1 ? html.IndexOf('>', htmlOpenIdx) + 1 : -1;
            int htmlCloseIdx = html.LastIndexOf("</html", StringComparison.OrdinalIgnoreCase);

            if (fragmentStartIdx < 0 && fragmentEndIdx < 0)
            {
                int bodyOpenIdx = html.IndexOf("<body", StringComparison.OrdinalIgnoreCase);
                int bodyOpenEndIdx = bodyOpenIdx > -1 ? html.IndexOf('>', bodyOpenIdx) + 1 : -1;

                if (htmlOpenEndIdx < 0 && bodyOpenEndIdx < 0)
                {
                    // the given html doesn't contain html or body tags so we need to add them and place start/end fragments around the given html only
                    sb.Append("<html><body>");
                    sb.Append(HtmlStartFragment);
                    fragmentStart = GetByteCount(sb);
                    sb.Append(html);
                    fragmentEnd = GetByteCount(sb);
                    sb.Append(HtmlEndFragment);
                    sb.Append("</body></html>");
                }
                else
                {
                    // insert start/end fragments in the proper place (related to html/body tags if exists) so the paste will work correctly
                    int bodyCloseIdx = html.LastIndexOf("</body", StringComparison.OrdinalIgnoreCase);

                    if (htmlOpenEndIdx < 0)
                    {
                        sb.Append("<html>");
                    }
                    else
                    {
                        sb.Append(html, 0, htmlOpenEndIdx);
                    }

                    if (bodyOpenEndIdx > -1)
                    {
                        sb.Append(html, htmlOpenEndIdx > -1 ? htmlOpenEndIdx : 0, bodyOpenEndIdx - (htmlOpenEndIdx > -1 ? htmlOpenEndIdx : 0));
                    }

                    sb.Append(HtmlStartFragment);
                    fragmentStart = GetByteCount(sb);

                    var innerHtmlStart = bodyOpenEndIdx > -1 ? bodyOpenEndIdx : (htmlOpenEndIdx > -1 ? htmlOpenEndIdx : 0);
                    var innerHtmlEnd = bodyCloseIdx > -1 ? bodyCloseIdx : (htmlCloseIdx > -1 ? htmlCloseIdx : html.Length);
                    sb.Append(html, innerHtmlStart, innerHtmlEnd - innerHtmlStart);

                    fragmentEnd = GetByteCount(sb);
                    sb.Append(HtmlEndFragment);

                    if (innerHtmlEnd < html.Length)
                        sb.Append(html, innerHtmlEnd, html.Length - innerHtmlEnd);

                    if (htmlCloseIdx < 0)
                    {
                        sb.Append("</html>");
                    }
                }
            }
            else
            {
                // handle html with existing start\end fragments just need to calculate the correct bytes offset (surround with html tag if missing)
                if (htmlOpenEndIdx < 0)
                {
                    sb.Append("<html>");
                }

                int start = GetByteCount(sb);
                sb.Append(html);
                fragmentStart = start + GetByteCount(sb, start, start + fragmentStartIdx) + HtmlStartFragment.Length;
                fragmentEnd = start + GetByteCount(sb, start, start + fragmentEndIdx);

                if (htmlCloseIdx < 0)
                {
                    sb.Append("</html>");
                }
            }

            // Back-patch offsets (scan only the header part for performance)
            sb.Replace("<<<<<<<<1", HtmlHeader.Length.ToString("D9"), 0, HtmlHeader.Length);
            sb.Replace("<<<<<<<<2", GetByteCount(sb).ToString("D9"), 0, HtmlHeader.Length);
            sb.Replace("<<<<<<<<3", fragmentStart.ToString("D9"), 0, HtmlHeader.Length);
            sb.Replace("<<<<<<<<4", fragmentEnd.ToString("D9"), 0, HtmlHeader.Length);

            return sb.ToString();
        }

        /// <summary>
        /// Calculates the number of bytes produced by encoding the string in the string builder in UTF-8 and not .NET default string encoding.
        /// </summary>
        /// <param name="sb">the string builder to count its string</param>
        /// <param name="start">optional: the start index to calculate from (default - start of string)</param>
        /// <param name="end">optional: the end index to calculate to (default - end of string)</param>
        /// <returns>the number of bytes required to encode the string in UTF-8</returns>
        private static int GetByteCount(StringBuilder sb, int start = 0, int end = -1)
        {
            char[] singleCharArray = new char[1];

            int count = 0;
            end = end > -1 ? end : sb.Length;
            for (int i = start; i < end; i++)
            {
                singleCharArray[0] = sb[i];
                count += Encoding.UTF8.GetByteCount(singleCharArray);
            }
            return count;
        }
    }
}
