using System;
using System.Linq;
using System.Windows.Forms;
using WslManager.Extensions;
using WslManager.Models;

namespace WslManager.Screens
{
    // Main Menu
    partial class MainForm
    {
        private MenuStrip menuStrip;

        private ToolStripItem[] distroSelectedMenuItems;

        private ToolStripMenuItem distroMenu;
        private ToolStripMenuItem openDistroMenuItem;
        private ToolStripMenuItem runAsDistroMenuItem;
        private ToolStripMenuItem openDistroFolderMenuItem;
        private ToolStripMenuItem backupDistroMenuItem;
        private ToolStripMenuItem terminateDistroMenuItem;
        private ToolStripMenuItem unregisterDistroMenuItem;
        private ToolStripMenuItem setAsDefaultDistroMenuItem;

        private ToolStripItem[] genericDistroMenuItems;

        private ToolStripMenuItem restoreDistroMenuItem;
        private ToolStripMenuItem shutdownMenuItem;
        private ToolStripMenuItem exitMenuItem;

        private ToolStripMenuItem[] viewTypeMenuItems;

        private ToolStripMenuItem viewMenu;
        private ToolStripMenuItem largeIconMenuItem;
        private ToolStripMenuItem smallIconMenuItem;
        private ToolStripMenuItem listMenuItem;
        private ToolStripMenuItem detailMenuItem;
        private ToolStripMenuItem tileMenuItem;
        private ToolStripMenuItem sortByMenuItem;
        private ToolStripMenuItem sortByDistroNameMenuItem;
        private ToolStripMenuItem sortByDistroStatusMenuItem;
        private ToolStripMenuItem sortByWSLVersionMenuItem;
        private ToolStripMenuItem sortByIsDefaultDistroMenuItem;
        private ToolStripMenuItem sortByAscendingMenuItem;
        private ToolStripMenuItem sortByDescendingMenuItem;
        private ToolStripMenuItem refreshMenuItem;

        private ToolStripMenuItem helpMenu;
        private ToolStripMenuItem aboutMenuItem;

        partial void InitializeMainMenu()
        {
            menuStrip = new MenuStrip()
            {
                Parent = layout.TopToolStripPanel,
                Dock = DockStyle.Fill,
            };

            distroMenu = menuStrip.Items.AddMenuItem("&Distro");

            distroSelectedMenuItems = new ToolStripItem[]
            {
                openDistroMenuItem = distroMenu.DropDownItems.AddMenuItem("&Open Distro..."),
                runAsDistroMenuItem = distroMenu.DropDownItems.AddMenuItem("&Run as..."),
                distroMenu.DropDownItems.AddSeparator(),
                openDistroFolderMenuItem = distroMenu.DropDownItems.AddMenuItem("E&xplore Distro File System..."),
                backupDistroMenuItem = distroMenu.DropDownItems.AddMenuItem("&Backup Distro..."),
                terminateDistroMenuItem = distroMenu.DropDownItems.AddMenuItem("&Terminate Distro..."),
                unregisterDistroMenuItem = distroMenu.DropDownItems.AddMenuItem("&Unregister Distro..."),
                distroMenu.DropDownItems.AddSeparator(),
                setAsDefaultDistroMenuItem = distroMenu.DropDownItems.AddMenuItem("Set as &default distro"),
                distroMenu.DropDownItems.AddSeparator(),
            };

            genericDistroMenuItems = new ToolStripItem[]
            {
                restoreDistroMenuItem = distroMenu.DropDownItems.AddMenuItem("&Restore Distro..."),
                distroMenu.DropDownItems.AddSeparator(),
                shutdownMenuItem = distroMenu.DropDownItems.AddMenuItem("&Shutdown WSL"),
                distroMenu.DropDownItems.AddSeparator(),
                exitMenuItem = distroMenu.DropDownItems.AddMenuItem("E&xit"),
            };

            distroMenu.DropDownItems.AddRange(distroSelectedMenuItems.Concat(genericDistroMenuItems).ToArray());
            distroMenu.DropDownOpening += DistroMenu_DropDownOpening;

            openDistroMenuItem.Click += Feature_LaunchDistro;
            runAsDistroMenuItem.Click += Feature_RunAsDistro;
            openDistroFolderMenuItem.Click += Feature_OpenDistroFileSystem;
            backupDistroMenuItem.Click += Feature_BackupDistro;
            terminateDistroMenuItem.Click += Feature_TerminateDistro;
            unregisterDistroMenuItem.Click += Feature_UnregisterDistro;
            setAsDefaultDistroMenuItem.Click += Feature_SetAsDefaultDistro;
            restoreDistroMenuItem.Click += Feature_RestoreDistro;
            shutdownMenuItem.Click += Feature_ShutdownWsl;
            exitMenuItem.Click += Feature_ExitApp;

            viewMenu = menuStrip.Items.AddMenuItem("&View");
            viewMenu.DropDownItems.AddRange(new ToolStripItem[] {
                largeIconMenuItem = viewMenu.DropDownItems.AddMenuItem("La&rge Icon"),
                smallIconMenuItem = viewMenu.DropDownItems.AddMenuItem("&Small Icon"),
                listMenuItem = viewMenu.DropDownItems.AddMenuItem("&List"),
                detailMenuItem = viewMenu.DropDownItems.AddMenuItem("&Detail"),
                tileMenuItem = viewMenu.DropDownItems.AddMenuItem("&Tile"),
                viewMenu.DropDownItems.AddSeparator(),
                sortByMenuItem = viewMenu.DropDownItems.AddMenuItem("&Sort by..."),
                viewMenu.DropDownItems.AddSeparator(),
                refreshMenuItem = viewMenu.DropDownItems.AddMenuItem("&Refresh List"),
            });

            sortByMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                sortByDistroNameMenuItem = sortByMenuItem.DropDownItems.AddMenuItem("Distro &Name"),
                sortByDistroStatusMenuItem = sortByMenuItem.DropDownItems.AddMenuItem("Distro &Status"),
                sortByWSLVersionMenuItem = sortByMenuItem.DropDownItems.AddMenuItem("WSL &Version"),
                sortByIsDefaultDistroMenuItem = sortByMenuItem.DropDownItems.AddMenuItem("&Is Default Distro"),
                sortByMenuItem.DropDownItems.AddSeparator(),
                sortByAscendingMenuItem = sortByMenuItem.DropDownItems.AddMenuItem("&Ascending"),
                sortByDescendingMenuItem = sortByMenuItem.DropDownItems.AddMenuItem("&Descending"),
            });

            sortByMenuItem.DropDownOpened += SortByMenuItem_DropDownOpened;
            sortByDistroNameMenuItem.Click += Feature_SortBy_DistroName;
            sortByDistroStatusMenuItem.Click += Feature_SortBy_DistroStatus;
            sortByWSLVersionMenuItem.Click += Feature_SortBy_WSLVersion;
            sortByIsDefaultDistroMenuItem.Click += Feature_SortBy_IsDefaultDistro;
            sortByAscendingMenuItem.Click += Feature_SortBy_Ascending;
            sortByDescendingMenuItem.Click += Feature_SortBy_Descending;

            viewTypeMenuItems = new[]
            {
                largeIconMenuItem,
                smallIconMenuItem,
                listMenuItem,
                detailMenuItem,
                tileMenuItem,
            };

            viewMenu.DropDownOpening += ViewMenu_DropDownOpening;
            largeIconMenuItem.Click += Feature_SetListView_LargeIcon;
            smallIconMenuItem.Click += Feature_SetListView_SmallIcon;
            listMenuItem.Click += Feature_SetListView_List;
            detailMenuItem.Click += Feature_SetListView_Details;
            tileMenuItem.Click += Feature_SetListView_Tile;
            refreshMenuItem.Click += Feature_RefreshDistroList;

            helpMenu = menuStrip.Items.AddMenuItem("&Help");
            helpMenu.DropDownItems.AddRange(new ToolStripItem[] {
                aboutMenuItem = helpMenu.DropDownItems.AddMenuItem("&About..."),
            });

            aboutMenuItem.Click += Feature_AboutApp;
        }

        private void SortByMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            foreach (var eachItem in sortByMenuItem.DropDownItems)
                if (eachItem is ToolStripMenuItem)
                    ((ToolStripMenuItem)eachItem).Checked = false;

            switch (listView.PrimarySortColumn?.Name)
            {
                case nameof(WslDistro.DistroName):
                    sortByDistroNameMenuItem.Checked = true;
                    break;

                case nameof(WslDistro.DistroStatus):
                    sortByDistroStatusMenuItem.Checked = true;
                    break;

                case nameof(WslDistro.WSLVersion):
                    sortByWSLVersionMenuItem.Checked = true;
                    break;

                case nameof(WslDistro.IsDefault):
                    sortByIsDefaultDistroMenuItem.Checked = true;
                    break;
            }

            switch (listView.PrimarySortOrder)
            {
                case SortOrder.Ascending:
                    sortByAscendingMenuItem.Checked = true;
                    break;

                case SortOrder.Descending:
                    sortByDescendingMenuItem.Checked = true;
                    break;
            }
        }

        private void DistroMenu_DropDownOpening(object sender, EventArgs e)
        {
            var isDistroSelected = listView.SelectedItem?.RowObject as WslDistro != null;

            foreach (var eachMenu in distroSelectedMenuItems)
                eachMenu.Visible = isDistroSelected;
        }

        private void ViewMenu_DropDownOpening(object sender, EventArgs e)
        {
            foreach (var eachMenuItem in viewTypeMenuItems)
                eachMenuItem.Checked = false;

            switch (listView.View)
            {
                case View.LargeIcon:
                    largeIconMenuItem.Checked = true;
                    break;

                case View.SmallIcon:
                    smallIconMenuItem.Checked = true;
                    break;

                case View.List:
                    listMenuItem.Checked = true;
                    break;

                case View.Details:
                    detailMenuItem.Checked = true;
                    break;

                case View.Tile:
                    tileMenuItem.Checked = true;
                    break;
            }
        }
    }
}
