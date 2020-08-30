using System;
using System.Windows.Forms;
using WslManager.Extensions;
using WslManager.ViewModels;

namespace WslManager.Screens
{
    // Distro features
    partial class MainForm
    {
        private void Feature_LaunchDistro(object sender, EventArgs e)
        {
            var targetItem = GetSelectedDistroBySender(sender);

            if (targetItem == null)
                return;

            var process = WslHelpers.CreateLaunchSpecificDistroProcess(targetItem.DistroName);
            var result = process.Start();
        }

        private void Feature_TerminateDistro(object sender, EventArgs e)
        {
            var targetItem = GetSelectedDistroBySender(sender);

            if (targetItem == null)
                return;

            var process = WslHelpers.CreateTerminateSpecificDistroProcess(targetItem.DistroName);
            var result = process.Start();
        }

        private void Feature_OpenDistroFileSystem(object sender, EventArgs e)
        {
            var targetItem = GetSelectedDistroBySender(sender);

            if (targetItem == null)
                return;

            var process = WslHelpers.CreateLaunchSpecificDistroExplorerProcess(targetItem.DistroName);
            var result = process.Start();
        }

        private void Feature_BackupDistro(object sender, EventArgs e)
        {
            if (backupWorker.IsBusy)
            {
                MessageBox.Show(
                    this, "Already one or more backup in progress. Please try again later.",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);

                return;
            }

            var targetItem = GetSelectedDistroBySender(sender);

            if (targetItem == null)
                return;

            using var saveFileDialog = new SaveFileDialog()
            {
                Title = $"Backup {targetItem.DistroName}",
                SupportMultiDottedExtensions = true,
                Filter = "Tape Archive File|*.tar",
                DefaultExt = ".tar",
                FileName = $"backup-{targetItem.DistroName.ToLowerInvariant()}-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.tar",
            };

            if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
                return;

            backupWorker.RunWorkerAsync(new DistroBackupRequest()
            {
                DistroName = targetItem.DistroName,
                SaveFilePath = saveFileDialog.FileName,
            });
        }

        private void Feature_UnregisterDistro(object sender, EventArgs e)
        {
            var targetItem = GetSelectedDistroBySender(sender);

            if (targetItem == null)
                return;

            if (MessageBox.Show(this, $"Really unregister `{targetItem.DistroName}` distro? This cannot be undone.",
                Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;

            var process = WslHelpers.CreateUnregisterDistroProcess(targetItem.DistroName);
            process.Start();
            process.WaitForExit();
            AppContext.RefreshDistroList();
        }

        private void Feature_SetAsDefaultDistro(object sender, EventArgs e)
        {
            var targetItem = GetSelectedDistroBySender(sender);

            if (targetItem == null)
                return;

            var process = WslHelpers.CreateSetAsDefaultProcess(targetItem.DistroName);
            process.Start();
            process.WaitForExit();
            AppContext.RefreshDistroList();
        }

        private void Feature_RestoreDistro(object sender, EventArgs e)
        {
            if (restoreWorker.IsBusy)
            {
                MessageBox.Show(
                    this, "Already one or more restore in progress. Please try again later.",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);

                return;
            }

            using var dialog = new RestoreForm();

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            var restoreRequest = dialog.ViewModel;

            if (restoreRequest == null)
                return;

            restoreWorker.RunWorkerAsync(restoreRequest);
        }

        private void Feature_RefreshDistroList(object sender, EventArgs e)
        {
            AppContext.RefreshDistroList();
        }
    }
}
