using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Internal.Tools.TeamMate.TeamFoundation.WebApi
{
    public static class PagingUtilities
    {
        public static async Task<ICollection<T>> PageAllAsync<T>(GetPageAsync<T> getPageAsync, int pageSize,
    CancellationToken cancellationToken = default(CancellationToken))
        {
            List<T> allResults = new List<T>();
            int? skip = null;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                IEnumerable<T> batch = await getPageAsync(pageSize, skip);

                int previousResultCount = allResults.Count;
                allResults.AddRange(batch);

                int resultsInBatch = allResults.Count - previousResultCount;
                if (resultsInBatch < pageSize)
                {
                    // We are done, no results where returned
                    break;
                }

                // Initialize amount to skip, will be null on the first request
                skip = (skip == null) ? resultsInBatch : skip.Value + resultsInBatch;
            }

            return allResults;
        }
    }

    public delegate Task<IEnumerable<T>> GetPageAsync<T>(int? top, int? skip);
}
