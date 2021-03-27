using System;
using System.Linq;
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
            process.Start();
            process.WaitForExit();

            AppContext.RefreshDistroList();
        }

        private void Feature_RunAsDistro(object sender, EventArgs e)
        {
            var targetItem = GetSelectedDistroBySender(sender);

            if (targetItem == null)
                return;

            using var dialog = new RunAsForm(new DistroRunAsRequest()
            {
                DistroName = targetItem.DistroName,
                DistroList = AppContext.WslDistroList.Select(x => x.DistroName).Distinct().ToArray(),
                User = "root",
                ExecCommandLine = "",
            });

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            var targetDistro = dialog.ViewModel.DistroName;
            var targetUserId = dialog.ViewModel.User;
            var execCommandLine = dialog.ViewModel.ExecCommandLine;

            var process = WslHelpers.CreateLaunchSpecificDistroAsUserProcess(
                targetDistro, targetUserId, execCommandLine);
            process.Start();
            process.WaitForExit();

            AppContext.RefreshDistroList();
        }

        private void Feature_TerminateDistro(object sender, EventArgs e)
        {
            var targetItem = GetSelectedDistroBySender(sender);

            if (targetItem == null)
                return;

            var process = WslHelpers.CreateTerminateSpecificDistroProcess(targetItem.DistroName);
            process.Start();
            process.WaitForExit();

            AppContext.RefreshDistroList();
        }

        private void Feature_OpenDistroFileSystem(object sender, EventArgs e)
        {
            var targetItem = GetSelectedDistroBySender(sender);

            if (targetItem == null)
                return;

            var process = WslHelpers.CreateLaunchSpecificDistroExplorerProcess(targetItem.DistroName);
            process.Start();
            process.WaitForExit();

            AppContext.RefreshDistroList();
        }

        private void Feature_OpenDistroProperties(object sender, EventArgs e)
        {
            var targetItem = GetSelectedDistroBySender(sender);

            if (targetItem == null)
                return;

            using var dialog = new PropertiesForm(new DistroPropertyRequest()
            {
                DistroName = targetItem.DistroName,
            });

            dialog.ShowDialog(this);
        }

        private void Feature_BackupDistro(object sender, EventArgs e)
        {
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

            var backupProcess = WslHelpers.CreateExportDistroProcess(targetItem.DistroName, saveFileDialog.FileName);
            backupProcess.Start();
            AppContext.RefreshDistroList();
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

            // todo
        }

        private void Feature_InstallDistro(object sender, EventArgs e)
        {
            using var dialog = new InstallForm();

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            var installRequest = dialog.ViewModel;

            if (installRequest == null)
                return;

            var process = WslHelpers.CreateImportDistroProcess(installRequest.NewName, installRequest.InstallDirPath, installRequest.DownloadedTarFilePath);
            process.Start();
            AppContext.RefreshDistroList();
        }

        private void Feature_RestoreDistro(object sender, EventArgs e)
        {
            using var dialog = new RestoreForm();

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            var restoreRequest = dialog.ViewModel;

            if (restoreRequest == null)
                return;

            var process = WslHelpers.CreateImportDistroProcess(restoreRequest.NewName, restoreRequest.RestoreDirPath, restoreRequest.TarFilePath);
            process.Start();
            AppContext.RefreshDistroList();
        }

        private void Feature_RefreshDistroList(object sender, EventArgs e)
        {
            AppContext.RefreshDistroList();
        }
    }
}
