using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Reflection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.Tools.TeamMate.Foundation.Xml
{
    /// <summary>
    /// Provides extension methods for manipulating XML using DOM.
    /// </summary>
    public static class XmlExtensions
    {
        /// <summary>
        /// Gets the root element for a given document, with the expecation that it matches a given name.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="expectedName">The expected name.</param>
        /// <exception cref="System.Xml.XmlException">
        /// If the root element did not exist or match the expected name
        /// </exception>
        public static XElement GetExpectedRoot(this XDocument document, XName expectedName)
        {
            Assert.ParamIsNotNull(document, "document");
            Assert.ParamIsNotNull(expectedName, "expectedName");

            var root = document.Root;
            if (root == null)
            {
                throw new XmlException("Document did not have a root element");
            }

            if (!root.Name.Equals(expectedName))
            {
                throw new XmlException("Unexpected root element " + root.Name);
            }

            return root;
        }

        /// <summary>
        /// Gets the element that matches the given path of names.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="namePath">A path of one or more element names.</param>
        /// <returns>
        /// The matching element, or <c>null</c> if not found.
        /// </returns>
        /// <exception cref="System.ArgumentException">names cannot be empty;names</exception>
        public static XElement Element(this XContainer parent, params XName[] namePath)
        {
            Assert.ParamIsNotNull(parent, "element");

            if (namePath.Length == 0)
            {
                throw new ArgumentException("names cannot be empty", "names");
            }

            XContainer next = parent;
            for (int i = 0; i < namePath.Length && next != null; i++)
            {
                next = next.Element(namePath[i]);
            }

            return next as XElement;
        }

        /// <summary>
        /// Gets all of the elements that match the given path of names.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="namePath">A path of one or more element names.</param>
        /// <returns>The matching elements, or an emtpy collection if no matches were found.</returns>
        public static IEnumerable<XElement> Elements(this XContainer parent, params XName[] namePath)
        {
            Assert.ParamIsNotNull(parent, "container");

            if (namePath.Length == 0)
            {
                throw new ArgumentException("names cannot be empty", "names");
            }

            IEnumerable<XContainer> current = new XContainer[] { parent };
            IEnumerable<XElement> result = null;

            for (int i = 0; i < namePath.Length; i++)
            {
                IEnumerable<XElement> next = null;
                foreach (var e in current)
                {
                    if (next == null)
                    {
                        next = e.Elements(namePath[i]);
                    }
                    else
                    {
                        next = next.Union(e.Elements(namePath[i]));
                    }
                }

                result = next;
                current = next;
            }

            return (result != null) ? result.ToArray() : new XElement[0];
        }

        /// <summary>
        /// Gets the value of an attribute coerced to a given type.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="element">An XML element.</param>
        /// <param name="attributeName">An attribute name.</param>
        /// <param name="defaultValue">A default value used if the attribute is not found.</param>
        /// <returns>The converted attribute value, or the default value if the attribute was not found.</returns>
        public static T GetAttribute<T>(this XmlElement element, string attributeName, T defaultValue = default(T))
        {
            // TryGetAttribute validates input parameters

            T value;
            if (!TryGetAttribute<T>(element, attributeName, out value))
            {
                value = defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Gets the value of an attribute coerced to a given type.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="element">An XML element.</param>
        /// <param name="attributeName">An attribute name.</param>
        /// <param name="defaultValue">A default value used if the attribute is not found.</param>
        /// <returns>The converted attribute value, or the default value if the attribute was not found.</returns>
        public static T GetAttribute<T>(this XElement element, string attributeName, T defaultValue = default(T))
        {
            // TryGetAttribute validates input parameters

            T value;
            if (!TryGetAttribute<T>(element, attributeName, out value))
            {
                value = defaultValue;
            }

            return value;
        }

        public static T GetRequiredAttribute<T>(this XElement element, string attributeName)
        {
            // TryGetAttribute validates input parameters

            T value;
            if (!TryGetAttribute<T>(element, attributeName, out value))
            {
                throw new XmlException("Expected a value for attribute " + attributeName + " in element " + element.Name);
            }

            return value;
        }

        /// <summary>
        /// Gets the value of an attribute coerced to a given type.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="element">An XML element.</param>
        /// <param name="attributeName">An attribute name.</param>
        /// <returns>The converted attribute value, or the default value of the type if the attribute was not found.</returns>
        public static bool TryGetAttribute<T>(this XmlElement element, string attributeName, out T value)
        {
            Assert.ParamIsNotNull(element, "element");
            Assert.ParamIsNotNullOrEmpty(attributeName, "attributeName");

            if (element.HasAttribute(attributeName))
            {
                string attributeValue = element.GetAttribute(attributeName);
                value = FromXmlString<T>(attributeValue);
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }


        /// <summary>
        /// Gets the value of an attribute coerced to a given type.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="element">An XML element.</param>
        /// <param name="attributeName">An attribute name.</param>
        /// <returns>The converted attribute value, or the default value of the type if the attribute was not found.</returns>
        public static bool TryGetAttribute<T>(this XElement element, string attributeName, out T value)
        {
            Assert.ParamIsNotNull(element, "element");
            Assert.ParamIsNotNullOrEmpty(attributeName, "attributeName");

            XAttribute attr = element.Attributes().FirstOrDefault(a => a.Name == attributeName);

            if (attr != null)
            {
                value = FromXmlString<T>(attr.Value);
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        /// <summary>
        /// Sets an attribute value for an XML element.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="element">An XML element.</param>
        /// <param name="attributeName">The XML attribute name.</param>
        /// <param name="value">The value to set.</param>
        /// <remarks>
        /// If the value was null, the attribute will be removed if present.
        /// </remarks>
        public static void SetAttribute<T>(this XmlElement element, string attributeName, T value)
        {
            Assert.ParamIsNotNull(element, "element");
            Assert.ParamIsNotNullOrEmpty(attributeName, "attributeName");

            string stringValue = (value != null) ? ToXmlString(value) : null;
            if (stringValue != null)
            {
                element.SetAttribute(attributeName, stringValue);
            }
            else
            {
                element.RemoveAttribute(attributeName);
            }
        }

        /// <summary>
        /// Sets an attribute value for an XML element.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="element">An XML element.</param>
        /// <param name="attributeName">The XML attribute name.</param>
        /// <param name="value">The value to set.</param>
        /// <remarks>
        /// If the value was null, the attribute will be removed if present.
        /// </remarks>
        public static void SetAttribute<T>(this XElement element, string attributeName, T value)
        {
            Assert.ParamIsNotNull(element, "element");
            Assert.ParamIsNotNullOrEmpty(attributeName, "attributeName");

            string stringValue = (value != null) ? ToXmlString(value) : null;
            if (stringValue != null)
            {
                element.SetAttributeValue(attributeName, stringValue);
            }
            else
            {
                XAttribute attribute = element.Attribute(attributeName);
                if (attribute != null)
                {
                    attribute.Remove();
                }
            }
        }

        /// <summary>
        /// Sets the content of a child XML element.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="parent">The parent XML element.</param>
        /// <param name="childName">The name of the child element.</param>
        /// <param name="value"><The value to set./param>
        /// <remarks>
        /// If the value was null, any existing child elements with a matching name would be removed.
        /// </remarks>
        public static void SetElementValue<T>(this XElement parent, XName childName, T value)
        {
            Assert.ParamIsNotNull(parent, "parent");
            Assert.ParamIsNotNull(childName, "childName");

            XElement childElement = parent.Element(childName);

            string stringValue = (value != null) ? ToXmlString(value) : null;
            if (stringValue != null)
            {
                if (childElement == null)
                {
                    childElement = new XElement(childName);
                    parent.Add(childElement);
                }

                childElement.ReplaceNodes(stringValue);
            }
            else
            {
                if (childElement != null)
                {
                    childElement.Remove();
                }
            }
        }

        /// <summary>
        /// Sets a child element value using a CData element. If the value is null, an existing entry
        /// is removed.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="childName">Name of the child.</param>
        /// <param name="value">The value.</param>
        public static void SetCDataElementValue(this XElement parent, XName childName, string value)
        {
            Assert.ParamIsNotNull(parent, "parent");
            Assert.ParamIsNotNull(childName, "childName");

            XElement childElement = parent.Element(childName);
            if (value != null)
            {
                XCData cdata = new XCData(value);

                if (childElement == null)
                {
                    childElement = new XElement(childName);
                    parent.Add(childElement);
                }

                childElement.ReplaceNodes(cdata);
            }
            else
            {
                if (childElement != null)
                {
                    childElement.Remove();
                }
            }
        }

        /// <summary>
        /// Sets a child element value. If the value is null, the existing child element is removed.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="childName">Name of the child.</param>
        /// <param name="value">The value.</param>
        public static void SetElementChild(this XElement parent, XName childName, XElement value)
        {
            Assert.ParamIsNotNull(parent, "parent");
            Assert.ParamIsNotNull(childName, "childName");

            XElement childElement = parent.Element(childName);

            if (value != null)
            {
                if (childElement == null)
                {
                    childElement = new XElement(childName);
                    parent.Add(childElement);
                }
                else
                {
                    childElement.RemoveAll();
                }

                childElement.Add(value);
            }
            else
            {
                if (childElement != null)
                {
                    childElement.Remove();
                }
            }
        }

        /// <summary>
        /// Gets the content of a child element.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="parent">The parent XML element.</param>
        /// <param name="childName">The name of the child element.</param>
        /// <param name="defaultValue">A default value used if the element is not found.</param>
        /// <returns>The converted attribute value, or the provided default value if the element was not found.</returns>
        public static T GetElementValue<T>(this XElement parent, XName childName, T defaultValue = default(T))
        {
            Assert.ParamIsNotNull(parent, "parent");
            Assert.ParamIsNotNull(childName, "childName");

            T value;
            return (parent.TryGetElementValue<T>(childName, out value)) ? value : defaultValue;
        }

        /// <summary>
        /// Attempts to the get element value.
        /// </summary>
        /// <typeparam name="T">The expected value type</typeparam>
        /// <param name="parent">The parent.</param>
        /// <param name="childName">Name of the child.</param>
        /// <param name="output">The output.</param>
        /// <returns><c>true</c> if the child existed and the value was parsed, otherwise <c>false</c>.</returns>
        public static bool TryGetElementValue<T>(this XElement parent, XName childName, out T output)
        {
            Assert.ParamIsNotNull(parent, "parent");
            Assert.ParamIsNotNull(childName, "childName");

            XElement child = parent.Element(childName);
            if (child != null)
            {
                output = child.GetValue<T>();
                return true;
            }
            else
            {
                output = default(T);
                return false;
            }
        }

        /// <summary>
        /// Reads an element value, invoking a delegate if a value is available.
        /// </summary>
        /// <typeparam name="T">The expected value type</typeparam>
        /// <param name="element">The element.</param>
        /// <param name="childName">Name of the child.</param>
        /// <param name="valueRead">A delegate that will be invoked if a value was available and read.</param>
        public static void ReadElementValue<T>(this XElement element, XName childName, ValueReadDelegate<T> valueRead)
        {
            T value;
            if (element.TryGetElementValue<T>(childName, out value))
            {
                valueRead(value);
            }
        }

        /// <summary>
        /// Gets the value for a given element.
        /// </summary>
        /// <typeparam name="T">The expected value type</typeparam>
        /// <param name="element">The element.</param>
        public static T GetValue<T>(this XElement element)
        {
            Assert.ParamIsNotNull(element, "element");

            // Get the content of the element...
            string stringValue = (string)element;
            return FromXmlString<T>(stringValue);
        }

        /// <summary>
        /// Gets a value for a child element that is required (expected to exist).
        /// </summary>
        /// <typeparam name="T">The expected value type</typeparam>
        /// <param name="parent">The parent.</param>
        /// <param name="childName">Name of the child.</param>
        /// <returns>The child element value.</returns>
        /// <exception cref="System.Xml.XmlException">
        /// If the child element was not found.
        /// </exception>
        public static T GetRequiredElementValue<T>(this XElement parent, XName childName)
        {
            Assert.ParamIsNotNull(parent, "parent");
            Assert.ParamIsNotNull(childName, "childName");

            XElement child = parent.Element(childName);
            if (child != null)
            {
                return child.GetValue<T>();
            }
            else
            {
                throw new XmlException("Expected a value for a child element with name " + childName + " in element " + parent.Name);
            }
        }


        /// <summary>
        /// Converts an XML string value into a target type.
        /// </summary>
        /// <typeparam name="T">The target value type.</typeparam>
        /// <param name="value">The input XML string value.</param>
        /// <returns>The converted value.</returns>
        public static T FromXmlString<T>(string value)
        {
            Type targetType = typeof(T);
            object result;

            Type nullableType = targetType.TryGetNullableSubType();
            if (nullableType != null)
            {
                // If it is a nullable type and the value is null, return early
                // Otherwise, flip the target type to do the appropriate conversion
                if (value == null)
                {
                    return (T)(object)null;
                }

                targetType = nullableType;
            }

            // A bunch of special cases not handled by XmlConvert come first...
            if (targetType.IsEnum)
            {
                result = Enum.Parse(targetType, value);
            }
            else if (targetType == typeof(TimeSpan))
            {
                result = TimeSpan.Parse(value, CultureInfo.InvariantCulture);
            }
            else if (targetType == typeof(Guid))
            {
                result = XmlConvert.ToGuid(value);
            }
            else if (targetType == typeof(Version))
            {
                result = Version.Parse(value);
            }
            else if (targetType == typeof(Uri))
            {
                result = new Uri(value, UriKind.RelativeOrAbsolute);
            }
            else
            {
                switch (Type.GetTypeCode(targetType))
                {
                    case TypeCode.Boolean:
                        result = XmlConvert.ToBoolean(value);
                        break;

                    case TypeCode.Byte:
                        result = XmlConvert.ToByte(value);
                        break;

                    case TypeCode.Char:
                        result = XmlConvert.ToChar(value);
                        break;

                    case TypeCode.DateTime:
                        // When we deserialize dates, convert them to local dates
                        result = XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.Local);
                        break;

                    case TypeCode.Decimal:
                        result = XmlConvert.ToDecimal(value);
                        break;

                    case TypeCode.Double:
                        result = XmlConvert.ToDouble(value);
                        break;

                    case TypeCode.Int16:
                        result = XmlConvert.ToInt16(value);
                        break;

                    case TypeCode.Int32:
                        result = XmlConvert.ToInt32(value);
                        break;

                    case TypeCode.Int64:
                        result = XmlConvert.ToInt64(value);
                        break;

                    case TypeCode.SByte:
                        result = XmlConvert.ToSByte(value);
                        break;

                    case TypeCode.Single:
                        result = XmlConvert.ToSingle(value);
                        break;

                    case TypeCode.String:
                        result = value;
                        break;

                    case TypeCode.UInt16:
                        result = XmlConvert.ToUInt16(value);
                        break;

                    case TypeCode.UInt32:
                        result = XmlConvert.ToUInt32(value);
                        break;

                    case TypeCode.UInt64:
                        result = XmlConvert.ToUInt64(value);
                        break;

                    default:
                        // Last resort, try an installed value converter...
                        TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
                        if (converter != null)
                        {
                            result = converter.ConvertFromInvariantString(value);
                        }
                        else
                        {
                            result = Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
                        }

                        break;
                }
            }

            return (T)result;
        }

        /// <summary>
        /// Converts an input value into a stable XML strings.
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="input">The input value.</param>
        /// <returns>The value converted to a string that can be serialized into XML safely.</returns>
        public static string ToXmlString(object input)
        {
            if (input == null)
            {
                return null;
            }

            object value = input;
            Type targetType = input.GetType();

            Type nullableType = targetType.TryGetNullableSubType();
            if (nullableType != null)
            {
                // If it is a nullable type and the input is null, return early
                // Otherwise, flip the target type to do the appropriate conversion
                if (input == null)
                {
                    return null;
                }

                targetType = nullableType;
            }

            // Check for this before getting the type code, otherwise an enum type might return an integer typecode
            if (targetType.IsEnum)
            {
                return Enum.GetName(targetType, value);
            }

            switch (Type.GetTypeCode(targetType))
            {
                case TypeCode.Boolean:
                    return XmlConvert.ToString((bool)value);

                case TypeCode.Byte:
                    return XmlConvert.ToString((byte)value);

                case TypeCode.Char:
                    return XmlConvert.ToString((char)value);

                case TypeCode.DateTime:
                    // When we deserialize dates, always serialize them as UTC
                    return XmlConvert.ToString((DateTime)value, XmlDateTimeSerializationMode.Utc);

                case TypeCode.Decimal:
                    return XmlConvert.ToString((Decimal)value);

                case TypeCode.Double:
                    return XmlConvert.ToString((Double)value);

                case TypeCode.Int16:
                    return XmlConvert.ToString((Int16)value);

                case TypeCode.Int32:
                    return XmlConvert.ToString((Int32)value);

                case TypeCode.Int64:
                    return XmlConvert.ToString((Int64)value);

                case TypeCode.SByte:
                    return XmlConvert.ToString((SByte)value);

                case TypeCode.Single:
                    return XmlConvert.ToString((Single)value);

                case TypeCode.String:
                    return (string)value;

                case TypeCode.UInt16:
                    return XmlConvert.ToString((UInt16)value);

                case TypeCode.UInt32:
                    return XmlConvert.ToString((UInt32)value);

                case TypeCode.UInt64:
                    return XmlConvert.ToString((UInt64)value);
            }

            if (targetType == typeof(Guid))
            {
                return XmlConvert.ToString((Guid)value);
            }

            // If a converter is available, use it
            TypeConverter converter = TypeDescriptor.GetConverter(targetType);
            if (converter != null)
            {
                return converter.ConvertToInvariantString(value);
            }

            // Prefer IFormattable, as we can pass the invariant culture
            if (value is IFormattable)
            {
                return ((IFormattable)value).ToString(null, CultureInfo.InvariantCulture);
            }

            // Last resort formatting, this could be using the current culture...
            return value.ToString();
        }
    }

    /// <summary>
    /// A delegate that is invoked when a value is read from an XML element.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The value.</param>
    public delegate void ValueReadDelegate<T>(T value);
}
