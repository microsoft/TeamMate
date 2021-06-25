using System.Collections.Generic;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Validation
{
    public class ValidationResult
    {
        private IEnumerable<ValidationFailure> failures;

        public ValidationResult()
        {
            // TODO: Initialize to empty
        }
        public ValidationResult(IEnumerable<ValidationFailure> failures)
        {
            this.failures = failures;
        }

        public IEnumerable<ValidationFailure> Failures
        {
            get { return this.failures; }
        }
    }
}
