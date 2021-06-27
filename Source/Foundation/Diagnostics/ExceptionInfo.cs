// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Tools.TeamMate.Foundation.Diagnostics
{
    /// <summary>
    /// Encapsulates the captured information for a given exception.
    /// </summary>
    public class ExceptionInfo
    {
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the stack trace.
        /// </summary>
        public string StackTrace { get; set; }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the h result.
        /// </summary>
        public int HResult { get; set; }

        /// <summary>
        /// Gets or sets the full text of the exception (message, stack trace, plus inner exceptions).
        /// </summary>
        public string FullText { get; set; }

        /// <summary>
        /// Gets or sets the inner exception.
        /// </summary>
        public ExceptionInfo InnerException { get; set; }

        /// <summary>
        /// Creates exception information from a given exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>The exception information.</returns>
        public static ExceptionInfo Create(Exception exception)
        {
            Assert.ParamIsNotNull(exception, "exception");

            return Create(exception, true);
        }

        /// <summary>
        /// Creates exception information from a given exception.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="captureFullText">if set to <c>true</c> captures the full text for the exception (used only for the root exception,
        /// but not for inner exceptions).</param>
        /// <returns>
        /// The exception information.
        /// </returns>
        private static ExceptionInfo Create(Exception error, bool captureFullText)
        {
            ExceptionInfo info = new ExceptionInfo();
            info.Type = error.GetType().FullName;
            info.Message = error.Message;
            info.StackTrace = error.StackTrace;
            info.Source = error.Source;
            info.HResult = error.HResult;

            if (captureFullText)
            {
                info.FullText = error.ToString();
            }

            if (error.InnerException != null)
            {
                info.InnerException = Create(error.InnerException, false);
            }

            return info;
        }

        public override string ToString()
        {
            return this.FullText;
        }
    }
}
