using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Location;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Internal.Tools.TeamMate.TeamFoundation.WebApi
{
    public enum ServerVersion
    {
        PreTfs2015,
        Tfs2015,
        Vsts
    }

    public static class ServerVersionUtilities
    {
        private static readonly Guid WiqlId = new Guid("1a9c53f7-f243-4447-b110-35ef023636e4");
        private static readonly Guid TemplatesId = new Guid("6a90345f-a676-4969-afce-8e163e1d5642");

        public static async Task<ServerVersion> GetVersionAsync(VssConnection connection, CancellationToken cancellationToken = default(CancellationToken))
        {
            var locationService = await connection.GetServiceAsync<ILocationService>(cancellationToken);
            var locationData = await locationService.GetLocationDataAsync(Guid.Empty);
            var resourceLocations = await locationData.GetResourceLocationsAsync(cancellationToken);

            // This is a very naive way of trying to figure out the TFS version

            if (resourceLocations.TryGetLocationById(WiqlId) == null)
            {
                return ServerVersion.PreTfs2015;
            }

            if (resourceLocations.TryGetLocationById(TemplatesId) == null)
            {
                return ServerVersion.Tfs2015;
            }

            return ServerVersion.Vsts;
        }
    }
}
