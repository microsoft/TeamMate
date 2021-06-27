// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Native;
using Microsoft.Tools.TeamMate.Foundation.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Forms
{
    /// <summary>
    /// A control that can display the preview for a given file.
    /// </summary>
    /// <remarks>
    /// Based on Stephen Toub's post: http://msdn.microsoft.com/en-us/magazine/cc163487.aspx
    /// </remarks>
    public partial class NativePreviewControl : UserControl
    {
        private IPreviewHandler previewHandler;
        private ComStreamAdapter lastOpenedStream;
        private string filePath;

        public event EventHandler<LoadEventArgs> LoadFinished;

        public NativePreviewControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The path of the file to be previewed. Set to <c>null</c> to reset the control.
        /// </summary>
        public string FilePath
        {
            get { return this.filePath; }

            set
            {
                if (!String.Equals(filePath, value, StringComparison.OrdinalIgnoreCase))
                {
                    this.filePath = value;

                    if (this.filePath != null)
                    {
                        if (!IsInDesignMode)
                        {
                            InvalidatePreview();
                        }
                    }
                    else
                    {
                        DisposeHandlerAndStream();
                    }
                }
            }
        }

        private bool IsInDesignMode
        {
            get
            {
                return (this.Site != null && this.Site.DesignMode);
            }
        }


        /// <summary>
        /// Determines if a given file can be previewed.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool CanPreview(string filePath)
        {
            Guid? id;
            string description;
            return TryGetPreviewHandler(filePath, out id, out description);
        }

        public static string GetPreviewHandlerDescription(string filePath)
        {
            Guid? id;
            string description;
            TryGetPreviewHandler(filePath, out id, out description);

            return description;
        }

        private static bool TryGetPreviewHandler(string filePath, out Guid? previewHandlerId, out string previewHandlerDescription)
        {
            previewHandlerId = null;
            previewHandlerDescription = null;

            /*
            // KLUDGE: http://go4answers.webhost4life.com/Example/preview-handler-msg-files-not-working-80634.aspx
            // Outlook msg files do not preview well on 64-bit... So we kludge this to see what is going on.
            // See also http://social.msdn.microsoft.com/Forums/eu/outlookdev/thread/040d8f15-56d9-4707-a652-48f4e07e8db3
            // for a potential workaround.
            string extension = Path.GetExtension(filePath);
            if (String.Equals(extension, ".msg", StringComparison.OrdinalIgnoreCase) &&
                Environment.Is64BitOperatingSystem)
            {
                return false;
            }
             */

            if (File.Exists(filePath))
            {
                FileTypeInfo info = FileTypeRegistry.Instance.GetInfoFromPath(filePath);
                if (info != null)
                {
                    previewHandlerId = info.PreviewHandlerId;
                    previewHandlerDescription = info.PreviewHandlerDescription;
                }
            }

            return (previewHandlerId != null);
        }

        private void InvalidatePreview()
        {
            IntPtr myHandle = this.Handle;

            // TODO: Avoid queuing multiple requests

            // TODO: Temporarily moved this out of a worker thread... THere are cleanup exceptions being caused by that I think, we need to see
            // what the deal is... Ideally, I want to move this back to its worker thread...
            bool initialized = false;
            Exception error = null;

            try
            {
                initialized = FindAndSetupPreviewHandler(myHandle);
                if (!initialized)
                {
                    throw new Exception("Preview is not available for this file.");
                }
            }
            catch (Exception e)
            {
                error = e;
            }

            if (LoadFinished != null)
            {
                this.BeginInvoke((Action)delegate()
                {
                    if (LoadFinished != null)
                    {
                        LoadEventArgs eventArgs = (error != null) ? new LoadEventArgs(error) : new LoadEventArgs();
                        LoadFinished(this, eventArgs);
                    }
                });
            }

        }

        private bool FindAndSetupPreviewHandler(IntPtr targetHandle)
        {
            bool initialized = false;

            try
            {
                DisposeHandlerAndStream();

                Guid? handlerId;
                string description;

                if (TryGetPreviewHandler(filePath, out handlerId, out description))
                {
                    Type comType = Type.GetTypeFromCLSID(handlerId.Value);

                    // TODO: If I iterate quickly through preview handlers, I sometimes get this normal.dotm message for word.
                    // Office preview handlers spawn office processes with the -Embedding flag. 
                    // http://www.pcreview.co.uk/forums/word-has-encountered-problem-t3949879.html
                    // http://social.technet.microsoft.com/Forums/en/itprovistasetup/thread/5b564970-23a7-40a5-b25e-bf3db08f060a
                    // We might need to kill the process if appropriate? How can we tell when we should kill it?

                    // Create an instance of the preview handler
                    object comObject = Activator.CreateInstance(comType);
                    previewHandler = comObject as IPreviewHandler;

                    if (previewHandler != null)
                    {
                        IInitializeWithFile initializeWithFile = previewHandler as IInitializeWithFile;
                        IInitializeWithStream initializeWithStream = previewHandler as IInitializeWithStream;
                        IInitializeWithItem initializeWithItem = previewHandler as IInitializeWithItem;

                        if (initializeWithFile != null)
                        {
                            initializeWithFile.Initialize(filePath, 0);
                            initialized = true;
                        }
                        else if (initializeWithStream != null)
                        {
                            ComStreamAdapter stream = new ComStreamAdapter(File.OpenRead(filePath));
                            lastOpenedStream = stream;
                            initializeWithStream.Initialize(stream, 0);
                            initialized = true;
                        }
                        else if (initializeWithItem != null)
                        {
                            // Added this to support e.g. preview of .msg files and others... There are problems though, not quite working:
                            // http://www.brad-smith.info/blog/archives/183
                            // http://social.msdn.microsoft.com/Forums/en/outlookdev/thread/040d8f15-56d9-4707-a652-48f4e07e8db3
                            // http://social.msdn.microsoft.com/Forums/en-US/netfxbcl/thread/91e4ee2d-344d-4bb9-9798-5a8194b523a1
                            // http://int.social.msdn.microsoft.com/Forums/en/outlookdev/thread/c38a53f5-36d0-4261-9186-e276ad12ac0d
                            IShellItem shellItem;
                            NativeMethods.SHCreateItemFromParsingName(filePath, IntPtr.Zero, Marshal.GenerateGuidForType(typeof(IShellItem)), out shellItem);
                            initializeWithItem.Initialize(shellItem, 0);
                            initialized = true;
                        }

                        if (initialized)
                        {
                            RECT rect = SizeAsRect;
                            previewHandler.SetWindow(targetHandle, ref rect);
                            previewHandler.DoPreview();
                        }
                        else
                        {
                            DisposeHandlerAndStream();
                        }
                    }
                }
            }
            catch (Exception)
            {
                DisposeHandlerAndStream();
                throw;
            }

            return initialized;
        }

        private RECT SizeAsRect
        {
            get
            {
                return new RECT(0, 0, this.Width, this.Height);
            }
        }

        private void OnResize(object sender, EventArgs e)
        {
            if (previewHandler != null)
            {
                RECT rect = SizeAsRect;

                try
                {
                    previewHandler.SetRect(ref rect);
                }
                catch (Exception ex)
                {
                    Log.ErrorAndBreak(ex);
                }
            }
        }

        private void DisposeHandlerAndStream()
        {
            if (previewHandler != null)
            {
                try
                {
                    previewHandler.Unload();
                }
                catch (Exception ex)
                {
                    Log.ErrorAndBreak(ex);
                }

                previewHandler = null;
            }

            if (lastOpenedStream != null)
            {
                lastOpenedStream.Dispose();
                lastOpenedStream = null;
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeHandlerAndStream();

                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }

    public class LoadEventArgs : EventArgs
    {
        public LoadEventArgs()
        {
            this.Success = true;
        }

        public LoadEventArgs(Exception error)
        {
            this.Error = error;
        }

        public bool Success { get; private set; }
        public Exception Error { get; private set; }
    }
}
