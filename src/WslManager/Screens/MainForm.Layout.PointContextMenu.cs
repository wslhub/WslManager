using System.Windows.Forms;
using WslManager.Extensions;

namespace WslManager.Screens
{
    // Point Contex Menu
    partial class MainForm
    {
        private ContextMenuStrip pointContextMenuStrip;
        private ToolStripMenuItem openDistroContextMenuItem;
        private ToolStripMenuItem runAsDistroContextMenuItem;
        private ToolStripMenuItem openDistroFolderContextMenuItem;
        private ToolStripMenuItem backupDistroContextMenuItem;
        private ToolStripMenuItem terminateDistroContextMenuItem;
        private ToolStripMenuItem unregisterDistroContextMenuItem;
        private ToolStripMenuItem setAsDefaultDistroContextMenuItem;

        partial void InitializePointContextMenu()
        {
            pointContextMenuStrip = new ContextMenuStrip();
            pointContextMenuStrip.Items.AddRange(new ToolStripItem[]
            {
                openDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("&Open Distro..."),
                runAsDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("&Run As..."),
                pointContextMenuStrip.Items.AddSeparator(),
                openDistroFolderContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("E&xplore Distro File System..."),
                backupDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("&Backup Distro..."),
                terminateDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("&Terminate Distro..."),
                unregisterDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("&Unregister Distro..."),
                pointContextMenuStrip.Items.AddSeparator(),
                setAsDefaultDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("Set as &default distro"),
            });

            openDistroContextMenuItem.Click += Feature_LaunchDistro;
            runAsDistroContextMenuItem.Click += Feature_RunAsDistro;
            openDistroFolderContextMenuItem.Click += Feature_OpenDistroFileSystem;
            backupDistroContextMenuItem.Click += Feature_BackupDistro;
            terminateDistroContextMenuItem.Click += Feature_TerminateDistro;
            unregisterDistroContextMenuItem.Click += Feature_UnregisterDistro;
            setAsDefaultDistroContextMenuItem.Click += Feature_SetAsDefaultDistro;
        }
    }
}
