using System.ComponentModel;

namespace WslManager.Screens.MainForm
{
    // Initialize
    public sealed partial class MainForm : CodeFirstForm
    {
        partial void InitializeImageList(IContainer components);
        partial void InitializeBackupWorker(IContainer components);
        partial void InitializeRestoreWorker(IContainer components);

        protected override void InitializeComponents(IContainer components)
        {
            base.InitializeComponents(components);

            InitializeImageList(components);
            InitializeBackupWorker(components);
            InitializeRestoreWorker(components);
        }

        partial void InitializeMainWindow();
        partial void InitializeMainMenu();
        partial void InitializePointContextMenu();
        partial void InitializeDefaultContextMenu();

        protected override void InitializeUserInterface()
        {
            base.InitializeUserInterface();

            InitializeMainWindow();
            InitializeMainMenu();
            InitializePointContextMenu();
            InitializeDefaultContextMenu();
        }
    }
}
