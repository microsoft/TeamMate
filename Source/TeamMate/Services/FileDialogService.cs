using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Windows;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using System.Windows;
using DialogResult = System.Windows.Forms.DialogResult;
using FileDialog = System.Windows.Forms.FileDialog;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;

namespace Microsoft.Tools.TeamMate.Services
{
    public class FileDialogService
    {
        public FileDialogResult ShowFileDialog(FileDialogOptions options)
        {
            Assert.ParamIsNotNull(options, "options");

            return ShowFileDialog(null, options);
        }

        public FileDialogResult ShowFileDialog(ViewModelBase ownerViewModel, FileDialogOptions options)
        {
            Assert.ParamIsNotNull(options, "options");

            // Using the WinForms dialogs as the Microsoft.Win32 ones don't center on the owner nicely (known bug)
            FileDialog dialog = (options.OpenDialog) ? (FileDialog) new OpenFileDialog() : (FileDialog) new SaveFileDialog();

            dialog.Title = options.Title;
            dialog.CheckFileExists = options.CheckFileExists;
            dialog.DereferenceLinks = options.DereferenceLinks;
            dialog.FileName = options.FileName;
            dialog.Filter = options.Filter;

            if (dialog is OpenFileDialog)
            {
                OpenFileDialog openFileDialog = (OpenFileDialog)dialog;
                openFileDialog.Multiselect = options.Multiselect;
            }

            DialogResult dialogResult;
            Window owner = View.GetWindow(ownerViewModel);
            if (owner != null)
            {
                dialogResult = dialog.ShowDialog(owner.GetWinFormsWin32Window());

                // Give focus back to this Window, sometimes it gets confused?
                owner.Focus();
            }
            else
            {
                dialogResult = dialog.ShowDialog();
            }

            FileDialogResult result = new FileDialogResult();
            result.Success = (dialogResult == System.Windows.Forms.DialogResult.OK);
            if (result.Success)
            {
                result.FileName = dialog.FileName;
                result.FileNames = dialog.FileNames;
            }

            return result;
        }

        public string ShowFolderDialog(ViewModelBase ownerViewModel, FileDialogOptions options)
        {
            string parentPath = null;

            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = options.Title;

            System.Windows.Forms.DialogResult result;

            Window owner = View.GetWindow(ownerViewModel);
            if (owner != null)
            {
                result = dialog.ShowDialog(owner.GetWinFormsWin32Window());
            }
            else
            {
                result = dialog.ShowDialog();
            }

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                parentPath = dialog.SelectedPath;
            }

            return parentPath;
        }
    }

    public class FileDialogOptions
    {
        public bool OpenDialog { get; set; }
        public string Title { get; set; }
        public bool CheckFileExists { get; set; }
        public bool DereferenceLinks { get; set; }
        public bool Multiselect { get; set; }
        public string FileName { get; set; }
        public string Filter { get; set; }
    }

    public class FileDialogResult
    {
        // TODO: Make setters private
        public bool Success { get; set; }
        public string FileName { get; set; }
        public string[] FileNames { get; set; }
    }
}
