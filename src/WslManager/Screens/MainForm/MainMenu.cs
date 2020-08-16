using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WslManager.Extensions;

namespace WslManager.Screens.MainForm
{
    // Main Menu
    partial class MainForm
    {
        private MenuStrip menuStrip;
        private ToolStripMenuItem distroMenu;
        private ToolStripMenuItem restoreDistroMenuItem;
        private ToolStripMenuItem shutdownMenuItem;
        private ToolStripMenuItem exitMenuItem;

        private List<ToolStripMenuItem> viewTypeMenuItems;

        private ToolStripMenuItem viewMenu;
        private ToolStripMenuItem largeIconMenuItem;
        private ToolStripMenuItem smallIconMenuItem;
        private ToolStripMenuItem listMenuItem;
        private ToolStripMenuItem detailMenuItem;
        private ToolStripMenuItem tileMenuItem;
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

            restoreDistroMenuItem = distroMenu.DropDownItems.AddMenuItem("&Restore Distro...");
            restoreDistroMenuItem.Click += Feature_RestoreDistro;

            distroMenu.DropDownItems.AddSeparator();

            shutdownMenuItem = distroMenu.DropDownItems.AddMenuItem("&Shutdown WSL");
            shutdownMenuItem.Click += Feature_ShutdownWsl;

            distroMenu.DropDownItems.AddSeparator();

            exitMenuItem = distroMenu.DropDownItems.AddMenuItem("E&xit");
            exitMenuItem.Click += Feature_ExitApp;

            viewMenu = menuStrip.Items.AddMenuItem("&View");
            viewMenu.DropDownOpening += ViewMenu_DropDownOpening;
            viewTypeMenuItems = new List<ToolStripMenuItem>();

            largeIconMenuItem = viewMenu.DropDownItems.AddMenuItem("La&rge Icon");
            viewTypeMenuItems.Add(largeIconMenuItem);
            largeIconMenuItem.Click += Feature_SetListView_LargeIcon;

            smallIconMenuItem = viewMenu.DropDownItems.AddMenuItem("&Small Icon");
            viewTypeMenuItems.Add(smallIconMenuItem);
            smallIconMenuItem.Click += Feature_SetListView_SmallIcon;

            listMenuItem = viewMenu.DropDownItems.AddMenuItem("&List");
            viewTypeMenuItems.Add(listMenuItem);
            listMenuItem.Click += Feature_SetListView_List;

            detailMenuItem = viewMenu.DropDownItems.AddMenuItem("&Detail");
            viewTypeMenuItems.Add(detailMenuItem);
            detailMenuItem.Click += Feature_SetListView_Details;

            tileMenuItem = viewMenu.DropDownItems.AddMenuItem("&Tile");
            viewTypeMenuItems.Add(tileMenuItem);
            tileMenuItem.Click += Feature_SetListView_Tile;

            viewMenu.DropDownItems.AddSeparator();

            refreshMenuItem = viewMenu.DropDownItems.AddMenuItem("&Refresh List");
            refreshMenuItem.Click += Feature_RefreshDistroList;

            helpMenu = menuStrip.Items.AddMenuItem("&Help");

            aboutMenuItem = helpMenu.DropDownItems.AddMenuItem("&About...");
            aboutMenuItem.Click += Feature_AboutApp;
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
