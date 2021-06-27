using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Xml.Linq;

namespace Microsoft.Tools.TeamMate.Services
{
    public class AsyncWriterService : IDisposable
    {
        private static readonly TimeSpan AsyncSaveDelay = TimeSpan.FromSeconds(1);

        private object queueLock = new object();
        private List<SaveRequest> queue = new List<SaveRequest>();

        private bool isDisposed;
        private Timer saveTimer;

        public AsyncWriterService()
        {
            saveTimer = new Timer(AsyncSaveDelay.TotalMilliseconds);
            saveTimer.AutoReset = false;
            saveTimer.Elapsed += HandleTimerElapsed;
        }

        public void Save(XDocument document, string filename)
        {
            Assert.ParamIsNotNull(document, "document");
            Assert.ParamIsNotNull(filename, "filename");

            if (isDisposed)
            {
                // For any requests that come in after this service is disposed, that means that we are in the dispose
                // cycle and that we should honor save requests immediately
                SaveImmediately(document, filename);
            }
            else
            {
                Enqueue(new SaveRequest(document, filename));
            }
        }

        private void HandleTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Flush();
            ScheduleNextFlush();
        }

        private void ScheduleNextFlush()
        {
            lock (queueLock)
            {
                if (queue.Any())
                {
                    saveTimer.Start();
                }
            }
        }

        private void Enqueue(SaveRequest request)
        {
            lock (queueLock)
            {
                var existingItem = queue.FirstOrDefault(item => PathUtilities.PathsAreEqual(item.Filename, request.Filename));
                if (existingItem != null)
                {
                    queue.Remove(existingItem);
                }

                queue.Add(request);
                ScheduleNextFlush();
            }
        }

        private SaveRequest Dequeue()
        {
            SaveRequest result = null;

            lock (queueLock)
            {
                SaveRequest firstRequest = queue.FirstOrDefault();
                if (firstRequest != null && (isDisposed || firstRequest.Timestamp <= DateTime.Now))
                {
                    result = firstRequest;
                    queue.RemoveAt(0);
                }
            }

            return result;
        }

        private static void SaveImmediately(XDocument document, string filename)
        {
            try
            {
                string baseName = Path.GetFileName(filename);
                using (Log.PerformanceBlock(String.Format("BACKGROUND: Saving {0} file asynchronously", baseName)))
                {
                    PathUtilities.EnsureParentDirectoryExists(filename);
                    document.Save(filename);
                }
            }
            catch (Exception ex)
            {
                Log.ErrorAndBreak(ex, "Failed to serialize XML file {0} in a background thread.", filename);
            }
        }

        public void Flush()
        {
            SaveRequest request;
            while ((request = Dequeue()) != null)
            {
                SaveImmediately(request.Document, request.Filename);
            }
        }

        public void Dispose()
        {
            isDisposed = true;

            if (saveTimer != null)
            {
                saveTimer.Dispose();
                saveTimer = null;
            }

            Flush();
        }

        private class SaveRequest
        {
            public SaveRequest(XDocument document, string filename)
            {
                this.Document = document;
                this.Filename = filename;
                this.Timestamp = DateTime.Now + AsyncSaveDelay;
            }

            public string Filename { get; set; }
            public XDocument Document { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}
