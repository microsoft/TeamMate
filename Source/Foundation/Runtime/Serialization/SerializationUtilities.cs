using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Microsoft.Tools.TeamMate.Foundation.Runtime.Serialization
{
    /// <summary>
    /// Provides serialization utility methods.
    /// </summary>
    public static class SerializationUtilities
    {
        /// <summary>
        /// Performs a deep copy of an object by relying on serialization semantics.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T Clone<T>(T source) where T : class
        {
            Assert.ParamIsNotNull(source, "source");
            Assert.ParamIs(typeof(T).IsSerializable, "source", "The type must be serializable");

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new MemoryStream())
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
