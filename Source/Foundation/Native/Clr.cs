using System.Runtime.InteropServices;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Native
{
    /// <summary>
    /// Exposes PInvoke method wrappers for functions in clr.dll.
    /// </summary>
    public static partial class NativeMethods
    {
        [DllImport("clr.dll", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
        public static extern void CorLaunchApplication(uint hostType, string applicationFullName, int manifestPathsCount, 
            string[] manifestPaths, int activationDataCount, string[] activationData, PROCESS_INFORMATION processInformation);
    }
}
