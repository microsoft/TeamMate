using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Microsoft.Internal.Tools.TeamMate.Platform.CodeBox
{
    /// <summary>
    /// Provides utility methods for dealing with DataSet objects returned from CodeBox web services.
    /// </summary>
    internal static class DataSetUtilities
    {
        /// <summary>
        /// Converts a data set to a collection of objects.
        /// </summary>
        /// <typeparam name="T">The type of objects that will be created from the dataset rows.</typeparam>
        /// <param name="dataSet">The data set.</param>
        /// <param name="create">The factory method used to convert rows to objects.</param>
        /// <returns>A collection of all the objects in the data set table.</returns>
        /// <remarks>
        /// This method assumes the data set contains a single table, and that the factory method converts
        /// rows from that table into a resulting object.
        /// </remarks>
        public static ICollection<T> ToCollection<T>(DataSet dataSet, Func<DataRow, T> create)
        {
            if (dataSet.Tables.Count > 0)
            {
                return dataSet.Tables[0].Rows.OfType<DataRow>().Select(row => create(row)).ToList();
            }

            return new T[0];
        }
    }
}
