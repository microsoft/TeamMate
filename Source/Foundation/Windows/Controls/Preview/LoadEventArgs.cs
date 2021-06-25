using System;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Controls
{
    /// <summary>
    /// The event arguments for a preview load event.
    /// </summary>
    public class LoadEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadEventArgs"/> class.
        /// </summary>
        public LoadEventArgs()
        {
            this.Success = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadEventArgs"/> class with an error.
        /// </summary>
        /// <param name="error">The error.</param>
        public LoadEventArgs(Exception error)
        {
            this.Error = error;
        }

        /// <summary>
        /// Gets a value indicating whether the load was successfull or not.
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        /// Gets an exception that occurred during loading, if loading failed.
        /// </summary>
        public Exception Error { get; private set; }
    }
}
