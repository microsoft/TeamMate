// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    public static class ViewModelFactory
    {
        public static T Create<T>()
        {
            if (ViewModelCreator != null)
            {
                return (T)ViewModelCreator(typeof(T));
            }

            return Activator.CreateInstance<T>();
        }

        public static ViewModelCreator ViewModelCreator { get; set; }
    }

    public delegate object ViewModelCreator(Type viewModelType);
}
