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
            defaultContextMenuStrip.Items.AddRange(new ToolStripItem[]
            {
                viewTypeContextMenuItem = defaultContextMenuStrip.Items.AddMenuItem("&View"),
                refreshListContextMenuItem = defaultContextMenuStrip.Items.AddMenuItem("Refresh &List"),
                defaultContextMenuStrip.Items.AddSeparator(),
                restoreDistroContextMenuItem = defaultContextMenuStrip.Items.AddMenuItem("&Restore Distro..."),
                defaultContextMenuStrip.Items.AddSeparator(),
                shutdownContextMenuItem = defaultContextMenuStrip.Items.AddMenuItem("&Shutdown WSL..."),
            });

            refreshListContextMenuItem.Click += Feature_RefreshDistroList;
            restoreDistroContextMenuItem.Click += Feature_RestoreDistro;
            shutdownContextMenuItem.Click += Feature_ShutdownWsl;

            viewTypeContextMenuItem.DropDownOpened += ViewTypeContextMenuItem_DropDownOpened;
            viewTypeContextMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                largeIconViewTypeContextMenuItem = viewTypeContextMenuItem.DropDownItems.AddMenuItem("&Large Icon"),
                smallIconViewTypeContextMenuItem = viewTypeContextMenuItem.DropDownItems.AddMenuItem("&Small Icon"),
                listViewTypeContextMenuItem = viewTypeContextMenuItem.DropDownItems.AddMenuItem("&List"),
                detailViewTypeContextMenuItem = viewTypeContextMenuItem.DropDownItems.AddMenuItem("&Detail"),
                tileViewTypeContextMenuItem = viewTypeContextMenuItem.DropDownItems.AddMenuItem("&Tile"),
            });

            largeIconViewTypeContextMenuItem.Click += Feature_SetListView_LargeIcon;
            smallIconViewTypeContextMenuItem.Click += Feature_SetListView_SmallIcon;
            listViewTypeContextMenuItem.Click += Feature_SetListView_List;
            detailViewTypeContextMenuItem.Click += Feature_SetListView_Details;
            tileViewTypeContextMenuItem.Click += Feature_SetListView_Tile;
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
