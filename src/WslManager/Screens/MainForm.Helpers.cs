using BrightIdeasSoftware;
using System.Windows.Forms;
using WslManager.Models;

namespace WslManager.Screens
{
    // Helpers
    partial class MainForm
    {
        private WslDistro GetSelectedDistroBySender(object sender)
        {
            var targetItem = default(WslDistro);

            if (sender is ToolStripMenuItem)
            {
                var menuItem = (ToolStripMenuItem)sender;
                var toolStrip = menuItem.GetCurrentParent();

                if (object.ReferenceEquals(toolStrip, pointContextMenuStrip))
                {
                    var hitTest = pointContextMenuStrip.Tag as OlvListViewHitTestInfo;
                    targetItem = hitTest?.Item?.RowObject as WslDistro;
                }
                else
                    targetItem = listView.SelectedItem?.RowObject as WslDistro;
            }

            return targetItem;
        }
    }
}
