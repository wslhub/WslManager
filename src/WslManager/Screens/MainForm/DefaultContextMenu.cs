using System;
using System.Windows.Forms;
using WslManager.Extensions;
using WslManager.Models;

namespace WslManager.Screens.MainForm
{
    // Default Context Menu
    partial class MainForm
    {
        private ContextMenuStrip defaultContextMenuStrip;
        private ToolStripMenuItem viewTypeContextMenuItem;
        private ToolStripMenuItem largeIconViewTypeContextMenuItem;
        private ToolStripMenuItem smallIconViewTypeContextMenuItem;
        private ToolStripMenuItem listViewTypeContextMenuItem;
        private ToolStripMenuItem detailViewTypeContextMenuItem;
        private ToolStripMenuItem tileViewTypeContextMenuItem;
        private ToolStripMenuItem refreshListContextMenuItem;
        private ToolStripMenuItem restoreDistroContextMenuItem;
        private ToolStripMenuItem shutdownContextMenuItem;

        partial void InitializeDefaultContextMenu()
        {
            defaultContextMenuStrip = new ContextMenuStrip();

            viewTypeContextMenuItem = defaultContextMenuStrip.Items.AddMenuItem("&View");
            viewTypeContextMenuItem.DropDownOpened += ViewTypeContextMenuItem_DropDownOpened;

            largeIconViewTypeContextMenuItem = viewTypeContextMenuItem.DropDownItems.AddMenuItem("&Large Icon");
            largeIconViewTypeContextMenuItem.Click += LargeIconViewTypeContextMenuItem_Click;

            smallIconViewTypeContextMenuItem = viewTypeContextMenuItem.DropDownItems.AddMenuItem("&Small Icon");
            smallIconViewTypeContextMenuItem.Click += SmallIconViewTypeContextMenuItem_Click;

            listViewTypeContextMenuItem = viewTypeContextMenuItem.DropDownItems.AddMenuItem("&List");
            listViewTypeContextMenuItem.Click += ListViewTypeContextMenuItem_Click;

            detailViewTypeContextMenuItem = viewTypeContextMenuItem.DropDownItems.AddMenuItem("&Detail");
            detailViewTypeContextMenuItem.Click += DetailViewTypeContextMenuItem_Click;

            tileViewTypeContextMenuItem = viewTypeContextMenuItem.DropDownItems.AddMenuItem("&Tile");
            tileViewTypeContextMenuItem.Click += TileViewTypeContextMenuItem_Click;

            refreshListContextMenuItem = defaultContextMenuStrip.Items.AddMenuItem("Refresh &List");
            refreshListContextMenuItem.Click += RefreshListContextMenuItem_Click;

            defaultContextMenuStrip.Items.AddSeparator();

            restoreDistroContextMenuItem = defaultContextMenuStrip.Items.AddMenuItem("&Restore Distro...");
            restoreDistroContextMenuItem.Click += RestoreDistroContextMenuItem_Click;

            defaultContextMenuStrip.Items.AddSeparator();

            shutdownContextMenuItem = defaultContextMenuStrip.Items.AddMenuItem("&Shutdown WSL...");
            shutdownContextMenuItem.Click += ShutdownContextMenuItem_Click;
        }

        private void LargeIconViewTypeContextMenuItem_Click(object sender, EventArgs e)
        {
            listView.View = View.LargeIcon;
        }

        private void SmallIconViewTypeContextMenuItem_Click(object sender, EventArgs e)
        {
            listView.View = View.SmallIcon;
        }

        private void ListViewTypeContextMenuItem_Click(object sender, EventArgs e)
        {
            listView.View = View.List;
        }

        private void DetailViewTypeContextMenuItem_Click(object sender, EventArgs e)
        {
            listView.View = View.Details;
        }

        private void TileViewTypeContextMenuItem_Click(object sender, EventArgs e)
        {
            listView.View = View.Tile;
        }

        private void RefreshListContextMenuItem_Click(object sender, EventArgs e)
        {
            RefreshListView(listView, statusItem, WslExtensions.GetDistroList());
        }

        private void ViewTypeContextMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem eachSubItem in viewTypeContextMenuItem.DropDownItems)
                eachSubItem.Checked = false;

            switch (listView.View)
            {
                case View.LargeIcon:
                    largeIconViewTypeContextMenuItem.Checked = true;
                    break;

                case View.SmallIcon:
                    smallIconViewTypeContextMenuItem.Checked = true;
                    break;

                case View.List:
                    listViewTypeContextMenuItem.Checked = true;
                    break;

                case View.Details:
                    detailViewTypeContextMenuItem.Checked = true;
                    break;

                case View.Tile:
                    tileViewTypeContextMenuItem.Checked = true;
                    break;
            }
        }

        private void RestoreDistroContextMenuItem_Click(object sender, EventArgs e)
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

            var restoreRequest = dialog.Model;

            if (restoreRequest == null)
                return;

            restoreWorker.RunWorkerAsync(restoreRequest);
        }

        private void ShutdownContextMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, $"Really shutdown WSL entirely? This operation can cause unintentional data loss.",
                    Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;

            var process = WslExtensions.CreateShutdownDistroProcess();
            process.Start();
            process.WaitForExit();
            RefreshListView(listView, statusItem, WslExtensions.GetDistroList());
        }
    }
}
