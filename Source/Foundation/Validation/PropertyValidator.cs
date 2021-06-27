// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Tools.TeamMate.Foundation.Validation
{
    public class PropertyValidator<T>
    {
        public PropertyValidator(Predicate<T> validator, string messageFormat)
        {
            this.Validator = validator;
            this.MessageFormat = messageFormat;
        }

        private Predicate<T> Validator { get; set; }

        private string MessageFormat { get; set; }

        public IEnumerable<ValidationFailure> Validate(IPropertyValidatorContext context)
        {
            T propertyValue = (T)context.GetPropertyValue();
            bool isValid = Validator(propertyValue);
            if (!isValid)
            {
                // TODO: Do message format...
                string message = FormatMessage(MessageFormat, context.PropertyDisplayName, propertyValue);
                ValidationFailure failure = new ValidationFailure(message);
                return new ValidationFailure[] { failure };
            }

            return null;
        }

        private string FormatMessage(string messageFormat, string displayName, object propertyValue)
        {
            messageFormat = messageFormat.Replace("{PropertyName}", displayName);

            // TODO: Proper formatting of property values... Avoid replacement, if we can, in terms of formatting the property value?
            // Or do trick #2, which is to replace the placeholders with {0}, {1}, etc... and run a String.Format(). That would also allow for string format
            // specifiers, which is nice
            messageFormat = messageFormat.Replace("{PropertyValue}", (propertyValue != null)? propertyValue.ToString() : String.Empty);
            return messageFormat;
        }
    }
}
