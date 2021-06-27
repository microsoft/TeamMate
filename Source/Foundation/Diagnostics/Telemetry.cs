using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.Tools.TeamMate.Foundation.Diagnostics
{
    /// <summary>
    /// Provides utility mehtods for logging telemetry events.
    /// </summary>
    /// <remarks>
    /// This class follows a similar pattern to the Trace class, and notifications to TraceListeners.
    /// </remarks>
    public static class Telemetry
    {
        private static bool isEnabled;
        private static bool hasListeners;
        private static ICollection<TelemetryListener> listeners = new TelemetryListener[0];

        private static Thread workerThread;

        private static ConcurrentQueue<NotifyDelegate> workQueue = new ConcurrentQueue<NotifyDelegate>();
        private static AutoResetEvent workQueuedEvent = new AutoResetEvent(false);
        private static ManualResetEvent workQueuedEmptiedEvent = new ManualResetEvent(false);

        /// <summary>
        /// Gets or sets a value indicating whether telemetry logging is enabled.
        /// </summary>
        public static bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    InvalidateHasListeners();

                    if (IsEnabled)
                    {
                        workerThread = new Thread(DoWork);
                        workerThread.Name = "Telemetry Worker";
                        workerThread.Priority = ThreadPriority.Lowest;
                        workerThread.Start();
                    }
                }
            }
        }

        /// <summary>
        /// A worker loop that will dequeue telemetry events and notify telemetry listeners.
        /// </summary>
        private static void DoWork()
        {
            while (workQueuedEvent.WaitOne())
            {
                workQueuedEmptiedEvent.Reset();

                NotifyDelegate notifyDelegate;
                while (workQueue.TryDequeue(out notifyDelegate))
                {
                    try
                    {
                        foreach (TelemetryListener listener in listeners)
                        {
                            notifyDelegate(listener);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.ErrorAndBreak(e, "Unexpected error in Telemetry background work");
                    }
                }

                workQueuedEmptiedEvent.Set();
            }
        }

        /// <summary>
        /// Flushes any pending telemetry events in this class to telemetry listeners.
        /// </summary>
        public static void Flush()
        {
            workQueuedEmptiedEvent.WaitOne();
        }

        /// <summary>
        /// Adds a listener.
        /// </summary>
        /// <param name="listener">The listener.</param>
        public static void AddListener(TelemetryListener listener)
        {
            Assert.ParamIsNotNull(listener, "listener");

            var newListeners = listeners.ToList();
            newListeners.Add(listener);
            listeners = newListeners.ToArray();
            InvalidateHasListeners();
        }

        /// <summary>
        /// Removes a listener.
        /// </summary>
        /// <param name="listener">The listener.</param>
        public static void RemoveListener(TelemetryListener listener)
        {
            Assert.ParamIsNotNull(listener, "listener");

            var newListeners = listeners.ToList();
            newListeners.Remove(listener);
            listeners = newListeners.ToArray();
            InvalidateHasListeners();
        }

        /// <summary>
        /// Logs a given telemetry event.
        /// </summary>
        /// <param name="name">The event name.</param>
        /// <param name="properties">The (optional) event properties.</param>
        public static void Event(string name, TelemetryEventProperties properties = null)
        {
            Assert.ParamIsNotNull(name, "name");

            if (!hasListeners)
            {
                return;
            }

            Event(new EventInfo(DateTime.Now, name, properties));
        }

        /// <summary>
        /// Logs the given event information.
        /// </summary>
        /// <param name="info">The event information.</param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]    // Marked as non-browsable as we want developers to use the more usable overload above
        public static void Event(EventInfo info)
        {
            QueueNotification(l => l.Event(info));
        }

        /// <summary>
        /// Logs the given exception information.
        /// </summary>
        /// <param name="info">The exception information.</param>
        public static void Exception(Exception info)
        {
            QueueNotification(l => l.Exception(info));
        }

        /// <summary>
        /// Invalidates the has listeners boolean flag based on the current set of listeners.
        /// </summary>
        private static void InvalidateHasListeners()
        {
            hasListeners = (isEnabled && listeners.Any());
        }

        /// <summary>
        /// Queues the notification of the given delegate for later.
        /// </summary>
        /// <param name="notifyDelegate">The notify delegate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void QueueNotification(NotifyDelegate notifyDelegate)
        {
            if (!hasListeners)
            {
                return;
            }

            workQueue.Enqueue(notifyDelegate);
            workQueuedEvent.Set();
        }

        /// <summary>
        /// A delegate that actions (notifies) a given listener.
        /// </summary>
        /// <param name="listener">The listener.</param>
        private delegate void NotifyDelegate(TelemetryListener listener);
    }
}
