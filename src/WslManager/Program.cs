using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Windows.Forms;
using WslManager.Screens.MainForm;

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
