using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Text
{
    public class CsvReader : IDisposable
    {
        private char delimiter = ',';
        private StreamReader reader;

        public CsvReader(string filename)
            : this(File.OpenRead(filename))
        {
        }

        public CsvReader(Stream stream)
            : this(new StreamReader(stream, CsvWriter.DefaultEncoding))
        {
        }

        public CsvReader(StreamReader reader)
        {
            this.reader = reader;
        }

        public string[] ReadRecord()
        {
            string[] record = this.ParseNextRecord();
            if (record.Length > 0 && (record.Length != 1 || !string.IsNullOrEmpty(record[0])))
            {
                return record;
            }

            return null;
        }

        public static IEnumerable<string[]> ReadRecords(string filename)
        {
            using (var reader = new CsvReader(filename))
            {
                string[] record;
                while ((record = reader.ReadRecord()) != null)
                {
                    yield return record;
                }
            }
        }

        private bool EOF
        {
            get
            {
                return this.reader.EndOfStream;
            }
        }

        private char ReadChar()
        {
            int num = this.reader.Read();
            return (char)num;
        }

        private bool PeekNextChar(char c)
        {
            int num = this.reader.Peek();
            return num != -1 && c == (char)num;
        }

        private string ReadLine()
        {
            return this.reader.ReadLine();
        }

        private string[] ParseNextRecord()
        {
            IList<string> record = new List<string>();
            StringBuilder buffer = new StringBuilder();
            bool inQuotedString = false;

            while (!this.EOF)
            {
                char c = this.ReadChar();

                if (c == this.delimiter)
                {
                    if (inQuotedString)
                    {
                        buffer.Append(c);
                    }
                    else
                    {
                        record.Add(GetStringAndClear(buffer));
                    }

                    if (this.EOF)
                    {
                        // Special case, hit end of file, with a comman at the end, indicating the last field was empty
                        record.Add(string.Empty);
                    }
                }
                else if (c == '"')
                {
                    if (inQuotedString)
                    {
                        if (this.PeekNextChar('"'))
                        {
                            this.ReadChar();
                            buffer.Append('"');
                        }
                        else
                        {
                            inQuotedString = false;
                            if (ReadField(buffer, record, eatTrailingBlanks: true))
                            {
                                break;
                            }
                        }
                    }
                    else if (buffer.Length == 0)
                    {
                        inQuotedString = true;
                    }
                    else
                    {
                        buffer.Append(c);
                        if (ReadField(buffer, record, eatTrailingBlanks: false))
                        {
                            break;
                        }
                    }
                }
                else if (c == ' ' || c == '\t')
                {
                    if (inQuotedString)
                    {
                        buffer.Append(c);
                    }
                    else
                    {
                        buffer.Append(c);
                        if (ReadField(buffer, record, eatTrailingBlanks: true))
                        {
                            break;
                        }
                    }
                }
                else if (this.IsNewLine(c))
                {
                    if (c == '\r')
                    {
                        this.ReadChar();
                    }

                    if (!inQuotedString)
                    {
                        record.Add(GetStringAndClear(buffer));
                        break;
                    }

                    buffer.Append(c);

                    if (c == '\r')
                    {
                        buffer.Append('\n');
                    }
                }
                else
                {
                    buffer.Append(c);
                }
            }

            if (buffer.Length != 0)
            {
                record.Add(buffer.ToString());
            }

            return record.ToArray();
        }

        private bool ReadField(StringBuilder buffer, IList<string> record, bool eatTrailingBlanks)
        {
            bool endOfRecord = false;
            this.ReadTillNextDelimiter(buffer, ref endOfRecord, eatTrailingBlanks);
            var text = GetStringAndClear(buffer);
            record.Add(text);
            return endOfRecord;
        }

        private static string GetStringAndClear(StringBuilder buffer)
        {
            string text = buffer.ToString();
            buffer.Remove(0, buffer.Length);
            return text;
        }

        private bool IsNewLine(char ch)
        {
            return (ch == '\n' || (ch == '\r' && this.PeekNextChar('\n')));
        }

        private void ReadTillNextDelimiter(StringBuilder current, ref bool endOfRecord, bool eatTrailingBlanks)
        {
            StringBuilder builder = new StringBuilder();
            bool isNotWhitespace = false;
            while (true)
            {
                if (this.EOF)
                {
                    endOfRecord = true;
                    break;
                }

                char ch = this.ReadChar();
                if (ch == this.delimiter)
                {
                    break;
                }

                if (this.IsNewLine(ch))
                {
                    endOfRecord = true;
                    if (ch == '\r')
                    {
                        this.ReadChar();
                    }

                    break;
                }

                builder.Append(ch);

                if ((ch != ' ') && (ch != '\t'))
                {
                    isNotWhitespace = true;
                }
            }

            if (eatTrailingBlanks && !isNotWhitespace)
            {
                string str = builder.ToString().Trim();
                current.Append(str);
            }
            else
            {
                current.Append(builder);
            }
        }

        public void Close()
        {
            this.reader.Close();
        }

        public void Dispose()
        {
            this.Close();
        }
    }
}