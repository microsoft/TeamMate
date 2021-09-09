using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Common.TokenStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using IssuedToken = Microsoft.VisualStudio.Services.Common.IssuedToken;

namespace Microsoft.Tools.TeamMate.Services
{
    class VstsClientCredentialCachingStorage : VssClientCredentialStorage
    {
        private const string TokenExpirationKey = "ExpirationDateTime";
        private double TokenLeaseInSeconds;

        public VstsClientCredentialCachingStorage(string storageKind = "VssApp", string storageNamespace = "VisualStudio", double tokenLeaseInSeconds = 86400)
            : base(storageKind, storageNamespace)
        {
            this.TokenLeaseInSeconds = tokenLeaseInSeconds;
        }

        public override void RemoveToken(Uri serverUrl, IssuedToken token)
        {
            this.RemoveToken(serverUrl, token, false);
        }

        public void RemoveToken(Uri serverUrl, IssuedToken token, bool force)
        {
            // Bypassing this allows the token to be stored in local
            // cache. Token is removed if lease is expired.
            if (force || token != null && this.IsTokenExpired(token))
            {
                base.RemoveToken(serverUrl, token);
            }
        }

        public override IssuedToken RetrieveToken(Uri serverUrl, VssCredentialsType credentialsType)
        {
            var token = base.RetrieveToken(serverUrl, credentialsType);

            if (token != null)
            {
                bool expireToken = this.IsTokenExpired(token);
                if (expireToken)
                {
                    base.RemoveToken(serverUrl, token);
                    token = null;
                }
                else
                {
                    // if retrieving the token before it is expired,
                    // refresh the lease period.
                    this.RefreshLeaseAndStoreToken(serverUrl, token);
                    token = base.RetrieveToken(serverUrl, credentialsType);
                }
            }

            return token;
        }

        public override void StoreToken(Uri serverUrl, IssuedToken token)
        {
            this.RefreshLeaseAndStoreToken(serverUrl, token);
        }

        public void ClearAllTokens(Uri url = null)
        {
            IEnumerable<VssToken> tokens = this.TokenStorage.RetrieveAll(base.TokenKind).ToList();

            if (url != default(Uri))
            {
                tokens = tokens.Where(t => StringComparer.InvariantCultureIgnoreCase.Compare(t.Resource, url.ToString().TrimEnd('/')) == 0);
            }

            foreach (var token in tokens)
            {
                this.TokenStorage.Remove(token);
            }
        }

        private void RefreshLeaseAndStoreToken(Uri serverUrl, IssuedToken token)
        {
            if (token.Properties == null)
            {
                token.Properties = new Dictionary<string, string>();
            }

            token.Properties[TokenExpirationKey] = this.GetNewExpirationDateTime().ToString();

            base.StoreToken(serverUrl, token);
        }

        private DateTime GetNewExpirationDateTime()
        {
            var now = DateTime.Now;

            // Ensure we don't overflow the max DateTime value
            var lease = Math.Min((DateTime.MaxValue - now.Add(TimeSpan.FromSeconds(1))).TotalSeconds, this.TokenLeaseInSeconds);

            // ensure we don't have negative leases
            lease = Math.Max(lease, 0);

            return now.AddSeconds(lease);
        }

        private bool IsTokenExpired(IssuedToken token)
        {
            bool expireToken = true;

            if (token != null && token.Properties.ContainsKey(TokenExpirationKey))
            {
                try
                {
                    DateTime expiration = Convert.ToDateTime(token.Properties[TokenExpirationKey]);

                    expireToken = DateTime.Compare(DateTime.Now, expiration) >= 0;
                }
                catch { }
            }

            return expireToken;
        }
    }
}
