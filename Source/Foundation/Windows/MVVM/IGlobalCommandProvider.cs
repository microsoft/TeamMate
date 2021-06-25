using System.Windows.Input;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows.MVVM
{
    public interface IGlobalCommandProvider
    {
        CommandBindingCollection GlobalCommandBindings { get; }
    }
}
