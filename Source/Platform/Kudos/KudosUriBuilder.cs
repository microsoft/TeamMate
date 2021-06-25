using System;
using System.Globalization;

namespace Microsoft.Internal.Tools.TeamMate.Platform.Kudos
{
    public static class KudosUriBuilder
    {
        public static Uri SendKudos(string alias)
        {
            return CreateUri("http://kudos/SendKudos.aspx?alias={0}", Uri.EscapeDataString(alias));
        }

        public static Uri ShowKudos(int employeeId)
        {
            return CreateUri("http://kudos/SendKudos.aspx?empId={0}", 
                Uri.EscapeDataString(employeeId.ToString(CultureInfo.InvariantCulture)));
        }

        public static Uri OrgStats(string managerAlias, int days)
        {
            return CreateUri("http://kudos/orgstats.aspx?Manager={0}&days={1}", 
                Uri.EscapeDataString(managerAlias), Uri.EscapeDataString(days.ToString(CultureInfo.InvariantCulture)));
        }

        public static Uri OrgStats(string managerAlias, DateTime startDate, DateTime endDate)
        {
            return CreateUri("http://kudos/orgstats.aspx?Manager={0}&StartDate={1}&EndDate={2}", 
                Uri.EscapeDataString(managerAlias), Uri.EscapeDataString(FormatDate(startDate)), Uri.EscapeDataString(FormatDate(endDate)));
        }

        private static string FormatDate(DateTime startDate)
        {
            return startDate.ToString("m/d/yyyy");
        }

        public static Uri OrgStatsCsv(string managerAlias, DateTime startDate, DateTime endDate)
        {
            return CreateUri("http://kudos/orgStatsCsv.aspx?Manager={0}&StartDate={1}&EndDate={2}",
                Uri.EscapeDataString(managerAlias), Uri.EscapeDataString(FormatDate(startDate)), Uri.EscapeDataString(FormatDate(endDate)));
        }

        private static Uri CreateUri(string format, params object[] args)
        {
            return new Uri(String.Format(format, args), UriKind.Absolute);
        }
    }
}
