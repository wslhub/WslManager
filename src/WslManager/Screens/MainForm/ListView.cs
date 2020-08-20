using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WslManager.Extensions;
using WslManager.ViewModels;

namespace WslManager.Screens.MainForm
{
    // Helpers
    partial class MainForm
    {
        /*
        public static void ConfigureListViewColumns(
            ListView listView)
        {
            listView.Columns.Add(string.Empty, "Distro Name", 200);
            listView.Columns.Add("status", "Distro Status", 120);
            listView.Columns.Add("wslver", "WSL Version", 120);
            listView.Columns.Add("default", "Is Default", 120);
        }

        public static ListViewItem AddDistroInfoIntoListView(
            ListView listView,
            DistroInfo distroInfo)
        {
            var lvItem = new ListViewItem(distroInfo.DistroName) { Tag = distroInfo, ImageKey = "linux", };
            var roughName = distroInfo?.DistroName?.Trim() ?? string.Empty;

            foreach (var eachKey in Resources.LogoImages.Keys)
            {
                if (roughName.Contains(eachKey, StringComparison.OrdinalIgnoreCase))
                    lvItem.ImageKey = eachKey;
            }

            if (distroInfo.IsDefault)
                lvItem.StateImageIndex = 0;
            else if (string.Equals(distroInfo.DistroStatus, "Installing", StringComparison.OrdinalIgnoreCase))
                lvItem.StateImageIndex = 1;

            foreach (ColumnHeader eachSubItem in listView.Columns)
            {
                switch (eachSubItem.Name)
                {
                    case "status":
                        lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Name = eachSubItem.Name, Text = distroInfo.DistroStatus, });
                        break;

                    case "wslver":
                        lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Name = eachSubItem.Name, Text = distroInfo.WSLVersion, });
                        break;

                    case "default":
                        lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Name = eachSubItem.Name, Text = distroInfo.IsDefault ? "*" : string.Empty, });
                        break;
                }
            }

            listView.Items.Add(lvItem);
            return lvItem;
        }

        public static void RefreshListView(
            ListView listView,
            ToolStripStatusLabel stateLabel,
            IEnumerable<DistroInfo> distroInfoList)
        {
            if (listView.InvokeRequired)
            {
                listView.Invoke(
                    new Action<ListView, ToolStripStatusLabel, IEnumerable<DistroInfo>>(RefreshListView),
                    listView, stateLabel, distroInfoList);
                return;
            }

            listView.BeginUpdate();
            var selectedDistroName = (listView.GetSelectedItem()?.Tag as DistroInfo)?.DistroName;

            if (listView.Items.Count > 0)
                listView.Items.Clear();

            foreach (var eachDistro in distroInfoList)
            {
                var createdItem = AddDistroInfoIntoListView(listView, eachDistro);

                if (string.Equals(eachDistro.DistroName, selectedDistroName, StringComparison.Ordinal))
                    createdItem.Selected = true;
            }

            stateLabel.Text = $"Total {distroInfoList.Count()} distros found. - {DateTime.Now}";
            listView.EndUpdate();
        }
        */
    }
}
