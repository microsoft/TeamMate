using System;
using System.Collections.Generic;

namespace Microsoft.Internal.Tools.TeamMate.Foundation
{
    /// <summary>
    /// A comparer that compares absolute URIs.
    /// </summary>
    public class AbsoluteUriComparer : IEqualityComparer<Uri>
    {
        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type <paramref name="T" /> to compare.</param>
        /// <param name="y">The second object of type <paramref name="T" /> to compare.</param>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        public bool Equals(Uri x, Uri y)
        {
            return String.Equals(Normalize(x), Normalize(y), StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public int GetHashCode(Uri obj)
        {
            return Normalize(obj).GetHashCode();
        }

        /// <summary>
        /// Normalizes the specified URI to a comparable value.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>The normalized URI string.</returns>
        private string Normalize(Uri uri)
        {
            // TODO: Match this more closely to TFS AbsoluteURIComparer. E.g. do we need to trim slashes et al?
            return uri.AbsoluteUri.ToLowerInvariant();
        }
    }
}
