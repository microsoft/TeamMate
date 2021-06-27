using System.Windows.Input;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.MVVM
{
    public interface ICommandProvider
    {
        void RegisterBindings(CommandBindingCollection bindings);
    }
}
