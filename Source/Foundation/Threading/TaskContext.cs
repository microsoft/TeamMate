using Microsoft.Tools.TeamMate.Foundation.ComponentModel;
using System;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.Tools.TeamMate.Foundation.Threading
{
    /// <summary>
    /// A default implementation of a task context interface.
    /// </summary>
    public class TaskContext : ObservableObjectBase, ITaskContext, IDisposable
    {
        private const double CompletedProgress = 1.0;

        // Our estimation algorithm will only use the last ten time data points gathered will a task is running.
        private const int MaximumTimeEstimationRegressionPoints = 10;
        private static readonly TimeSpan IntervalForReestimatingRemainignTime = TimeSpan.FromSeconds(1);

        private LinearRegression timeEstimationRegression;
        private object timeEstimationRegressionLock = new object();

        private Timer remainingTimeEstimationTimer;
        private bool isRunning;
        private string errorMessage;
        private Exception error;
        private bool isFailed;
        private bool isCompleteWithoutFailures;
        private bool reportsProgress;
        private double progress;
        private string status;
        private string title;
        private bool isComplete;
        private TimeSpan? estimatedRemainingTime;

        /// <summary>
        /// Notifies observers that the running task has completed.
        /// </summary>
        public event EventHandler Completed;

        /// <summary>
        /// Returns an empty task content (null object).
        /// </summary>
        public static TaskContext None
        {
            get
            {
                TaskContext context = new TaskContext(null);
                context.ReportsProgress = false;
                context.isRunning = false;
                return context;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskContext"/> class, with a new
        /// cancellation token source.
        /// </summary>
        public TaskContext()
            : this(new CancellationTokenSource())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskContext"/> class.
        /// </summary>
        /// <param name="source">The cancellation token source source.</param>
        public TaskContext(CancellationTokenSource source)
        {
            this.IsRunning = true;
            this.ReportsProgress = true;
            this.CancellationTokenSource = source;

            // Listen on cancellation requests to fire a notification on this item...
            this.CancellationToken.Register(delegate()
            {
                OnPropertyChanged("IsCancellationRequested");
            });
        }

        /// <summary>
        /// Gets a value indicating whether the task is currently running or not.
        /// </summary>
        public bool IsRunning
        {
            get { return this.isRunning; }
            private set { SetProperty(ref this.isRunning, value); }
        }

        /// <summary>
        /// Gets the cancellation token source or <c>null</c> if one is not defined.
        /// </summary>
        private CancellationTokenSource CancellationTokenSource { get; set; }

        /// <summary>
        /// Gets the cancellation token for the current task.
        /// </summary>
        public CancellationToken CancellationToken
        {
            get { return (CancellationTokenSource != null) ? CancellationTokenSource.Token : CancellationToken.None; }
        }

        /// <summary>
        /// Requests the cancellation for this task, if it can be cancelled.
        /// </summary>
        public void Cancel()
        {
            if (CancellationTokenSource != null)
            {
                CancellationTokenSource.Cancel();
            }
        }

        /// <summary>
        /// Gets whether cancellation has been requested for this task.
        /// </summary>
        public bool IsCancellationRequested
        {
            get { return CancellationToken.IsCancellationRequested; }
        }

        /// <summary>
        /// Gets whether this task is capable of being cancelled.
        /// </summary>
        public bool CanBeCanceled
        {
            get { return CancellationToken.CanBeCanceled; }
        }

        /// <summary>
        /// Throws an OperationCanceledException if cancellation has been requested.
        /// </summary>
        public void ThrowIfCancellationRequested()
        {
            CancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Signals that the task has failed with the specified error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public void Fail(string errorMessage)
        {
            Fail(errorMessage, null);
        }

        /// <summary>
        /// Signals that the task has failed with the specified exception.
        /// </summary>
        /// <param name="error">The error.</param>
        public void Fail(Exception error)
        {
            Fail(null, error);
        }

        /// <summary>
        /// Signals that the task has failed with the specified error message and exception.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="error">The error.</param>
        public void Fail(string errorMessage, Exception error)
        {
            this.ErrorMessage = errorMessage ?? ((error != null) ? error.Message : null);
            this.Error = error;
            this.IsFailed = true;
        }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string ErrorMessage
        {
            get { return this.errorMessage; }
            private set { SetProperty(ref this.errorMessage, value); }
        }

        /// <summary>
        /// Gets the error.
        /// </summary>
        public Exception Error
        {
            get { return this.error; }
            private set { SetProperty(ref this.error, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the task has failed or not.
        /// </summary>
        public bool IsFailed
        {
            get { return this.isFailed; }
            private set { SetProperty(ref this.isFailed, value); }
        }

        /// <summary>
        /// Gets a value indicating whether this task has completed without failing.
        /// </summary>
        public bool IsCompleteWithoutFailures
        {
            get { return this.isCompleteWithoutFailures; }
            private set { SetProperty(ref this.isCompleteWithoutFailures, value); }
        }

        /// <summary>
        /// Reports a progress update.
        /// </summary>
        /// <param name="value">The value of the updated progress.</param>
        public void Report(double value)
        {
            if (value >= 0 && value <= 1)
            {
                this.Progress = value;
            }
            else
            {
                Debug.Fail(String.Format("Progress out of range: {0}", value));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this task reports determinate progress.
        /// </summary>
        public bool ReportsProgress
        {
            get { return this.reportsProgress; }
            set { SetProperty(ref this.reportsProgress, value); }
        }

        /// <summary>
        /// Gets the progress for the current task.
        /// </summary>
        public double Progress
        {
            get { return this.progress; }

            private set
            {
                if (SetProperty(ref this.progress, value))
                {
                    // Lazy initialize the timer to estimate the remaining time
                    if (IsRunning && ReportsProgress && this.remainingTimeEstimationTimer == null)
                    {
                        this.remainingTimeEstimationTimer = new Timer(RecalculateEstimatedRemainingTime, null, IntervalForReestimatingRemainignTime, IntervalForReestimatingRemainignTime);
                    }

                    UpdateTimeEstimationRegression();
                }
            }
        }

        /// <summary>
        /// Gets or sets the status text of the task operation.
        /// </summary>
        public string Status
        {
            get { return this.status; }
            set { SetProperty(ref this.status, value); }
        }


        /// <summary>
        /// Gets or sets the title of the task operation.
        /// </summary>
        public string Title
        {
            get { return this.title; }
            set { SetProperty(ref this.title, value); }
        }


        /// <summary>
        /// Gets a value indicating whether the task is complete.
        /// </summary>
        public bool IsComplete
        {
            get { return this.isComplete; }
            private set { SetProperty(ref this.isComplete, value); }
        }

        /// <summary>
        /// Marks the current task execution as completed.
        /// </summary>
        public void Complete()
        {
            // TODO: This should probably be locked for multithreaded access

            if (!IsComplete)
            {
                if (this.remainingTimeEstimationTimer != null)
                {
                    this.remainingTimeEstimationTimer.Dispose();
                    this.remainingTimeEstimationTimer = null;
                }

                if (this.CancellationTokenSource != null)
                {
                    this.CancellationTokenSource.Dispose();
                    this.CancellationTokenSource = null;
                }

                this.IsRunning = false;
                this.IsComplete = true;
                this.IsCompleteWithoutFailures = !IsFailed;

                Completed?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the estimated remaining time.
        /// </summary>
        public TimeSpan? EstimatedRemainingTime
        {
            get { return this.estimatedRemainingTime; }
            private set { SetProperty(ref this.estimatedRemainingTime, value); }
        }

        /// <summary>
        /// Completes the current task if not already complete.
        /// </summary>
        public void Dispose()
        {
            Complete();
        }

        /// <summary>
        /// Estimates the remaining time for the task.
        /// </summary>
        /// <returns>The estimated remaining time.</returns>
        private TimeSpan EstimateRemainingTime()
        {
            if (!ReportsProgress || timeEstimationRegression == null || timeEstimationRegression.PointCount < 2)
            {
                return TimeSpan.MaxValue;
            }
            else if (Progress == CompletedProgress)
            {
                return TimeSpan.Zero;
            }
            else
            {
                long ticksAtEnd;

                lock (timeEstimationRegressionLock)
                {
                    UpdateTimeEstimationRegression();
                    ticksAtEnd = (long)timeEstimationRegression.ValueAt(CompletedProgress);
                }

                DateTime now = DateTime.Now;
                DateTime endTime = (ticksAtEnd > 0) ? new DateTime(ticksAtEnd) : now;
                TimeSpan remainingTime = endTime - now;
                return (remainingTime > TimeSpan.Zero) ? remainingTime : TimeSpan.Zero;
            }
        }

        /// <summary>
        /// Updates the time estimation regression with the current progress and time.
        /// </summary>
        private void UpdateTimeEstimationRegression()
        {
            if (timeEstimationRegression == null)
            {
                timeEstimationRegression = new LinearRegression(MaximumTimeEstimationRegressionPoints);
            }

            lock (timeEstimationRegressionLock)
            {
                timeEstimationRegression.AddPoint(Progress, DateTime.Now.Ticks);
            }
        }

        /// <summary>
        /// Invalidates the remaining time.
        /// </summary>
        /// <param name="state">The state.</param>
        private void RecalculateEstimatedRemainingTime(object state)
        {
            this.EstimatedRemainingTime = this.EstimateRemainingTime();
        }
    }
}
