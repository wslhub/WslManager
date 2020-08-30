using System.ComponentModel;
using System.Windows.Forms;

namespace WslManager.Screens
{
    // Components
    partial class RestoreForm
    {
        private ErrorProvider errorProvider;
        private OpenFileDialog distroBackupFileOpenDialog;
        private FolderBrowserDialog distroRestoreDirOpenDialog;

        protected override void InitializeComponents(IContainer components)
        {
            base.InitializeComponents(components);

            errorProvider = new ErrorProvider(this)
            {
                BlinkStyle = ErrorBlinkStyle.BlinkIfDifferentError,
            };
            components.Add(errorProvider);

            distroBackupFileOpenDialog = new OpenFileDialog()
            {
                Title = "Open WSL Backup File",
                SupportMultiDottedExtensions = true,
                DefaultExt = ".tar",
                Filter = "Tape Archive File|*.tar",
                AutoUpgradeEnabled = true,
            };
            components.Add(distroBackupFileOpenDialog);

            distroRestoreDirOpenDialog = new FolderBrowserDialog()
            {
                ShowNewFolderButton = true,
                AutoUpgradeEnabled = true,
                Description = "Select a directory to restore WSL distro.",
                UseDescriptionForTitle = true,
            };
            components.Add(distroRestoreDirOpenDialog);
        }
    }
}
