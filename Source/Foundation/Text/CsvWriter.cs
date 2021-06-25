using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Text
{
    public class CsvWriter : IDisposable
    {
        private static readonly char[] EscapeCharacters = ",\r\n\"".ToCharArray();

        private TextWriter writer;
        private bool firstRow = true;

        public CsvWriter(string filename)
            : this(File.Create(filename))
        {
        }

        public CsvWriter(Stream stream)
            : this(new StreamWriter(stream, DefaultEncoding))
        {
        }

        public static Encoding DefaultEncoding
        {
            get
            {
                // This encoding plays well with Excel apparently
                return Encoding.GetEncoding("WINDOWS-1252");
            }
        }

        public CsvWriter(TextWriter writer)
        {
            this.writer = writer;
        }

        public void Close()
        {
            writer.Close();
        }

        public void Dispose()
        {
            Close();
        }

        public void WriteRecord(params object[] values)
        {
            DoWriteRecord(values);
        }

        public void WriteRecord(IEnumerable<object> values)
        {
            DoWriteRecord(values);
        }

        private void DoWriteRecord(IEnumerable<object> values)
        {
            if (!firstRow)
            {
                writer.WriteLine();
            }

            bool firstColumn = true;
            foreach (var item in values)
            {
                if (!firstColumn)
                {
                    writer.Write(',');
                }

                string value = (item != null) ? item.ToString() : String.Empty;
                value = Escape(value);
                writer.Write(value);
                firstColumn = false;
            }

            firstRow = false;
        }

        private static string Escape(string value)
        {
            if (value.IndexOfAny(EscapeCharacters) >= 0)
            {
                // Surround with quotes, escape quote with double quote
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }

            return value;
        }
    }
}
