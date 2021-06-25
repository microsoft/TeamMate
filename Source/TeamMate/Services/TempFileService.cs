using Microsoft.Internal.Tools.TeamMate.Foundation.IO;
using System;

namespace Microsoft.Internal.Tools.TeamMate.Services
{
    public class TempFileService : IDisposable
    {
        private TempDirectory tempDirectory;

        public TempDirectory TempDirectory
        {
            get
            {
                if (this.tempDirectory == null)
                {
                    this.tempDirectory = TempDirectory.CreateForProcess();
                }

                return this.tempDirectory;
            }
        }

        public TempDirectory CreateTempSubDirectory()
        {
            return TempDirectory.CreateTempSubDirectory();
        }

        public void Dispose()
        {
            if (this.tempDirectory != null)
            {
                this.tempDirectory.TryDelete();
                this.tempDirectory = null;
            }
        }
    }
}
