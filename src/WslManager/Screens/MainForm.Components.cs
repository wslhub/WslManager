using System.ComponentModel;

namespace WslManager.Screens
{
    // Components
    partial class MainForm
    {
        partial void InitializeBindingSource(IContainer components);
        partial void InitializeImageList(IContainer components);
        partial void InitializeBackupWorker(IContainer components);
        partial void InitializeRestoreWorker(IContainer components);

        protected override void InitializeComponents(IContainer components)
        {
            base.InitializeComponents(components);

            InitializeImageList(components);
            InitializeBackupWorker(components);
            InitializeRestoreWorker(components);
            InitializeBindingSource(components);
        }
    }
}
