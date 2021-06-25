
using System;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics
{
    /// <summary>
    /// Implements a basic telemetry listener.
    /// </summary>
    /// <remarks>
    /// Follows the same pattern as the base TraceListener.
    /// </remarks>
    public abstract class TelemetryListener
    {
        /// <summary>
        /// Invoked to log event information.
        /// </summary>
        /// <param name="info">The event information.</param>
        public virtual void Event(EventInfo info)
        {
        }

        /// <summary>
        /// Invoked to log exception information.
        /// </summary>
        /// <param name="info">The exception information.</param>
        public virtual void Exception(Exception ex)
        {
        }
    }
}
