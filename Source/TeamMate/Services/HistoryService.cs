using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Model;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Xml.Linq;

namespace Microsoft.Tools.TeamMate.Services
{
    public class HistoryService
    {
        private ApplicationHistory history;
        private string historyFile;


        [Import]
        public AsyncWriterService AsyncWriterService { get; set; }

        private string HistoryFile
        {
            get
            {
                if (this.historyFile == null)
                {
                    this.historyFile = Path.Combine(TeamMateApplicationInfo.DataDirectory, "History.xml");
                }

                return this.historyFile;
            }
        }


        public ApplicationHistory History
        {
            get
            {
                if (this.history == null)
                {
                    this.history = LoadHistory();
                    this.history.PropertyChanged += HandleHistoryChanged;
                }

                return this.history;
            }
        }

        private void HandleHistoryChanged(object sender, PropertyChangedEventArgs e)
        {
            FlushHistory();
        }

        public void FlushHistory()
        {
            XDocument doc = history.Write();
            this.AsyncWriterService.Save(doc, HistoryFile);
        }


        private ApplicationHistory LoadHistory()
        {
            TeamMateApplicationInfo.AssertDataDirectoryAccessIsAllowed();

            ApplicationHistory history = new ApplicationHistory();

            try
            {
                if (File.Exists(HistoryFile))
                {
                    history = ApplicationHistory.Load(HistoryFile);
                }
            }
            catch (Exception e)
            {
                Log.WarnAndBreak(e, "Failed to read history from file {0}", HistoryFile);
            }

            return history;
        }
    }
}
