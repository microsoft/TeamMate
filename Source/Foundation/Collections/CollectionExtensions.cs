using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using System.Collections.Generic;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Collections
{
    /// <summary>
    /// Defines extension methods for collection types.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Adds the elements in another collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="items">The items to add.</param>
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            Assert.ParamIsNotNull(collection, "collection");
            Assert.ParamIsNotNull(items, "items");

            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements in another collection.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="items">The items to add.</param>
        public static void AddRange(this System.Collections.IList collection, System.Collections.IEnumerable items)
        {
            Assert.ParamIsNotNull(collection, "collection");
            Assert.ParamIsNotNull(items, "items");

            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        /// <summary>
        /// Removes the elements in another collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="items">The items to remove.</param>
        public static void RemoveRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            Assert.ParamIsNotNull(collection, "collection");
            Assert.ParamIsNotNull(items, "items");

            foreach (var item in items)
            {
                collection.Remove(item);
            }
        }

        /// <summary>
        /// Removes the elements in another collection.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="items">The items to remove.</param>
        public static void RemoveRange(this System.Collections.IList collection, System.Collections.IEnumerable items)
        {
            Assert.ParamIsNotNull(collection, "collection");
            Assert.ParamIsNotNull(items, "items");

            foreach (var item in items)
            {
                collection.Remove(item);
            }
        }

        /// <summary>
        /// Attempts to get a value for a given key. If not present, returns the default value.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dict">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <returns>The looked up value, or if not found, the default value for the corresponding type.</returns>
        public static TValue TryGetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        {
            TValue value;
            if (!dict.TryGetValue(key, out value))
            {
                value = default(TValue);
            }

            return value;
        }
    }
}
