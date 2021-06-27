using System.Threading.Tasks;

namespace Microsoft.Tools.TeamMate.Foundation.Threading
{
    /// <summary>
    /// Provides utility methods for working with tasks.
    /// </summary>
    public static class TaskUtilities
    {
        private static readonly Task NullTaskObject = Task.FromResult<object>(null);

        /// <summary>
        /// Returns a null task.
        /// </summary>
        public static Task NullTask 
        { 
            get { return NullTaskObject; } 
        }
    }
}
