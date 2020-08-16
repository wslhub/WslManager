using System;
using System.Windows.Forms;
using WslManager.Extensions;
using WslManager.Models;

namespace WslManager.Screens.MainForm
{
    // Point Contex Menu
    partial class MainForm
    {
        private ContextMenuStrip pointContextMenuStrip;
        private ToolStripMenuItem openDistroContextMenuItem;
        private ToolStripMenuItem openDistroFolderContextMenuItem;
        private ToolStripMenuItem backupDistroContextMenuItem;
        private ToolStripMenuItem unregisterDistroContextMenuItem;
        private ToolStripMenuItem setAsDefaultDistroContextMenuItem;

        partial void InitializePointContextMenu()
        {
            pointContextMenuStrip = new ContextMenuStrip();

            openDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("&Open Distro...");
            openDistroContextMenuItem.Click += OpenDistroContextMenuItem_Click;

            pointContextMenuStrip.Items.AddSeparator();

            openDistroFolderContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("E&xplore Distro File System...");
            openDistroFolderContextMenuItem.Click += OpenDistroFolderContextMenuItem_Click;

            backupDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("&Backup Distro...");
            backupDistroContextMenuItem.Click += BackupDistroContextMenuItem_Click;

            unregisterDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("&Unregister Distro...");
            unregisterDistroContextMenuItem.Click += UnregisterDistroContextMenuItem_Click;

            pointContextMenuStrip.Items.AddSeparator();

            setAsDefaultDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("Set as &default distro");
            setAsDefaultDistroContextMenuItem.Click += SetAsDefaultDistroContextMenuItem_Click;
        }

        private void OpenDistroContextMenuItem_Click(object sender, EventArgs e)
        {
            var hitTest = pointContextMenuStrip.Tag as ListViewHitTestInfo;
            var targetItem = hitTest?.Item?.Tag as DistroInfo;

            if (targetItem == null)
                return;

            var process = targetItem.CreateLaunchSpecificDistroProcess();
            var result = process.Start();
        }

        private void OpenDistroFolderContextMenuItem_Click(object sender, EventArgs e)
        {
            var hitTest = pointContextMenuStrip.Tag as ListViewHitTestInfo;
            var targetItem = hitTest?.Item?.Tag as DistroInfo;

            if (targetItem == null)
                return;

            var process = targetItem.CreateLaunchSpecificDistroExplorerProcess();
            var result = process.Start();
        }

        private void BackupDistroContextMenuItem_Click(object sender, EventArgs e)
        {
            if (backupWorker.IsBusy)
            {
                MessageBox.Show(
                    this, "Already one or more backup in progress. Please try again later.",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);

                return;
            }

            var hitTest = pointContextMenuStrip.Tag as ListViewHitTestInfo;
            var targetItem = hitTest?.Item?.Tag as DistroInfo;

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

        private void UnregisterDistroContextMenuItem_Click(object sender, EventArgs e)
        {
            var hitTest = pointContextMenuStrip.Tag as ListViewHitTestInfo;
            var targetItem = hitTest?.Item?.Tag as DistroInfo;

            if (targetItem == null)
                return;

            if (MessageBox.Show(this, $"Really unregister `{targetItem.DistroName}` distro? This cannot be undone.",
                Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;

            var process = targetItem.CreateUnregisterDistroProcess();
            process.Start();
            process.WaitForExit();
            RefreshListView(listView, statusItem, WslExtensions.GetDistroList());
        }

        private void SetAsDefaultDistroContextMenuItem_Click(object sender, EventArgs e)
        {
            var hitTest = pointContextMenuStrip.Tag as ListViewHitTestInfo;
            var targetItem = hitTest?.Item?.Tag as DistroInfo;

            if (targetItem == null)
                return;

            var process = targetItem.CreateSetAsDefaultProcess();
            process.Start();
            process.WaitForExit();
            RefreshListView(listView, statusItem, WslExtensions.GetDistroList());
        }
    }
}
