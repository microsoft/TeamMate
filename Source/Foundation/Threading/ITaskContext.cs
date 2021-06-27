// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.Tools.TeamMate.Foundation.Threading
{
    /// <summary>
    /// Defines the interface for the context in which a task is running, allowing support for
    /// cancellation, status and progress reporting.
    /// </summary>
    public interface ITaskContext : IProgress<double>
    {
        /// <summary>
        /// Gets the cancellation token for the current task operation.
        /// </summary>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Gets a value indicating whether cancellation has been requested.
        /// </summary>
        bool IsCancellationRequested { get; }

        /// <summary>
        /// Gets a value indicating whether the task can be canceled.
        /// </summary>
        bool CanBeCanceled { get; }

        /// <summary>
        /// Throws if cancellation has been requested.
        /// </summary>
        void ThrowIfCancellationRequested();

        /// <summary>
        /// Gets or sets the title of the task operation.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Gets or sets the status text of the task operation.
        /// </summary>
        string Status { get; set; }
    }

    /// <summary>
    /// Provides extension methods for the IProgress interface.
    /// </summary>
    public static class ProgressExtensions
    {
        /// <summary>
        /// Reports the progress update between 0.0 and 1.0, given a current and a total value.
        /// </summary>
        /// <param name="progress">The progress.</param>
        /// <param name="current">The current progress value.</param>
        /// <param name="total">The total progress value.</param>
        public static void Report(this IProgress<double> progress, double current, double total)
        {
            Assert.ParamIsNotNull(progress, "progress");

            if (current >= 0 && total > 0 && current <= total)
            {
                progress.Report(current / total);
            }
            else
            {
                Debug.Fail(String.Format("Current or total out of range: {0} / {1}", current, total));
            }
        }
    }
}
