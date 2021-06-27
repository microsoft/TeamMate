// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Tools.TeamMate.Foundation.Validation
{
    public class ValidationRule : IValidationRule
    {
        public ValidationRule(Func<bool> validator, string message)
        {
            this.Validator = validator;
            this.Message = message;
        }

        private Func<bool> Validator { get; set; }

        private string Message { get; set; }

        public ValidationResult Validate()
        {
            bool isValid = Validator();
            if (!isValid)
            {
                return new ValidationResult(new ValidationFailure[] { new ValidationFailure(this.Message) });
            }

            return null;
        }
    }
}
