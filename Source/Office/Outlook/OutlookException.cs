// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Tools.TeamMate.Office.Outlook
{
    /// <summary>
    /// An exception class that indicates an error condition when Outlook cannot be launched.
    /// </summary>
    public class OutlookException : Exception
    {
        /// <summary>
        /// Initializes a new instance of this exception.
        /// </summary>
        public OutlookException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the this exception class with a specified
        /// error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public OutlookException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the System.Exception class with a specified
        /// error message and a reference to the inner exception that is the cause of
        /// this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, 
        /// or a <c>null</c> reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public OutlookException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
