using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using WslManager.Extensions;
using WslManager.ViewModels;

namespace WslManager.Screens.MainForm
{
    // Main Window
    partial class MainForm
    {
        private ToolStripContainer layout;
        private ListView listView;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusItem;

        partial void InitializeMainWindow()
        {
            ClientSize = new Size(640, 480);
            Text = "WSL Manager";
            StartPosition = FormStartPosition.WindowsDefaultBounds;

            Load += MainForm_Load;

            layout = new ToolStripContainer()
            {
                Parent = this,
                Dock = DockStyle.Fill,
            };

            listView = new ListView()
            {
                Parent = layout.ContentPanel,
                Dock = DockStyle.Fill,
                View = View.Details,
                MultiSelect = false,
                Sorting = SortOrder.None,
                FullRowSelect = true,
                LargeImageList = largeImageList,
                SmallImageList = smallImageList,
                StateImageList = stateImageList,
            };

            ConfigureListViewColumns(listView);

            listView.KeyUp += ListView_KeyUp;
            listView.MouseDown += ListView_MouseDown;
            listView.ItemActivate += ListView_ItemActivate;

            statusStrip = new StatusStrip()
            {
                Parent = layout.BottomToolStripPanel,
                Dock = DockStyle.Fill,
            };

            statusItem = new ToolStripStatusLabel()
            {
                Spring = true,
                Text = "Ready",
                TextAlign = ContentAlignment.MiddleLeft,
            };

            statusStrip.Items.Add(statusItem);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var wslHostPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.System),
                    "lxss", "wslhost.exe");

            if (!File.Exists(wslHostPath))
            {
                MessageBox.Show(this, "It looks like WSL does not available on this system. Please install WSL first.",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1);
                Close();
                return;
            }

            RefreshListView(listView, statusItem, WslExtensions.GetDistroList());
        }

        private void ListView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                RefreshListView(listView, statusItem, WslExtensions.GetDistroList());
                return;
            }
        }

        private void ListView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTest = listView.HitTest(e.Location);

                if (hitTest.Location == ListViewHitTestLocations.None)
                    defaultContextMenuStrip.Show(Cursor.Position);
                else
                {
                    pointContextMenuStrip.Show(Cursor.Position);
                    pointContextMenuStrip.Tag = hitTest;
                }
            }
        }

        private void ListView_ItemActivate(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count != 1)
                return;

            var targetItem = listView.SelectedItems[0].Tag as DistroInfo;

            if (targetItem == null)
                return;

            var process = targetItem.CreateLaunchSpecificDistroProcess();
            process.Start();
        }
    }
}
