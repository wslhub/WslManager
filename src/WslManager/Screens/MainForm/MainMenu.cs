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
        private ToolStripMenuItem appMenu;
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

            appMenu = menuStrip.Items.AddMenuItem("&App");

            exitMenuItem = appMenu.DropDownItems.AddMenuItem("E&xit");
            exitMenuItem.Click += ExitMenuItem_Click;

            viewMenu = menuStrip.Items.AddMenuItem("&View");
            viewMenu.DropDownOpening += ViewMenu_DropDownOpening;
            viewTypeMenuItems = new List<ToolStripMenuItem>();

            largeIconMenuItem = viewMenu.DropDownItems.AddMenuItem("La&rge Icon");
            viewTypeMenuItems.Add(largeIconMenuItem);
            largeIconMenuItem.Click += LargeIconMenuItem_Click;

            smallIconMenuItem = viewMenu.DropDownItems.AddMenuItem("&Small Icon");
            viewTypeMenuItems.Add(smallIconMenuItem);
            smallIconMenuItem.Click += SmallIconMenuItem_Click;

            listMenuItem = viewMenu.DropDownItems.AddMenuItem("&List");
            viewTypeMenuItems.Add(listMenuItem);
            listMenuItem.Click += ListMenuItem_Click;

            detailMenuItem = viewMenu.DropDownItems.AddMenuItem("&Detail");
            viewTypeMenuItems.Add(detailMenuItem);
            detailMenuItem.Click += DetailMenuItem_Click;

            tileMenuItem = viewMenu.DropDownItems.AddMenuItem("&Tile");
            viewTypeMenuItems.Add(tileMenuItem);
            tileMenuItem.Click += TileMenuItem_Click;

            viewMenu.DropDownItems.AddSeparator();

            refreshMenuItem = viewMenu.DropDownItems.AddMenuItem("&Refresh List");
            refreshMenuItem.Click += RefreshMenuItem_Click;

            helpMenu = menuStrip.Items.AddMenuItem("&Help");

            aboutMenuItem = helpMenu.DropDownItems.AddMenuItem("&About...");
            aboutMenuItem.Click += AboutMenuItem_Click;
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void LargeIconMenuItem_Click(object sender, EventArgs e)
        {
            listView.View = View.LargeIcon;
        }

        private void SmallIconMenuItem_Click(object sender, EventArgs e)
        {
            listView.View = View.SmallIcon;
        }

        private void ListMenuItem_Click(object sender, EventArgs e)
        {
            listView.View = View.List;
        }

        private void DetailMenuItem_Click(object sender, EventArgs e)
        {
            listView.View = View.Details;
        }

        private void TileMenuItem_Click(object sender, EventArgs e)
        {
            listView.View = View.Tile;
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

        private void RefreshMenuItem_Click(object sender, EventArgs e)
        {
            RefreshListView(listView, statusItem, WslExtensions.GetDistroList());
        }

        private void AboutMenuItem_Click(object sender, EventArgs e)
        {
            var message = string.Join(Environment.NewLine,
                "WSL Manager v0.1",
                "(c) 2019 rkttu.com, All rights reserved.");

            MessageBox.Show(this, message, Text,
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }
    }
}
