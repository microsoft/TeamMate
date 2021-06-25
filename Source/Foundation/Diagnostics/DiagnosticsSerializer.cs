using Microsoft.Internal.Tools.TeamMate.Foundation.Xml;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics
{
    /// <summary>
    /// Serializes & deserializes elements in the Diagnostics namespace to/from XML.
    /// </summary>
    public class DiagnosticsSerializer
    {
        /// <summary>
        /// A special marker to indicate that an unknwon XML element was deserialized from a telemetry stream.
        /// </summary>
        public static readonly object UnknownElement = new object();

        /// <summary>
        /// Serializes the specified system information to a stream.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="stream">The stream.</param>
        public void Serialize(SystemInfo info, Stream stream)
        {
            new XDocument(Serialize(info)).Save(stream);
        }

        /// <summary>
        /// Serializes the specified exception information to a stream.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="stream">The stream.</param>
        public void Serialize(ExceptionInfo info, Stream stream)
        {
            new XDocument(Serialize(info)).Save(stream);
        }

        /// <summary>
        /// Deserializes the event information.
        /// </summary>
        /// <param name="element">The XML element.</param>
        /// <returns>The event information.</returns>
        private EventInfo DeserializeEventInfo(XElement element)
        {
            string name = element.GetAttribute<string>(EventInfoSchema.Name);
            DateTime time = element.GetAttribute<DateTime>(EventInfoSchema.Time);
            TelemetryEventProperties eventProperties = null;

            var properties = element.Element(EventInfoSchema.Properties);
            if (properties != null)
            {
                eventProperties = new TelemetryEventProperties();

                foreach (var property in properties.Elements(EventInfoSchema.Property))
                {
                    string propertyName = property.GetAttribute<string>(EventInfoSchema.Name);
                    string propertyValue = property.GetAttribute<string>(EventInfoSchema.Value);

                    eventProperties[propertyName] = propertyValue;
                }
            }

            return new EventInfo(time, name, eventProperties);
        }

        /// <summary>
        /// Serializes the specified event information.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <returns>The XML element.</returns>
        public XElement Serialize(EventInfo info)
        {
            XElement e = new XElement(EventInfoSchema.Event,
                new XAttribute(EventInfoSchema.Name, info.Name),
                new XAttribute(EventInfoSchema.Time, XmlExtensions.ToXmlString(info.Time))
            );

            var properties = info.Properties;
            if (properties != null && properties.Any())
            {
                XElement propertiesElement = new XElement(EventInfoSchema.Properties);
                foreach (var item in properties)
                {
                    if (item.Value != null)
                    {
                        propertiesElement.Add(new XElement(EventInfoSchema.Property,
                            new XAttribute(EventInfoSchema.Name, item.Key),
                            new XAttribute(EventInfoSchema.Value, XmlExtensions.ToXmlString(item.Value))
                        ));
                    }
                }
                e.Add(propertiesElement);
            }
            return e;
        }

        /// <summary>
        /// Serializes the specified exception exception.
        /// </summary>
        /// <param name="exception">The exception information.</param>
        /// <returns>The XML element.</returns>
        public XElement Serialize(ExceptionInfo exception)
        {
            return Serialize(exception, ExceptionInfoSchema.Exception);
        }

        /// <summary>
        /// Serializes the specified exception information to an element with a given element name.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="name">The output element name.</param>
        /// <returns>The XML element.</returns>
        private XElement Serialize(ExceptionInfo exception, XName name)
        {
            XElement e = new XElement(name);

            e.SetElementValue<string>(ExceptionInfoSchema.Source, exception.Source);
            e.SetElementValue<string>(ExceptionInfoSchema.Type, exception.Type);
            e.SetElementValue<string>(ExceptionInfoSchema.Message, exception.Message);
            e.SetCDataElementValue(ExceptionInfoSchema.StackTrace, exception.StackTrace);
            e.SetElementValue<int>(ExceptionInfoSchema.HResult, exception.HResult);

            if (exception.InnerException != null)
            {
                e.Add(Serialize(exception.InnerException, ExceptionInfoSchema.InnerException));
            }

            e.SetCDataElementValue(ExceptionInfoSchema.FullText, exception.FullText);

            return e;
        }

        /// <summary>
        /// Serializes the specified system information.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <returns>The XML element.</returns>
        public XElement Serialize(SystemInfo info)
        {
            var memory = info.Memory;

            return new XElement(SystemInfoSchema.SystemInfo, new XAttribute(SystemInfoSchema.SchemaVersion, SystemInfoSchema.CurrentVersion),
                new XElement(SystemInfoSchema.OSName, info.OSName),
                new XElement(SystemInfoSchema.OSVersion, info.OSVersion),
                new XElement(SystemInfoSchema.Is64BitOperatingSystem, info.Is64BitOperatingSystem),
                new XElement(SystemInfoSchema.ClrVersion, info.ClrVersion),
                new XElement(SystemInfoSchema.SystemCultureName, info.SystemCultureName),
                new XElement(SystemInfoSchema.UserCultureName, info.UserCultureName),
                new XElement(SystemInfoSchema.TimeZoneName, info.TimeZoneName),
                new XElement(SystemInfoSchema.TimeZoneUtcOffset, XmlExtensions.ToXmlString(info.TimeZoneUtcOffset)),
                new XElement(SystemInfoSchema.IsRemoteDesktopSession, info.IsRemoteDesktopSession),
                new XElement(SystemInfoSchema.IsMultiTouchEnabled, info.IsMultiTouchEnabled),

                new XElement(SystemInfoSchema.Memory,
                    new XElement(SystemInfoSchema.TotalPhysicalMemory, memory.TotalPhysicalMemory),
                    new XElement(SystemInfoSchema.AvailablePhysicalMemory, memory.AvailablePhysicalMemory),
                    new XElement(SystemInfoSchema.TotalVirtualMemory, memory.TotalVirtualMemory),
                    new XElement(SystemInfoSchema.AvailableVirtualMemory, memory.AvailableVirtualMemory),
                    new XElement(SystemInfoSchema.MemoryLoad, memory.MemoryLoad)
                ),

                new XElement(SystemInfoSchema.Processors,
                    info.Processors.Select(processor => new XElement(SystemInfoSchema.Processor,
                        new XElement(SystemInfoSchema.Name, processor.Name),
                        new XElement(SystemInfoSchema.Family, processor.Family),
                        new XElement(SystemInfoSchema.NumberOfCores, processor.NumberOfCores),
                        new XElement(SystemInfoSchema.NumberOfLogicalProcessors, processor.NumberOfLogicalProcessors),
                        new XElement(SystemInfoSchema.MaxClockSpeed, processor.MaxClockSpeed)
                    ))
                ),

                new XElement(SystemInfoSchema.VirtualScreen,
                    new XElement(SystemInfoSchema.Width, info.VirtualScreen.Width),
                    new XElement(SystemInfoSchema.Height, info.VirtualScreen.Height)
                ),

                new XElement(SystemInfoSchema.Screens,
                    info.Screens.Select(screen => new XElement(SystemInfoSchema.Screen,
                        new XElement(SystemInfoSchema.Width, screen.Width),
                        new XElement(SystemInfoSchema.Height, screen.Height),
                        new XElement(SystemInfoSchema.BitsPerPixel, screen.BitsPerPixel),
                        new XElement(SystemInfoSchema.IsPrimary, screen.IsPrimary)
                    ))
                ),

                new XElement(SystemInfoSchema.IEVersion, info.IEVersion),

                new XElement(SystemInfoSchema.VisualStudioVersions,
                    info.VisualStudioVersions.Select(version => new XElement(SystemInfoSchema.VisualStudioVersion, version))
                )
            );
        }

        /// <summary>
        /// Deserializes the exception information.
        /// </summary>
        /// <param name="element">The XML element.</param>
        /// <returns>The exception information.</returns>
        public ExceptionInfo DeserializeExceptionInfo(XElement element)
        {
            ExceptionInfo exception = new ExceptionInfo();
            exception.Source = element.GetElementValue<string>(ExceptionInfoSchema.Source);
            exception.Type = element.GetElementValue<string>(ExceptionInfoSchema.Type);
            exception.Message = element.GetElementValue<string>(ExceptionInfoSchema.Message);
            exception.StackTrace = element.GetElementValue<string>(ExceptionInfoSchema.StackTrace);
            exception.HResult = element.GetElementValue<int>(ExceptionInfoSchema.HResult);

            var innerExceptionElement = element.Element(ExceptionInfoSchema.InnerException);
            if (innerExceptionElement != null)
            {
                exception.InnerException = DeserializeExceptionInfo(innerExceptionElement);
            }

            exception.FullText = element.GetElementValue<string>(ExceptionInfoSchema.FullText);

            return exception;
        }

        /// <summary>
        /// Deserializes an arbitrary telemetry object from an XML element. The result could be an event,
        /// exception, application information, session information, or system information, or the unknown element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>A deserialized telemetry object, or the "Unknown" element if unrecognized.</returns>
        public object DeserializeElement(XElement element)
        {
            // Ordered in descending order of expected frequency
            if (element.Name == EventInfoSchema.Event)
            {
                return DeserializeEventInfo(element);
            }
            else if (element.Name == ExceptionInfoSchema.Exception)
            {
                return DeserializeExceptionInfo(element);
            }
            else if (element.Name == SystemInfoSchema.SystemInfo)
            {
                return DeserializeSystemInfo(element);
            }

            return UnknownElement;
        }

        /// <summary>
        /// Deserializes the exception information.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>The exception information.</returns>
        public ExceptionInfo DeserializeExceptionInfo(Stream stream)
        {
            var doc = XDocument.Load(stream);
            var root = doc.GetExpectedRoot(ExceptionInfoSchema.Exception);
            return DeserializeExceptionInfo(root);
        }

        /// <summary>
        /// Deserializes the system information.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>The system information.</returns>
        public SystemInfo DeserializeSystemInfo(Stream stream)
        {
            var doc = XDocument.Load(stream);
            var root = doc.GetExpectedRoot(SystemInfoSchema.SystemInfo);
            return DeserializeSystemInfo(root);
        }

        /// <summary>
        /// Deserializes the system information.
        /// </summary>
        /// <param name="e">The XML element.</param>
        /// <returns>The system information.</returns>
        public SystemInfo DeserializeSystemInfo(XElement e)
        {
            SystemInfo systemInfo = new SystemInfo();
            systemInfo.OSName = e.GetElementValue<string>(SystemInfoSchema.OSName);
            systemInfo.OSVersion = e.GetElementValue<string>(SystemInfoSchema.OSName);
            systemInfo.Is64BitOperatingSystem = e.GetElementValue<bool>(SystemInfoSchema.Is64BitOperatingSystem);
            systemInfo.ClrVersion = e.GetElementValue<string>(SystemInfoSchema.ClrVersion);
            systemInfo.SystemCultureName = e.GetElementValue<string>(SystemInfoSchema.SystemCultureName);
            systemInfo.UserCultureName = e.GetElementValue<string>(SystemInfoSchema.UserCultureName);
            systemInfo.TimeZoneName = e.GetElementValue<string>(SystemInfoSchema.TimeZoneName);
            systemInfo.TimeZoneUtcOffset = e.GetElementValue<TimeSpan>(SystemInfoSchema.TimeZoneUtcOffset);
            systemInfo.IsRemoteDesktopSession = e.GetElementValue<bool>(SystemInfoSchema.IsRemoteDesktopSession);
            systemInfo.IsMultiTouchEnabled = e.GetElementValue<bool>(SystemInfoSchema.IsMultiTouchEnabled);

            var memoryElement = e.Element(SystemInfoSchema.Memory);
            MemoryInfo memory = new MemoryInfo();
            memory.TotalPhysicalMemory = memoryElement.GetElementValue<long>(SystemInfoSchema.TotalPhysicalMemory);
            memory.AvailablePhysicalMemory = memoryElement.GetElementValue<long>(SystemInfoSchema.AvailablePhysicalMemory);
            memory.TotalVirtualMemory = memoryElement.GetElementValue<long>(SystemInfoSchema.TotalVirtualMemory);
            memory.AvailableVirtualMemory = memoryElement.GetElementValue<long>(SystemInfoSchema.AvailableVirtualMemory);
            memory.MemoryLoad = memoryElement.GetElementValue<int>(SystemInfoSchema.MemoryLoad);
            systemInfo.Memory = memory;

            var processors = e.Elements(SystemInfoSchema.Processors, SystemInfoSchema.Processor);
            foreach (var processor in processors)
            {
                ProcessorInfo processorInfo = new ProcessorInfo();
                processorInfo.Name = processor.GetElementValue<string>(SystemInfoSchema.Name);
                processorInfo.Family = processor.GetElementValue<string>(SystemInfoSchema.Family);
                processorInfo.NumberOfCores = processor.GetElementValue<int>(SystemInfoSchema.NumberOfCores);
                processorInfo.NumberOfLogicalProcessors = processor.GetElementValue<int>(SystemInfoSchema.NumberOfLogicalProcessors);
                processorInfo.MaxClockSpeed = processor.GetElementValue<int>(SystemInfoSchema.MaxClockSpeed);

                systemInfo.Processors.Add(processorInfo);
            }

            var virtualScreenElement = e.Element(SystemInfoSchema.VirtualScreen);
            VirtualScreenInfo virtualScreen = new VirtualScreenInfo()
            {
                Width = virtualScreenElement.GetElementValue<int>(SystemInfoSchema.Width),
                Height = virtualScreenElement.GetElementValue<int>(SystemInfoSchema.Height)
            };
            systemInfo.VirtualScreen = virtualScreen;

            var screens = e.Elements(SystemInfoSchema.Screens, SystemInfoSchema.Screen);
            foreach (var screen in screens)
            {
                ScreenInfo screenInfo = new ScreenInfo();
                screenInfo.Width = screen.GetElementValue<int>(SystemInfoSchema.Width);
                screenInfo.Height = screen.GetElementValue<int>(SystemInfoSchema.Height);
                screenInfo.BitsPerPixel = screen.GetElementValue<int>(SystemInfoSchema.BitsPerPixel);
                screenInfo.IsPrimary = screen.GetElementValue<bool>(SystemInfoSchema.IsPrimary);
                systemInfo.Screens.Add(screenInfo);
            }

            systemInfo.IEVersion = e.GetElementValue<string>(SystemInfoSchema.IEVersion);

            var visualStudioVersions = e.Elements(SystemInfoSchema.VisualStudioVersions, SystemInfoSchema.VisualStudioVersion);
            foreach (var version in visualStudioVersions)
            {
                systemInfo.VisualStudioVersions.Add(version.Value);
            }

            return systemInfo;
        }

        /// <summary>
        /// Describes the XML schema for an EventInfo type.
        /// </summary>
        private static class EventInfoSchema
        {
            public static XName Event = "Event";
            public static string Name = "Name";
            public static string Time = "Time";
            public static XName Properties = "Properties";
            public static XName Property = "Property";
            public static string Value = "Value";
        }

        /// <summary>
        /// Describes the XML schema for an ExceptionInfo type.
        /// </summary>
        public static class ExceptionInfoSchema
        {
            public static readonly XName Exception = "Exception";
            public static readonly XName Type = "Type";
            public static readonly XName StackTrace = "StackTrace";
            public static readonly XName Message = "Message";
            public static readonly XName HResult = "HResult";
            public static readonly XName Source = "Source";
            public static readonly XName InnerException = "InnerException";
            public static readonly XName FullText = "FullText";
        }

        /// <summary>
        /// Describes the XML schema for a SystemInfo type.
        /// </summary>
        private static class SystemInfoSchema
        {
            public static readonly Version CurrentVersion = new Version("1.0");
            public static readonly string SchemaVersion = "Version";

            public static readonly XName SystemInfo = "SystemInfo";
            public static readonly XName OSName = "OSName";
            public static readonly XName OSVersion = "OSVersion";
            public static readonly XName Is64BitOperatingSystem = "Is64BitOperatingSystem";
            public static readonly XName ClrVersion = "ClrVersion";
            public static readonly XName SystemCultureName = "SystemCultureName";
            public static readonly XName UserCultureName = "UserCultureName";
            public static readonly XName TimeZoneName = "TimeZoneName";
            public static readonly XName TimeZoneUtcOffset = "TimeZoneUtcOffset";
            public static readonly XName IsRemoteDesktopSession = "IsRemoteDesktopSession";
            public static readonly XName IsMultiTouchEnabled = "IsMultiTouchEnabled";
            public static readonly XName Memory = "Memory";
            public static readonly XName TotalPhysicalMemory = "TotalPhysicalMemory";
            public static readonly XName AvailablePhysicalMemory = "AvailablePhysicalMemory";
            public static readonly XName TotalVirtualMemory = "TotalVirtualMemory";
            public static readonly XName AvailableVirtualMemory = "AvailableVirtualMemory";
            public static readonly XName MemoryLoad = "MemoryLoad";
            public static readonly XName Processors = "Processors";
            public static readonly XName Processor = "Processor";
            public static readonly XName Name = "Name";
            public static readonly XName Family = "Family";
            public static readonly XName NumberOfCores = "NumberOfCores";
            public static readonly XName NumberOfLogicalProcessors = "NumberOfLogicalProcessors";
            public static readonly XName MaxClockSpeed = "MaxClockSpeed";
            public static readonly XName VirtualScreen = "VirtualScreen";
            public static readonly XName Width = "Width";
            public static readonly XName Height = "Height";
            public static readonly XName Screens = "Screens";
            public static readonly XName Screen = "Screen";
            public static readonly XName BitsPerPixel = "BitsPerPixel";
            public static readonly XName IsPrimary = "IsPrimary";
            public static readonly XName IEVersion = "IEVersion";
            public static readonly XName VisualStudioVersions = "VisualStudioVersions";
            public static readonly XName VisualStudioVersion = "VisualStudioVersion";
        }
    }
}
