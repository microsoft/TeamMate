using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Utilities;

namespace Microsoft.Tools.TeamMate.Services
{
    public class TracingService
    {
        private TraceLogFile traceLogFile;
        private bool isTracingEnabled;

        public void Initialize()
        {
            FileUtilities.CleanStaleFiles(TeamMateApplicationInfo.LogsFolder, 5);
        }

        public bool IsTracingEnabled
        {
            get { return this.isTracingEnabled; }

            set
            {
                if (this.isTracingEnabled != value)
                {
                    this.isTracingEnabled = value;
                    InvalidateTracing();
                }
            }
        }


        private void InvalidateTracing()
        {
            if (this.isTracingEnabled)
            {
                if (this.traceLogFile == null)
                {
                    string logPath = FileUtilities.GetTimeBasedFilePath(TeamMateApplicationInfo.LogsFolder, ".log");
                    this.traceLogFile = new TraceLogFile(logPath);
                    this.traceLogFile.Register();
                }
            }
            else
            {
                if (this.traceLogFile != null)
                {
                    this.traceLogFile.Unregister();
                    this.traceLogFile = null;
                }
            }
        }
    }
}
