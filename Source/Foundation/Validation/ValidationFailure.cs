// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


namespace Microsoft.Tools.TeamMate.Foundation.Validation
{
    public class ValidationFailure
    {
        public ValidationFailure(string error)
        {
            this.Error = error;
        }

        public string Error { get; private set; }
    }
}
