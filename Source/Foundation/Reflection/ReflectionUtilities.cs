using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Microsoft.Tools.TeamMate.Foundation.Reflection
{
    /// <summary>
    /// Provides utility methods for reflection.
    /// </summary>
    public static class ReflectionUtilities
    {
        private static readonly object[] NoArgs = new object[0];
        private const BindingFlags DefaultBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        /// <summary>
        /// Retrieves the description for a given enum value, defined in a DescriptionAttribute on the value.
        /// </summary>
        /// <param name="enumValue">The enum value.</param>
        /// <returns>The description or the enum value as a string if not defined.</returns>
        public static string GetEnumDescription(object enumValue)
        {
            Assert.ParamIsNotNull(enumValue, "enumValue");

            var type = enumValue.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException("The input value was not a valid enum value");
            }

            var memberInfo = type.GetMember(enumValue.ToString()).FirstOrDefault();
            if (memberInfo != null)
            {
                DescriptionAttribute descriptionAttribute = memberInfo.GetAttribute<DescriptionAttribute>();
                if (descriptionAttribute != null)
                {
                    return descriptionAttribute.Description;
                }
            }

            return enumValue.ToString();
        }

        /// <summary>
        /// Sets an instance property value.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="instance">The object instance.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        /// <param name="flags">The optional binding flags to use.</param>
        public static void SetProperty<T>(object instance, string propertyName, T value, BindingFlags flags = DefaultBindingFlags)
        {
            Assert.ParamIsNotNull(instance, "instance");
            Assert.ParamIsNotNull(propertyName, "propertyName");

            try
            {
                instance.GetType().InvokeMember(propertyName, flags | BindingFlags.SetProperty, null, instance, new object[] { value }, CultureInfo.InvariantCulture);
            }
            catch (TargetInvocationException e)
            {
                throw HandledReflectionException(e);
            }
        }

        /// <summary>
        /// Gets a property value.
        /// </summary>
        /// <typeparam name="T">The expected property type.</typeparam>
        /// <param name="instance">An instance of an object.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="flags">The binding flags for the invocation.</param>
        /// <returns>The property value.</returns>
        public static T GetProperty<T>(object instance, string propertyName, BindingFlags flags = DefaultBindingFlags)
        {
            return GetProperty<T>(instance, propertyName, flags, NoArgs);
        }

        /// <summary>
        /// Gets a property value.
        /// </summary>
        /// <typeparam name="T">The expected property type.</typeparam>
        /// <param name="instance">An instance of an object.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="args">An optional set of arguments for the property accessor.</param>
        /// <returns>The property value.</returns>
        public static T GetProperty<T>(object instance, string propertyName, params object[] args)
        {
            return GetProperty<T>(instance, propertyName, DefaultBindingFlags, args);
        }

        /// <summary>
        /// Gets a property value.
        /// </summary>
        /// <typeparam name="T">The expected property type.</typeparam>
        /// <param name="instance">An instance of an object.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="flags">The binding flags for the invocation.</param>
        /// <param name="args">An optional set of arguments for the property accessor.</param>
        /// <returns>The property value.</returns>
        public static T GetProperty<T>(object instance, string propertyName, BindingFlags flags, params object[] args)
        {
            Assert.ParamIsNotNull(instance, "instance");
            Assert.ParamIsNotNull(propertyName, "propertyName");

            try
            {
                object rawValue = instance.GetType().InvokeMember(propertyName, flags | BindingFlags.Instance | BindingFlags.GetProperty, null, instance, args, CultureInfo.InvariantCulture);
                return (T)rawValue;
            }
            catch (TargetInvocationException e)
            {
                throw HandledReflectionException(e);
            }
        }

        /// <summary>
        /// Invokes a method on an object.
        /// </summary>
        /// <param name="instance">An instance of an object.</param>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <param name="args">An optional set of arguments for the property accessor.</param>
        /// <returns>The method return value, or <c>null</c> if no return value was expected.</returns>
        public static object InvokeMethod(object instance, string methodName, params object[] args)
        {
            return InvokeMethod(instance, methodName, DefaultBindingFlags, args);
        }

        /// <summary>
        /// Invokes a method on an object.
        /// </summary>
        /// <param name="instance">An instance of an object.</param>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <param name="flags">The binding flags for the invocation.</param>
        /// <param name="args">An optional set of arguments for the property accessor.</param>
        /// <returns>The method return value, or <c>null</c> if no return value was expected.</returns>
        public static object InvokeMethod(object instance, string methodName, BindingFlags flags, params object[] args)
        {
            Assert.ParamIsNotNull(instance, "instance");
            Assert.ParamIsNotNull(methodName, "methodName");

            try
            {
                return instance.GetType().InvokeMember(methodName, flags | BindingFlags.Instance | BindingFlags.InvokeMethod, null, instance, args, CultureInfo.InvariantCulture);
            }
            catch (TargetInvocationException e)
            {
                throw HandledReflectionException(e);
            }
        }

        /// <summary>
        /// Handles a target invocation exception, and unwraps it to get the core exception.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private static Exception HandledReflectionException(TargetInvocationException e)
        {
            // Be safe in case the inner exception is null. Typically it shouldn't be.
            Exception rootException = (e.InnerException != null) ? e.InnerException : null;

            // At this point, the inner exception has its full, original stack trace... Let's trace it
            // to at least keep track of it.
            Log.Warn(rootException);

            // Return the inner exception, which will be rethrown by the caller (and then we will lose the original stack
            // trace). This is fine, the goal here is to unwrap the exception to make callers easier to write (they don't have
            // to catch/unwrap/test the TargetInvocationException themselves)
            return rootException;
        }
    }
}
