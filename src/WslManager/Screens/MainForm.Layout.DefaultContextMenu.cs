using System;
using System.Windows.Forms;
using WslManager.Extensions;
using WslManager.Models;

namespace WslManager.Screens
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

        private ToolStripMenuItem sortByContextMenuItem;
        private ToolStripMenuItem sortByDistroNameContextMenuItem;
        private ToolStripMenuItem sortByDistroStatusContextMenuItem;
        private ToolStripMenuItem sortByWSLVersionContextMenuItem;
        private ToolStripMenuItem sortByIsDefaultDistroContextMenuItem;
        private ToolStripMenuItem sortByAscendingContextMenuItem;
        private ToolStripMenuItem sortByDescendingContextMenuItem;

        private ToolStripMenuItem refreshListContextMenuItem;
        private ToolStripMenuItem restoreDistroContextMenuItem;

        partial void InitializeDefaultContextMenu()
        {
            defaultContextMenuStrip = new ContextMenuStrip();
            defaultContextMenuStrip.Items.AddRange(new ToolStripItem[]
            {
                viewTypeContextMenuItem = defaultContextMenuStrip.Items.AddMenuItem("&View"),
                sortByContextMenuItem = defaultContextMenuStrip.Items.AddMenuItem("&Sort by..."),
                refreshListContextMenuItem = defaultContextMenuStrip.Items.AddMenuItem("Refresh &List"),
                defaultContextMenuStrip.Items.AddSeparator(),
                restoreDistroContextMenuItem = defaultContextMenuStrip.Items.AddMenuItem("&Restore Distro..."),
            });

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

            sortByContextMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                sortByDistroNameContextMenuItem = sortByContextMenuItem.DropDownItems.AddMenuItem("Distro &Name"),
                sortByDistroStatusContextMenuItem = sortByContextMenuItem.DropDownItems.AddMenuItem("Distro &Status"),
                sortByWSLVersionContextMenuItem = sortByContextMenuItem.DropDownItems.AddMenuItem("WSL &Version"),
                sortByIsDefaultDistroContextMenuItem = sortByContextMenuItem.DropDownItems.AddMenuItem("&Is Default Distro"),
                sortByContextMenuItem.DropDownItems.AddSeparator(),
                sortByAscendingContextMenuItem = sortByContextMenuItem.DropDownItems.AddMenuItem("&Ascending"),
                sortByDescendingContextMenuItem = sortByContextMenuItem.DropDownItems.AddMenuItem("&Descending"),
            });

            sortByContextMenuItem.DropDownOpened += SortByContextMenuItem_DropDownOpened;
            sortByDistroNameContextMenuItem.Click += Feature_SortBy_DistroName;
            sortByDistroStatusContextMenuItem.Click += Feature_SortBy_DistroStatus;
            sortByWSLVersionContextMenuItem.Click += Feature_SortBy_WSLVersion;
            sortByIsDefaultDistroContextMenuItem.Click += Feature_SortBy_IsDefaultDistro;
            sortByAscendingContextMenuItem.Click += Feature_SortBy_Ascending;
            sortByDescendingContextMenuItem.Click += Feature_SortBy_Descending;

            refreshListContextMenuItem.Click += Feature_RefreshDistroList;
            restoreDistroContextMenuItem.Click += Feature_RestoreDistro;
        }

        private void SortByContextMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            foreach (var eachItem in sortByContextMenuItem.DropDownItems)
                if (eachItem is ToolStripMenuItem)
                    ((ToolStripMenuItem)eachItem).Checked = false;

            switch (listView.PrimarySortColumn?.Name)
            {
                case nameof(WslDistro.DistroName):
                    sortByDistroNameContextMenuItem.Checked = true;
                    break;

                case nameof(WslDistro.DistroStatus):
                    sortByDistroStatusContextMenuItem.Checked = true;
                    break;

                case nameof(WslDistro.WSLVersion):
                    sortByWSLVersionContextMenuItem.Checked = true;
                    break;

                case nameof(WslDistro.IsDefault):
                    sortByIsDefaultDistroContextMenuItem.Checked = true;
                    break;
            }

            switch (listView.PrimarySortOrder)
            {
                case SortOrder.Ascending:
                    sortByAscendingContextMenuItem.Checked = true;
                    break;

                case SortOrder.Descending:
                    sortByDescendingContextMenuItem.Checked = true;
                    break;
            }
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
