using System;
using System.ServiceModel;

namespace Microsoft.Tools.TeamMate.Foundation.ServiceModel
{
    /// <summary>
    /// Generic helper class for a WCF service proxy. The class simplifies the management of WCF proxy classes
    /// when they are faulted.
    /// </summary>
    /// <typeparam name="TProxy">The type of WCF service proxy to wrap.</typeparam>
    /// <typeparam name="TChannel">The type of WCF service interface to wrap.</typeparam>
    public class WcfClientProxy<TProxy, TChannel> : IDisposable
        where TProxy : ClientBase<TChannel>
        where TChannel : class
    {
        /// <summary>
        /// Private instance of the WCF service proxy.
        /// </summary>
        private WeakReference<TProxy> weakProxy;

        private Func<TProxy> createProxy;

        /// <summary>
        /// Constructs an instance.
        /// </summary>
        public WcfClientProxy(Func<TProxy> createProxy)
        {
            this.createProxy = createProxy;
        }

        /// <summary>
        /// Gets the WCF service proxy wrapped by this instance.
        /// </summary>
        public TProxy GetInstance()
        {
            var proxy = TryGetProxy();

            if (proxy != null && !IsReady(proxy.State))
            {
                Shutdown(proxy);
                proxy = null;
                this.weakProxy = null;
            }

            if (proxy == null)
            {
                proxy = createProxy();
                this.weakProxy = new WeakReference<TProxy>(proxy);
                proxy.Open();
            }

            return proxy;
        }

        /// <summary>
        /// Tries to get the instance of the weakly reference proxy.
        /// </summary>
        /// <returns></returns>
        private TProxy TryGetProxy()
        {
            TProxy proxy = null;

            var weakProxy = this.weakProxy;
            if (weakProxy != null)
            {
                weakProxy.TryGetTarget(out proxy);
            }

            return proxy;
        }

        /// <summary>
        /// Determines whether the specified state indicates that the proxy is ready for use.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns><c>true</c> if the proxy is ready for use</returns>
        private static bool IsReady(CommunicationState state)
        {
            return state == CommunicationState.Created || state == CommunicationState.Opening || state == CommunicationState.Opened;
        }

        /// <summary>
        /// Disposes of this instance.
        /// </summary>
        public void Dispose()
        {
            if (this.weakProxy != null)
            {
                try
                {
                    var proxy = TryGetProxy();
                    if (proxy != null)
                    {
                        Shutdown(proxy);
                    }
                }
                finally
                {
                    this.weakProxy = null;
                }
            }
        }

        /// <summary>
        /// Shuts down the specified proxy, invoking the appropriate method.
        /// </summary>
        /// <param name="proxy">The proxy.</param>
        private static void Shutdown(TProxy proxy)
        {
            try
            {
                if (proxy.State != CommunicationState.Faulted)
                {
                    proxy.Close();
                }
                else
                {
                    proxy.Abort();
                }
            }
            catch (Exception e)
            {
                proxy.Abort();

                if (!(e is CommunicationException || e is TimeoutException))
                {
                    throw;
                }
            }
        }
    }
}
