using Microsoft.Tools.TeamMate.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.IO;
using Microsoft.Tools.TeamMate.Model;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Tools.TeamMate.Services
{
    public class TelemetryService
    {
        private bool isTelemetryEnabled;
        private HockeyAppTelemetryListener hockeyAppTelemetryListener;

        public bool IsTelemetryEnabled
        {
            get { return this.isTelemetryEnabled; }
            set
            {
                if (this.isTelemetryEnabled != value)
                {
                    this.isTelemetryEnabled = value;
                    Telemetry.IsEnabled = value;
                }
            }
        }

        public async Task InitializeAsync()
        {
            this.hockeyAppTelemetryListener = new HockeyAppTelemetryListener(TeamMateApplicationInfo.HockeyAppId);
            await this.hockeyAppTelemetryListener.InitializeAsync();
            Telemetry.AddListener(this.hockeyAppTelemetryListener);

            // TODO: Remove this in the next version of TeamMate, give a stepping-stone version to upgrade
            // the telemetry system
            if (Directory.Exists(TelemetryFolder))
            {
                PathUtilities.TryDelete(TelemetryFolder);
            }
        }

        private static string TelemetryFolder
        {
            get
            {
                return Path.Combine(TeamMateApplicationInfo.DataDirectory, "Telemetry");
            }
        }
    }
}
