using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Internal.Tools.TeamMate.Foundation
{
    /// <summary>
    /// Provides utility methods for building predicates.
    /// </summary>
    public static class PredicateUtilities
    {
        /// <summary>
        /// Builds a predicate that is an OR expression of all of the input predicates.
        /// </summary>
        /// <param name="predicates">The predicates.</param>
        public static Predicate<T> Or<T>(IEnumerable<Predicate<T>> predicates)
        {
            Assert.ParamIsNotNull(predicates, "predicates");

            return (x) => predicates.Any(p => p(x));
        }

        /// <summary>
        /// Builds a predicate that is an AND expression of all of the input predicates.
        /// </summary>
        /// <param name="predicates">The predicates.</param>
        public static Predicate<T> And<T>(IEnumerable<Predicate<T>> predicates)
        {
            Assert.ParamIsNotNull(predicates, "predicates");

            return (x) => predicates.All(p => p(x));
        }
    }
}
