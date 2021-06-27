// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.Tools.TeamMate.Foundation.ComponentModel
{
    /// <summary>
    /// Provides extension methods for types in the ComponentModel namespace.
    /// </summary>
    public static class ComponentModelExtensions
    {
        /// <summary>
        /// Gets the count of items in a collection view.
        /// </summary>
        /// <param name="collectionView">The collection view.</param>
        /// <returns>The number of items in the collection view.</returns>
        public static int GetCount(this ICollectionView collectionView)
        {
            Assert.ParamIsNotNull(collectionView, "collectionView");

            var collection = collectionView.SourceCollection as System.Collections.ICollection;
            if (collection != null)
            {
                // If the source collection is a collection (most likely the case), optimize to get the count directly
                return collection.Count;
            }
            else
            {
                var enumerable = collectionView.SourceCollection;
                if (enumerable != null)
                {
                    return enumerable.OfType<object>().Count();
                }
            }

            return 0;
        }

        /// <summary>
        /// Determines whether the current item is in the first position of the collection.
        /// </summary>
        /// <param name="collectionView">The collection view.</param>
        /// <returns><c>true</c> if the current item is in the first position, otherwise <c>false</c>.</returns>
        public static bool IsCurrentFirst(this ICollectionView collectionView)
        {
            Assert.ParamIsNotNull(collectionView, "collectionView");

            return collectionView.CurrentPosition == 0;
        }

        /// <summary>
        /// Determines whether the current item is in the last position of the collection.
        /// </summary>
        /// <param name="collectionView">The collection view.</param>
        /// <returns><c>true</c> if the current item is in the last position, otherwise <c>false</c>.</returns>
        public static bool IsCurrentLast(this ICollectionView collectionView)
        {
            Assert.ParamIsNotNull(collectionView, "collectionView");

            int count = collectionView.GetCount();
            return count > 0 && collectionView.CurrentPosition == count - 1;
        }
    }
}
