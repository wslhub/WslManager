using System;
using System.Windows.Forms;

namespace WslManager
{
    internal static class ExtensionMethods
    {
        public static ToolStripMenuItem AddMenuItem(this ToolStripItemCollection parent, string text)
            => (ToolStripMenuItem)parent.Add(text);

        public static void AddSeparator(this ToolStripItemCollection parent)
            => parent.Add(new ToolStripSeparator());

        public static bool? IsDistroStarted(this DistroInfo info)
        {
            if (info == null)
                return null;

            return string.Equals(info.DistroStatus, "Running", StringComparison.OrdinalIgnoreCase);
        }
    }
}
