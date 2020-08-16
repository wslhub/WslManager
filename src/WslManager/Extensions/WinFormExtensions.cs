using System.Windows.Forms;

namespace WslManager.Extensions
{
    internal static class WinFormExtensions
    {
        public static ListViewItem GetSelectedItem(this ListView listView)
        {
            var items = listView.SelectedItems;

            if (items == null || items.Count < 1)
                return null;

            return items[0];
        }

        public static ToolStripMenuItem AddMenuItem(this ToolStripItemCollection parent, string text)
            => (ToolStripMenuItem)parent.Add(text);

        public static void AddSeparator(this ToolStripItemCollection parent)
            => parent.Add(new ToolStripSeparator());
    }
}
