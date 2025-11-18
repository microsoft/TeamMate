using Microsoft.Tools.TeamMate.Controls;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Windows;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Windows
{
    /// <summary>
    /// Interaction logic for WorkItemQueryPickerDialog.xaml
    /// </summary>
    [SupportedOSPlatform("windows10.0.19041.0")]
    public partial class WorkItemQueryPickerDialog : Window
    {
        public WorkItemQueryPickerDialog()
        {
            InitializeComponent();

            this.picker.ItemActivated += HandleItemActivated;
        }

        public QueryPickerControl Picker => this.picker;

        public QueryHierarchyItem Selection
        {
            get
            {
                return picker.SelectedQuery;
            }
        }

        private void HandleItemActivated(object sender, EventArgs e)
        {
            if (Selection != null)
            {
                this.DialogResult = true;
            }
        }
    }
}
