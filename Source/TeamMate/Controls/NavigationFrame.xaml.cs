using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Controls;
using Microsoft.Internal.Tools.TeamMate.Services;
using Microsoft.Internal.Tools.TeamMate.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Internal.Tools.TeamMate.Controls
{
    /// <summary>
    /// Interaction logic for NavigationFrame.xaml
    /// </summary>
    public partial class NavigationFrame : UserControl
    {
        public NavigationFrame()
        {
            InitializeComponent();
            this.DataContextChanged += HandleDataContextChanged;
        }

        public static readonly DependencyProperty CurrentViewProperty = DependencyProperty.Register(
            "CurrentView", typeof(object), typeof(NavigationFrame)
        );

        public FrameworkElement CurrentView
        {
            get { return (FrameworkElement)GetValue(CurrentViewProperty); }
            set { SetValue(CurrentViewProperty, value); }
        }

        private void HandleDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            NavigationViewModel oldValue = e.OldValue as NavigationViewModel;
            NavigationViewModel newValue = e.NewValue as NavigationViewModel;

            if (oldValue != null)
            {
                oldValue.PropertyChanged -= HandleModelPropertyChanged;
            }

            if (newValue != null)
            {
                newValue.PropertyChanged += HandleModelPropertyChanged;
            }

            InvalidateCurrentView();
        }

        private void HandleModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Page")
            {
                InvalidateCurrentView();
                MetroAnimations.SlideUp(pageContainer);
            }
        }

        private NavigationViewModel ViewModel
        {
            get { return this.DataContext as NavigationViewModel; }
        }

        public ViewService ViewService { get; set; }

        private void InvalidateCurrentView()
        {
            FrameworkElement currentView = null;

            NavigationViewModel viewModel = ViewModel;
            if (viewModel != null && viewModel.Page != null)
            {
                if (this.ViewService != null)
                {
                    currentView = this.ViewService.CreateView(viewModel.Page);
                }
            }

            this.CurrentView = currentView;
        }
    }
}
