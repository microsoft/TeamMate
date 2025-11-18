using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Utilities
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class ScheduledAction : IDisposable
    {
        private static TimeSpan UpdateSchedulingBuffer = TimeSpan.FromSeconds(10);

        public Action Action { get; set; }
        public DateTime? LastRun { get; set; }
        public bool IsRunning { get; set; }

        private Timer Timer { get; set; }

        private TimeSpan interval;

        public TimeSpan Interval
        {
            get { return this.interval; }
            set
            {
                this.interval = value;

                // If the interval changed while the scheduled task was running, restart it
                // to make the new interval take effect
                if (IsRunning)
                {
                    Stop();
                    Start();
                }
            }
        }

        public void Start()
        {
            if (!IsRunning)
            {
                IsRunning = true;

                if (IsExpired)
                {
                    RunImmediatelyInBackground();
                }
                else
                {
                    ScheduleForLater(TimeToNextCheck);
                }
            }
        }

        public void Reset()
        {
            if (IsRunning)
            {
                Stop();
                this.LastRun = DateTime.Now;
                Start();
            }
        }

        private bool IsExpired
        {
            get
            {
                return (TimeToNextCheck == TimeSpan.Zero);
            }
        }

        private TimeSpan? TimeSinceLastCheck
        {
            get
            {
                return (this.LastRun != null) ? (TimeSpan?)(DateTime.Now - this.LastRun.Value) : null;
            }
        }

        private TimeSpan TimeToNextCheck
        {
            get
            {
                TimeSpan? timeSinceLastCheck = TimeSinceLastCheck;
                if (timeSinceLastCheck == null || timeSinceLastCheck > this.Interval)
                {
                    // Now
                    return TimeSpan.Zero;
                }
                else
                {
                    return this.Interval - timeSinceLastCheck.Value + UpdateSchedulingBuffer;
                }
            }
        }

        // Only meant to be called on start, timer Elapsed events will fire on their own thread pool thread
        private async void RunImmediatelyInBackground()
        {
            Log.Info("Background action {0}", GetType().Name);

            try
            {
                await Task.Run(() => RunImmediately());
            }
            catch (Exception e)
            {
                Log.ErrorAndBreak(e, "Unexpected error running a scheduled action.");
            }
        }

        public void RunImmediately()
        {
            try
            {
                ClearTimer();
                this.LastRun = DateTime.Now;
                this.Action();
            }
            catch (Exception e)
            {
                Log.ErrorAndBreak(e);
            }
            finally
            {
                if (IsRunning)
                {
                    ScheduleForLater(this.Interval);
                }
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                ClearTimer();
            }
        }

        private void ClearTimer()
        {
            Timer localTimer = this.Timer;
            this.Timer = null;

            if (localTimer != null)
            {
                localTimer.Dispose();
            }
        }

        private void ScheduleForLater(TimeSpan interval)
        {
            ClearTimer();

            Timer timer = new Timer(interval.TotalMilliseconds);
            timer.AutoReset = false;
            timer.Elapsed += delegate(object sender, ElapsedEventArgs e)
            {
                // Paranoid chekc to see if the timer was not cleared while it was firing (job is still active)
                if (this.Timer == sender)
                {
                    RunImmediately();
                }
            };

            this.Timer = timer;
            this.Timer.Start();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
