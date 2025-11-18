using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Native;
using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Shell
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class ApplicationInstance : IDisposable
    {
        /// <summary>
        /// Waits forever for a message.
        /// </summary>
        private const int MAILSLOT_WAIT_FOREVER = -1;

        private string name;
        private bool owned;

        private EventWaitHandle eventWaitHandle;
        private RegisteredWaitHandle registeredWaitHandle;
        private SafeFileHandle mailslotHandle;
        private SynchronizationContext synchronizationContext;

        public event MessageReceivedHandler MessageReceived;

        [DebuggerStepThrough]
        public static ApplicationInstance GetOrCreate(string applicationName)
        {
            try
            {
                return Create(applicationName);
            }
            catch
            {
                try
                {
                    return Get(applicationName);
                }
                catch (Exception)
                {
                    // So we couldn't create, and we couldn't get, now what?
                    throw;
                }
            }
        }

        // Step through as the event handle is expected...
        [DebuggerStepThrough]
        public static ApplicationInstance Create(string applicationName)
        {
            EventWaitHandle eventWaitHandle = null;
            SafeFileHandle mailslotHandle = null;

            try
            {
                eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, GetEventName(applicationName));

                SECURITY_ATTRIBUTES lpSecurityAttributes = CreateMailSlotSecurityAttributes();
                mailslotHandle = NativeMethods.CreateMailslot(GetMailslotName(applicationName), 0, MAILSLOT_WAIT_FOREVER, lpSecurityAttributes);
                ReleaseMailslotSecurityAttributes(lpSecurityAttributes);

                if (mailslotHandle.IsInvalid)
                {
                    // This is the most likely chance we could not open the mail slot
                    throw new Exception("Another instance of the application is already running under different privileges.");
                }
            }
            catch
            {
                if (eventWaitHandle != null)
                {
                    eventWaitHandle.Close();
                }

                if (mailslotHandle != null && !mailslotHandle.IsInvalid)
                {
                    mailslotHandle.Close();
                }

                throw;
            }

            return new ApplicationInstance(applicationName, eventWaitHandle, mailslotHandle, true);
        }

        public static ApplicationInstance Get(string applicationName)
        {
            EventWaitHandle eventWaitHandle = null;
            SafeFileHandle mailslotHandle = null;

            try
            {
                eventWaitHandle = EventWaitHandle.OpenExisting(GetEventName(applicationName));

                mailslotHandle = NativeMethods.CreateFile(GetMailslotName(applicationName),
                    (uint)FileAccess.Write, (uint)FileShare.Read, 0, (uint)FileMode.Open, (uint)FileAttributes.Normal, 0);

                if (mailslotHandle.IsInvalid)
                {
                    // This is the most likely chance we could not open the mail slot
                    throw new Exception("Another instance of the application is already running under different privileges.");
                }
            }
            catch
            {
                if (eventWaitHandle != null)
                    eventWaitHandle.Close();

                if (mailslotHandle != null && !mailslotHandle.IsInvalid)
                    mailslotHandle.Close();

                throw;
            }

            return new ApplicationInstance(applicationName, eventWaitHandle, mailslotHandle, false);
        }

        private ApplicationInstance(string name, EventWaitHandle eventWaitHandle, SafeFileHandle slotHandle, bool owned)
        {
            this.name = name;
            this.eventWaitHandle = eventWaitHandle;
            this.mailslotHandle = slotHandle;
            this.owned = owned;
            this.synchronizationContext = SynchronizationContext.Current;

            if (owned)
            {
                this.registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(eventWaitHandle, HandleEventCallback, null, Timeout.Infinite, false);

                // We can release this now, not needed anymore.
                eventWaitHandle.Close();
            }
        }

        public string ApplicationName
        {
            get { return this.name; }
        }

        public bool Owned
        {
            get { return this.owned; }
        }

        public void SendMessage(object o)
        {
            using (FileStream fs = new FileStream(mailslotHandle, FileAccess.Write, 400, false))
            {
                JsonSerializer.Serialize(fs, o);
            }

            eventWaitHandle.Set();
        }

        private object Receive()
        {
            int messageMaxSize;
            int messageSize;
            int numberOfMessages;
            int readTimeout;

            NativeMethods.GetMailslotInfo(mailslotHandle, out messageMaxSize, out messageSize, out numberOfMessages, out readTimeout);

            while (numberOfMessages > 0)
            {
                // Using SafeFileHandle to prevent slot handle from being closed
                using (FileStream fs = new FileStream(new SafeFileHandle(mailslotHandle.DangerousGetHandle(), ownsHandle: false), FileAccess.Read))
                {
                    // Sometimes we get messages of size 0, these have to be "read" and discarded from the queue to check for future messages...
                    if (messageSize > 0)
                    {
                        byte[] message = new byte[messageSize];
                        fs.ReadExactly(message, 0, messageSize);

                        return JsonSerializer.Deserialize<object>(new MemoryStream(message));
                    }
                    else
                    {
                        NativeMethods.GetMailslotInfo(mailslotHandle, out messageMaxSize, out messageSize, out numberOfMessages, out readTimeout);
                    }
                }
            }

            return null;
        }

        public void Close()
        {
            if (registeredWaitHandle != null)
            {
                registeredWaitHandle.Unregister(null);
                registeredWaitHandle = null;
            }

            if (mailslotHandle != null && !mailslotHandle.IsClosed)
            {
                mailslotHandle.Close();
                mailslotHandle = null;
            }

            if (eventWaitHandle != null)
            {
                eventWaitHandle.Close();
                eventWaitHandle = null;
            }
        }

        private void HandleEventCallback(Object state, Boolean timedOut)
        {
            try
            {
                object message = Receive();
                if (message != null)
                {
                    if (MessageReceived != null)
                    {
                        synchronizationContext.Post(delegate(object arg)
                        {
                            MessageReceived(this, message);
                        }, null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Warn("Failed to deserialize incoming application instance message: " + e);
            }
        }

        private static string GetMailslotName(string applicationName)
        {
            return @"\\.\mailslot\" + applicationName;
        }

        private static string GetEventName(string applicationName)
        {
            return applicationName;
        }


        private static SECURITY_ATTRIBUTES CreateMailSlotSecurityAttributes()
        {
            // BACKGROUND
            // If we initialized the mailslot with null security attributes, the "default" security 
            // settings would be inheritted. This is a problem if the processes attempting to read/write from the mailslot
            // are running under different privileges (e.g. admin process, vs. non admin process). If the process was first started in
            // admin mode, and the same process was started again later in non-admin mode, the two would not be able to communicate
            // through the same mailslot.
            //
            // To fix this, we need to specify a tailored security descriptor and security attributes. The descriptor allows the
            // current user to have full control over the mailslot. This allows processes running under different privileges to communicate
            // freely.

            // Initialize security access rule allowing full control to the current user
            FileSecurity security = new FileSecurity();
            var currentUser = WindowsIdentity.GetCurrent().User;
            security.AddAccessRule(new FileSystemAccessRule(currentUser, FileSystemRights.FullControl, AccessControlType.Allow));

            // Marshal this to the native world
            byte[] bytes = security.GetSecurityDescriptorBinaryForm();
            IntPtr lpSecurityDescriptor = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, lpSecurityDescriptor, bytes.Length);

            // Initialize the security attributes object with the right descriptor;
            SECURITY_ATTRIBUTES lpSecurityAttributes = new SECURITY_ATTRIBUTES();
            lpSecurityAttributes.lpSecurityDescriptor = lpSecurityDescriptor;
            return lpSecurityAttributes;
        }

        private static void ReleaseMailslotSecurityAttributes(SECURITY_ATTRIBUTES securityAttributes)
        {
            if (securityAttributes.lpSecurityDescriptor != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(securityAttributes.lpSecurityDescriptor);
                securityAttributes.lpSecurityDescriptor = IntPtr.Zero;
            }
        }


        private bool disposed;

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                Close();

                // Note disposing has been done.
                disposed = true;
            }
        }

        ~ApplicationInstance()
        {
            Dispose(false);
        }
    }

    public delegate void MessageReceivedHandler(ApplicationInstance instance, object message);
}
