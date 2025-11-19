//*************************************************************************************************
// HttpUtility.cs
//
// This partial class of HttpUtility contains functionality forked from System.Web.HttpUtility.
// Only our server assemblies can take a dependency on System.Web because it is not part of the
// .NET Framework "Client Profile". Client and common code that needs to use helper functions
// such as UrlEncode and UrlDecode from System.Web.HttpUtility must call the methods on this class
// instead to avoid a dependency on System.Web.
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.Tools.TeamMate.Foundation.Web
{
    /// <summary>
    /// Provides methods for encoding and decoding URLs.
    /// </summary>
    /// <remarks>
    /// This class contains forked functionality from .NET System.Web.HttpUtility, without requiring a dependency
    /// to the System.Web.dll.
    /// </remarks>
    public static class HttpUtility
    {
        #region ParseQueryString

        // *** Source: ndp/fx/src/xsp/system/web/httpserverutility.cs
                
        public static NameValueCollection ParseQueryString(string query)
        {
            return ParseQueryString(query, Encoding.UTF8);
        }

        public static NameValueCollection ParseQueryString(string query, Encoding encoding)
        {
            return ParseQueryString(query, encoding, true);
        }

        public static NameValueCollection ParseQueryString(string query, Encoding encoding, Boolean urlEncoded)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }

            if (query.Length > 0 && query[0] == '?')
            {
                query = query.Substring(1);
            }

            return new HttpValueCollection(query, false, urlEncoded, encoding);
        }

        #endregion

        #region UrlEncode implementation

        // *** Source: ndp/fx/src/xsp/system/web/util/httpencoder.cs

        private static byte[] UrlEncode(byte[] bytes, int offset, int count, bool alwaysCreateNewReturnValue)
        {
            byte[] encoded = UrlEncode(bytes, offset, count);

            return (alwaysCreateNewReturnValue && (encoded != null) && (encoded == bytes))
                ? (byte[])encoded.Clone()
                : encoded;
        }

        private static byte[] UrlEncode(byte[] bytes, int offset, int count)
        {
            if (!ValidateUrlEncodingParameters(bytes, offset, count))
            {
                return null;
            }

            int cSpaces = 0;
            int cUnsafe = 0;

            // count them first
            for (int i = 0; i < count; i++)
            {
                char ch = (char)bytes[offset + i];

                if (ch == ' ')
                    cSpaces++;
                else if (!IsUrlSafeChar(ch))
                    cUnsafe++;
            }

            // nothing to expand?
            if (cSpaces == 0 && cUnsafe == 0)
                return bytes;

            // expand not 'safe' characters into %XX, spaces to +s
            byte[] expandedBytes = new byte[count + cUnsafe * 2];
            int pos = 0;

            for (int i = 0; i < count; i++)
            {
                byte b = bytes[offset + i];
                char ch = (char)b;

                if (IsUrlSafeChar(ch))
                {
                    expandedBytes[pos++] = b;
                }
                else if (ch == ' ')
                {
                    expandedBytes[pos++] = (byte)'+';
                }
                else
                {
                    expandedBytes[pos++] = (byte)'%';
                    expandedBytes[pos++] = (byte)IntToHex((b >> 4) & 0xf);
                    expandedBytes[pos++] = (byte)IntToHex(b & 0x0f);
                }
            }

            return expandedBytes;
        }

        //  Helper to encode the non-ASCII url characters only
        private static String UrlEncodeNonAscii(string str, Encoding e)
        {
            if (String.IsNullOrEmpty(str))
                return str;
            if (e == null)
                e = Encoding.UTF8;
            byte[] bytes = e.GetBytes(str);
            byte[] encodedBytes = UrlEncodeNonAscii(bytes, 0, bytes.Length, false /* alwaysCreateNewReturnValue */);
            return Encoding.ASCII.GetString(encodedBytes);
        }

        private static byte[] UrlEncodeNonAscii(byte[] bytes, int offset, int count, bool alwaysCreateNewReturnValue)
        {
            if (!ValidateUrlEncodingParameters(bytes, offset, count))
            {
                return null;
            }

            int cNonAscii = 0;

            // count them first
            for (int i = 0; i < count; i++)
            {
                if (IsNonAsciiByte(bytes[offset + i]))
                    cNonAscii++;
            }

            // nothing to expand?
            if (!alwaysCreateNewReturnValue && cNonAscii == 0)
                return bytes;

            // expand not 'safe' characters into %XX, spaces to +s
            byte[] expandedBytes = new byte[count + cNonAscii * 2];
            int pos = 0;

            for (int i = 0; i < count; i++)
            {
                byte b = bytes[offset + i];

                if (IsNonAsciiByte(b))
                {
                    expandedBytes[pos++] = (byte)'%';
                    expandedBytes[pos++] = (byte)IntToHex((b >> 4) & 0xf);
                    expandedBytes[pos++] = (byte)IntToHex(b & 0x0f);
                }
                else
                {
                    expandedBytes[pos++] = b;
                }
            }

            return expandedBytes;
        }

        #endregion

        #region UrlEncode public methods

        // *** Source: ndp/fx/src/xsp/system/web/httpserverutility.cs

        public static string UrlEncode(string str)
        {
            if (str == null)
                return null;

            return UrlEncode(str, Encoding.UTF8);
        }

        public static string Base64Encode(string str)
        {
            if (str == null)
                return null;

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
        }

        public static string UrlEncode(string str, Encoding e)
        {
            if (str == null)
                return null;

            return Encoding.ASCII.GetString(UrlEncodeToBytes(str, e));
        }

        public static string UrlEncode(byte[] bytes)
        {
            if (bytes == null)
                return null;

            return Encoding.ASCII.GetString(UrlEncodeToBytes(bytes));
        }

        public static byte[] UrlEncodeToBytes(string str)
        {
            if (str == null)
                return null;

            return UrlEncodeToBytes(str, Encoding.UTF8);
        }

        public static byte[] UrlEncodeToBytes(byte[] bytes)
        {
            if (bytes == null)
                return null;

            return UrlEncodeToBytes(bytes, 0, bytes.Length);
        }

        public static byte[] UrlEncodeToBytes(string str, Encoding e)
        {
            if (str == null)
                return null;

            byte[] bytes = e.GetBytes(str);
            return UrlEncode(bytes, 0, bytes.Length, false /* alwaysCreateNewReturnValue */);
        }

        public static byte[] UrlEncodeToBytes(byte[] bytes, int offset, int count)
        {
            return UrlEncode(bytes, offset, count, true /* alwaysCreateNewReturnValue */);
        }

        // *** Source: ndp/fx/src/xsp/system/web/util/httpencoder.cs

        public static string UrlPathEncode(string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return str;
            }

            // recurse in case there is a query string
            int i = str.IndexOf('?');
            if (i >= 0)
                return UrlPathEncode(str.Substring(0, i)) + str.Substring(i);

            // encode DBCS characters and spaces only
            return UrlEncodeSpaces(UrlEncodeNonAscii(str, Encoding.UTF8));
        }

        #endregion

        #region UrlEncodeUnicode

        // *** Source: ndp/fx/src/xsp/system/web/util/httpencoder.cs

        public static string UrlEncodeUnicode(string value)
        {
            if (value == null)
            {
                return null;
            }

            int l = value.Length;
            StringBuilder sb = new StringBuilder(l);

            for (int i = 0; i < l; i++)
            {
                char ch = value[i];

                if ((ch & 0xff80) == 0)
                {  // 7 bit?
                    if (IsUrlSafeChar(ch))
                    {
                        sb.Append(ch);
                    }
                    else if (ch == ' ')
                    {
                        sb.Append('+');
                    }
                    else
                    {
                        sb.Append('%');
                        sb.Append(IntToHex((ch >> 4) & 0xf));
                        sb.Append(IntToHex((ch) & 0xf));
                    }
                }
                else
                { // arbitrary Unicode?
                    sb.Append("%u");
                    sb.Append(IntToHex((ch >> 12) & 0xf));
                    sb.Append(IntToHex((ch >> 8) & 0xf));
                    sb.Append(IntToHex((ch >> 4) & 0xf));
                    sb.Append(IntToHex((ch) & 0xf));
                }
            }

            return sb.ToString();
        }

        #endregion

        #region UrlDecode implementation

        // *** Source: ndp/fx/src/xsp/system/web/util/httpencoder.cs

        private static string UrlDecodeInternal(string value, Encoding encoding)
        {
            if (value == null)
            {
                return null;
            }

            int count = value.Length;
            UrlDecoder helper = new UrlDecoder(count, encoding);

            // go through the string's chars collapsing %XX and %uXXXX and
            // appending each char as char, with exception of %XX constructs
            // that are appended as bytes

            for (int pos = 0; pos < count; pos++)
            {
                char ch = value[pos];

                if (ch == '+')
                {
                    ch = ' ';
                }
                else if (ch == '%' && pos < count - 2)
                {
                    if (value[pos + 1] == 'u' && pos < count - 5)
                    {
                        int h1 = HexToInt(value[pos + 2]);
                        int h2 = HexToInt(value[pos + 3]);
                        int h3 = HexToInt(value[pos + 4]);
                        int h4 = HexToInt(value[pos + 5]);

                        if (h1 >= 0 && h2 >= 0 && h3 >= 0 && h4 >= 0)
                        {   // valid 4 hex chars
                            ch = (char)((h1 << 12) | (h2 << 8) | (h3 << 4) | h4);
                            pos += 5;

                            // only add as char
                            helper.AddChar(ch);
                            continue;
                        }
                    }
                    else
                    {
                        int h1 = HexToInt(value[pos + 1]);
                        int h2 = HexToInt(value[pos + 2]);

                        if (h1 >= 0 && h2 >= 0)
                        {     // valid 2 hex chars
                            byte b = (byte)((h1 << 4) | h2);
                            pos += 2;

                            // don't add as char
                            helper.AddByte(b);
                            continue;
                        }
                    }
                }

                if ((ch & 0xFF80) == 0)
                    helper.AddByte((byte)ch); // 7 bit have to go as bytes because of Unicode
                else
                    helper.AddChar(ch);
            }

            return helper.GetString();
        }

        private static byte[] UrlDecodeInternal(byte[] bytes, int offset, int count)
        {
            if (!ValidateUrlEncodingParameters(bytes, offset, count))
            {
                return null;
            }

            int decodedBytesCount = 0;
            byte[] decodedBytes = new byte[count];

            for (int i = 0; i < count; i++)
            {
                int pos = offset + i;
                byte b = bytes[pos];

                if (b == '+')
                {
                    b = (byte)' ';
                }
                else if (b == '%' && i < count - 2)
                {
                    int h1 = HexToInt((char)bytes[pos + 1]);
                    int h2 = HexToInt((char)bytes[pos + 2]);

                    if (h1 >= 0 && h2 >= 0)
                    {     // valid 2 hex chars
                        b = (byte)((h1 << 4) | h2);
                        i += 2;
                    }
                }

                decodedBytes[decodedBytesCount++] = b;
            }

            if (decodedBytesCount < decodedBytes.Length)
            {
                byte[] newDecodedBytes = new byte[decodedBytesCount];
                Array.Copy(decodedBytes, newDecodedBytes, decodedBytesCount);
                decodedBytes = newDecodedBytes;
            }

            return decodedBytes;
        }

        private static string UrlDecodeInternal(byte[] bytes, int offset, int count, Encoding encoding)
        {
            if (!ValidateUrlEncodingParameters(bytes, offset, count))
            {
                return null;
            }

            UrlDecoder helper = new UrlDecoder(count, encoding);

            // go through the bytes collapsing %XX and %uXXXX and appending
            // each byte as byte, with exception of %uXXXX constructs that
            // are appended as chars

            for (int i = 0; i < count; i++)
            {
                int pos = offset + i;
                byte b = bytes[pos];

                // The code assumes that + and % cannot be in multibyte sequence

                if (b == '+')
                {
                    b = (byte)' ';
                }
                else if (b == '%' && i < count - 2)
                {
                    if (bytes[pos + 1] == 'u' && i < count - 5)
                    {
                        int h1 = HexToInt((char)bytes[pos + 2]);
                        int h2 = HexToInt((char)bytes[pos + 3]);
                        int h3 = HexToInt((char)bytes[pos + 4]);
                        int h4 = HexToInt((char)bytes[pos + 5]);

                        if (h1 >= 0 && h2 >= 0 && h3 >= 0 && h4 >= 0)
                        {   // valid 4 hex chars
                            char ch = (char)((h1 << 12) | (h2 << 8) | (h3 << 4) | h4);
                            i += 5;

                            // don't add as byte
                            helper.AddChar(ch);
                            continue;
                        }
                    }
                    else
                    {
                        int h1 = HexToInt((char)bytes[pos + 1]);
                        int h2 = HexToInt((char)bytes[pos + 2]);

                        if (h1 >= 0 && h2 >= 0)
                        {     // valid 2 hex chars
                            b = (byte)((h1 << 4) | h2);
                            i += 2;
                        }
                    }
                }

                helper.AddByte(b);
            }

            return helper.GetString();
        }

        #endregion

        #region UrlDecode public methods

        // *** Source: ndp/fx/src/xsp/system/web/httpserverutility.cs

        public static string UrlDecode(string str)
        {
            if (str == null)
                return null;

            return UrlDecode(str, Encoding.UTF8);
        }

        public static string Base64Decode(string str)
        {
            if (str == null)
                return null;

            return Encoding.UTF8.GetString(Convert.FromBase64String(str));
        }

        public static string UrlDecode(string str, Encoding e)
        {
            return UrlDecodeInternal(str, e);
        }

        public static string UrlDecode(byte[] bytes, Encoding e)
        {
            if (bytes == null)
                return null;

            return UrlDecode(bytes, 0, bytes.Length, e);
        }

        public static string UrlDecode(byte[] bytes, int offset, int count, Encoding e)
        {
            return UrlDecodeInternal(bytes, offset, count, e);
        }

        public static byte[] UrlDecodeToBytes(string str)
        {
            if (str == null)
                return null;

            return UrlDecodeToBytes(str, Encoding.UTF8);
        }

        public static byte[] UrlDecodeToBytes(string str, Encoding e)
        {
            if (str == null)
                return null;

            return UrlDecodeToBytes(e.GetBytes(str));
        }

        public static byte[] UrlDecodeToBytes(byte[] bytes)
        {
            if (bytes == null)
                return null;

            return UrlDecodeToBytes(bytes, 0, (bytes != null) ? bytes.Length : 0);
        }

        public static byte[] UrlDecodeToBytes(byte[] bytes, int offset, int count)
        {
            return UrlDecodeInternal(bytes, offset, count);
        }

        #endregion

        #region Helper methods

        // *** Source: ndp/fx/src/xsp/system/web/util/httpencoderutility.cs

        public static int HexToInt(char h)
        {
            return (h >= '0' && h <= '9') ? h - '0' :
            (h >= 'a' && h <= 'f') ? h - 'a' + 10 :
            (h >= 'A' && h <= 'F') ? h - 'A' + 10 :
            -1;
        }

        public static char IntToHex(int n)
        {
            Debug.Assert(n < 0x10);

            if (n <= 9)
                return (char)(n + (int)'0');
            else
                return (char)(n - 10 + (int)'a');
        }

        // Set of safe chars, from RFC 1738.4 minus '+'
        public static bool IsUrlSafeChar(char ch)
        {
            if (ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z' || ch >= '0' && ch <= '9')
                return true;

            switch (ch)
            {
                case '-':
                case '_':
                case '.':
                case '!':
                case '*':
                case '(':
                case ')':
                    return true;
            }

            return false;
        }

        //  Helper to encode spaces only
        internal static String UrlEncodeSpaces(string str)
        {
            if (str != null && str.IndexOf(' ') >= 0)
                str = str.Replace(" ", "%20");
            return str;
        }

        // *** Source: ndp/fx/src/xsp/system/web/util/httpencoder.cs

        private static bool ValidateUrlEncodingParameters(byte[] bytes, int offset, int count)
        {
            if (bytes == null && count == 0)
                return false;
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (offset < 0 || offset > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0 || offset + count > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            return true;
        }

        private static bool IsNonAsciiByte(byte b)
        {
            return (b >= 0x7F || b < 0x20);
        }

        #endregion

        #region UrlDecoder nested class

        // *** Source: ndp/fx/src/xsp/system/web/util/httpencoder.cs

        // Internal class to facilitate URL decoding -- keeps char buffer and byte buffer, allows appending of either chars or bytes
        private class UrlDecoder
        {
            private int _bufferSize;

            // Accumulate characters in a special array
            private int _numChars;
            private char[] _charBuffer;

            // Accumulate bytes for decoding into characters in a special array
            private int _numBytes;
            private byte[] _byteBuffer;

            // Encoding to convert chars to bytes
            private Encoding _encoding;

            private void FlushBytes()
            {
                if (_numBytes > 0) {
                    _numChars += _encoding.GetChars(_byteBuffer, 0, _numBytes, _charBuffer, _numChars);
                    _numBytes = 0;
                }
            }

            internal UrlDecoder(int bufferSize, Encoding encoding)
            {
                _bufferSize = bufferSize;
                _encoding = encoding;

                _charBuffer = new char[bufferSize];
                // byte buffer created on demand
            }

            internal void AddChar(char ch)
            {
                if (_numBytes > 0)
                    FlushBytes();

                _charBuffer[_numChars++] = ch;
            }

            internal void AddByte(byte b)
            {
                if (_byteBuffer == null)
                    _byteBuffer = new byte[_bufferSize];

                _byteBuffer[_numBytes++] = b;
            }

            internal String GetString()
            {
                if (_numBytes > 0)
                    FlushBytes();

                if (_numChars > 0)
                    return new String(_charBuffer, 0, _numChars);
                else
                    return String.Empty;
            }
        }

        #endregion

        #region HttpValueCollection nested class

        // *** Source: ndp/fx/src/xsp/system/web/httpvaluecollection.cs

        internal class HttpValueCollection : NameValueCollection
        {
            internal HttpValueCollection()
                : base(StringComparer.OrdinalIgnoreCase)
            {
            }

            internal HttpValueCollection(String str, bool readOnly, bool urlencoded, Encoding encoding)
                : base(StringComparer.OrdinalIgnoreCase)
            {
                if (!String.IsNullOrEmpty(str))
                    FillFromString(str, urlencoded, encoding);

                IsReadOnly = readOnly;
            }

            internal HttpValueCollection(int capacity)
                : base(capacity, StringComparer.OrdinalIgnoreCase)
            {
            }

            internal void MakeReadOnly()
            {
                IsReadOnly = true;
            }

            internal void MakeReadWrite()
            {
                IsReadOnly = false;
            }

            internal void FillFromString(String s)
            {
                FillFromString(s, false, null);
            }

            internal void FillFromString(String s, bool urlencoded, Encoding encoding)
            {
                int l = (s != null) ? s.Length : 0;
                int i = 0;

                while (i < l)
                {
                    // find next & while noting first = on the way (and if there are more)

                    int si = i;
                    int ti = -1;

                    while (i < l)
                    {
                        char ch = s[i];

                        if (ch == '=')
                        {
                            if (ti < 0)
                                ti = i;
                        }
                        else if (ch == '&')
                        {
                            break;
                        }

                        i++;
                    }

                    // extract the name / value pair

                    String name = null;
                    String value = null;

                    if (ti >= 0)
                    {
                        name = s.Substring(si, ti - si);
                        value = s.Substring(ti + 1, i - ti - 1);
                    }
                    else
                    {
                        value = s.Substring(si, i - si);
                    }

                    // add name / value pair to the collection

                    if (urlencoded)
                        base.Add(
                           HttpUtility.UrlDecode(name, encoding),
                           HttpUtility.UrlDecode(value, encoding));
                    else
                        base.Add(name, value);

                    // trailing '&'

                    if (i == l - 1 && s[i] == '&')
                        base.Add(null, String.Empty);

                    i++;
                }
            }

            internal void FillFromEncodedBytes(byte[] bytes, Encoding encoding)
            {
                int l = (bytes != null) ? bytes.Length : 0;
                int i = 0;

                while (i < l)
                {
                    // find next & while noting first = on the way (and if there are more)

                    int si = i;
                    int ti = -1;

                    while (i < l)
                    {
                        byte b = bytes[i];

                        if (b == '=')
                        {
                            if (ti < 0)
                                ti = i;
                        }
                        else if (b == '&')
                        {
                            break;
                        }

                        i++;
                    }

                    // extract the name / value pair

                    String name, value;

                    if (ti >= 0)
                    {
                        name = HttpUtility.UrlDecode(bytes, si, ti - si, encoding);
                        value = HttpUtility.UrlDecode(bytes, ti + 1, i - ti - 1, encoding);
                    }
                    else
                    {
                        name = null;
                        value = HttpUtility.UrlDecode(bytes, si, i - si, encoding);
                    }

                    // add name / value pair to the collection

                    base.Add(name, value);

                    // trailing '&'

                    if (i == l - 1 && bytes[i] == '&')
                        base.Add(null, String.Empty);

                    i++;
                }
            }

            internal void Reset()
            {
                base.Clear();
            }

            public override String ToString()
            {
                return ToString(true);
            }

            internal virtual String ToString(bool urlencoded)
            {
                return ToString(urlencoded, null);
            }

            internal virtual String ToString(bool urlencoded, IDictionary excludeKeys)
            {
                int n = Count;
                if (n == 0)
                    return String.Empty;

                StringBuilder s = new StringBuilder();
                String key, keyPrefix, item;

                for (int i = 0; i < n; i++)
                {
                    key = GetKey(i);

                    if (excludeKeys != null && key != null && excludeKeys[key] != null)
                        continue;
                    if (urlencoded)
                        key = HttpUtility.UrlEncodeUnicode(key);
                    keyPrefix = (key != null) ? (key + "=") : String.Empty;

                    ArrayList values = (ArrayList)BaseGet(i);
                    int numValues = (values != null) ? values.Count : 0;

                    if (s.Length > 0)
                        s.Append('&');

                    if (numValues == 1)
                    {
                        s.Append(keyPrefix);
                        item = (String)values[0];
                        if (urlencoded)
                            item = HttpUtility.UrlEncodeUnicode(item);
                        s.Append(item);
                    }
                    else if (numValues == 0)
                    {
                        s.Append(keyPrefix);
                    }
                    else
                    {
                        for (int j = 0; j < numValues; j++)
                        {
                            if (j > 0)
                                s.Append('&');
                            s.Append(keyPrefix);
                            item = (String)values[j];
                            if (urlencoded)
                                item = HttpUtility.UrlEncodeUnicode(item);
                            s.Append(item);
                        }
                    }
                }

                return s.ToString();
            }
        }

        #endregion

        #region HtmlEncode

        // *** Source: ndp/fx/src/net/system/net/webutility.cs

        public static string HtmlEncode(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return value;
            }

            // Don't create string writer if we don't have nothing to encode
            int index = IndexOfHtmlEncodingChars(value, 0);
            if (index == -1)
            {
                return value;
            }

            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            HtmlEncode(value, writer);
            return writer.ToString();
        }

        public static unsafe void HtmlEncode(string value, TextWriter output)
        {
            if (value == null)
            {
                return;
            }
            if (output == null)
            {
                throw new ArgumentNullException("output");
            }

            int index = IndexOfHtmlEncodingChars(value, 0);
            if (index == -1)
            {
                output.Write(value);
                return;
            }

            Debug.Assert(0 <= index && index <= value.Length, "0 <= index && index <= value.Length");

            int cch = value.Length - index;
            fixed (char* str = value)
            {
                char* pch = str;
                while (index-- > 0)
                {
                    output.Write(*pch++);
                }

                while (cch-- > 0)
                {
                    char ch = *pch++;
                    if (ch <= '>')
                    {
                        switch (ch)
                        {
                            case '<':
                                output.Write("&lt;");
                                break;
                            case '>':
                                output.Write("&gt;");
                                break;
                            case '"':
                                output.Write("&quot;");
                                break;
                            case '\'':
                                output.Write("&#39;");
                                break;
                            case '&':
                                output.Write("&amp;");
                                break;
                            default:
                                output.Write(ch);
                                break;
                        }
                    }
#if ENTITY_ENCODE_HIGH_ASCII_CHARS
                    else if (ch >= 160 && ch < 256)
                    {
                        // The seemingly arbitrary 160 comes from RFC
                        output.Write("&#");
                        output.Write(((int)ch).ToString(NumberFormatInfo.InvariantInfo));
                        output.Write(';');
                    }
#endif // ENTITY_ENCODE_HIGH_ASCII_CHARS
                    else
                    {
                        output.Write(ch);
                    }
                }
            }
        }

        #endregion

        #region HtmlDecode

        // *** Source: ndp/fx/src/net/system/net/webutility.cs

        public static string HtmlDecode(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return value;
            }

            // Don't create string writer if we don't have nothing to encode
            if (value.IndexOf('&') < 0)
            {
                return value;
            }

            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            HtmlDecode(value, writer);
            return writer.ToString();
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.UInt16.TryParse(System.String,System.Globalization.NumberStyles,System.IFormatProvider,System.UInt16@)", Justification = "UInt16.TryParse guarantees that result is zero if the parse fails.")]
        public static void HtmlDecode(string value, TextWriter output)
        {
            if (value == null)
            {
                return;
            }
            if (output == null)
            {
                throw new ArgumentNullException("output");
            }

            if (value.IndexOf('&') < 0)
            {
                output.Write(value);        // good as is
                return;
            }

            int l = value.Length;
            for (int i = 0; i < l; i++)
            {
                char ch = value[i];

                if (ch == '&')
                {
                    // We found a '&'. Now look for the next ';' or '&'. The idea is that
                    // if we find another '&' before finding a ';', then this is not an entity,
                    // and the next '&' might start a real entity (VSWhidbey 275184)
                    int index = value.IndexOfAny(_htmlEntityEndingChars, i + 1);
                    if (index > 0 && value[index] == ';')
                    {
                        string entity = value.Substring(i + 1, index - i - 1);

                        if (entity.Length > 1 && entity[0] == '#')
                        {
                            // The # syntax can be in decimal or hex, e.g.
                            //      &#229;  --> decimal
                            //      &#xE5;  --> same char in hex
                            // See http://www.w3.org/TR/REC-html40/charset.html#entities

                            ushort parsed;
                            if (entity[1] == 'x' || entity[1] == 'X')
                            {
                                UInt16.TryParse(entity.Substring(2), NumberStyles.AllowHexSpecifier, NumberFormatInfo.InvariantInfo, out parsed);
                            }
                            else
                            {
                                UInt16.TryParse(entity.Substring(1), NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out parsed);
                            }

                            if (parsed != 0)
                            {
                                ch = (char)parsed;
                                i = index; // already looked at everything until semicolon
                            }
                        }
                        else
                        {
                            i = index; // already looked at everything until semicolon

                            char entityChar = HtmlEntities.Lookup(entity);
                            if (entityChar != (char)0)
                            {
                                ch = entityChar;
                            }
                            else
                            {
                                output.Write('&');
                                output.Write(entity);
                                output.Write(';');
                                continue;
                            }
                        }

                    }
                }

                output.Write(ch);
            }
        }

        #endregion

        #region HtmlEncode/Decode helper methods

        // *** Source: ndp/fx/src/net/system/net/webutility.cs

        private static unsafe int IndexOfHtmlEncodingChars(string s, int startPos)
        {
            Debug.Assert(0 <= startPos && startPos <= s.Length, "0 <= startPos && startPos <= s.Length");

            int cch = s.Length - startPos;
            fixed (char* str = s)
            {
                for (char* pch = &str[startPos]; cch > 0; pch++, cch--)
                {
                    char ch = *pch;
                    if (ch <= '>')
                    {
                        switch (ch)
                        {
                            case '<':
                            case '>':
                            case '"':
                            case '\'':
                            case '&':
                                return s.Length - cch;
                        }
                    }
#if ENTITY_ENCODE_HIGH_ASCII_CHARS
                    else if (ch >= 160 && ch < 256)
                    {
                        return s.Length - cch;
                    }
#endif // ENTITY_ENCODE_HIGH_ASCII_CHARS
                }
            }

            return -1;
        }

        #endregion

        #region HtmlEntities nested class

        // *** Source: ndp/fx/src/net/system/net/webutility.cs

        // helper class for lookup of HTML encoding entities
        private static class HtmlEntities
        {
            // The list is from http://www.w3.org/TR/REC-html40/sgml/entities.html, except for &apos;, which
            // is defined in http://www.w3.org/TR/2008/REC-xml-20081126/#sec-predefined-ent.

            private static String[] _entitiesList = new String[] {
                "\x0022-quot",
                "\x0026-amp",
                "\x0027-apos",
                "\x003c-lt",
                "\x003e-gt",
                "\x00a0-nbsp",
                "\x00a1-iexcl",
                "\x00a2-cent",
                "\x00a3-pound",
                "\x00a4-curren",
                "\x00a5-yen",
                "\x00a6-brvbar",
                "\x00a7-sect",
                "\x00a8-uml",
                "\x00a9-copy",
                "\x00aa-ordf",
                "\x00ab-laquo",
                "\x00ac-not",
                "\x00ad-shy",
                "\x00ae-reg",
                "\x00af-macr",
                "\x00b0-deg",
                "\x00b1-plusmn",
                "\x00b2-sup2",
                "\x00b3-sup3",
                "\x00b4-acute",
                "\x00b5-micro",
                "\x00b6-para",
                "\x00b7-middot",
                "\x00b8-cedil",
                "\x00b9-sup1",
                "\x00ba-ordm",
                "\x00bb-raquo",
                "\x00bc-frac14",
                "\x00bd-frac12",
                "\x00be-frac34",
                "\x00bf-iquest",
                "\x00c0-Agrave",
                "\x00c1-Aacute",
                "\x00c2-Acirc",
                "\x00c3-Atilde",
                "\x00c4-Auml",
                "\x00c5-Aring",
                "\x00c6-AElig",
                "\x00c7-Ccedil",
                "\x00c8-Egrave",
                "\x00c9-Eacute",
                "\x00ca-Ecirc",
                "\x00cb-Euml",
                "\x00cc-Igrave",
                "\x00cd-Iacute",
                "\x00ce-Icirc",
                "\x00cf-Iuml",
                "\x00d0-ETH",
                "\x00d1-Ntilde",
                "\x00d2-Ograve",
                "\x00d3-Oacute",
                "\x00d4-Ocirc",
                "\x00d5-Otilde",
                "\x00d6-Ouml",
                "\x00d7-times",
                "\x00d8-Oslash",
                "\x00d9-Ugrave",
                "\x00da-Uacute",
                "\x00db-Ucirc",
                "\x00dc-Uuml",
                "\x00dd-Yacute",
                "\x00de-THORN",
                "\x00df-szlig",
                "\x00e0-agrave",
                "\x00e1-aacute",
                "\x00e2-acirc",
                "\x00e3-atilde",
                "\x00e4-auml",
                "\x00e5-aring",
                "\x00e6-aelig",
                "\x00e7-ccedil",
                "\x00e8-egrave",
                "\x00e9-eacute",
                "\x00ea-ecirc",
                "\x00eb-euml",
                "\x00ec-igrave",
                "\x00ed-iacute",
                "\x00ee-icirc",
                "\x00ef-iuml",
                "\x00f0-eth",
                "\x00f1-ntilde",
                "\x00f2-ograve",
                "\x00f3-oacute",
                "\x00f4-ocirc",
                "\x00f5-otilde",
                "\x00f6-ouml",
                "\x00f7-divide",
                "\x00f8-oslash",
                "\x00f9-ugrave",
                "\x00fa-uacute",
                "\x00fb-ucirc",
                "\x00fc-uuml",
                "\x00fd-yacute",
                "\x00fe-thorn",
                "\x00ff-yuml",
                "\x0152-OElig",
                "\x0153-oelig",
                "\x0160-Scaron",
                "\x0161-scaron",
                "\x0178-Yuml",
                "\x0192-fnof",
                "\x02c6-circ",
                "\x02dc-tilde",
                "\x0391-Alpha",
                "\x0392-Beta",
                "\x0393-Gamma",
                "\x0394-Delta",
                "\x0395-Epsilon",
                "\x0396-Zeta",
                "\x0397-Eta",
                "\x0398-Theta",
                "\x0399-Iota",
                "\x039a-Kappa",
                "\x039b-Lambda",
                "\x039c-Mu",
                "\x039d-Nu",
                "\x039e-Xi",
                "\x039f-Omicron",
                "\x03a0-Pi",
                "\x03a1-Rho",
                "\x03a3-Sigma",
                "\x03a4-Tau",
                "\x03a5-Upsilon",
                "\x03a6-Phi",
                "\x03a7-Chi",
                "\x03a8-Psi",
                "\x03a9-Omega",
                "\x03b1-alpha",
                "\x03b2-beta",
                "\x03b3-gamma",
                "\x03b4-delta",
                "\x03b5-epsilon",
                "\x03b6-zeta",
                "\x03b7-eta",
                "\x03b8-theta",
                "\x03b9-iota",
                "\x03ba-kappa",
                "\x03bb-lambda",
                "\x03bc-mu",
                "\x03bd-nu",
                "\x03be-xi",
                "\x03bf-omicron",
                "\x03c0-pi",
                "\x03c1-rho",
                "\x03c2-sigmaf",
                "\x03c3-sigma",
                "\x03c4-tau",
                "\x03c5-upsilon",
                "\x03c6-phi",
                "\x03c7-chi",
                "\x03c8-psi",
                "\x03c9-omega",
                "\x03d1-thetasym",
                "\x03d2-upsih",
                "\x03d6-piv",
                "\x2002-ensp",
                "\x2003-emsp",
                "\x2009-thinsp",
                "\x200c-zwnj",
                "\x200d-zwj",
                "\x200e-lrm",
                "\x200f-rlm",
                "\x2013-ndash",
                "\x2014-mdash",
                "\x2018-lsquo",
                "\x2019-rsquo",
                "\x201a-sbquo",
                "\x201c-ldquo",
                "\x201d-rdquo",
                "\x201e-bdquo",
                "\x2020-dagger",
                "\x2021-Dagger",
                "\x2022-bull",
                "\x2026-hellip",
                "\x2030-permil",
                "\x2032-prime",
                "\x2033-Prime",
                "\x2039-lsaquo",
                "\x203a-rsaquo",
                "\x203e-oline",
                "\x2044-frasl",
                "\x20ac-euro",
                "\x2111-image",
                "\x2118-weierp",
                "\x211c-real",
                "\x2122-trade",
                "\x2135-alefsym",
                "\x2190-larr",
                "\x2191-uarr",
                "\x2192-rarr",
                "\x2193-darr",
                "\x2194-harr",
                "\x21b5-crarr",
                "\x21d0-lArr",
                "\x21d1-uArr",
                "\x21d2-rArr",
                "\x21d3-dArr",
                "\x21d4-hArr",
                "\x2200-forall",
                "\x2202-part",
                "\x2203-exist",
                "\x2205-empty",
                "\x2207-nabla",
                "\x2208-isin",
                "\x2209-notin",
                "\x220b-ni",
                "\x220f-prod",
                "\x2211-sum",
                "\x2212-minus",
                "\x2217-lowast",
                "\x221a-radic",
                "\x221d-prop",
                "\x221e-infin",
                "\x2220-ang",
                "\x2227-and",
                "\x2228-or",
                "\x2229-cap",
                "\x222a-cup",
                "\x222b-int",
                "\x2234-there4",
                "\x223c-sim",
                "\x2245-cong",
                "\x2248-asymp",
                "\x2260-ne",
                "\x2261-equiv",
                "\x2264-le",
                "\x2265-ge",
                "\x2282-sub",
                "\x2283-sup",
                "\x2284-nsub",
                "\x2286-sube",
                "\x2287-supe",
                "\x2295-oplus",
                "\x2297-otimes",
                "\x22a5-perp",
                "\x22c5-sdot",
                "\x2308-lceil",
                "\x2309-rceil",
                "\x230a-lfloor",
                "\x230b-rfloor",
                "\x2329-lang",
                "\x232a-rang",
                "\x25ca-loz",
                "\x2660-spades",
                "\x2663-clubs",
                "\x2665-hearts",
                "\x2666-diams",
            };

            private static Dictionary<string, char> _lookupTable = GenerateLookupTable();

            private static Dictionary<string, char> GenerateLookupTable()
            {
                // e[0] is unicode char, e[1] is '-', e[2+] is entity string

                Dictionary<string, char> lookupTable = new Dictionary<string, char>(StringComparer.Ordinal);
                foreach (string e in _entitiesList)
                {
                    lookupTable.Add(e.Substring(2), e[0]);
                }

                return lookupTable;
            }

            public static char Lookup(string entity)
            {
                char theChar;
                _lookupTable.TryGetValue(entity, out theChar);
                return theChar;
            }
        }

        #endregion

        // *** Source: ndp/fx/src/net/system/net/webutility.cs
        private static char[] _htmlEntityEndingChars = new char[] { ';', '&' };
    }
}
