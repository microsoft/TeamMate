using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Tools.TeamMate.Controls
{
    /// <summary>
    /// Interaction logic for WorkItemStateView.xaml
    /// </summary>
    public partial class WorkItemStateView : UserControl
    {
        public WorkItemStateView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty StateProperty = DependencyProperty.Register(
            "State", typeof(string), typeof(WorkItemStateView)
        );

        public string State
        {
            get { return (string)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }
    }
}
