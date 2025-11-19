using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using System;
using System.Windows.Media;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class ToastViewModel : ViewModelBase
    {
        private ImageSource photo;
        private string title;
        private string description;
        private ImageSource icon;

        public event EventHandler Activated;

        public ToastViewModel()
        {
        }

        public ToastViewModel(string title)
        {
            this.title = title;
        }

        public ImageSource Photo
        {
            get { return this.photo; }
            set { SetProperty(ref this.photo, value); }
        }

        public string Title
        {
            get { return this.title; }
            set { SetProperty(ref this.title, value); }
        }

        public string Description
        {
            get { return this.description; }
            set { SetProperty(ref this.description, value); }
        }

        public ImageSource Icon
        {
            get { return this.icon; }
            set { SetProperty(ref this.icon, value); }
        }

        private string activationArguments;

        public string ActivationArguments
        {
            get { return this.activationArguments; }
            set { SetProperty(ref this.activationArguments, value); }
        }

        private Brush background;

        public Brush Background
        {
            get { return this.background; }
            set { SetProperty(ref this.background, value); }
        }

        private Brush foreground;

        public Brush Foreground
        {
            get { return this.foreground; }
            set { SetProperty(ref this.foreground, value); }
        }

        public void Activate()
        {
            Activated?.Invoke(this, EventArgs.Empty);
        }
    }
}
