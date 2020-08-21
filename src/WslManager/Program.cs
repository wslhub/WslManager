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
        private static void Main(string[] args)
        {
            InitApplication();
            Application.Run(new AppContext(args));
        }
    }
}
