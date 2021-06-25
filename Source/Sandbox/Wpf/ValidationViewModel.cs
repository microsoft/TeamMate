using Microsoft.Internal.Tools.TeamMate.Foundation.Validation;
using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.MVVM;

namespace Microsoft.Internal.Tools.TeamMate.Sandbox.Wpf
{
    public class ValidatingFormViewModel : ValidatableViewModelBase
    {
        private string someText;

        public ValidatingFormViewModel()
        {
            InitializeValidation();
        }

        private void InitializeValidation()
        {
            Validator.RuleForProperty(() => SomeText)
                .IsNotEmpty()
                .HasMaxLength(5)
                .Is((x) => x != null && !x.Contains("ben"), "{PropertyName} cannot contain 'ben'");
        }

        public string SomeText
        {
            get { return this.someText; }
            set { SetAndValidateProperty(ref this.someText, value); }
        }
    }
}
