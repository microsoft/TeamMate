using System;
using System.Diagnostics;

namespace Microsoft.Tools.TeamMate.Foundation.Diagnostics
{
    /// <summary>
    /// Provides methods for tracing information.
    /// </summary>
    public static class Log
    {
        static Log()
        {
            Level = LogLevel.Performance;
        }

        /// <summary>
        /// Gets or sets the diagnosis level. Only items at a level matching or above the current
        /// level will be emitted.
        /// </summary>
        public static LogLevel Level { get; set; }

        /// <summary>
        /// Traces the performance of the block enclosed in the using statement.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>A disposable item that can be used in a using() statement to trace its performance.</returns>
        public static IDisposable PerformanceBlock(string message, params object[] args)
        {
            Assert.ParamIsNotNull(message, "message");

            return (Log.AcceptsLevel(LogLevel.Performance)) ? new PerformanceScopeTracer(message, args) : null;
        }

        /// <summary>
        /// Traces a message with a diagnose level of Performance.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public static void Performance(string message, params object[] args)
        {
            Trace(LogLevel.Performance, message, args);
        }

        /// <summary>
        /// Traces a message with a diagnose level of Error.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public static void Error(string message, params object[] args)
        {
            Trace(LogLevel.Error, message, args);
        }

        /// <summary>
        /// Traces a message with a diagnose level of Information.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public static void Info(string message, params object[] args)
        {
            Trace(LogLevel.Info, message, args);
        }

        /// <summary>
        /// Traces a message with a diagnose level of Warning.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public static void Warn(string message, params object[] args)
        {
            Trace(LogLevel.Warn, message, args);
        }

        /// <summary>
        /// Traces a message with a diagnose level of error.
        /// </summary>
        /// <param name="exception">An exception.</param>
        public static void Error(Exception exception)
        {
            Trace(LogLevel.Error, exception.ToString());
        }

        /// <summary>
        /// Traces a message with a diagnose level of Error.
        /// </summary>
        /// <param name="exception">An exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public static void Error(Exception exception, string message, params object[] args)
        {
            string formattedMessage = FormatMessage(message, args, exception);
            Trace(LogLevel.Error, formattedMessage);
        }

        /// <summary>
        /// Traces a message with a diagnose level of Warning.
        /// </summary>
        /// <param name="exception">An exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public static void Warn(Exception exception, string message, params object[] args)
        {
            string formattedMessage = FormatMessage(message, args, exception);
            Trace(LogLevel.Warn, formattedMessage);
        }

        /// <summary>
        /// Traces a message with a diagnose level of Warning.
        /// </summary>
        /// <param name="exception">An exception.</param>
        public static void Warn(Exception exception)
        {
            Trace(LogLevel.Warn, exception.ToString());
        }

        /// <summary>
        /// Traces a message with a diagnose level of Error, and raises an assertion (when in debug mode) to
        /// potentially start the debugger.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public static void ErrorAndBreak(string message, params object[] args)
        {
            ErrorAndBreak(null, message, args);
        }

        /// <summary>
        /// Traces a message with a diagnose level of Error, and raises an assertion (when in debug mode) to
        /// potentially start the debugger.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public static void ErrorAndBreak(Exception exception)
        {
            ErrorAndBreak(exception, null);
        }

        /// <summary>
        /// Traces a message with a diagnose level of Error, and raises an assertion (when in debug mode) to
        /// potentially start the debugger.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public static void ErrorAndBreak(Exception exception, string message, params object[] args)
        {
            TraceAndBreak(LogLevel.Error, exception, message, args);
        }

        /// <summary>
        /// Traces a message with a diagnose level of Warning, and raises an assertion (when in debug mode) to
        /// potentially start the debugger.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public static void WarnAndBreak(string message, params object[] args)
        {
            WarnAndBreak(null, message, args);
        }

        /// <summary>
        /// Traces a message with a diagnose level of Warning, and raises an assertion (when in debug mode) to
        /// potentially start the debugger.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public static void WarnAndBreak(Exception exception)
        {
            WarnAndBreak(exception, null);
        }

        /// <summary>
        /// Traces a message with a diagnose level of Warning, and raises an assertion (when in debug mode) to
        /// potentially start the debugger.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public static void WarnAndBreak(Exception exception, string message, params object[] args)
        {
            TraceAndBreak(LogLevel.Warn, exception, message, args);
        }

        /// <summary>
        /// Traces a message with the specified diagnose level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        private static void Trace(LogLevel level, string message, object[] args = null)
        {
            if (AcceptsLevel(level))
            {
                string formattedMessage = FormatMessage(message, args);
                string fullMessage = String.Format("{0}: {1}", GetHeader(level), formattedMessage);
                System.Diagnostics.Trace.WriteLine(fullMessage);

                /*
                switch (level)
                {
                    case DiagnoseLevel.Warning:
                        System.Diagnostics.Trace.TraceWarning(fullMessage);
                        break;

                    case DiagnoseLevel.Error:
                        System.Diagnostics.Trace.TraceError(fullMessage);
                        break;

                    case DiagnoseLevel.Information:
                    default:
                        System.Diagnostics.Trace.TraceInformation(fullMessage);
                        break;
                }
                 */
            }
        }

        /// <summary>
        /// Checks if a message with a given level should be outputted.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns><c>true</c> if it should be outputted, otherwise <c>false</c></returns>
        private static bool AcceptsLevel(LogLevel level)
        {
            return level <= Log.Level;
        }

        /// <summary>
        /// Formats a message, optionally appending an exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="exception">The exception.</param>
        /// <returns>The formatted message.</returns>
        private static string FormatMessage(string message, object[] args, Exception exception = null)
        {
            string formattedMessage = (args != null && args.Length > 0) ? String.Format(message, args) : message;
            if (exception != null)
            {
                formattedMessage += Environment.NewLine + exception.ToString();
            }
            return formattedMessage;
        }

        /// <summary>
        /// Traces a message with a given diagnose level, and raises an assertion (when in debug mode) to
        /// potentially start the debugger.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        private static void TraceAndBreak(LogLevel level, Exception exception, string message, object[] args = null)
        {
            if (message == null && exception != null)
            {
                message = "Unexpected exception thrown";
            }

            string formattedMessage = FormatMessage(message, args, exception);
            Trace(level, formattedMessage);
            Debug.Fail(formattedMessage);
        }

        /// <summary>
        /// Gets the message header for a given diagnose level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>The header text.</returns>
        private static string GetHeader(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Info: return "INFORMATION";
                case LogLevel.Warn: return "WARNING";
                case LogLevel.Error: return "ERROR";
                case LogLevel.Performance: return "PERFORMANCE";
                case LogLevel.Debug : return "DEBUG";

                default:
                    Debug.Fail("Need to update GetHeader() to handle new DiagnoseLevel: " + level);
                    return level.ToString().ToUpper();
            }
        }

        /// <summary>
        /// A helper disposable class to start measure the performance in a using statement, and trace
        /// its result when the item is disposed.
        /// </summary>
        private class PerformanceScopeTracer : IDisposable
        {
            private DateTime startTime;
            private string formattedMessage;
            private bool disposed;

            /// <summary>
            /// Initializes a new instance of the <see cref="PerformanceScopeTracer"/> class.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="args">The arguments.</param>
            public PerformanceScopeTracer(string message, object[] args)
            {
                this.formattedMessage = Log.FormatMessage(message, args);
                this.startTime = DateTime.Now;

                if (Log.AcceptsLevel(LogLevel.Debug))
                {
                    Log.Performance(formattedMessage);
                }
            }

            /// <summary>
            /// Traces the elapsed time of the block.
            /// </summary>
            public void Dispose()
            {
                if(!disposed)
                {
                    disposed = true;
                    TimeSpan elapsed = DateTime.Now - startTime;
                    string message = String.Format("END {0}. ELAPSED TIME: {1}", this.formattedMessage, elapsed);
                    Log.Performance(message);
                }
            }
        }
    }

    /// <summary>
    /// An output level for diagnosis messages.
    /// </summary>
    public enum LogLevel
    {
        // Update GetHeader() whenever you add more levels here...
        None,
        Error,
        Warn,
        Info,
        Performance,
        Debug,
    }
}
