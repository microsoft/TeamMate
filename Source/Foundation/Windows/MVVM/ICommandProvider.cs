using System.Windows.Input;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows.MVVM
{
    public interface ICommandProvider
    {
        void RegisterBindings(CommandBindingCollection bindings);
    }
}
