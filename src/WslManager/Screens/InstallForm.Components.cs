using System.ComponentModel;
using System.Windows.Forms;

namespace WslManager.Screens
{
    // Components
    partial class InstallForm
    {
        private ErrorProvider errorProvider;
        private FolderBrowserDialog distroInstallDirOpenDialog;
        private BackgroundWorker rootFsDownloadWorker;

        protected override void InitializeComponents(IContainer components)
        {
            base.InitializeComponents(components);

            errorProvider = new ErrorProvider(this)
            {
                BlinkStyle = ErrorBlinkStyle.BlinkIfDifferentError,
            };
            components.Add(errorProvider);

            distroInstallDirOpenDialog = new FolderBrowserDialog()
            {
                ShowNewFolderButton = true,
                AutoUpgradeEnabled = true,
                Description = "Select a directory to install WSL distro.",
                UseDescriptionForTitle = true,
            };
            components.Add(distroInstallDirOpenDialog);

            rootFsDownloadWorker = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true,
            };
            components.Add(rootFsDownloadWorker);
        }
    }
}
