using System.Diagnostics;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics
{
    /// <summary>
    /// A helper class to register and unregister the logging of Trace information to a log file.
    /// </summary>
    public class TraceLogFile
    {
        private bool registered;
        private TextWriterTraceListener traceListener;
        private bool originalAutoFlush;
        private string traceFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceLogFile"/> class.
        /// </summary>
        /// <param name="path">The output file path.</param>
        public TraceLogFile(string path)
        {
            Assert.ParamIsNotNullOrEmpty(path, "path");

            this.traceFile = path;
        }

        /// <summary>
        /// Registers the trace log file as a global trace listener. Traced information
        /// will start to be output to the output trace file.
        /// </summary>
        public void Register()
        {
            if (!registered)
            {
                this.traceListener = new TextWriterTraceListener(traceFile);
                this.originalAutoFlush = Trace.AutoFlush;
                Trace.Listeners.Add(this.traceListener);
                Trace.AutoFlush = true;

                registered = true;
            }
        }

        /// <summary>
        /// Unregisters this instance as a global trace listener. Traced information will no longer
        /// be output to the output trace file.
        /// </summary>
        public void Unregister()
        {
            if (registered)
            {
                Trace.Listeners.Remove(this.traceListener);
                Trace.AutoFlush = originalAutoFlush;
                this.traceListener.Close();
                this.traceListener = null;
                this.originalAutoFlush = false;

                registered = false;
            }
        }
    }
}
