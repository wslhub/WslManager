using System;
using System.Windows.Forms;
using WslManager.Models;

namespace WslManager.Extensions
{
    internal static class WinFormExtensions
    {
        public static ToolStripMenuItem AddMenuItem(this ToolStripItemCollection parent, string text)
            => (ToolStripMenuItem)parent.Add(text);

        public static void AddSeparator(this ToolStripItemCollection parent)
            => parent.Add(new ToolStripSeparator());
    }
}
