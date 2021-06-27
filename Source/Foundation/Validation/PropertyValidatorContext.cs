﻿
namespace Microsoft.Tools.TeamMate.Foundation.Validation
{
    public interface IPropertyValidatorContext
    {
        string PropertyName { get; }
        string PropertyDisplayName { get; }
        object GetPropertyValue();
    }
}
