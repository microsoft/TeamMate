// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


namespace Microsoft.Tools.TeamMate.Foundation.Validation
{
    public interface IValidationRule
    {
        ValidationResult Validate();
    }

    public interface IPropertyValidationRule : IValidationRule
    {
        string PropertyName { get; }
    }
}
