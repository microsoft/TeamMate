using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.Threading.Tasks;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Threading
{
    /// <summary>
    /// A utility class to only execute a task if a previous instance of the same task
    /// was not already running.
    /// </summary>
    public class SingleTaskRunner
    {
        private object syncLock = new object();
        private Task currentTask;

        /// <summary>
        /// Runs the specified action, if and only if a previous instance of it was not
        /// already running.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>The newly invoked task, or the previously running task.</returns>
        public Task Run(Func<Task> action)
        {
            Assert.ParamIsNotNull(action, "action");

            Task task = this.currentTask;
            if (task == null)
            {
                // TODO: Something is smelly here. This lock and extra check on task == null does nothing
                lock (syncLock)
                {
                    if (task == null)
                    {
                        task = action().ContinueWith(delegate(Task t)
                        {
                            if (this.currentTask == task)
                            {
                                this.currentTask = null;
                            }

                            if (t.Exception != null)
                            {
                                throw t.Exception;
                            }
                        });

                        this.currentTask = task;
                    }
                }
            }

            return task;
        }
    }
}
