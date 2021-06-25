using Microsoft.Internal.Tools.TeamMate.TeamFoundation.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Threading.Tasks;

namespace Microsoft.Internal.Tools.TeamMate.Sandbox
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                PlayWithVstsAsync().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static async Task PlayWithVstsAsync()
        {
            VssConnection connection = new VssConnection(new Uri("https://garage-02.visualstudio.com"), new VssClientCredentials());
            await connection.ConnectAsync();

            var locationClient = connection.GetClient<ResourceLocationHttpClient>();

            var location = await locationClient.GetResourceLocationAsync(WitConstants.WorkItemTrackingLocationIds.QueriesByProjectAndQueryReference);
            Console.WriteLine(location);
        }
    }
}