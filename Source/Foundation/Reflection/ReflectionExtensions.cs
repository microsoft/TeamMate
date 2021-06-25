using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Reflection
{
    /// <summary>
    /// Provides extension methods for the Reflection namespace.
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Loads the specified manifest resource, scoped by the namespace of the specified
        /// type, from its assembly. Validates that the resource indeed exists.
        /// </summary>
        /// <param name="type">The type whose namespace is used to scope the manifest resource name.</param>
        /// <param name="name">The case-sensitive name of the manifest resource being requested.</param>
        /// <returns>A stream representing the manifest resource.</returns>
        /// <exception cref="InvalidOperationException">The resource was not found in the assembly.</exception>
        public static Stream GetRequiredManifestResourceStream(this Type type, string name)
        {
            Assert.ParamIsNotNull(type, "type");
            Assert.ParamIsNotNullOrEmpty(name, "name");

            Stream stream = type.Assembly.GetManifestResourceStream(type, name);
            if (stream == null)
            {
                string fullResourceName = String.Format("{0}.{1}", type.Namespace, name);
                throw new InvalidOperationException("Couldn't find expected embedded resource " + fullResourceName
                    + " in assembly " + type.Assembly.FullName);
            }

            return stream;
        }

        /// <summary>
        /// Loads the specified manifest resource, validating that the resource indeed exists.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="name">The resource name.</param>
        /// <returns>A stream representing the manifest resource.</returns>
        /// <exception cref="InvalidOperationException">The resource was not found in the assembly.</exception>
        public static Stream GetRequiredManifestResourceStream(this Assembly assembly, string name)
        {
            Assert.ParamIsNotNull(assembly, "assembly");
            Assert.ParamIsNotNullOrEmpty(name, "name");

            Stream stream = assembly.GetManifestResourceStream(name);
            if (stream == null)
            {
                string fullResourceName = name;
                throw new InvalidOperationException("Couldn't find expected embedded resource " + fullResourceName
                    + " in assembly " + assembly.FullName);
            }

            return stream;
        }

        /// <summary>
        /// Checks to see if a type is a nullable type (e.g. Nullable{T}).
        /// </summary>
        /// <param name="type">A type.</param>
        /// <returns><c>true</c> if the type was nullable, otherwise <c>false</c>.</returns>
        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// Tries to get the Nullable{T} subtype (generic argument type) for a given type.
        /// </summary>
        /// <param name="type">A type.</param>
        /// <returns>The nullable subtype if the type was indeed a Nullable{T}. Otherwise, <c>false</c>.</returns>
        public static Type TryGetNullableSubType(this Type type)
        {
            Assert.ParamIsNotNull(type, "type");

            if (type.IsNullable())
            {
                Type[] argTypes = type.GetGenericArguments();
                Debug.Assert(argTypes.Length == 1, "Expected Nullable type to have a single generic argument");
                if (argTypes.Length == 1)
                {
                    return argTypes[0];
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the file version for a given assembly, by looking up the file version attribute.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>The file version, or <c>null</c> if not found.</returns>
        public static Version GetFileVersion(this Assembly assembly)
        {
            Assert.ParamIsNotNull(assembly, "assembly");

            AssemblyFileVersionAttribute attr = assembly.GetAttribute<AssemblyFileVersionAttribute>();
            return (attr != null) ? new Version(attr.Version) : null;
        }

        /// <summary>
        /// Gets the version for a given assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>The assembly version.</returns>
        public static Version GetVersion(this Assembly assembly)
        {
            Assert.ParamIsNotNull(assembly, "assembly");

            return new AssemblyName(assembly.FullName).Version;
        }

        /// <summary>
        /// Gets a custom attribute defined in a type.
        /// </summary>
        /// <param name="type">The attribute type.</param>
        /// <param name="inherit">if set to <c>true</c>, looks for attributes defined in base types.</param>
        /// <returns>The first instance of the defined attribute, or <c>null</c> if not found.</returns>
        /// <returns></returns>
        public static T GetAttribute<T>(this Type type, bool inherit = false) where T : Attribute
        {
            Assert.ParamIsNotNull(type, "type");

            T[] attributes = type.GetAttributes<T>(inherit);
            return (attributes.Length > 0) ? attributes[0] : null;
        }

        /// <summary>
        /// Gets the custom attributes defined in a type.
        /// </summary>
        /// <param name="type">The attribute type.</param>
        /// <param name="inherit">if set to <c>true</c>, looks for attributes defined in base types.</param>
        /// <returns>An array of the attributes defined in the given type.</returns>
        public static T[] GetAttributes<T>(this Type type, bool inherit = false) where T : Attribute
        {
            Assert.ParamIsNotNull(type, "type");

            return (T[])type.GetCustomAttributes(typeof(T), inherit);
        }

        /// <summary>
        /// Gets a custom attribute defined for a given type member.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="inherit">if set to <c>true</c>, looks for attributes defined in base members.</param>
        /// <returns>The first instance of the defined attribute, or <c>null</c> if not found.</returns>
        public static T GetAttribute<T>(this MemberInfo member, bool inherit = false) where T : Attribute
        {
            Assert.ParamIsNotNull(member, "member");

            T[] attributes = member.GetAttributes<T>(inherit);
            return (attributes.Length > 0) ? attributes[0] : null;
        }

        /// <summary>
        /// Gets the custom attributes defined in a type member.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="inherit">if set to <c>true</c>, looks for attributes defined in base members.</param>
        /// <returns>An array of the attributes defined in the given type member.</returns>
        public static T[] GetAttributes<T>(this MemberInfo member, bool inherit = false) where T : Attribute
        {
            Assert.ParamIsNotNull(member, "member");

            return (T[])member.GetCustomAttributes(typeof(T), inherit);
        }

        /// <summary>
        /// Gets a custom attribute defined for a given assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>The first instance of the defined attribute, or <c>null</c> if not found.</returns>
        public static T GetAttribute<T>(this Assembly assembly) where T : Attribute
        {
            Assert.ParamIsNotNull(assembly, "assembly");

            T[] attributes = assembly.GetAttributes<T>();
            return (attributes.Length > 0) ? attributes[0] : null;
        }

        /// <summary>
        /// Gets the custom attributes defined for a given assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>An array of the attributes defined for the given assembly.</returns>
        public static T[] GetAttributes<T>(this Assembly assembly) where T : Attribute
        {
            Assert.ParamIsNotNull(assembly, "assembly");

            return (T[])assembly.GetCustomAttributes(typeof(T), false);
        }

        /// <summary>
        /// Gets a custom attribute defined for a given member, or its declaring type,
        /// or its assembly container. This mechanism is useful for inheriting attributes
        /// from containers, and overriding as necessary.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>The first instance of the defined attribute, or <c>null</c> if not found.</returns>
        public static T GetInheritedAttribute<T>(this MemberInfo member) where T : Attribute
        {
            Assert.ParamIsNotNull(member, "member");

            T attribute = member.GetAttribute<T>();
            if (attribute == null)
            { 
                attribute = member.DeclaringType.GetInheritedAttribute<T>();
            }

            return attribute;
        }

        /// <summary>
        /// Gets a custom attribute defined for a given type,
        /// or its assembly container. This mechanism is useful for inheriting attributes
        /// from containers, and overriding as necessary.
        /// </summary>
        /// <param name="type">The declaring type.</param>
        /// <returns>The first instance of the defined attribute, or <c>null</c> if not found.</returns>
        public static T GetInheritedAttribute<T>(this Type type) where T : Attribute
        {
            Assert.ParamIsNotNull(type, "type");

            T attribute = type.GetAttribute<T>(true);

            if (attribute == null)
            { 
                attribute = type.Assembly.GetAttribute<T>();
            }

            return attribute;
        }

        /// <summary>
        /// Gets the custom attributes defined for a given member, and its declaring type,
        /// and its assembly container. This will effectively return the union of all the
        /// instances of an attribute type defined.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>An array containing all the attributes of the given type defined for
        /// a member, its declaring type, and its assembly.</returns>
        public static T[] GetAllInheritedAttributes<T>(this MemberInfo member) where T : Attribute
        {
            Assert.ParamIsNotNull(member, "member");

            List<T> result = new List<T>();
            result.AddRange(member.GetAttributes<T>());
            result.AddRange(member.DeclaringType.GetAttributes<T>(true));
            result.AddRange(member.DeclaringType.Assembly.GetAttributes<T>());
            return result.ToArray();
        }

        /// <summary>
        /// Gets the custom attributes defined for a given member, and its declaring type,
        /// and its assembly container. This will effectively return the union of all the
        /// instances of an attribute type defined.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>An array containing all the attributes of the given type defined for
        /// a member, its declaring type, and its assembly.</returns>
        public static T[] GetAllInheritedAttributes<T>(this Type type) where T : Attribute
        {
            Assert.ParamIsNotNull(type, "type");

            List<T> result = new List<T>();
            result.AddRange(type.GetAttributes<T>(true));
            result.AddRange(type.Assembly.GetAttributes<T>());
            return result.ToArray();
        }
    }
}
