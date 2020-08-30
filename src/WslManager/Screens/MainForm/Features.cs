using BrightIdeasSoftware;
using System;
using System.Windows.Forms;
using WslManager.Extensions;
using WslManager.Models;
using WslManager.ViewModels;

namespace WslManager.Screens.MainForm
{
    // Features
    partial class MainForm
    {
        private WslDistro GetSelectedDistroBySender(object sender)
        {
            var targetItem = default(WslDistro);

            if (sender is ToolStripMenuItem)
            {
                var menuItem = (ToolStripMenuItem)sender;
                var toolStrip = menuItem.GetCurrentParent();

                if (object.ReferenceEquals(toolStrip, pointContextMenuStrip))
                {
                    var hitTest = pointContextMenuStrip.Tag as OlvListViewHitTestInfo;
                    targetItem = hitTest?.Item?.RowObject as WslDistro;
                }
                else if (object.ReferenceEquals(toolStrip, menuStrip))
                {
                    targetItem = listView.SelectedItem?.RowObject as WslDistro;
                }
            }

            return targetItem;
        }

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

        private void Feature_SetListView_LargeIcon(object sender, EventArgs e)
        {
            listView.View = View.LargeIcon;
        }

        private void Feature_SetListView_SmallIcon(object sender, EventArgs e)
        {
            listView.View = View.SmallIcon;
        }

        private void Feature_SetListView_List(object sender, EventArgs e)
        {
            listView.View = View.List;
        }

        private void Feature_SetListView_Details(object sender, EventArgs e)
        {
            listView.View = View.Details;
        }

        private void Feature_SetListView_Tile(object sender, EventArgs e)
        {
            listView.View = View.Tile;
        }

        private void Feature_SortBy_DistroName(object sender, EventArgs e)
        {
            var targetColumn = listView.AllColumns.Find(
                x => string.Equals(x.Name, nameof(WslDistro.DistroName), StringComparison.Ordinal));

            if (targetColumn != null)
                listView.Sort(targetColumn, listView.PrimarySortOrder);
        }

        private void Feature_SortBy_DistroStatus(object sender, EventArgs e)
        {
            var targetColumn = listView.AllColumns.Find(
                x => string.Equals(x.Name, nameof(WslDistro.DistroStatus), StringComparison.Ordinal));

            if (targetColumn != null)
                listView.Sort(targetColumn, listView.PrimarySortOrder);
        }

        private void Feature_SortBy_WSLVersion(object sender, EventArgs e)
        {
            var targetColumn = listView.AllColumns.Find(
                x => string.Equals(x.Name, nameof(WslDistro.WSLVersion), StringComparison.Ordinal));

            if (targetColumn != null)
                listView.Sort(targetColumn, listView.PrimarySortOrder);
        }

        private void Feature_SortBy_IsDefaultDistro(object sender, EventArgs e)
        {
            var targetColumn = listView.AllColumns.Find(
                x => string.Equals(x.Name, nameof(WslDistro.IsDefault), StringComparison.Ordinal));

            if (targetColumn != null)
                listView.Sort(targetColumn, listView.PrimarySortOrder);
        }

        private void Feature_SortBy_Ascending(object sender, EventArgs e)
        {
            listView.Sort(listView.PrimarySortColumn, SortOrder.Ascending);
        }

        private void Feature_SortBy_Descending(object sender, EventArgs e)
        {
            listView.Sort(listView.PrimarySortColumn, SortOrder.Descending);
        }

        private void Feature_RefreshDistroList(object sender, EventArgs e)
        {
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

            using var dialog = new RestoreForm.RestoreForm();

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            var restoreRequest = dialog.ViewModel;

            if (restoreRequest == null)
                return;

            restoreWorker.RunWorkerAsync(restoreRequest);
        }

        private void Feature_ShutdownWsl(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, $"Really shutdown WSL entirely? This operation can cause unintentional data loss.",
                    Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;

            var process = WslHelpers.CreateShutdownDistroProcess();
            process.Start();
            process.WaitForExit();
            AppContext.RefreshDistroList();
        }

        private void Feature_AboutApp(object sender, EventArgs e)
        {
            var message = string.Join(Environment.NewLine,
                "WSL Manager v0.1",
                "(c) 2019 rkttu.com, All rights reserved.");

            MessageBox.Show(this, message, Text,
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void Feature_ExitApp(object sender, EventArgs e)
        {
            Close();
        }
    }
}
