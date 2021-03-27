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
        private ToolStripMenuItem createDistroShortcutContextMenuItem;
        private ToolStripMenuItem backupDistroContextMenuItem;
        private ToolStripMenuItem terminateDistroContextMenuItem;
        private ToolStripMenuItem unregisterDistroContextMenuItem;
        private ToolStripMenuItem setAsDefaultDistroContextMenuItem;
        private ToolStripMenuItem propertiesDistroContextMenuItem;

        partial void InitializePointContextMenu()
        {
            pointContextMenuStrip = new ContextMenuStrip();
            pointContextMenuStrip.Items.AddRange(new ToolStripItem[]
            {
                openDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("&Open Distro..."),
                runAsDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("&Run As..."),
                pointContextMenuStrip.Items.AddSeparator(),
                openDistroFolderContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("E&xplore Distro File System..."),
                createDistroShortcutContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("Create &Shortcut..."),
                backupDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("&Backup Distro..."),
                terminateDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("&Terminate Distro..."),
                unregisterDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("&Unregister Distro..."),
                pointContextMenuStrip.Items.AddSeparator(),
                setAsDefaultDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("Set as &default distro"),
                pointContextMenuStrip.Items.AddSeparator(),
                propertiesDistroContextMenuItem = pointContextMenuStrip.Items.AddMenuItem("&Properties..."),
            });

            openDistroContextMenuItem.Click += Feature_LaunchDistro;
            runAsDistroContextMenuItem.Click += Feature_RunAsDistro;
            openDistroFolderContextMenuItem.Click += Feature_OpenDistroFileSystem;
            createDistroShortcutContextMenuItem.Click += Feature_CreateDistroShortcut;
            backupDistroContextMenuItem.Click += Feature_BackupDistro;
            terminateDistroContextMenuItem.Click += Feature_TerminateDistro;
            unregisterDistroContextMenuItem.Click += Feature_UnregisterDistro;
            setAsDefaultDistroContextMenuItem.Click += Feature_SetAsDefaultDistro;
            propertiesDistroContextMenuItem.Click += Feature_OpenDistroProperties;
        }
    }
}
