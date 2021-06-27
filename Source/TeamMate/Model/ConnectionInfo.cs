using Microsoft.Tools.TeamMate.Foundation.ComponentModel;
using System;

namespace Microsoft.Tools.TeamMate.Model
{
    public class ConnectionInfo : ObservableObjectBase
    {
        private ProjectInfo project;
        private ConnectionState connectionState = ConnectionState.Disconnected;
        private Exception connectionError;
        private bool isConnected;

        public ConnectionInfo()
        {
            InvalidateState();
        }

        public ProjectInfo Project 
        { 	
            get { return this.project; }
            set { SetProperty(ref this.project, value); }
        }

        public ConnectionState ConnectionState 
        { 	
            get { return this.connectionState; }
            set 
            {
                if (SetProperty(ref this.connectionState, value))
                {
                    InvalidateState();
                }
            }
        }

        private void InvalidateState()
        {
            IsDisconnected = (ConnectionState == ConnectionState.Disconnected);
            IsConnected = (ConnectionState == ConnectionState.Connected);
            IsConnecting = (ConnectionState == ConnectionState.Connecting);
            IsConnectionFailed = (ConnectionState == ConnectionState.ConnectionFailed);
        }

        private bool isDisconnected;

        public bool IsDisconnected
        {
            get { return this.isDisconnected; }
            private set { SetProperty(ref this.isDisconnected, value); }
        }

        private bool isConnectionFailed;

        public bool IsConnectionFailed
        {
            get { return this.isConnectionFailed; }
            private set { SetProperty(ref this.isConnectionFailed, value); }
        }

        public bool IsConnected
        {
            get { return this.isConnected; }
            private set { SetProperty(ref this.isConnected, value); }
        }

        private bool isConnecting;

        public bool IsConnecting
        {
            get { return this.isConnecting; }
            private set { SetProperty(ref this.isConnecting, value); }
        }

        public Exception ConnectionError
        {
            get { return this.connectionError; }
            set { SetProperty(ref this.connectionError, value); }
        }
    }

    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        ConnectionFailed
    }
}
