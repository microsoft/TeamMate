using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Collections
{
    /// <summary>
    /// Provides utility methods for collections.
    /// </summary>
    public static class CollectionUtilities
    {
        /// <summary>
        /// Adds an item to a list in MRU fashion, keeping the number of items in the list under
        /// a maximum limit.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="item">The item to add.</param>
        /// <param name="maxItems">The maximum items to keep in the list.</param>
        /// <returns><c>true</c> if changes to the list were made, otherwise <c>false</c>.</returns>
        public static bool AddToMruList<T>(this IList<T> list, T item, int maxItems)
        {
            Assert.ParamIsNotNull(list, "list");
            Assert.ParamIsNotNull(item, "item");
            Assert.ParamIsNotNegative(maxItems, "maxItems");

            int originalIndex = list.IndexOf(item);
            if (originalIndex == 0)
            {
                // Item is already at the front, no changes required
            }
            else if (originalIndex == -1)
            {
                // Item is not in the list, add it to the front and trim
                list.Insert(0, item);
                while (list.Count > maxItems)
                {
                    list.RemoveAt(list.Count - 1);
                }
            }
            else
            {
                // Item is in the list, it just needs to be moved to the front
                ObservableCollection<T> observable = list as ObservableCollection<T>;
                if (observable != null)
                {
                    // Optimization for observable collection, to avoid firing multiple events.
                    observable.Move(originalIndex, 0);
                }
                else
                {
                    list.RemoveAt(originalIndex);
                    list.Insert(0, item);
                }
            }

            bool neededModifications = (originalIndex != 0);
            return neededModifications;
        }
    }
}
