using BrightIdeasSoftware;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WslManager.Controls;
using WslManager.Extensions;
using WslManager.Models;

namespace WslManager.Screens
{
    // Main Window
    partial class MainForm
    {
        private ToolStripContainer layout;
        private CustomListView listView;
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

            listView = new CustomListView()
            {
                Parent = layout.ContentPanel,
                Dock = DockStyle.Fill,
                View = View.Details,
                MultiSelect = false,
                Sorting = SortOrder.None,
                FullRowSelect = true,
                LargeImageList = largeImageList,
                SmallImageList = smallImageList,
                BaseSmallImageList = smallImageList,
                StateImageList = stateImageList,
                DataSource = bindingSource,
                EnableAutoScaleColumn = true,
                ColumnScaleList = new Collection<float>(new float[] { 3f, 1f, 1f, 1f, }),
                UseExplorerTheme = true,
                UseTranslucentHotItem = true,
                UseTranslucentSelection = true,
                ShowHeaderInAllViews = false,
                ShowGroups = false,
            };

            listView.PrimarySortColumn = listView.AllColumns.Find(x => string.Equals(
                x.Name, nameof(WslDistro.DistroName), StringComparison.Ordinal));

            listView.PrimarySortOrder = SortOrder.Ascending;

            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

            var defaultDistroColumn = listView.AllColumns.Find(x => string.Equals(
                x.Name, nameof(WslDistro.IsDefault), StringComparison.Ordinal));

            if (defaultDistroColumn != null)
                defaultDistroColumn.IsEditable = false;

            var distroNameColumn = listView.AllColumns.Find(x => string.Equals(
                x.Name, nameof(WslDistro.DistroName), StringComparison.Ordinal));

            if (distroNameColumn != null)
            {
                distroNameColumn.ImageGetter = new ImageGetterDelegate(o =>
                {
                    var modelName = ((o as WslDistro)?.DistroName ?? string.Empty).Trim();
                    var keyList = Resources.LogoImages.Keys.ToArray();
                    return keyList.FirstOrDefault(x => modelName.Contains(x, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
                });
            }

            listView.KeyUp += ListView_KeyUp;
            listView.MouseDown += ListView_MouseDown;
            listView.ItemActivate += ListView_ItemActivate;
            listView.FormatRow += ListView_FormatRow;

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

            using var memStream = new MemoryStream(Convert.FromBase64String(Resources.AppIconFileContent));
            Icon = new Icon(memStream);
        }

        private void ListView_FormatRow(object sender, FormatRowEventArgs e)
        {
            var dataRow = e.Model as WslDistro;
            var lvItem = e.Item;

            if (dataRow != null)
            {
                var roughName = dataRow?.DistroName?.Trim() ?? string.Empty;
                var found = false;

                foreach (var eachKey in Resources.LogoImages.Keys)
                {
                    if (roughName.Contains(eachKey, StringComparison.OrdinalIgnoreCase))
                    {
                        lvItem.ImageKey = eachKey;
                        found = true;
                        break;
                    }
                }

                if (!found)
                    lvItem.ImageKey = Resources.GenericLinuxLogoImage.Key;

                lvItem.StateImageIndex = Resources.GetStateImageIndex(dataRow.DistroStatus);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (!File.Exists(WslHelpers.GetCanonicalWslExePath()))
            {
                MessageBox.Show(this, "This system does not have wsl CLI.",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1);
                Close();
                return;
            }

            if (!File.Exists(WslHelpers.GetCanonicalWslHostExePath()))
            {
                MessageBox.Show(this, "It looks like WSL does not available on this system. Please install WSL first.",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1);
                Close();
                return;
            }

            AppContext.RefreshDistroList();
        }

        private void ListView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                AppContext.RefreshDistroList();
                return;
            }
        }

        private void ListView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTest = listView.OlvHitTest(e.Location.X, e.Location.Y);

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

            var targetItem = listView.SelectedItem?.RowObject as WslDistro;

            if (targetItem == null)
                return;

            var process = WslHelpers.CreateLaunchSpecificDistroProcess(targetItem.DistroName);
            process.Start();

            AppContext.RefreshDistroList();
        }
    }
}
