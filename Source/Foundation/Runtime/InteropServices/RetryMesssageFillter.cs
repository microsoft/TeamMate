// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Native;
using System;
using System.Diagnostics;

namespace Microsoft.Tools.TeamMate.Foundation.Runtime.InteropServices
{
    /// <summary>
    /// A COM message filter implementation used to retry rejected COM calls to a COM
    /// Server when the server is busy.
    /// </summary>
    public class RetryMessageFilter : IOleMessageFilter
    {
        // Some constants returned by the COM Message Filter
        private const int SERVERCALL_ISHANDLED = 0;
        private const int SERVERCALL_RETRYLATER = 2;
        private const int PENDINGMSG_WAITDEFPROCESS = 2;
        private const int RETRY_IMMEDIATELY = 99;
        private const int CANCEL_CALL = -1;

        private TimeSpan retryTimeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Registers the message filter as the current global instance that will be invoked to handle concurrency issues.
        /// </summary>
        /// <returns>The previously registerd global instance. This can be used with Unregister() to restore that instance.</returns>
        public IOleMessageFilter Register()
        {
            IOleMessageFilter oldFilter = null;
            NativeMethods.CoRegisterMessageFilter(this, out oldFilter);
            return oldFilter;
        }

        /// <summary>
        /// Unregisters this filter from the global handler.
        /// </summary>
        /// <returns><c>true</c> if the filter was successfully unregistered; <c>false</c> if the filter
        /// didn't need to be unregistered as it was not the currently registered one.</returns>
        public bool Unregister()
        {
            return Unregister(null);
        }

        /// <summary>
        /// Unregisters the global message filter instance.
        /// </summary>
        /// <returns><c>true</c> if the filter was successfully unregistered and the given filter was restored; 
        /// <c>false</c> if the filter didn't need to be unregistered as it was not the currently registered one,
        /// and the other filter was not restored.</returns>
        public bool Unregister(IOleMessageFilter restored)
        {
            IOleMessageFilter oldFilter = null;
            NativeMethods.CoRegisterMessageFilter(restored, out oldFilter);

            if (oldFilter != this)
            {
                // Shoot... We unregistered a global filter that does not match this instance.
                // Revert that, and return false to indicate a failure...
                IOleMessageFilter dummy = null;
                NativeMethods.CoRegisterMessageFilter(oldFilter, out dummy);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets or sets the retry timeout (in milliseconds) for COM Calls. If the value is less than 0,
        /// then retries will happen indefinitely. The default timeout is <b>30000 ms</b>.
        /// </summary>
        /// <value>The retry timeout (in milliseconds).</value>
        public TimeSpan RetryTimeout
        {
            get { return retryTimeout; }
            set { retryTimeout = value; }
        }

        #region Interface Implementation

        #endregion

        /// <summary>
        /// This client-based method gives the application an opportunity to display a dialog box so
        /// the user can retry or cancel the call, or switch to the task identified by hTaskCallee.
        /// </summary>
        /// <param name="hTaskCallee">Handle of the server task that rejected the call.</param>
        /// <param name="dwTickCount">Number of elapsed ticks since the call was made.</param>
        /// <param name="dwRejectType">Specifies either SERVERCALL_REJECTED or SERVERCALL_RETRYLATER, as returned by the object application.</param>
        /// <returns>
        /// 	<para>-1: The call should be canceled. COM then returns RPC_E_CALL_REJECTED from the original method call.</para>
        /// 	<para>Value &gt;= 0 and &lt;100: The call is to be retried immediately.</para>
        /// 	<para>Value &gt;= 100: COM will wait for this many milliseconds and then retry the call.</para>
        /// </returns>
        int IOleMessageFilter.RetryRejectedCall(IntPtr hTaskCallee, int dwTickCount, int dwRejectType)
        {
            if (dwRejectType == SERVERCALL_RETRYLATER)
            {
                // Thread call was rejected, so try again.
                if (retryTimeout == TimeSpan.MaxValue || dwTickCount <= retryTimeout.Ticks)
                {
                    Debug.WriteLine("Retrying rejected call...");
                    return RETRY_IMMEDIATELY;
                }
            }

            return CANCEL_CALL;
        }

        /// <summary>
        /// This method is an object-based method that provides the ability to filter or reject incoming calls
        /// (or call backs) to an object or a process. This method is called prior to each method
        /// invocation originating outside the current process.
        /// </summary>
        /// <param name="dwCallType">Kind of incoming call that has been received.</param>
        /// <param name="hTaskCaller">Handle of the task calling this task.</param>
        /// <param name="dwTickCount">Elapsed tick count since the outgoing call was made if dwCallType is not CALLTYPE_TOPLEVEL.</param>
        /// <param name="lpInterfaceInfo">Pointer to an INTERFACEINFO structure, which identifies the object, the interface, and the method making the call.</param>
        /// <returns>
        /// 	<para>SERVERCALL_ISHANDLED: The application might be able to process the call.</para>
        /// 	<para>SERVERCALL_REJECTED: The application cannot handle the call due to an unforeseen problem,
        /// such as network unavailability, or if it is in the process of terminating.</para>
        /// 	<para>SERVERCALL_RETRYLATER: The application cannot handle the call at this time.</para>
        /// </returns>
        int IOleMessageFilter.HandleInComingCall(int dwCallType, IntPtr hTaskCaller,
            int dwTickCount, IntPtr lpInterfaceInfo)
        {
            return SERVERCALL_ISHANDLED;
        }

        /// <summary>
        /// This client-based method is called by COM when a Windows message appears in a COM application's
        /// message queue while the application is waiting for a reply to a remote call.
        /// </summary>
        /// <param name="hTaskCallee">Task handle of the called application that has not yet responded.</param>
        /// <param name="dwTickCount">Number of ticks since the call was made.</param>
        /// <param name="dwPendingType">Type of call made during which a message or event was received.</param>
        int IOleMessageFilter.MessagePending(IntPtr hTaskCallee, int dwTickCount, int dwPendingType)
        {
            return PENDINGMSG_WAITDEFPROCESS;
        }
    }
}
