// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using SimpleInjector;
using SimpleInjector.Advanced;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;

namespace Microsoft.Tools.TeamMate.Model
{
    public static class ContainerConfiguration
    {
        public static void Configure(Container container)
        {
            // Enable explicit property injection
            container.Options.PropertySelectionBehavior = new PropertySelectionBehavior<ImportAttribute>();

            // Register services by convention
            RegisterServices(container);

            // Verify dependencies
            container.Verify();
        }

        private static void RegisterServices(Container container)
        {
            IEnumerable<Type> serviceTypes = GetServiceTypes();
            foreach (var type in serviceTypes)
            {
                container.Register(type, type, Lifestyle.Singleton);
            }
        }

        private static IEnumerable<Type> GetServiceTypes()
        {
            var assembly = typeof(ContainerConfiguration).Assembly;

            var serviceTypes =
                from type in assembly.GetExportedTypes()
                where type.Namespace == "Microsoft.Tools.TeamMate.Services"
                   && type.Name.EndsWith("Service")
                select type;
            return serviceTypes;
        }

        private class PropertySelectionBehavior<T> : IPropertySelectionBehavior where T : Attribute
        {
            // Support explicit property injection

            public bool SelectProperty(Type implementationType, PropertyInfo propertyInfo)
            {
                return propertyInfo.GetCustomAttributes(typeof(T)).Any();
            }
        }
    }
}
