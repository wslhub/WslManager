namespace WslManager.Screens
{
    // Layout
    partial class MainForm
    {
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
