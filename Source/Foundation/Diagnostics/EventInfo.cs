using System;

namespace Microsoft.Tools.TeamMate.Foundation.Diagnostics
{
    /// <summary>
    /// Representts telemetry information for a given "event" in an application. An event has a name,
    /// an occurrence timestamp, and an optional set of properties.
    /// </summary>
    public class EventInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventInfo"/> class.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <param name="name">The name.</param>
        /// <param name="properties">The (optional) event properties.</param>
        public EventInfo(DateTime time, string name, TelemetryEventProperties properties)
        {
            Assert.ParamIsNotNull(name, "name");

            this.Time = time;
            this.Name = name;
            this.Properties = properties;
        }

        /// <summary>
        /// Gets the time.
        /// </summary>
        public DateTime Time { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the properties (could be <c>null</c>).
        /// </summary>
        public TelemetryEventProperties Properties { get; private set; }
    }
}
