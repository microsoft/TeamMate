using System.Windows.Input;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.MVVM
{
    public interface IGlobalCommandProvider
    {
        CommandBindingCollection GlobalCommandBindings { get; }
    }
}
