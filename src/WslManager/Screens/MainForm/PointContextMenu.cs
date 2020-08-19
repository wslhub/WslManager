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
            pointContextMenuStrip.Items.AddRange(new ToolStripItem[]
            {
                openDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("&Open Distro..."),
                pointContextMenuStrip.Items.AddSeparator(),
                openDistroFolderContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("E&xplore Distro File System..."),
                backupDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("&Backup Distro..."),
                unregisterDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("&Unregister Distro..."),
                pointContextMenuStrip.Items.AddSeparator(),
                setAsDefaultDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("Set as &default distro"),
            });

            openDistroContextMenuItem.Click += Feature_LaunchDistro;
            openDistroFolderContextMenuItem.Click += Feature_OpenDistroFileSystem;
            backupDistroContextMenuItem.Click += Feature_BackupDistro;
            unregisterDistroContextMenuItem.Click += Feature_UnregisterDistro;
            setAsDefaultDistroContextMenuItem.Click += Feature_SetAsDefaultDistro;
        }
    }
}
