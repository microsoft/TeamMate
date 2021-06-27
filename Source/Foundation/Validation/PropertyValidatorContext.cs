// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


namespace Microsoft.Tools.TeamMate.Foundation.Validation
{
    public interface IPropertyValidatorContext
    {
        string PropertyName { get; }
        string PropertyDisplayName { get; }
        object GetPropertyValue();
    }
}
