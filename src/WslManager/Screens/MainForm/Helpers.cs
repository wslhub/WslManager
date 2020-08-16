using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using WslManager.Models;

namespace WslManager.Screens.MainForm
{
    // Helpers
    partial class MainForm
    {
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
                        lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Name = "status", Text = distroInfo.DistroStatus, });
                        break;

                    case "wslver":
                        lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Name = "wslver", Text = distroInfo.WSLVersion, });
                        break;

                    case "default":
                        lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Name = "default", Text = distroInfo.IsDefault ? "*" : string.Empty, });
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
            var selectedDistroName = default(string);

            if (listView.SelectedItems.Count > 0)
                selectedDistroName = (listView.SelectedItems[0]?.Tag as DistroInfo)?.DistroName;

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

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using var wrapMode = new ImageAttributes();
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            }

            return destImage;
        }
    }
}
