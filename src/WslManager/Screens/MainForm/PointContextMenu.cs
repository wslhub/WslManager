using System.Windows.Forms;
using WslManager.Extensions;

namespace WslManager.Screens.MainForm
{
    // Point Contex Menu
    partial class MainForm
    {
        private ContextMenuStrip pointContextMenuStrip;
        private ToolStripMenuItem openDistroContextMenuItem;
        private ToolStripMenuItem openDistroFolderContextMenuItem;
        private ToolStripMenuItem backupDistroContextMenuItem;
        private ToolStripMenuItem unregisterDistroContextMenuItem;
        private ToolStripMenuItem setAsDefaultDistroContextMenuItem;

        partial void InitializePointContextMenu()
        {
            pointContextMenuStrip = new ContextMenuStrip();

            openDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("&Open Distro...");
            openDistroContextMenuItem.Click += Feature_LaunchDistro;

            pointContextMenuStrip.Items.AddSeparator();

            openDistroFolderContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("E&xplore Distro File System...");
            openDistroFolderContextMenuItem.Click += Feature_OpenDistroFileSystem;

            backupDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("&Backup Distro...");
            backupDistroContextMenuItem.Click += Feature_BackupDistro;

            unregisterDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("&Unregister Distro...");
            unregisterDistroContextMenuItem.Click += Feature_UnregisterDistro;

            pointContextMenuStrip.Items.AddSeparator();

            setAsDefaultDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("Set as &default distro");
            setAsDefaultDistroContextMenuItem.Click += Feature_SetAsDefaultDistro;
        }
    }
}
