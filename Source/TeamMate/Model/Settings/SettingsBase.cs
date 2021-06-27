using Microsoft.Tools.TeamMate.Foundation;
using Microsoft.Tools.TeamMate.Foundation.ComponentModel;
using System;

namespace Microsoft.Tools.TeamMate.Model.Settings
{
    public abstract class SettingsBase : ObservableObjectBase
    {
        private bool updatesOccurredWhileDeferring;

        public event EventHandler SettingsChanged;
        private DeferredAction deferredSettingsChanged;

        public SettingsBase()
        {
            this.deferredSettingsChanged = new DeferredAction(DeferredRaiseSettingsChanged);
        }

        public IDisposable DeferUpdate()
        {
            return this.deferredSettingsChanged.Acquire();
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            RaiseSettingsChanged();
        }

        private void RaiseSettingsChanged()
        {
            if (!this.deferredSettingsChanged.IsDeferring)
            {
                SettingsChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                updatesOccurredWhileDeferring = true;
            }
        }

        private void DeferredRaiseSettingsChanged()
        {
            if (updatesOccurredWhileDeferring)
            {
                updatesOccurredWhileDeferring = false;
                RaiseSettingsChanged();
            }
        }
    }
}
