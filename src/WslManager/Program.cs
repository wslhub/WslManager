using System;
using System.Windows.Forms;

namespace WslManager
{
    internal static class Program
    {
        private static void InitApplication()
        {
            Application.OleRequired();
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
        }

        [STAThread]
        private static void Main()
        {
            InitApplication();

            var appContext = new ApplicationContext()
            {
                MainForm = MainForm.Create(),
            };

            Application.Run(appContext);
        }
    }
}
