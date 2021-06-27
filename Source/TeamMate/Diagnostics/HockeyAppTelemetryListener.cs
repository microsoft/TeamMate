using Microsoft.HockeyApp;
using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Tools.TeamMate.Diagnostics
{
    public class HockeyAppTelemetryListener : TelemetryListener
    {
        private string identifier;

        public HockeyAppTelemetryListener(string identifier)
        {
            this.identifier = identifier;
        }

        public async Task InitializeAsync()
        {
            HockeyClient.Current.Configure(identifier);
            await HockeyClient.Current.SendCrashesAsync(sendAutomatically: true);
        }

        public override void Exception(Exception info)
        {
            HockeyClient.Current.TrackException(info);
        }

        public override void Event(EventInfo info)
        {
            var properties = info.Properties;
            if (properties != null && properties.Any())
            {
                HockeyClient.Current.TrackEvent(info.Name, GetProperties(properties), GetMetrics(properties));
            }
            else
            {
                HockeyClient.Current.TrackEvent(info.Name);
            }
        }

        private static IDictionary<string, double> GetMetrics(TelemetryEventProperties bag)
        {
            IDictionary<string, double> dictionary = null;
            var entries = bag.Where(kv => kv.Value is double).ToArray();
            if (entries.Any())
            {
                dictionary = new Dictionary<string, double>();
                foreach (var keyValue in entries)
                {
                    dictionary[keyValue.Key] = (double)keyValue.Value;
                }
            }

            return dictionary;
        }

        private static IDictionary<string, string> GetProperties(TelemetryEventProperties bag)
        {
            IDictionary<string, string> dictionary = null;
            var entries = bag.Where(kv => !(kv.Value is double)).ToArray();
            if (entries.Any())
            {
                dictionary = new Dictionary<string, string>();
                foreach (var keyValue in entries)
                {
                    var value = keyValue.Value;
                    var stringValue = (value is string) ? (string)value : String.Format(CultureInfo.InvariantCulture, "{0}", value);
                    dictionary[keyValue.Key] = stringValue;
                }
            }

            return dictionary;
        }
    }
}
