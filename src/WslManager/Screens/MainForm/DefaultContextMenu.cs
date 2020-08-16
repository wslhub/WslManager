using System;
using System.Windows.Forms;
using WslManager.Extensions;

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
            largeIconViewTypeContextMenuItem.Click += Feature_SetListView_LargeIcon;

            smallIconViewTypeContextMenuItem = viewTypeContextMenuItem.DropDownItems.AddMenuItem("&Small Icon");
            smallIconViewTypeContextMenuItem.Click += Feature_SetListView_SmallIcon;

            listViewTypeContextMenuItem = viewTypeContextMenuItem.DropDownItems.AddMenuItem("&List");
            listViewTypeContextMenuItem.Click += Feature_SetListView_List;

            detailViewTypeContextMenuItem = viewTypeContextMenuItem.DropDownItems.AddMenuItem("&Detail");
            detailViewTypeContextMenuItem.Click += Feature_SetListView_Details;

            tileViewTypeContextMenuItem = viewTypeContextMenuItem.DropDownItems.AddMenuItem("&Tile");
            tileViewTypeContextMenuItem.Click += Feature_SetListView_Tile;

            refreshListContextMenuItem = defaultContextMenuStrip.Items.AddMenuItem("Refresh &List");
            refreshListContextMenuItem.Click += Feature_RefreshDistroList;

            defaultContextMenuStrip.Items.AddSeparator();

            restoreDistroContextMenuItem = defaultContextMenuStrip.Items.AddMenuItem("&Restore Distro...");
            restoreDistroContextMenuItem.Click += Feature_RestoreDistro;

            defaultContextMenuStrip.Items.AddSeparator();

            shutdownContextMenuItem = defaultContextMenuStrip.Items.AddMenuItem("&Shutdown WSL...");
            shutdownContextMenuItem.Click += Feature_ShutdownWsl;
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
    }
}
